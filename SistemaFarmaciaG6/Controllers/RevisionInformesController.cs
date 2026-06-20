using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Helpers;
using SistemaFarmaciaG6.Models;

namespace SistemaFarmaciaG6.Controllers
{
    public class RevisionInformesController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public RevisionInformesController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        private bool PuedeRevisar()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Director" || rol == "Administrador";
        }

        private int? ObtenerDepartamentoDirector()
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
                return null;

            var usuario = _context.Usuarios.Find(idUsuario);

            return usuario?.IdDepartamento;
        }

        public IActionResult Index()
        {
            if (!PuedeRevisar())
                return RedirectToAction("Index", "Home");

            var rol = HttpContext.Session.GetString("Rol");
            var idDepartamento = ObtenerDepartamentoDirector();

            var informes = _context.InformeDocentes
                .Include(i => i.IdUsuarioNavigation)
                .Include(i => i.IdEstadoNavigation)
                .AsQueryable();

            if (rol == "Director")
            {
                informes = informes.Where(i =>
                    i.IdUsuarioNavigation.IdDepartamento == idDepartamento);
            }

            informes = informes.Where(i =>
                i.IdEstado == 2 ||
                i.IdEstado == 3 ||
                i.IdEstado == 4);

            return View(informes.OrderByDescending(i => i.FechaEnvio).ToList());
        }

        public IActionResult Details(int id)
        {
            if (!PuedeRevisar())
                return RedirectToAction("Index", "Home");

            var rol = HttpContext.Session.GetString("Rol");
            var idDepartamento = ObtenerDepartamentoDirector();

            var informe = _context.InformeDocentes
                .Include(i => i.IdUsuarioNavigation)
                .Include(i => i.IdEstadoNavigation)
                .FirstOrDefault(i => i.IdInformeDocente == id);

            if (informe == null)
                return NotFound();

            if (rol == "Director" &&
                informe.IdUsuarioNavigation.IdDepartamento != idDepartamento)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(informe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Aprobar(int id)
        {
            if (!PuedeRevisar())
                return RedirectToAction("Index", "Home");

            var rol = HttpContext.Session.GetString("Rol");
            var idDepartamento = ObtenerDepartamentoDirector();

            var informe = _context.InformeDocentes
                .Include(i => i.IdUsuarioNavigation)
                .FirstOrDefault(i => i.IdInformeDocente == id);

            if (informe == null)
                return NotFound();

            if (rol == "Director" &&
                informe.IdUsuarioNavigation.IdDepartamento != idDepartamento)
            {
                TempData["Error"] = "No puede aprobar informes de otro departamento.";
                return RedirectToAction(nameof(Index));
            }

            if (informe.IdEstado != 2)
            {
                TempData["Error"] = "Solo se pueden aprobar informes pendientes.";
                return RedirectToAction(nameof(Index));
            }

            informe.IdEstado = 4;
            informe.FechaAprobacion = DateTime.Now;

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeDocente",
                "Aprobar",
                $"Se aprobó el informe docente #{informe.IdInformeDocente} del año {informe.Anio}."
            );

            TempData["Exito"] = "Informe aprobado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Devolver(int id, string observacion)
        {
            if (!PuedeRevisar())
                return RedirectToAction("Index", "Home");

            var rol = HttpContext.Session.GetString("Rol");
            var idDepartamento = ObtenerDepartamentoDirector();

            var informe = _context.InformeDocentes
                .Include(i => i.IdUsuarioNavigation)
                .FirstOrDefault(i => i.IdInformeDocente == id);

            if (informe == null)
                return NotFound();

            if (rol == "Director" &&
                informe.IdUsuarioNavigation.IdDepartamento != idDepartamento)
            {
                TempData["Error"] = "No puede devolver informes de otro departamento.";
                return RedirectToAction(nameof(Index));
            }

            if (informe.IdEstado != 2)
            {
                TempData["Error"] = "Solo se pueden devolver informes pendientes.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(observacion))
            {
                TempData["Error"] = "Debe escribir una observación.";
                return RedirectToAction(nameof(Details), new { id });
            }

            informe.IdEstado = 3;

            var nuevaObservacion = new Observacione
            {
                IdInformeDocente = id,
                IdUsuarioRealiza = HttpContext.Session.GetInt32("IdUsuario")!.Value,
                Comentario = observacion,
                Fecha = DateTime.Now,
                TipoObservacion = "Devolución"
            };

            _context.Observaciones.Add(nuevaObservacion);
            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeDocente",
                "Devolver",
                $"Se devolvió el informe docente #{informe.IdInformeDocente}."
            );

            TempData["Exito"] = "Informe devuelto correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}