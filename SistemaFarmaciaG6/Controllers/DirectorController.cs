using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaG6.Data;

namespace SistemaFarmaciaG6.Controllers
{
    public class DirectorController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public DirectorController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");
            var rol = HttpContext.Session.GetString("Rol");

            if (idUsuario == null || rol != "Director")
            {
                return RedirectToAction("Index", "Home");
            }

            var director = _context.Usuarios.Find(idUsuario);

            if (director == null)
            {
                return NotFound();
            }

            var informesDepartamento = _context.InformeDocentes
                .Include(i => i.IdUsuarioNavigation)
                .Where(i => i.IdUsuarioNavigation.IdDepartamento == director.IdDepartamento);

            ViewBag.Pendientes = informesDepartamento.Count(i => i.IdEstado == 2);
            ViewBag.Devueltos = informesDepartamento.Count(i => i.IdEstado == 3);
            ViewBag.Aprobados = informesDepartamento.Count(i => i.IdEstado == 4);
            ViewBag.Total = informesDepartamento.Count();

            return View();
        }
    }
}