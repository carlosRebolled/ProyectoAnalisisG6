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

        private bool PuedeRevisarInformes()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Director" || rol == "Administrador";
        }

        private IActionResult RedirigirNoAutorizado()
        {
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index()
        {
            if (!PuedeRevisarInformes())
            {
                return RedirigirNoAutorizado();
            }

            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var director = _context.Usuarios.Find(idUsuario);

            if (director == null)
            {
                return NotFound();
            }

            var informes = _context.InformeDocentes
                .Include(i => i.IdUsuarioNavigation)
                .Include(i => i.IdEstadoNavigation)
                .Where(i => i.IdUsuarioNavigation.IdDepartamento == director.IdDepartamento)
                .Where(i => i.IdEstado == 2 || i.IdEstado == 3 || i.IdEstado == 4)
                .OrderByDescending(i => i.FechaEnvio)
                .ToList();

            return View(informes);
        }

        public IActionResult Details(int id)
        {
            if (!PuedeRevisarInformes())
            {
                return RedirigirNoAutorizado();
            }

            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var director = _context.Usuarios.Find(idUsuario);

            if (director == null)
            {
                return NotFound();
            }

            var informe = _context.InformeDocentes
                .Include(i => i.IdUsuarioNavigation)
                .Include(i => i.IdEstadoNavigation)
                .FirstOrDefault(i =>
                    i.IdInformeDocente == id &&
                    i.IdUsuarioNavigation.IdDepartamento == director.IdDepartamento);

            if (informe == null)
            {
                return NotFound();
            }

            return View(informe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Aprobar(int id)
        {
            if (!PuedeRevisarInformes())
            {
                return RedirigirNoAutorizado();
            }

            var informe = _context.InformeDocentes.Find(id);

            if (informe == null)
            {
                return NotFound();
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
                $"El director aprobó el informe docente #{informe.IdInformeDocente} del año {informe.Anio}."
            );

            TempData["Exito"] = "Informe aprobado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Devolver(int id, string observacion)
        {
            if (!PuedeRevisarInformes())
            {
                return RedirigirNoAutorizado();
            }

            var informe = _context.InformeDocentes.Find(id);

            if (informe == null)
            {
                return NotFound();
            }

            if (informe.IdEstado != 2)
            {
                TempData["Error"] = "Solo se pueden devolver informes pendientes.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(observacion))
            {
                TempData["Error"] = "Debe escribir una observación para devolver el informe.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            // Cambiar estado a Devuelto
            informe.IdEstado = 3;

            // Crear observación
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
                $"El director devolvió el informe docente #{informe.IdInformeDocente} con observación."
            );

            TempData["Exito"] = "Informe devuelto correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
    }