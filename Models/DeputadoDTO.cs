using System.Text.Json.Serialization;

namespace VotoTrack.Models
{
    public class DeputadoRecord
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string SiglaPartido { get; set; }
        public string SiglaUf { get; set; }
        public string UrlFoto { get; set; }
        public string Esfera { get; set; } = "Federal"; // Default: Federal
        public string IdGlobal => $"{Esfera}_{Id}"; // ID Único para evitar colisões
    }

    public class TopGastadorRecord
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string SiglaPartido { get; set; }
        public string SiglaUf { get; set; }
        public string UrlFoto { get; set; }
        public decimal TotalGasto { get; set; }
    }

    public class TopPresencaRecord
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string SiglaPartido { get; set; }
        public string SiglaUf { get; set; }
        public string UrlFoto { get; set; }
        public double PresencaPorcentagem { get; set; }
        public int SessoesPresenca { get; set; }
        public int SessoesTotal { get; set; }
    }

    public class ApiResponse
    {
        public List<DeputadoRecord> Dados { get; set; }
    }

    public class EventoRecord
    {
        public int Id { get; set; }
        public string DataHoraInicio { get; set; }
        public string Descricao { get; set; }
    }

    public class EventosApiResponse
    {
        public List<EventoRecord> Dados { get; set; }
    }

    public class ApiResponseDetalhe
    {
        public DeputadoDetalheRecord Dados { get; set; }
    }

    public class DeputadoDetalheRecord
    {
        public int Id { get; set; }
        public string NomeCivil { get; set; }
        public string Escolaridade { get; set; }
        public string DataNascimento { get; set; }
        public string MunicipioNascimento { get; set; }
        public string UfNascimento { get; set; }
        public UltimoStatusRecord UltimoStatus { get; set; }
    }

    public class UltimoStatusRecord
    {
        public string Nome { get; set; }
        public string SiglaPartido { get; set; }
        public string SiglaUf { get; set; }
        public string UrlFoto { get; set; }
        public string CondicaoEleitoral { get; set; }
        public string Email { get; set; }
        public GabineteRecord Gabinete { get; set; }
    }

    public class GabineteRecord
    {
        public string Nome { get; set; }
        public string Predio { get; set; }
        public string Sala { get; set; }
        public string Andar { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
    }

    public class VotacaoRecord
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("siglaOrgao")]
        public string SiglaOrgao { get; set; }
        [JsonPropertyName("proposicaoNome")]
        public string ProposicaoNome { get; set; }
        [JsonPropertyName("data")]
        public string Data { get; set; }
    }

    public class VotacaoResponse
    {
        [JsonPropertyName("dados")]
        public List<VotacaoRecord> Dados { get; set; }
    }

    public class ProjetoRecord
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("siglaTipo")]
        public string SiglaTipo { get; set; }
        [JsonPropertyName("numero")]
        public int Numero { get; set; }
        [JsonPropertyName("ano")]
        public int Ano { get; set; }
        [JsonPropertyName("ementa")]
        public string Ementa { get; set; }
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }

    public class ProjetoResponse
    {
        [JsonPropertyName("dados")]
        public List<ProjetoRecord> Dados { get; set; }
    }

    public class DespesaRecord
    {
        [JsonPropertyName("tipoDespesa")]
        public string TipoDespesa { get; set; }
        [JsonPropertyName("valorDocumento")]
        public decimal ValorDocumento { get; set; }
        [JsonPropertyName("dataDocumento")]
        public string DataDocumento { get; set; }
        [JsonPropertyName("nomeFornecedor")]
        public string NomeFornecedor { get; set; }
        [JsonPropertyName("urlDocumento")]
        public string UrlDocumento { get; set; }
    }

    public class DespesaResponse
    {
        [JsonPropertyName("dados")]
        public List<DespesaRecord> Dados { get; set; }
    }

    public class DiscursoRecord
    {
        [JsonPropertyName("tipoDiscurso")]
        public string TipoDiscurso { get; set; }
        [JsonPropertyName("dataHoraInicio")]
        public string DataHoraInicio { get; set; }
        [JsonPropertyName("keywords")]
        public string Keywords { get; set; }
        [JsonPropertyName("ementa")]
        public string Ementa { get; set; }
        [JsonPropertyName("titulo")]
        public string Titulo { get; set; }
        [JsonPropertyName("urlTexto")]
        public string UrlTexto { get; set; }
        [JsonPropertyName("urlVideo")]
        public string UrlVideo { get; set; }
        [JsonPropertyName("urlAudio")]
        public string UrlAudio { get; set; }
    }

    public class DiscursoResponse
    {
        [JsonPropertyName("dados")]
        public List<DiscursoRecord> Dados { get; set; }
    }

    public class SenateResponse
    {
        [JsonPropertyName("ListaParticipacaoAtual")]
        public ListaParticipacaoAtual ListaParticipacaoAtual { get; set; }
    }

    public class ListaParticipacaoAtual
    {
        [JsonPropertyName("Parlamentares")]
        public Parlamentares Parlamentares { get; set; }
    }

    public class Parlamentares
    {
        [JsonPropertyName("Parlamentar")]
        public List<SenadorRecord> Parlamentar { get; set; }
    }

    public class SenadorRecord
    {
        [JsonPropertyName("IdentificacaoParlamentar")]
        public IdentificacaoParlamentar IdentificacaoParlamentar { get; set; }
    }

    public class IdentificacaoParlamentar
    {
        [JsonPropertyName("CodigoParlamentar")]
        public int CodigoParlamentar { get; set; }
        [JsonPropertyName("NomeParlamentar")]
        public string NomeParlamentar { get; set; }
        [JsonPropertyName("SiglaPartidoParlamentar")]
        public string SiglaPartidoParlamentar { get; set; }
        [JsonPropertyName("UfParlamentar")]
        public string UfParlamentar { get; set; }
        [JsonPropertyName("UrlFotoParlamentar")]
        public string UrlFotoParlamentar { get; set; }
    }
}