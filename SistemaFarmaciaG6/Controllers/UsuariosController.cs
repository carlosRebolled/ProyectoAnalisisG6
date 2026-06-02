using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Models;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace SistemaFarmaciaG6.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public UsuariosController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }


        private void CargarCombos()
        {

            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "IdRol", "NombreRol");
            ViewBag.Generos = new SelectList(_context.Generos.ToList(), "IdGenero", "NombreGenero");
            ViewBag.Departamentos = new SelectList(_context.Departamentos.ToList(), "IdDepartamento", "NombreDepartamento");
            ViewBag.Categorias = new SelectList(_context.Categorias.ToList(), "IdCategoria", "NombreCategoria");
            ViewBag.TiposNombramiento = new SelectList(_context.TiposNombramientos.ToList(), "IdTipoNombramiento", "NombreNombramiento");
        }

        public IActionResult Index(string estado)
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (rol != "Administrador")
            {
                return RedirectToAction("Login", "Account");
            }

            var usuarios = _context.Usuarios
                .Include(u => u.IdDepartamentoNavigation)
                .Include(u => u.IdGeneroNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
            {
                usuarios = usuarios.Where(u => u.Estado == estado);
            }

            ViewBag.EstadoSeleccionado = estado;

            return View(usuarios.ToList());
        }


        public IActionResult Create()
        {
            CargarCombos();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Usuario usuario, int idRol)
        {
            try
            {
                usuario.Estado = "Activo";
                usuario.FechaRegistro = DateTime.Now;
                usuario.CambiarContrasena = true;
                usuario.UltimoInicioSesion = null;

                ModelState.Clear();

                if (string.IsNullOrWhiteSpace(usuario.Cedula))
                    ModelState.AddModelError("Cedula", "La cédula es obligatoria.");

                if (string.IsNullOrWhiteSpace(usuario.Nombre))
                    ModelState.AddModelError("Nombre", "El nombre es obligatorio.");

                if (string.IsNullOrWhiteSpace(usuario.Apellido1))
                    ModelState.AddModelError("Apellido1", "El primer apellido es obligatorio.");

                if (string.IsNullOrWhiteSpace(usuario.Correo))
                    ModelState.AddModelError("Correo", "El correo es obligatorio.");

                if (string.IsNullOrWhiteSpace(usuario.Contrasena))
                    ModelState.AddModelError("Contrasena", "La contraseña es obligatoria.");

                if (usuario.FechaNacimiento == default)
                    ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento es obligatoria.");

                if (usuario.FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
                    ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento no puede ser futura.");

                if (usuario.IdGenero == 0)
                    ModelState.AddModelError("IdGenero", "Debe seleccionar un género.");

                if (usuario.IdDepartamento == 0)
                    ModelState.AddModelError("IdDepartamento", "Debe seleccionar un departamento.");

                if (usuario.IdCategoria == 0)
                    ModelState.AddModelError("IdCategoria", "Debe seleccionar una categoría.");

                if (usuario.IdTipoNombramiento == 0)
                    ModelState.AddModelError("IdTipoNombramiento", "Debe seleccionar un tipo de nombramiento.");

                if (idRol == 0)
                    ModelState.AddModelError("idRol", "Debe seleccionar un rol.");

                if (!string.IsNullOrWhiteSpace(usuario.Correo) &&
                    !usuario.Correo.EndsWith("@ucr.ac.cr"))
                    ModelState.AddModelError("Correo", "El correo debe terminar en @ucr.ac.cr.");

                if (!string.IsNullOrWhiteSpace(usuario.Cedula) &&
                    _context.Usuarios.Any(u => u.Cedula == usuario.Cedula))
                    ModelState.AddModelError("Cedula", "Ya existe un usuario con esa cédula.");

                if (!string.IsNullOrWhiteSpace(usuario.Correo) &&
                    _context.Usuarios.Any(u => u.Correo == usuario.Correo))
                    ModelState.AddModelError("Correo", "Ya existe un usuario con ese correo.");

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Revise los datos ingresados.";
                    CargarCombos();
                    return View(usuario);
                }

                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                var usuarioRol = new UsuarioRol
                {
                    IdUsuario = usuario.IdUsuario,
                    IdRol = idRol
                };

                _context.UsuarioRols.Add(usuarioRol);
                _context.SaveChanges();

                TempData["Exito"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var mensaje = ex.InnerException != null
                    ? ex.InnerException.Message
                    : ex.Message;

                TempData["Error"] = "Error al crear usuario: " + mensaje;
                CargarCombos();
                return View(usuario);
            }
        }


        public IActionResult Edit(int id)
        {
            var usuario = _context.Usuarios.Find(id);

            if (usuario == null)
            {
                return NotFound();
            }

            CargarCombos();

            var rolUsuario = _context.UsuarioRols
                .FirstOrDefault(x => x.IdUsuario == id);

            ViewBag.IdRolActual = rolUsuario?.IdRol;

            return View(usuario);
        }

[HttpPost]
public IActionResult Edit(int id, Usuario usuario, int idRol)
{
    try
    {
        var usuarioBD = _context.Usuarios.Find(id);

        if (usuarioBD == null)
        {
            return NotFound();
        }

        ModelState.Clear();

        if (string.IsNullOrWhiteSpace(usuario.Apellido1))
            ModelState.AddModelError("Apellido1", "El primer apellido es obligatorio.");

        if (usuario.FechaNacimiento == default)
            ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento es obligatoria.");

        if (usuario.FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
            ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento no puede ser futura.");

        if (usuario.IdGenero == 0)
            ModelState.AddModelError("IdGenero", "Debe seleccionar un género.");

        if (usuario.IdDepartamento == 0)
            ModelState.AddModelError("IdDepartamento", "Debe seleccionar un departamento.");

        if (usuario.IdCategoria == 0)
            ModelState.AddModelError("IdCategoria", "Debe seleccionar una categoría.");

        if (usuario.IdTipoNombramiento == 0)
            ModelState.AddModelError("IdTipoNombramiento", "Debe seleccionar un tipo de nombramiento.");

        if (string.IsNullOrWhiteSpace(usuario.Estado))
            ModelState.AddModelError("Estado", "Debe seleccionar un estado.");

        if (idRol == 0)
            ModelState.AddModelError("idRol", "Debe seleccionar un rol.");

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Revise los datos ingresados.";
            CargarCombos();
            ViewBag.IdRolActual = idRol;
            return View(usuario);
        }

        // RF-14: estos NO se modifican:
        // Cédula, Nombre y Correo

        usuarioBD.Apellido1 = usuario.Apellido1;
        usuarioBD.Apellido2 = usuario.Apellido2;
        usuarioBD.FechaNacimiento = usuario.FechaNacimiento;
        usuarioBD.Telefono = usuario.Telefono;
        usuarioBD.IdGenero = usuario.IdGenero;
        usuarioBD.IdDepartamento = usuario.IdDepartamento;
        usuarioBD.IdCategoria = usuario.IdCategoria;
        usuarioBD.IdTipoNombramiento = usuario.IdTipoNombramiento;
        usuarioBD.Estado = usuario.Estado;

        _context.SaveChanges();

        var usuarioRol = _context.UsuarioRols
            .FirstOrDefault(x => x.IdUsuario == id);

        if (usuarioRol != null)
        {
            usuarioRol.IdRol = idRol;
        }
        else
        {
            usuarioRol = new UsuarioRol
            {
                IdUsuario = id,
                IdRol = idRol
            };

            _context.UsuarioRols.Add(usuarioRol);
        }

        _context.SaveChanges();

        TempData["Exito"] = "Usuario actualizado correctamente.";

        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        TempData["Error"] = ex.InnerException?.Message ?? ex.Message;

        CargarCombos();
        ViewBag.IdRolActual = idRol;
        return View(usuario);
    }
}

    }
}