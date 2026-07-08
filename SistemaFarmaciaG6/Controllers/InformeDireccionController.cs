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

        private bool EsAdministrador()
        {
            return HttpContext.Session.GetString("Rol") == "Administrador";
        }

        private int? IdUsuarioSesion()
        {
            return HttpContext.Session.GetInt32("IdUsuario");
        }

        public IActionResult Index()
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

            var informes = _context.InformeDireccions
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .AsQueryable();

            if (!EsAdministrador())
            {
                informes = informes.Where(i => i.IdUsuario == idUsuario);
            }

            return View(informes
                .OrderByDescending(i => i.Anio)
                .ToList());
        }

        [HttpGet]
        public IActionResult Create()
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

            var usuario = _context.Usuarios
                .Include(u => u.IdDepartamentoNavigation)
                .FirstOrDefault(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                return NotFound();
            }

            ViewBag.NombreDirector = $"{usuario.Nombre} {usuario.Apellido1} {usuario.Apellido2}";
            ViewBag.Correo = usuario.Correo;
            ViewBag.Departamento = usuario.IdDepartamentoNavigation.NombreDepartamento;
            ViewBag.Anio = DateTime.Now.Year;

            ViewBag.Cursos = _context.Cursos
                .OrderBy(c => c.SiglaCurso)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(

    InformeDireccion informe,

    string NumeroSesion,
    DateOnly FechaSesion,
    string PuntosVistos,

    int Curso,

    int CoordinacionCantidad,
    int ColaboradoresCantidad,
    int InvitadosCantidad,
    int ExperienciasPracticasCantidad,

    string ActividadesDocenciaIntegradasDetalle,
    string ActividadesAnalisisContextoDetalle,
    string TecnicasDidacticasDetalle,

    int AsistentesCurso
)
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

            SesionesDepartamento sesion = new SesionesDepartamento
            {
                IdInformeDireccion = informe.IdInformeDireccion,

                NumeroSesion = NumeroSesion,

                FechaSesion = FechaSesion,

                PuntosVistos = PuntosVistos
            };

            _context.SesionesDepartamentos.Add(sesion);

            Curso? curso = _context.Cursos.FirstOrDefault(c => c.IdCurso == Curso);

            if (curso != null)
            {
                CursosDireccion cursoDireccion = new CursosDireccion
                {
                    IdInformeDireccion = informe.IdInformeDireccion,

                    IdCurso = curso.IdCurso,

                    CoordinacionCantidad = CoordinacionCantidad,

                    ColaboradoresCantidad = ColaboradoresCantidad,

                    InvitadosCantidad = InvitadosCantidad,

                    ExperienciasPracticasCantidad = ExperienciasPracticasCantidad,

                    ActividadesDocenciaIntegradasDetalle =
                        ActividadesDocenciaIntegradasDetalle,

                    ActividadesAnalisisContextoDetalle =
                        ActividadesAnalisisContextoDetalle,

                    TecnicasDidacticasDetalle =
                        TecnicasDidacticasDetalle,

                    AsistentesCurso = AsistentesCurso
                };

                _context.CursosDireccions.Add(cursoDireccion);
            }

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
            {
                return RedirectToAction("Index", "Home");
            }

            int? idUsuario = IdUsuarioSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDireccions
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .FirstOrDefault(i => i.IdInformeDireccion == id);

            if (informe == null)
            {
                return NotFound();
            }

            if (!EsAdministrador() && informe.IdUsuario != idUsuario)
            {
                TempData["Error"] = "No puede ver informes de otro director.";
                return RedirectToAction(nameof(Index));
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
        public IActionResult Enviar(int id)
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

            var informe = _context.InformeDireccions.Find(id);

            if (informe == null)
            {
                return NotFound();
            }

            if (!EsAdministrador() && informe.IdUsuario != idUsuario)
            {
                TempData["Error"] = "No puede enviar informes de otro director.";
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
            {
                return RedirectToAction("Index", "Home");
            }

            int? idUsuario = IdUsuarioSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDireccions.Find(id);

            if (informe == null)
            {
                return NotFound();
            }

            if (!EsAdministrador() && informe.IdUsuario != idUsuario)
            {
                TempData["Error"] = "No puede editar informes de otro director.";
                return RedirectToAction(nameof(Index));
            }

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
            {
                return RedirectToAction("Index", "Home");
            }

            int? idUsuario = IdUsuarioSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informeBD = _context.InformeDireccions.Find(id);

            if (informeBD == null)
            {
                return NotFound();
            }

            if (!EsAdministrador() && informeBD.IdUsuario != idUsuario)
            {
                TempData["Error"] = "No puede editar informes de otro director.";
                return RedirectToAction(nameof(Index));
            }

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
            {
                return RedirectToAction("Index", "Home");
            }

            int? idUsuario = IdUsuarioSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDireccions
                .Include(i => i.IdEstadoNavigation)
                .FirstOrDefault(i => i.IdInformeDireccion == id);

            if (informe == null)
            {
                return NotFound();
            }

            if (!EsAdministrador() && informe.IdUsuario != idUsuario)
            {
                TempData["Error"] = "No puede eliminar informes de otro director.";
                return RedirectToAction(nameof(Index));
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
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            int? idUsuario = IdUsuarioSesion();

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var informe = _context.InformeDireccions.Find(id);

            if (informe == null)
            {
                return NotFound();
            }

            if (!EsAdministrador() && informe.IdUsuario != idUsuario)
            {
                TempData["Error"] = "No puede eliminar informes de otro director.";
                return RedirectToAction(nameof(Index));
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reabrir(int id)
        {
            if (!EsAdministrador())
            {
                return RedirectToAction("Index", "Home");
            }

            var informe = _context.InformeDireccions.Find(id);

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
                "InformeDireccion",
                "Reabrir",
                $"El administrador reabrió el informe de dirección #{informe.IdInformeDireccion} del año {informe.Anio}."
            );

            TempData["Exito"] = "Informe de dirección reabierto correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}