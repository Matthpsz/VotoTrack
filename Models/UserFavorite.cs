using Postgrest.Attributes;
using Postgrest.Models;

namespace VotoTrack.Models
{
    [Table("user_favorites")]
    public class UserFavorite : BaseModel
    {
        [Column("user_id")]
        public string UserId { get; set; }

        [Column("deputado_id")]
        public int DeputadoId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}