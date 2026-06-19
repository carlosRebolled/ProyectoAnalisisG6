using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Helpers;
using SistemaFarmaciaG6.Models;

namespace SistemaFarmaciaG6.Controllers
{
    public class RevisionInformeDireccionController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public RevisionInformeDireccionController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        private bool PuedeRevisar()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Decano" || rol == "Administrador";
        }

        public IActionResult Index()
        {
            if (!PuedeRevisar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informes = _context.InformeDireccions
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .OrderByDescending(i => i.FechaEnvio)
                .ToList();

            return View(informes);
        }

        public IActionResult Details(int id)
        {
            if (!PuedeRevisar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informe = _context.InformeDireccions
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .FirstOrDefault(i => i.IdInformeDireccion == id);

            if (informe == null)
            {
                return NotFound();
            }

            var observaciones = _context.Observaciones
                .Where(o => o.IdInformeDireccion == id)
                .OrderByDescending(o => o.Fecha)
                .ToList();

            ViewBag.Observaciones = observaciones;

            return View(informe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Aprobar(int id)
        {
            if (!PuedeRevisar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informe = _context.InformeDireccions.Find(id);

            if (informe == null)
            {
                return NotFound();
            }

            if (informe.IdEstado != 2)
            {
                TempData["Error"] = "Solo se pueden aprobar informes en estado Pendiente.";
                return RedirectToAction(nameof(Index));
            }

            informe.IdEstado = 4;
            informe.FechaAprobacion = DateTime.Now;

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeDireccion",
                "Aprobar",
                $"El Decano aprobó el informe de dirección #{informe.IdInformeDireccion} del año {informe.Anio}."
            );

            TempData["Exito"] = "Informe de dirección aprobado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Devolver(int id, string observacion)
        {
            if (!PuedeRevisar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informe = _context.InformeDireccions.Find(id);

            if (informe == null)
            {
                return NotFound();
            }

            if (informe.IdEstado != 2)
            {
                TempData["Error"] = "Solo se pueden devolver informes en estado Pendiente.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(observacion))
            {
                TempData["Error"] = "Debe escribir una observación para devolver el informe.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            informe.IdEstado = 3;

            var nuevaObservacion = new Observacione
            {
                IdInformeDireccion = id,
                IdUsuarioRealiza = HttpContext.Session.GetInt32("IdUsuario")!.Value,
                Comentario = observacion,
                Fecha = DateTime.Now,
                TipoObservacion = "Devolución Decano"
            };

            _context.Observaciones.Add(nuevaObservacion);
            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeDireccion",
                "Devolver",
                $"El Decano devolvió el informe de dirección #{informe.IdInformeDireccion} con observación."
            );

            TempData["Exito"] = "Informe de dirección devuelto correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}