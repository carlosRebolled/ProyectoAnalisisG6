using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Models;

namespace SistemaFarmaciaG6.Controllers
{
    public class AccountController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public AccountController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string correo, string contrasena)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Correo == correo &&
                                     u.Contrasena == contrasena);

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos";
                return View();
            }

            HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);
            HttpContext.Session.SetString("Nombre", usuario.Nombre);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}