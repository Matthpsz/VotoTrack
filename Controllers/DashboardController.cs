using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VotoTrack.Models;
using System.Linq;

namespace VotoTrack.Controllers
{
    public class DashboardController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public DashboardController(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClient = httpClientFactory.CreateClient("CamaraApi");
            _cache = cache;
        }

        public async Task<IActionResult> Publico([FromServices] Supabase.Client supabase)
        {
            // Busca TODOS os deputados (paginação paralela, ~513 deputados, 6 páginas de 100)
            var deputados = await _cache.GetOrCreateAsync("todos_deputados", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                var pageTasks = Enumerable.Range(1, 6)
                    .Select(page => SafeGetFromCache<ApiResponse>($"dep_page_{page}",
                        () => _httpClient.GetFromJsonAsync<ApiResponse>(
                            $"deputados?itens=100&ordem=ASC&ordenarPor=nome&pagina={page}")))
                    .ToList();

                await Task.WhenAll(pageTasks);

                return pageTasks
                    .Select(t => t.Result)
                    .Where(r => r?.Dados != null)
                    .SelectMany(r => r!.Dados)
                    .ToList();
            }) ?? new List<DeputadoRecord>();

            var votacoes = await SafeGetFromCache("votacoes_recentes",
                () => _httpClient.GetFromJsonAsync<VotacaoResponse>("votacoes?itens=15&ordem=DESC&ordenarPor=dataHoraRegistro"))
                .ContinueWith(t => t.Result?.dados ?? new List<VotacaoRecord>());

            var projetos = await SafeGetFromCache("projetos_recentes",
                () => _httpClient.GetFromJsonAsync<ProjetoResponse>("proposicoes?itens=5&ordem=DESC&ordenarPor=id"))
                .ContinueWith(t => t.Result?.dados ?? new List<ProjetoRecord>());

            ViewBag.Votacoes = votacoes
                .GroupBy(v => new { v.proposicaoNome, v.siglaOrgao })
                .Select(g => g.First())
                .Take(5)
                .ToList();

            ViewBag.Projetos = projetos;

            // Listas únicas para os filtros de dropdown
            ViewBag.Estados = deputados
                .Where(d => !string.IsNullOrEmpty(d.SiglaUf))
                .Select(d => d.SiglaUf)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            ViewBag.Partidos = deputados
                .Where(d => !string.IsNullOrEmpty(d.SiglaPartido))
                .Select(d => d.SiglaPartido)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            // Favoritos
            var favoritos = new HashSet<int>();
            try
            {
                var session = supabase.Auth.CurrentSession;
                if (session != null)
                {
                    var favResponse = await supabase.From<FavoritoModel>()
                        .Filter("user_id", Postgrest.Constants.Operator.Equals, session.User.Id)
                        .Get();
                    foreach (var f in favResponse.Models)
                        favoritos.Add(f.DeputadoId);
                }
            }
            catch { /* continua anônimo */ }
            ViewBag.Favoritos = favoritos;

