using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Integrador_DevOps.Models;
using Proyecto_Integrador_DevOps.Service;

namespace Proyecto_Integrador_DevOps.Controllers
{
    public class HomeController : Controller
    {
        private readonly ReportService _queryService;

        public HomeController(ReportService queryService)
        {
            _queryService = queryService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var user = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(user))
                return RedirectToAction("Login", "Auth");

            return View(new QueryResultViewModel());
        }

        [HttpPost]
        public IActionResult Execute(QueryResultViewModel model)
        {
            var user = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(user))
                return RedirectToAction("Login", "Auth");

            var report = _queryService.GetReportById(model.QueryReport.ReportId);

            var validationErrors = _queryService.ValidateQueryRequest(report);

            if (validationErrors.Any())
            {
                model.ErrorMessages = validationErrors;
            }
            else
            {
                var result = _queryService.ExecuteQuery(report, out string? errorMessage);

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    model.ErrorMessage = errorMessage;
                }
                else if (result == null || result.Rows.Count == 0)
                {
                    model.WarningMessage = "La consulta se ejecutó correctamente, pero no devolvió resultados.";
                }
                else
                {
                    model.Result = result;
                }
            }

            return View("Index", model);
        }
    }
}
