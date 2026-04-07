using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Proyecto_Integrador_DevOps.Infrastructure;
using Proyecto_Integrador_DevOps.Models;

namespace Proyecto_Integrador_DevOps.Service
{
    public class ReportService
    {
        private readonly IConfiguration _configuration;
        private readonly ReportDAO _queryDao;

        private static readonly string[] ForbiddenKeywords =
        {
            "insert", "update", "delete", "drop", "alter", "truncate", "create"
        };

        public ReportService(IConfiguration configuration, ReportDAO queryDao)
        {
            _configuration = configuration;
            _queryDao = queryDao;
        }

        public User AuthenticateUser(string email, string password)
        {
            return _queryDao.GetUserByEmailAndPassword(email, password);
        }

        public void RegisterUser(RegisterViewModel model)
        {
            User user = new User
            {
                Email = model.Email,
                PasswordHash = model.Password,
                FullName = model.FullName,
                Area = model.Area,
                IsAdmin = false
            };

            _queryDao.InsertUser(user);
        }

        public List<QueryReport> GetReports(string area, bool isAdmin)
        {
            if (isAdmin)
                return _queryDao.GetAllReports();

            return _queryDao.GetReportsByArea(area);
        }

        public QueryReport GetReportById(int id)
        {
            return _queryDao.GetReportById(id)!;
        }

        public async Task CreateReportAsync(QueryReport report)
        {
            await _queryDao.CreateAsync(report);
        }

        public void UpdateReport(QueryReport report)
        {
            _queryDao.UpdateReport(report);
        }

        public void DeleteReport(int id)
        {
            _queryDao.DeleteReport(id);
        }

        public List<string> ValidateQueryRequest(QueryReport report)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(report.ConnectionString))
            {
                errors.Add("Debe seleccionar una base de datos válida.");
            }
            else
            {
                var connectionString = _configuration.GetConnectionString(report.ConnectionString);

                if (string.IsNullOrEmpty(connectionString))
                {
                    errors.Add("La cadena de conexión seleccionada no existe.");
                }
            }

            if (string.IsNullOrWhiteSpace(report.Query))
            {
                errors.Add("La query no puede estar vacía.");
            }
            else
            {
                var queryLower = report.Query.ToLowerInvariant();

                foreach (var keyword in ForbiddenKeywords)
                {
                    var regex = new Regex(@"\b" + keyword + @"\b", RegexOptions.IgnoreCase);

                    if (regex.IsMatch(queryLower))
                    {
                        errors.Add($"La palabra '{keyword.ToUpper()}' no está permitida para consultas.");
                    }
                }
            }

            return errors;
        }

        public DataTable? ExecuteQuery(QueryReport report, out string? errorMessage)
        {
            errorMessage = null;

            var connectionString = _configuration.GetConnectionString(report.ConnectionString);

            if (string.IsNullOrEmpty(connectionString))
            {
                errorMessage = "Cadena de conexión no válida.";
                return null;
            }

            var (result, daoError) = _queryDao.ExecuteReadOnlyQuery(connectionString, report.Query);

            if (!string.IsNullOrEmpty(daoError))
            {
                errorMessage = daoError;
                return null;
            }

            return result;
        }
    }
}
