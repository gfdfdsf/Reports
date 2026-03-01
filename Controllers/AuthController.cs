using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Integrador_DevOps.Models;
using Proyecto_Integrador_DevOps.Service;

namespace Proyecto_Integrador_DevOps.Controllers
{
    public class AuthController : Controller
    {
        private readonly ReportService _reportService;

        public AuthController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _reportService.AuthenticateUser(model.Email, model.Password);

            if (user == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos";
                return View(model);
            }

            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Area", user.Area);
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

            return RedirectToAction("List", "Reports");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _reportService.RegisterUser(model);

            return RedirectToAction("Login");
        }
    }
}