            return View(deputados);
        }

        // Helper: busca do cache; se der timeout/erro, retorna null sem estourar a página
        private async Task<T?> SafeGetFromCache<T>(string key, Func<Task<T?>> factory) where T : class
        {
            if (_cache.TryGetValue(key, out T? cached))
                return cached;

            try
            {
                var result = await factory();
                if (result != null)
                    _cache.Set(key, result, CacheDuration);
                return result;
            }
            catch (Exception)
            {
                return null; // Timeout ou erro de rede: retorna vazio, não explode
            }
        }

        [HttpPost]
        public async Task<IActionResult> Favoritar([FromServices] Supabase.Client supabase, [FromBody] int deputadoId)
        {
            var session = supabase.Auth.CurrentSession;

            if (session == null)
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            try
            {
                var favorito = new FavoritoModel
                {
                    UserId = session.User.Id,
                    DeputadoId = deputadoId
                };

                await supabase.From<FavoritoModel>().Insert(favorito);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoverFavorito([FromServices] Supabase.Client supabase, [FromBody] int deputadoId)
        {
            var session = supabase.Auth.CurrentSession;

            if (session == null)
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            try
            {
                // Deleta todos os registros deste deputado para este usuário
                await supabase.From<FavoritoModel>()
                    .Filter("user_id", Postgrest.Constants.Operator.Equals, session.User.Id)
                    .Filter("deputado_id", Postgrest.Constants.Operator.Equals, deputadoId)
                    .Delete();
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MinhaBancada([FromServices] Supabase.Client supabase)
        {
            await supabase.InitializeAsync();
            var session = supabase.Auth.CurrentSession;

            if (session == null)
            {
                return RedirectToAction("Index", "Auth");
            }

            var viewModel = new MinhaBancadaViewModel();

            try
            {
                var favResponse = await supabase.From<FavoritoModel>()
                    .Filter("user_id", Postgrest.Constants.Operator.Equals, session.User.Id)
                    .Get();

                foreach (var f in favResponse.Models)
                {
                    try
                    {
                        var depData = await _httpClient.GetFromJsonAsync<ApiResponseDetalhe>($"deputados/{f.DeputadoId}");
                        if (depData?.Dados != null)
                        {
                            var detalhe = new DeputadoDetalhe
                            {
                                Deputado = new DeputadoRecord
                                {
                                    Id = depData.Dados.Id,
                                    Nome = depData.Dados.UltimoStatus?.Nome ?? depData.Dados.NomeCivil,
                                    SiglaPartido = depData.Dados.UltimoStatus?.SiglaPartido,
                                    SiglaUf = depData.Dados.UltimoStatus?.SiglaUf,
                                    UrlFoto = depData.Dados.UltimoStatus?.UrlFoto
                                }
                            };

                            // Busca discursos reais como "notícias"
                            try
                            {
                                var discursos = await _httpClient.GetFromJsonAsync<DiscursoResponse>($"deputados/{f.DeputadoId}/discursos?itens=2&ordem=DESC&ordenarPor=dataHoraInicio");
                                if (discursos?.dados != null)
                                {
                                    foreach (var d in discursos.dados)
                                    {
                                        detalhe.Noticias.Add(new Noticia
                                        {
                                            Titulo = d.titulo ?? "Pronunciamento Parlamentar",
                                            Resumo = d.ementa ?? d.keywords,
                                            Data = DateTime.TryParse(d.dataHoraInicio, out var dt) ? dt : DateTime.Now,
                                            Fonte = "Câmara dos Deputados"
                                        });
                                    }
                                }
                            }
                            catch { }

                            viewModel.DeputadosFavoritos.Add(detalhe);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao buscar deputado {f.DeputadoId}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> DetalhesDeputado(int id, [FromServices] Supabase.Client supabase)
        {
            await supabase.InitializeAsync();
            var session = supabase.Auth.CurrentSession;

            var viewModel = new DetalhesDeputadoViewModel();
            viewModel.IsFavorito = false;

            if (session != null)
            {
                var favResponse = await supabase.From<FavoritoModel>()
                    .Filter("user_id", Postgrest.Constants.Operator.Equals, session.User.Id)
                    .Filter("deputado_id", Postgrest.Constants.Operator.Equals, id)
                    .Get();

                if (favResponse.Models.Count > 0)
                {
                    viewModel.IsFavorito = true;
                }
            }

            try
            {
                var depData = await _httpClient.GetFromJsonAsync<ApiResponseDetalhe>($"deputados/{id}");
                if (depData?.Dados != null)
                {
                    var d = depData.Dados;
                    viewModel.Deputado = new DeputadoRecord
                    {
                        Id = d.Id,
                        Nome = d.UltimoStatus?.Nome ?? d.NomeCivil,
                        SiglaPartido = d.UltimoStatus?.SiglaPartido,
                        SiglaUf = d.UltimoStatus?.SiglaUf,
                        UrlFoto = d.UltimoStatus?.UrlFoto
                    };

                    viewModel.Detalhes = new DetalhesGerais
                    {
                        NomeCivil = d.NomeCivil,
                        DataNascimento = d.DataNascimento,
                        Escolaridade = d.Escolaridade,
                        MunicipioNascimento = d.MunicipioNascimento,
                        UfNascimento = d.UfNascimento,
                        CondicaoEleitoral = d.UltimoStatus?.CondicaoEleitoral,
                        Email = d.UltimoStatus?.Email,
                        Gabinete = d.UltimoStatus?.Gabinete != null ? new GabineteInfo
                        {
                            Nome = d.UltimoStatus.Gabinete.Nome,
                            Predio = d.UltimoStatus.Gabinete.Predio,
                            Sala = d.UltimoStatus.Gabinete.Sala,
                            Andar = d.UltimoStatus.Gabinete.Andar,
                            Telefone = d.UltimoStatus.Gabinete.Telefone,
                            Email = d.UltimoStatus.Gabinete.Email
                        } : null
                    };

                    // Despesas Reais
                    try
                    {
                        var despesasData = await _httpClient.GetFromJsonAsync<DespesaResponse>($"deputados/{id}/despesas?itens=15&ordem=DESC&ordenarPor=dataDocumento");
                        if (despesasData?.dados != null)
                        {
                            viewModel.Despesas = despesasData.dados.Select(d => new Despesa
                            {
                                TipoDespesa = d.tipoDespesa,
                                Valor = d.valorDocumento,
                                Data = DateTime.TryParse(d.dataDocumento, out var dt) ? dt : DateTime.MinValue,
                                Fornecedor = d.nomeFornecedor
                            }).ToList();
                        }
                    }
                    catch { }

                    // Atividades Reais (usando Discursos como exemplo de atividade)
                    try
                    {
                        var discursosData = await _httpClient.GetFromJsonAsync<DiscursoResponse>($"deputados/{id}/discursos?itens=15&ordem=DESC&ordenarPor=dataHoraInicio");
                        if (discursosData?.dados != null)
                        {
                            viewModel.Atividades = discursosData.dados.Select(d => new AtividadeLegislativa
                            {
                                Tipo = d.tipoDiscurso ?? "Discurso",
                                Titulo = d.titulo ?? "Pronunciamento",
                                Data = DateTime.TryParse(d.dataHoraInicio, out var dt) ? dt : DateTime.MinValue,
                                Descricao = !string.IsNullOrEmpty(d.ementa) ? d.ementa : d.keywords
                            }).ToList();
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }

            return View(viewModel);
        }
    }
}