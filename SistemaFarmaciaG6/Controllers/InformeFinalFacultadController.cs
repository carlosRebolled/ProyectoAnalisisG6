using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Helpers;
using SistemaFarmaciaG6.Models;

namespace SistemaFarmaciaG6.Controllers
{
    public class InformeFinalFacultadController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public InformeFinalFacultadController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        private string RolSesion()
        {
            return HttpContext.Session.GetString("Rol") ?? "";
        }

        private int? IdUsuarioSesion()
        {
            return HttpContext.Session.GetInt32("IdUsuario");
        }

        private bool PuedeGestionar()
        {
            var rol = RolSesion();
            return rol == "Decano" || rol == "Administrador";
        }

        private bool EsAdministrador()
        {
            return RolSesion() == "Administrador";
        }

        public IActionResult Index()
        {
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informes = _context.InformeFinalFacultads
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioGeneraNavigation)
                .OrderByDescending(i => i.Anio)
                .ToList();

            return View(informes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(InformeFinalFacultad informe)
        {
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            int? idUsuario = IdUsuarioSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int anioActual = DateTime.Now.Year;

            bool existe = _context.InformeFinalFacultads.Any(i =>
                i.Anio == anioActual);

            if (existe)
            {
                TempData["Error"] = $"Ya existe un informe final para el año {anioActual}.";
                return RedirectToAction(nameof(Index));
            }

            informe.IdUsuarioGenera = idUsuario.Value;
            informe.IdEstado = 1;
            informe.Anio = anioActual;
            informe.FechaGeneracion = DateTime.Now;
            informe.FechaAprobacion = null;

            _context.InformeFinalFacultads.Add(informe);
            _context.SaveChanges();

            var informesDireccion = _context.InformeDireccions
                .Include(i => i.IdUsuarioNavigation)
                .Where(i => i.Anio == anioActual && i.IdEstado == 4)
                .ToList();

            foreach (var item in informesDireccion)
            {
                var detalle = new DetalleInformeFinal
                {
                    IdInformeFinal = informe.IdInformeFinal,
                    TipoActividad = "Informe Dirección",
                    Cantidad = 1,
                    DetalleActividad =
                        $"Informe Dirección #{item.IdInformeDireccion} generado por {item.IdUsuarioNavigation.Nombre} {item.IdUsuarioNavigation.Apellido1}"
                };

                _context.DetalleInformeFinals.Add(detalle);
            }

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeFinalFacultad",
                "Crear",
                $"Se creó el informe final de Facultad #{informe.IdInformeFinal} del año {informe.Anio}."
            );

            TempData["Exito"] = "Informe final creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informe = _context.InformeFinalFacultads
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioGeneraNavigation)
                .Include(i => i.DetalleInformeFinals)
                .FirstOrDefault(i => i.IdInformeFinal == id);

            if (informe == null)
            {
                return NotFound();
            }

            return View(informe);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informe = _context.InformeFinalFacultads.Find(id);

            if (informe == null)
            {
                return NotFound();
            }

            if (informe.IdEstado != 1)
            {
                TempData["Error"] = "Solo se pueden editar informes en estado Borrador.";
                return RedirectToAction(nameof(Index));
            }

            return View(informe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, InformeFinalFacultad informe)
        {
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informeBD = _context.InformeFinalFacultads.Find(id);

            if (informeBD == null)
            {
                return NotFound();
            }

            if (informeBD.IdEstado != 1)
            {
                TempData["Error"] = "Solo se pueden editar informes en estado Borrador.";
                return RedirectToAction(nameof(Index));
            }

            informeBD.Observaciones = informe.Observaciones;

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeFinalFacultad",
                "Editar",
                $"Se editó el informe final de Facultad #{informeBD.IdInformeFinal} del año {informeBD.Anio}."
            );

            TempData["Exito"] = "Informe final actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Finalizar(int id)
        {
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informe = _context.InformeFinalFacultads.Find(id);

            if (informe == null)
            {
                return NotFound();
            }

            if (informe.IdEstado != 1)
            {
                TempData["Error"] = "Solo se pueden finalizar informes en estado Borrador.";
                return RedirectToAction(nameof(Index));
            }

            informe.IdEstado = 5;
            informe.FechaAprobacion = DateTime.Now;

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeFinalFacultad",
                "Finalizar",
                $"Se finalizó el informe final de Facultad #{informe.IdInformeFinal} del año {informe.Anio}."
            );

            TempData["Exito"] = "Informe final finalizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reabrir(int id)
        {
            if (!EsAdministrador())
            {
                return RedirectToAction("Index", "Home");
            }

            var informe = _context.InformeFinalFacultads.Find(id);

            if (informe == null)
            {
                return NotFound();
            }

            if (informe.IdEstado != 5)
            {
                TempData["Error"] = "Solo se pueden reabrir informes finalizados.";
                return RedirectToAction(nameof(Index));
            }

            informe.IdEstado = 1;
            informe.FechaAprobacion = null;

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeFinalFacultad",
                "Reabrir",
                $"El administrador reabrió el informe final de Facultad #{informe.IdInformeFinal} del año {informe.Anio}."
            );

            TempData["Exito"] = "Informe final reabierto correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}