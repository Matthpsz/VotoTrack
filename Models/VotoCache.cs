using Postgrest.Attributes;
using Postgrest.Models;

namespace VotoTrack.Models
{
    [Table("votos_cache")]
    public class VotoCache : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; } // ID da Votação na API da Câmara

        [Column("ementa")]
        public string Ementa { get; set; }

        [Column("data_votacao")]
        public DateTime DataVotacao { get; set; }

        [Column("categoria")]
        public string Categoria { get; set; } // Ex: Segurança, Economia
    }
}