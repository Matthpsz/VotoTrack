using System;
using System.Collections.Generic;

namespace VotoTrack.Models
{
    public class DetalhesDeputadoViewModel
    {
        public DeputadoRecord Deputado { get; set; }
        public DetalhesGerais Detalhes { get; set; }
        public List<AtividadeLegislativa> Atividades { get; set; } = new List<AtividadeLegislativa>();
        public List<Despesa> Despesas { get; set; } = new List<Despesa>();
        public bool IsFavorito { get; set; }
    }

    public class DetalhesGerais
    {
        public string NomeCivil { get; set; }
        public string DataNascimento { get; set; }
        public string Escolaridade { get; set; }
        public string MunicipioNascimento { get; set; }
        public string UfNascimento { get; set; }
        public string CondicaoEleitoral { get; set; }
        public string Email { get; set; }
        public GabineteInfo Gabinete { get; set; }
    }

    public class GabineteInfo
    {
        public string Nome { get; set; }
        public string Predio { get; set; }
        public string Sala { get; set; }
        public string Andar { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
    }

    public class AtividadeLegislativa
    {
        public string Titulo { get; set; }
        public string Tipo { get; set; } // Discurso, Presença, Voto, Projeto
        public DateTime Data { get; set; }
        public string Descricao { get; set; }
        public string UrlLink { get; set; }
    }

    public class Despesa
    {
        public string TipoDespesa { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string Fornecedor { get; set; }
        public string UrlDocumento { get; set; }
    }
}
