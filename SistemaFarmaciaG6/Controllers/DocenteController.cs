using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Data;

namespace SistemaFarmaciaG6.Controllers
{
    public class DocenteController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public DocenteController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");
            var rol = HttpContext.Session.GetString("Rol");

            if (idUsuario == null || rol != "Docente")
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.TotalInformes = _context.InformeDocentes.Count(i => i.IdUsuario == idUsuario);
            ViewBag.Borradores = _context.InformeDocentes.Count(i => i.IdUsuario == idUsuario && i.IdEstado == 1);
            ViewBag.Pendientes = _context.InformeDocentes.Count(i => i.IdUsuario == idUsuario && i.IdEstado == 2);
            ViewBag.Devueltos = _context.InformeDocentes.Count(i => i.IdUsuario == idUsuario && i.IdEstado == 3);
            ViewBag.Aprobados = _context.InformeDocentes.Count(i => i.IdUsuario == idUsuario && i.IdEstado == 4);

            return View();
        }
    }
}