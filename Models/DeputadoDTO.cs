namespace VotoTrack.Models
{
    public class DeputadoRecord
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string SiglaPartido { get; set; }
        public string SiglaUf { get; set; }
        public string UrlFoto { get; set; }
    }

    public class ApiResponse
    {
        public List<DeputadoRecord> Dados { get; set; }
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
        public string id { get; set; } // Adicione o ID
        public string siglaOrgao { get; set; }
        public string proposicaoNome { get; set; }
        public string data { get; set; }
    }

    public class VotacaoResponse
    {
        public List<VotacaoRecord> dados { get; set; }
    }

    public class ProjetoRecord
    {
        public int id { get; set; } // Adicione o ID se não tiver
        public string siglaTipo { get; set; }
        public int numero { get; set; }
        public int ano { get; set; }
        public string ementa { get; set; }
        public string uri { get; set; } // Link da API ou detalhamento
    }

    public class ProjetoResponse
    {
        public List<ProjetoRecord> dados { get; set; }
    }

    public class DespesaRecord
    {
        public string tipoDespesa { get; set; }
        public decimal valorDocumento { get; set; }
        public string dataDocumento { get; set; }
        public string nomeFornecedor { get; set; }
    }

    public class DespesaResponse
    {
        public List<DespesaRecord> dados { get; set; }
    }

    public class DiscursoRecord
    {
        public string tipoDiscurso { get; set; }
        public string dataHoraInicio { get; set; }
        public string keywords { get; set; }
        public string ementa { get; set; }
        public string titulo { get; set; }
    }

    public class DiscursoResponse
    {
        public List<DiscursoRecord> dados { get; set; }
    }
}