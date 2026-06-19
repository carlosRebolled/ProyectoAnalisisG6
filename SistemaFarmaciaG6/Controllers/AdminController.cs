using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Data;

namespace SistemaFarmaciaG6.Controllers
{
    public class AdminController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public AdminController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        private bool EsAdministrador()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Administrador";
        }

        public IActionResult Dashboard()
        {
            if (!EsAdministrador())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.TotalUsuarios = _context.Usuarios.Count();
            ViewBag.UsuariosActivos = _context.Usuarios.Count(u => u.Estado == "Activo");
            ViewBag.UsuariosInactivos = _context.Usuarios.Count(u => u.Estado == "Inactivo");

            ViewBag.TotalDocentes = _context.UsuarioRols
                .Count(ur => ur.IdRol == 1 && ur.IdUsuarioNavigation.Estado == "Activo");

            ViewBag.TotalDirectores = _context.UsuarioRols
                .Count(ur => ur.IdRol == 2 && ur.IdUsuarioNavigation.Estado == "Activo");

            ViewBag.TotalDecanos = _context.UsuarioRols
                .Count(ur => ur.IdRol == 3 && ur.IdUsuarioNavigation.Estado == "Activo");

            ViewBag.TotalAdministradores = _context.UsuarioRols
                .Count(ur => ur.IdRol == 4 && ur.IdUsuarioNavigation.Estado == "Activo");

            ViewBag.TotalJefatura = _context.UsuarioRols
                .Count(ur => ur.IdRol == 5 && ur.IdUsuarioNavigation.Estado == "Activo");

            ViewBag.TotalVisualizadores = _context.UsuarioRols
                .Count(ur => ur.IdRol == 6 && ur.IdUsuarioNavigation.Estado == "Activo");

            ViewBag.TotalInformes = _context.InformeDocentes.Count();
            ViewBag.InformesBorrador = _context.InformeDocentes.Count(i => i.IdEstado == 1);
            ViewBag.InformesPendientes = _context.InformeDocentes.Count(i => i.IdEstado == 2);
            ViewBag.InformesDevueltos = _context.InformeDocentes.Count(i => i.IdEstado == 3);
            ViewBag.InformesAprobados = _context.InformeDocentes.Count(i => i.IdEstado == 4);
            ViewBag.InformesFinalizados = _context.InformeDocentes.Count(i => i.IdEstado == 5);

            return View();
        }
    }
}