using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaG6.Data;

namespace SistemaFarmaciaG6.Controllers
{
    public class AuditoriaController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public AuditoriaController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        private bool EsAdministrador()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Administrador";
        }

        public IActionResult Index()
        {
            if (!EsAdministrador())
            {
                return RedirectToAction("Index", "Home");
            }

            var auditorias = _context.Auditoria
                .Include(a => a.IdUsuarioNavigation)
                .OrderByDescending(a => a.Fecha)
                .ToList();

            return View(auditorias);
        }
    }
}