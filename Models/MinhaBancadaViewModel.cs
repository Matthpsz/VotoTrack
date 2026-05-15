using System;
using System.Collections.Generic;

namespace VotoTrack.Models
{
    public class MinhaBancadaViewModel
    {
        public List<DeputadoDetalhe> DeputadosFavoritos { get; set; } = new List<DeputadoDetalhe>();
    }

    public class DeputadoDetalhe
    {
        public DeputadoRecord Deputado { get; set; }
        public List<VotoDetalhe> VotosRecentes { get; set; } = new List<VotoDetalhe>();
        public List<Noticia> Noticias { get; set; } = new List<Noticia>();
    }

    public class VotoDetalhe
    {
        public string Pec { get; set; }
        public string Descricao { get; set; }
        public string Voto { get; set; } // "Sim", "Não"
        public DateTime Data { get; set; }
    }

    public class Noticia
    {
        public string Titulo { get; set; }
        public string Resumo { get; set; }
        public DateTime Data { get; set; }
        public string Fonte { get; set; }
    }
}
