using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Data;

namespace SistemaFarmaciaG6.Controllers
{
    public class JefaturaController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public JefaturaController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "Jefatura")
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.TotalInformes = _context.InformeDocentes.Count();
            ViewBag.Pendientes = _context.InformeDocentes.Count(i => i.IdEstado == 2);
            ViewBag.Devueltos = _context.InformeDocentes.Count(i => i.IdEstado == 3);
            ViewBag.Aprobados = _context.InformeDocentes.Count(i => i.IdEstado == 4);

            return View();
        }
    }
}