using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace VotoTrack.Models
{
    [Table("LogsTelemetria")]
    public class LogTelemetria : BaseModel
    {
        [PrimaryKey("Id", false)]
        public long Id { get; set; }

        [Column("Rota")]
        public string Rota { get; set; } = string.Empty;

        [Column("MetodoHttp")]
        public string MetodoHttp { get; set; } = string.Empty;

        [Column("StatusCode")]
        public int StatusCode { get; set; }

        [Column("TempoExecucaoMs")]
        public long TempoExecucaoMs { get; set; }

        [Column("DataRequisicao")]
        public DateTime DataRequisicao { get; set; }

        [Column("ChaveApi")]
        public string? ChaveApi { get; set; }

        [Column("Projeto")]
        public string Projeto { get; set; } = "VotoTrack";
    }
}
