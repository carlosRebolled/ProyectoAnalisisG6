using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Data;

namespace SistemaFarmaciaG6.Controllers
{
    public class VisualizadorController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public VisualizadorController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "Visualizador")
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.TotalInformes = _context.InformeDocentes.Count();
            ViewBag.Aprobados = _context.InformeDocentes.Count(i => i.IdEstado == 4);
            ViewBag.Finalizados = _context.InformeDocentes.Count(i => i.IdEstado == 5);

            return View();
        }
    }
}