using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Models;
using SistemaFarmaciaG6.Helpers;

namespace SistemaFarmaciaG6.Controllers
{
    public class InformeDireccionController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public InformeDireccionController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        private bool PuedeGestionar()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Director" || rol == "Administrador";
        }

        public IActionResult Index()
        {
            if (!PuedeGestionar())
                return RedirectToAction("Index", "Home");

            var informes = _context.InformeDireccions
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .OrderByDescending(i => i.Anio)
                .ToList();

            return View(informes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!PuedeGestionar())
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(InformeDireccion informe)
        {
            if (!PuedeGestionar())
                return RedirectToAction("Index", "Home");

            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
                return RedirectToAction("Login", "Account");

            int anioActual = DateTime.Now.Year;

            bool existe = _context.InformeDireccions.Any(i =>
                i.IdUsuario == idUsuario &&
                i.Anio == anioActual);

            if (existe)
            {
                TempData["Error"] = $"Ya existe un informe de dirección para el año {anioActual}.";
                return RedirectToAction(nameof(Index));
            }

            informe.IdUsuario = idUsuario.Value;
            informe.IdEstado = 1;
            informe.Anio = anioActual;
            informe.FechaCreacion = DateTime.Now;
            informe.FechaEnvio = null;
            informe.FechaAprobacion = null;

            _context.InformeDireccions.Add(informe);
            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeDireccion",
                "Crear",
                $"Se creó el informe de dirección #{informe.IdInformeDireccion} del año {informe.Anio}."
            );

            TempData["Exito"] = "Informe de dirección creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            if (!PuedeGestionar())
                return RedirectToAction("Index", "Home");

            var informe = _context.InformeDireccions
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .FirstOrDefault(i => i.IdInformeDireccion == id);

            if (informe == null)
                return NotFound();

            var observaciones = _context.Observaciones
                .Where(o => o.IdInformeDireccion == id)
                .OrderByDescending(o => o.Fecha)
                .ToList();

            ViewBag.Observaciones = observaciones;

            return View(informe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Enviar(int id)
        {
            if (!PuedeGestionar())
                return RedirectToAction("Index", "Home");

            var informe = _context.InformeDireccions.Find(id);

            if (informe == null)
                return NotFound();

            if (informe.IdEstado != 1 && informe.IdEstado != 3)
            {
                TempData["Error"] = "Solo se pueden enviar informes en estado Borrador o Devuelto.";
                return RedirectToAction(nameof(Index));
            }

            informe.IdEstado = 2;
            informe.FechaEnvio = DateTime.Now;

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeDireccion",
                "Enviar",
                $"Se envió el informe de dirección #{informe.IdInformeDireccion} del año {informe.Anio}."
            );

            TempData["Exito"] = "Informe de dirección enviado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!PuedeGestionar())
                return RedirectToAction("Index", "Home");

            var informe = _context.InformeDireccions.Find(id);

            if (informe == null)
                return NotFound();

            if (informe.IdEstado != 1 && informe.IdEstado != 3)
            {
                TempData["Error"] = "Solo se pueden editar informes en estado Borrador o Devuelto.";
                return RedirectToAction(nameof(Index));
            }

            return View(informe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, InformeDireccion informe)
        {
            if (!PuedeGestionar())
                return RedirectToAction("Index", "Home");

            var informeBD = _context.InformeDireccions.Find(id);

            if (informeBD == null)
                return NotFound();

            if (informeBD.IdEstado != 1 && informeBD.IdEstado != 3)
            {
                TempData["Error"] = "Solo se pueden editar informes en estado Borrador o Devuelto.";
                return RedirectToAction(nameof(Index));
            }

            informeBD.ObservacionesDirector = informe.ObservacionesDirector;

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeDireccion",
                "Editar",
                $"Se editó el informe de dirección #{informeBD.IdInformeDireccion} del año {informeBD.Anio}."
            );

            TempData["Exito"] = "Informe actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!PuedeGestionar())
                return RedirectToAction("Index", "Home");

            var informe = _context.InformeDireccions
                .Include(i => i.IdEstadoNavigation)
                .FirstOrDefault(i => i.IdInformeDireccion == id);

            if (informe == null)
                return NotFound();

            if (informe.IdEstado != 1)
            {
                TempData["Error"] = "Solo se pueden eliminar informes en estado Borrador.";
                return RedirectToAction(nameof(Index));
            }

            return View(informe);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!PuedeGestionar())
                return RedirectToAction("Index", "Home");

            var informe = _context.InformeDireccions.Find(id);

            if (informe == null)
                return NotFound();

            if (informe.IdEstado != 1)
            {
                TempData["Error"] = "Solo se pueden eliminar informes en estado Borrador.";
                return RedirectToAction(nameof(Index));
            }

            int idInforme = informe.IdInformeDireccion;
            int anio = informe.Anio;

            _context.InformeDireccions.Remove(informe);
            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeDireccion",
                "Eliminar",
                $"Se eliminó el informe de dirección #{idInforme} del año {anio}."
            );

            TempData["Exito"] = "Informe eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}