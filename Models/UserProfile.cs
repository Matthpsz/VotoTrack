using Postgrest.Attributes;
using Postgrest.Models;

namespace VotoTrack.Models
{
    [Table("user_profiles")]
    public class UserProfile : BaseModel
    {
        [PrimaryKey("id", true)]
        public string Id { get; set; }

        [Column("tema_preferido")]
        public string TemaPreferido { get; set; } // Ex: "Saúde", "Educação"

        [Column("nome_exibicao")]
        public string NomeExibicao { get; set; }
    }
}