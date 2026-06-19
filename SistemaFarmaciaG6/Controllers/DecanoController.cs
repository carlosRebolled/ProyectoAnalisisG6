using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Data;

namespace SistemaFarmaciaG6.Controllers
{
    public class DecanoController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public DecanoController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "Decano")
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.TotalInformesDocentes = _context.InformeDocentes.Count();
            ViewBag.InformesPendientes = _context.InformeDocentes.Count(i => i.IdEstado == 2);
            ViewBag.InformesAprobados = _context.InformeDocentes.Count(i => i.IdEstado == 4);

            return View();
        }
    }
}