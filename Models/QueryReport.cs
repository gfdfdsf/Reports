namespace Proyecto_Integrador_DevOps.Models
{
    public class QueryReport
    {
        public int ReportId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Query { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
    }
}
