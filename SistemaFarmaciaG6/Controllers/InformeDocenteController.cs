using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Models;
using SistemaFarmaciaG6.Helpers;

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
                    TempData["Error"] = $"Ya existe un informe para el año {anioActual}.";
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

                AuditoriaHelper.Registrar(
                    _context,
                    HttpContext,
                    "InformeDocente",
                    "Crear",
                    $"Se creó el informe docente #{informe.IdInformeDocente} del año {informe.Anio}."
                );

                TempData["LimpiarLocalStorage"] = true;
                TempData["Exito"] = "Informe guardado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.InnerException?.Message ?? ex.Message;
                return RedirectToAction(nameof(Create));
            }
        }

        public IActionResult Details(int id)
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDocentes
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .FirstOrDefault(i => i.IdInformeDocente == id &&
                                     i.IdUsuario == idUsuario);

            if (informe == null)
            {
                return NotFound();
            }

            return View(informe);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDocentes
                .Include(i => i.IdUsuarioNavigation)
                    .ThenInclude(u => u.IdGeneroNavigation)
                .Include(i => i.IdUsuarioNavigation)
                    .ThenInclude(u => u.IdDepartamentoNavigation)
                .Include(i => i.IdUsuarioNavigation)
                    .ThenInclude(u => u.IdCategoriaNavigation)
                .Include(i => i.IdUsuarioNavigation)
                    .ThenInclude(u => u.IdTipoNombramientoNavigation)
                .FirstOrDefault(i => i.IdInformeDocente == id &&
                                     i.IdUsuario == idUsuario);

            if (informe == null)
            {
                return NotFound();
            }

            if (informe.IdEstado != 1 && informe.IdEstado != 3)
            {
                TempData["Error"] = "Solo se pueden editar informes en estado Borrador o Devuelto.";
                return RedirectToAction(nameof(Index));
            }

            var usuario = informe.IdUsuarioNavigation;

            var hoy = DateTime.Today;
            var edad = hoy.Year - usuario.FechaNacimiento.Year;

            if (usuario.FechaNacimiento.ToDateTime(TimeOnly.MinValue) > hoy.AddYears(-edad))
            {
                edad--;
            }

            ViewBag.Usuario = usuario;
            ViewBag.Edad = edad;

            return View(informe);
        }

        [HttpPost]
        public IActionResult Edit(int id, InformeDocente informe)
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informeBD = _context.InformeDocentes
                .FirstOrDefault(i => i.IdInformeDocente == id &&
                                     i.IdUsuario == idUsuario);

            if (informeBD == null)
            {
                return NotFound();
            }

            if (informeBD.IdEstado != 1 && informeBD.IdEstado != 3)
            {
                TempData["Error"] = "No se puede editar este informe porque ya fue enviado o aprobado.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                informeBD.CantidadCongresosActivos = informe.CantidadCongresosActivos;
                informeBD.DetalleCongresosActivos = informe.DetalleCongresosActivos;

                informeBD.CantidadCongresosPasivos = informe.CantidadCongresosPasivos;
                informeBD.DetalleCongresosPasivos = informe.DetalleCongresosPasivos;

                informeBD.CantidadAccionSocial = informe.CantidadAccionSocial;
                informeBD.DetalleAccionSocial = informe.DetalleAccionSocial;

                informeBD.CantidadInvestigacion = informe.CantidadInvestigacion;
                informeBD.DetalleInvestigacion = informe.DetalleInvestigacion;

                informeBD.CantidadDocencia = informe.CantidadDocencia;
                informeBD.DetalleDocencia = informe.DetalleDocencia;

                informeBD.CantidadPublicaciones = informe.CantidadPublicaciones;
                informeBD.DetallePublicaciones = informe.DetallePublicaciones;

                informeBD.CantidadCursosGrado = informe.CantidadCursosGrado;
                informeBD.DetalleCursosGrado = informe.DetalleCursosGrado;

                informeBD.CantidadPosgrado = informe.CantidadPosgrado;
                informeBD.DetallePosgrado = informe.DetallePosgrado;

                informeBD.CantidadRepresentacion = informe.CantidadRepresentacion;
                informeBD.DetalleRepresentacion = informe.DetalleRepresentacion;

                informeBD.DetalleOtros = informe.DetalleOtros;
                informeBD.ObservacionesDocente = informe.ObservacionesDocente;

                _context.SaveChanges();

                AuditoriaHelper.Registrar(
                    _context,
                    HttpContext,
                    "InformeDocente",
                    "Editar",
                    $"Se editó el informe docente #{informeBD.IdInformeDocente} del año {informeBD.Anio}."
                );

                TempData["Exito"] = "Informe actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.InnerException?.Message ?? ex.Message;
                return RedirectToAction(nameof(Edit), new { id = id });
            }
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDocentes
                .Include(i => i.IdEstadoNavigation)
                .FirstOrDefault(i =>
                    i.IdInformeDocente == id &&
                    i.IdUsuario == idUsuario);

            if (informe == null)
            {
                return NotFound();
            }

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
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDocentes
                .FirstOrDefault(i =>
                    i.IdInformeDocente == id &&
                    i.IdUsuario == idUsuario);

            if (informe == null)
            {
                return NotFound();
            }

            if (informe.IdEstado != 1)
            {
                TempData["Error"] = "Solo se pueden eliminar informes en estado Borrador.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                int idInformeEliminado = informe.IdInformeDocente;
                int anioInformeEliminado = informe.Anio;

                _context.InformeDocentes.Remove(informe);
                _context.SaveChanges();

                AuditoriaHelper.Registrar(
                    _context,
                    HttpContext,
                    "InformeDocente",
                    "Eliminar",
                    $"Se eliminó el informe docente #{idInformeEliminado} del año {anioInformeEliminado}."
                );

                TempData["Exito"] = "Informe eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.InnerException?.Message ?? ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Enviar(int id)
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDocentes
                .Include(i => i.IdEstadoNavigation)
                .FirstOrDefault(i =>
                    i.IdInformeDocente == id &&
                    i.IdUsuario == idUsuario);

            if (informe == null)
            {
                return NotFound();
            }

            if (informe.IdEstado != 1)
            {
                TempData["Error"] = "Solo se pueden enviar informes en estado Borrador.";
                return RedirectToAction(nameof(Index));
            }

            return View(informe);
        }

        [HttpPost, ActionName("Enviar")]
        [ValidateAntiForgeryToken]
        public IActionResult EnviarConfirmado(int id)
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDocentes
                .FirstOrDefault(i =>
                    i.IdInformeDocente == id &&
                    i.IdUsuario == idUsuario);

            if (informe == null)
            {
                return NotFound();
            }

            if (informe.IdEstado != 1)
            {
                TempData["Error"] = "Solo se pueden enviar informes en estado Borrador.";
                return RedirectToAction(nameof(Index));
            }

            informe.IdEstado = 2;
            informe.FechaEnvio = DateTime.Now;

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeDocente",
                "Enviar",
                $"Se envió el informe docente #{informe.IdInformeDocente} del año {informe.Anio} para revisión."
            );

            TempData["Exito"] = "Informe enviado correctamente para revisión.";

            return RedirectToAction(nameof(Index));
        }
    }
}