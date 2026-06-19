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

            if (usuario.Estado != "Activo")
            {
                ViewBag.Error = "Su cuenta se encuentra inactiva. Contacte al administrador.";
                return View();
            }

            var usuarioRol = _context.UsuarioRols
                .FirstOrDefault(ur => ur.IdUsuario == usuario.IdUsuario);

            if (usuarioRol == null)
            {
                ViewBag.Error = "El usuario no tiene un rol asignado";
                return View();
            }

            var rol = _context.Roles
                .FirstOrDefault(r => r.IdRol == usuarioRol.IdRol);

            if (rol == null)
            {
                ViewBag.Error = "El rol asignado no existe";
                return View();
            }

            HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);
            HttpContext.Session.SetString("Nombre", $"{usuario.Nombre} {usuario.Apellido1} {usuario.Apellido2}");
            HttpContext.Session.SetString("Rol", rol.NombreRol);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}