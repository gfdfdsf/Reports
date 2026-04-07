using Microsoft.AspNetCore.Mvc;
using Proyecto_Integrador_DevOps.Models;
using Proyecto_Integrador_DevOps.Service;

namespace Proyecto_Integrador_DevOps.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ReportService _queryService;

        public ReportsController(ReportService queryService)
        {
            _queryService = queryService;
        }

        [HttpGet]
        public IActionResult List()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var area = HttpContext.Session.GetString("Area");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var reports = _queryService.GetReports(area!, isAdmin);

            return View(reports);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";

            if (!isAdmin)
                return RedirectToAction("List");

            var model = new QueryResultViewModel
            {
                QueryReport = new QueryReport()
            };

            return View("~/Views/Home/Index.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(QueryResultViewModel model)
        {
            if (!ModelState.IsValid)
                return View("ReportsList", model);

            model.QueryReport.UserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            await _queryService.CreateReportAsync(model.QueryReport);

            return RedirectToAction("List");
        }

        [HttpGet]
        public IActionResult Open(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var report = _queryService.GetReportById(id);

            if (report == null)
                return RedirectToAction("List");

            var model = new QueryResultViewModel
            {
                QueryReport = report
            };

            return View("~/Views/Home/Index.cshtml", model);
        }

        [HttpPost]
        public IActionResult Update(QueryResultViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            if (!isAdmin)
                return RedirectToAction("List");

            if (model.QueryReport == null)
                return RedirectToAction("List");

            _queryService.UpdateReport(model.QueryReport);

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult DeleteReport(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "True")
            {
                return Unauthorized();
            }

            _queryService.DeleteReport(id);

            return RedirectToAction("List");
        }
    }
}
