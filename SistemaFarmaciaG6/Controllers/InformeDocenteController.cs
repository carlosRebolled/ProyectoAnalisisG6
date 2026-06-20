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

        private int? IdUsuarioSesion()
        {
            return HttpContext.Session.GetInt32("IdUsuario");
        }

        private string RolSesion()
        {
            return HttpContext.Session.GetString("Rol") ?? "";
        }

        private bool EsAdministrador()
        {
            return RolSesion() == "Administrador";
        }

        private bool PuedeVerInforme(InformeDocente informe)
        {
            int? idUsuario = IdUsuarioSesion();
            string rol = RolSesion();

            if (idUsuario == null)
                return false;

            if (rol == "Administrador" || rol == "Decano" || rol == "Jefatura" || rol == "Visualizador")
                return true;

            if (rol == "Docente")
                return informe.IdUsuario == idUsuario;

            if (rol == "Director")
            {
                var director = _context.Usuarios.Find(idUsuario);

                if (director == null)
                    return false;

                return informe.IdUsuarioNavigation.IdDepartamento == director.IdDepartamento;
            }

            return false;
        }

        public IActionResult Index()
        {
            int? idUsuario = IdUsuarioSesion();
            string rol = RolSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informes = _context.InformeDocentes
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioNavigation)
                    .ThenInclude(u => u.IdDepartamentoNavigation)
                .AsQueryable();

            if (rol == "Docente")
            {
                informes = informes.Where(i => i.IdUsuario == idUsuario);
            }
            else if (rol == "Director")
            {
                var director = _context.Usuarios.Find(idUsuario);

                if (director == null)
                {
                    return NotFound();
                }

                informes = informes.Where(i =>
                    i.IdUsuarioNavigation.IdDepartamento == director.IdDepartamento);
            }
            else if (rol != "Administrador" &&
                     rol != "Decano" &&
                     rol != "Jefatura" &&
                     rol != "Visualizador")
            {
                return RedirectToAction("Index", "Home");
            }

            return View(informes
                .OrderByDescending(i => i.Anio)
                .ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            int? idUsuario = IdUsuarioSesion();
            string rol = RolSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (rol != "Docente" && rol != "Administrador")
            {
                return RedirectToAction("Index", "Home");
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
        [ValidateAntiForgeryToken]
        public IActionResult Create(InformeDocente informe)
        {
            int? idUsuario = IdUsuarioSesion();
            string rol = RolSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (rol != "Docente" && rol != "Administrador")
            {
                return RedirectToAction("Index", "Home");
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
                    return RedirectToAction(nameof(Index));
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
            int? idUsuario = IdUsuarioSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDocentes
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioNavigation)
                    .ThenInclude(u => u.IdDepartamentoNavigation)
                .FirstOrDefault(i => i.IdInformeDocente == id);

            if (informe == null)
            {
                return NotFound();
            }

            if (!PuedeVerInforme(informe))
            {
                return RedirectToAction("Index", "Home");
            }

            var observaciones = _context.Observaciones
                .Where(o => o.IdInformeDocente == id)
                .OrderByDescending(o => o.Fecha)
                .ToList();

            ViewBag.Observaciones = observaciones;

            return View(informe);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            int? idUsuario = IdUsuarioSesion();
            string rol = RolSesion();

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
                .FirstOrDefault(i => i.IdInformeDocente == id);

            if (informe == null)
            {
                return NotFound();
            }

            if (rol != "Administrador" && informe.IdUsuario != idUsuario)
            {
                TempData["Error"] = "No puede editar informes de otro usuario.";
                return RedirectToAction(nameof(Index));
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
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, InformeDocente informe)
        {
            int? idUsuario = IdUsuarioSesion();
            string rol = RolSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informeBD = _context.InformeDocentes
                .FirstOrDefault(i => i.IdInformeDocente == id);

            if (informeBD == null)
            {
                return NotFound();
            }

            if (rol != "Administrador" && informeBD.IdUsuario != idUsuario)
            {
                TempData["Error"] = "No puede editar informes de otro usuario.";
                return RedirectToAction(nameof(Index));
            }

            if (informeBD.IdEstado != 1 && informeBD.IdEstado != 3)
            {
                TempData["Error"] = "Solo se pueden editar informes en estado Borrador o Devuelto.";
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
            int? idUsuario = IdUsuarioSesion();
            string rol = RolSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDocentes
                .Include(i => i.IdEstadoNavigation)
                .FirstOrDefault(i => i.IdInformeDocente == id);

            if (informe == null)
            {
                return NotFound();
            }

            if (rol != "Administrador" && informe.IdUsuario != idUsuario)
            {
                TempData["Error"] = "No puede eliminar informes de otro usuario.";
                return RedirectToAction(nameof(Index));
            }

            if (informe.IdEstado != 1 && informe.IdEstado != 3)
            {
                TempData["Error"] = "Solo se pueden eliminar informes en estado Borrador o Devuelto.";
                return RedirectToAction(nameof(Index));
            }

            return View(informe);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            int? idUsuario = IdUsuarioSesion();
            string rol = RolSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDocentes
                .FirstOrDefault(i => i.IdInformeDocente == id);

            if (informe == null)
            {
                return NotFound();
            }

            if (rol != "Administrador" && informe.IdUsuario != idUsuario)
            {
                TempData["Error"] = "No puede eliminar informes de otro usuario.";
                return RedirectToAction(nameof(Index));
            }

            if (informe.IdEstado != 1 && informe.IdEstado != 3)
            {
                TempData["Error"] = "Solo se pueden eliminar informes en estado Borrador o Devuelto.";
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
            int? idUsuario = IdUsuarioSesion();
            string rol = RolSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDocentes
                .Include(i => i.IdEstadoNavigation)
                .FirstOrDefault(i => i.IdInformeDocente == id);

            if (informe == null)
            {
                return NotFound();
            }

            if (rol != "Administrador" && informe.IdUsuario != idUsuario)
            {
                TempData["Error"] = "No puede enviar informes de otro usuario.";
                return RedirectToAction(nameof(Index));
            }

            if (informe.IdEstado != 1 && informe.IdEstado != 3)
            {
                TempData["Error"] = "Solo se pueden enviar informes en estado Borrador o Devuelto.";
                return RedirectToAction(nameof(Index));
            }

            return View(informe);
        }

        [HttpPost, ActionName("Enviar")]
        [ValidateAntiForgeryToken]
        public IActionResult EnviarConfirmado(int id)
        {
            int? idUsuario = IdUsuarioSesion();
            string rol = RolSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDocentes
                .FirstOrDefault(i => i.IdInformeDocente == id);

            if (informe == null)
            {
                return NotFound();
            }

            if (rol != "Administrador" && informe.IdUsuario != idUsuario)
            {
                TempData["Error"] = "No puede enviar informes de otro usuario.";
                return RedirectToAction(nameof(Index));
            }

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
                "InformeDocente",
                "Enviar",
                $"Se envió el informe docente #{informe.IdInformeDocente} del año {informe.Anio} para revisión."
            );

            TempData["Exito"] = "Informe enviado correctamente para revisión.";

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

            var informe = _context.InformeDocentes.Find(id);

            if (informe == null)
            {
                return NotFound();
            }

            if (informe.IdEstado != 2 &&
                informe.IdEstado != 4 &&
                informe.IdEstado != 5)
            {
                TempData["Error"] = "Solo se pueden reabrir informes enviados, aprobados o finalizados.";
                return RedirectToAction(nameof(Index));
            }

            informe.IdEstado = 1;
            informe.FechaEnvio = null;
            informe.FechaAprobacion = null;

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeDocente",
                "Reabrir",
                $"El administrador reabrió el informe docente #{informe.IdInformeDocente} del año {informe.Anio}."
            );

            TempData["Exito"] = "Informe docente reabierto correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}