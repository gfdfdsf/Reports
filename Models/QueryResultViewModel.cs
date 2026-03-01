using System.Data;

namespace Proyecto_Integrador_DevOps.Models
{
    public class QueryResultViewModel
    {
        public QueryReport? QueryReport { get; set; }
        public DataTable? Result { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> ErrorMessages { get; set; } = new();
        public string? WarningMessage { get; set; }
    }
}
