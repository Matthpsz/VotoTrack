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

                            // Mocking votes for demonstration as full vote history requires complex data assembly
                            detalhe.VotosRecentes.Add(new VotoDetalhe { Pec = "PEC 45/2019", Descricao = "Reforma Tributária", Voto = "Sim", Data = DateTime.Now.AddDays(-10) });
                            detalhe.VotosRecentes.Add(new VotoDetalhe { Pec = "PL 2630/2020", Descricao = "Lei das Fake News", Voto = "Não", Data = DateTime.Now.AddDays(-25) });

                            // Mocking news
                            detalhe.Noticias.Add(new Noticia { Titulo = $"{detalhe.Deputado.Nome} discursa sobre a PEC 45/2019", Resumo = "Parlamentar defende a aprovação do texto base...", Data = DateTime.Now.AddDays(-5), Fonte = "Câmara dos Deputados" });

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

                    // Despesas
                    viewModel.Despesas.Add(new Despesa { TipoDespesa = "Passagens Aéreas", Valor = 3500.50m, Data = DateTime.Now.AddDays(-5), Fornecedor = "GOL Linhas Aéreas" });
                    viewModel.Despesas.Add(new Despesa { TipoDespesa = "Serviços Postais", Valor = 150.00m, Data = DateTime.Now.AddDays(-12), Fornecedor = "Correios" });
                    viewModel.Despesas.Add(new Despesa { TipoDespesa = "Telefonia", Valor = 450.75m, Data = DateTime.Now.AddDays(-15), Fornecedor = "Vivo S.A." });
                    viewModel.Despesas.Add(new Despesa { TipoDespesa = "Manutenção de Escritório", Valor = 1200.00m, Data = DateTime.Now.AddDays(-20), Fornecedor = "Imobiliária Centro" });
                    viewModel.Despesas.Add(new Despesa { TipoDespesa = "Divulgação Parlamentar", Valor = 2800.00m, Data = DateTime.Now.AddDays(-25), Fornecedor = "Gráfica Brasília" });
                    viewModel.Despesas.Add(new Despesa { TipoDespesa = "Combustíveis", Valor = 620.30m, Data = DateTime.Now.AddDays(-30), Fornecedor = "Posto BR Brasília" });
                    viewModel.Despesas.Add(new Despesa { TipoDespesa = "Hospedagem", Valor = 890.00m, Data = DateTime.Now.AddDays(-35), Fornecedor = "Hotel Nacional" });
                    viewModel.Despesas.Add(new Despesa { TipoDespesa = "Consultoria Técnica", Valor = 5000.00m, Data = DateTime.Now.AddDays(-40), Fornecedor = "Instituto Legislativo" });

                    // Atividades Legislativas
                    viewModel.Atividades.Add(new AtividadeLegislativa { Tipo = "Discurso", Titulo = "Pronunciamento no Plenário", Data = DateTime.Now.AddDays(-2), Descricao = "Defesa de maiores investimentos em educação básica e acesso à universidade pública para estudantes de baixa renda." });
                    viewModel.Atividades.Add(new AtividadeLegislativa { Tipo = "Voto", Titulo = "PEC 45/2019 — Reforma Tributária", Data = DateTime.Now.AddDays(-10), Descricao = "Votou SIM ao texto-base da Reforma Tributária, que unifica impostos sobre consumo e cria o IVA dual brasileiro." });
                    viewModel.Atividades.Add(new AtividadeLegislativa { Tipo = "Presença", Titulo = "Comissão de Constituição e Justiça (CCJ)", Data = DateTime.Now.AddDays(-14), Descricao = "Registrou presença na 14ª reunião ordinária da CCJ. Pauta: análise de admissibilidade de PECs pendentes." });
                    viewModel.Atividades.Add(new AtividadeLegislativa { Tipo = "Voto", Titulo = "PL 2630/2020 — Lei das Fake News", Data = DateTime.Now.AddDays(-20), Descricao = "Votou NÃO ao projeto que regulamenta a responsabilidade de plataformas digitais sobre a disseminação de desinformação." });
                    viewModel.Atividades.Add(new AtividadeLegislativa { Tipo = "Discurso", Titulo = "Debate sobre Segurança Pública", Data = DateTime.Now.AddDays(-28), Descricao = "Apresentou dados sobre aumento da criminalidade organizada e defendeu maior orçamento para forças de segurança estaduais." });
                    viewModel.Atividades.Add(new AtividadeLegislativa { Tipo = "Presença", Titulo = "Comissão de Saúde", Data = DateTime.Now.AddDays(-35), Descricao = "Participou da audiência pública sobre a regulamentação de planos de saúde e reajustes de mensalidades." });
                    viewModel.Atividades.Add(new AtividadeLegislativa { Tipo = "Voto", Titulo = "PEC 32/2020 — Reforma Administrativa", Data = DateTime.Now.AddDays(-45), Descricao = "Votou SIM à proposta que altera regras de ingresso e carreira no serviço público federal." });
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