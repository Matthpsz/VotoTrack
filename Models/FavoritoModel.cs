using Postgrest.Models;
using Postgrest.Attributes;

namespace VotoTrack.Models
{
    [Table("user_favorites")]
    public class FavoritoModel : BaseModel
    {
        [PrimaryKey("id", false)] // false means it is auto-incremented by db
        public int Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("deputado_id")]
        public int DeputadoId { get; set; }
    }
}
