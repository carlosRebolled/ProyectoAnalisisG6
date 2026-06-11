using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Models;

namespace SistemaFarmaciaG6.Controllers
{
    public class InformeDocenteController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public InformeDocenteController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informes = _context.InformeDocentes
                .Include(i => i.IdEstadoNavigation)
                .Where(i => i.IdUsuario == idUsuario)
                .OrderByDescending(i => i.Anio)
                .ToList();

            return View(informes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var usuario = _context.Usuarios
                .Include(u => u.IdGeneroNavigation)
                .Include(u => u.IdDepartamentoNavigation)
                .Include(u => u.IdCategoriaNavigation)
                .Include(u => u.IdTipoNombramientoNavigation)
                .FirstOrDefault(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                return NotFound();
            }

            var hoy = DateTime.Today;

            var edad = hoy.Year - usuario.FechaNacimiento.Year;

            if (usuario.FechaNacimiento.ToDateTime(TimeOnly.MinValue) > hoy.AddYears(-edad))
            {
                edad--;
            }

            ViewBag.Edad = edad;
            ViewBag.Edad = edad;

            ViewBag.Usuario = usuario;

            return View();
        }

        [HttpPost]
        public IActionResult Create(InformeDocente informe)
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                int anioActual = DateTime.Now.Year;

                bool existeInforme = _context.InformeDocentes.Any(i =>
                    i.IdUsuario == idUsuario &&
                    i.Anio == anioActual);

                if (existeInforme)
                {
                    TempData["Error"] =
                        $"Ya existe un informe para el año {anioActual}.";

                    return RedirectToAction(nameof(Create));
                }

                bool formularioIncompleto = false;

                if (string.IsNullOrWhiteSpace(informe.DetalleCongresosActivos))
                    formularioIncompleto = true;

                if (string.IsNullOrWhiteSpace(informe.DetalleCongresosPasivos))
                    formularioIncompleto = true;

                if (string.IsNullOrWhiteSpace(informe.DetalleAccionSocial))
                    formularioIncompleto = true;

                if (string.IsNullOrWhiteSpace(informe.DetalleInvestigacion))
                    formularioIncompleto = true;

                if (string.IsNullOrWhiteSpace(informe.DetalleDocencia))
                    formularioIncompleto = true;

                if (string.IsNullOrWhiteSpace(informe.DetallePublicaciones))
                    formularioIncompleto = true;

                if (string.IsNullOrWhiteSpace(informe.DetalleCursosGrado))
                    formularioIncompleto = true;

                if (string.IsNullOrWhiteSpace(informe.DetallePosgrado))
                    formularioIncompleto = true;

                if (string.IsNullOrWhiteSpace(informe.DetalleRepresentacion))
                    formularioIncompleto = true;

                if (string.IsNullOrWhiteSpace(informe.DetalleOtros))
                    formularioIncompleto = true;

                if (formularioIncompleto)
                {
                    TempData["Error"] =
                        "Existen espacios obligatorios sin completar. Complete la información o marque 'No aplica'.";

                    return RedirectToAction(nameof(Create));
                }

                informe.IdUsuario = idUsuario.Value;

                informe.IdEstado = 1;

                informe.Anio = anioActual;

                informe.FechaCreacion = DateTime.Now;

                informe.FechaEnvio = null;

                informe.FechaAprobacion = null;

                _context.InformeDocentes.Add(informe);

                _context.SaveChanges();

                TempData["LimpiarLocalStorage"] = true;

                TempData["Exito"] =
                    "Informe guardado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.InnerException?.Message ?? ex.Message;

                return RedirectToAction(nameof(Create));
            }
        }

    }
}