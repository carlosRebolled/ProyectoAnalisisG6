using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Models;
using SistemaFarmaciaG6.Helpers;

namespace SistemaFarmaciaG6.Controllers
{
    public class RolesController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public RolesController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        private bool EsAdministrador()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Administrador";
        }

        private IActionResult RedirigirNoAutorizado()
        {
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index()
        {
            if (!EsAdministrador())
            {
                return RedirigirNoAutorizado();
            }

            var usuarios = _context.Usuarios
                .Include(u => u.UsuarioRols)
                .ThenInclude(ur => ur.IdRolNavigation)
                .ToList();

            return View(usuarios);
        }

        public IActionResult Edit(int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirNoAutorizado();
            }

            var usuario = _context.Usuarios.Find(id);

            if (usuario == null)
            {
                return NotFound();
            }

            var usuarioRol = _context.UsuarioRols
                .FirstOrDefault(ur => ur.IdUsuario == id);

            ViewBag.Roles = new SelectList(
                _context.Roles.ToList(),
                "IdRol",
                "NombreRol",
                usuarioRol?.IdRol
            );

            ViewBag.IdRolActual = usuarioRol?.IdRol;

            return View(usuario);
        }

        [HttpPost]
        public IActionResult Edit(int id, int idRol)
        {
            if (!EsAdministrador())
            {
                return RedirigirNoAutorizado();
            }

            var usuario = _context.Usuarios.Find(id);

            if (usuario == null)
            {
                return NotFound();
            }

            if (idRol == 0)
            {
                TempData["Error"] = "Debe seleccionar un rol.";
                return RedirectToAction(nameof(Edit), new { id = id });
            }

            var usuarioRol = _context.UsuarioRols
                .Include(ur => ur.IdRolNavigation)
                .FirstOrDefault(ur => ur.IdUsuario == id);

            string rolAnterior = usuarioRol?.IdRolNavigation?.NombreRol ?? "Sin rol";

            var rolNuevo = _context.Roles
                .FirstOrDefault(r => r.IdRol == idRol);

            string nombreRolNuevo = rolNuevo?.NombreRol ?? "Rol desconocido";

            if (usuarioRol == null)
            {
                usuarioRol = new UsuarioRol
                {
                    IdUsuario = id,
                    IdRol = idRol
                };

                _context.UsuarioRols.Add(usuarioRol);
            }
            else
            {
                usuarioRol.IdRol = idRol;
            }

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "UsuarioRol",
                "Cambiar Rol",
                $"Se cambió el rol del usuario {usuario.Nombre} {usuario.Apellido1}. Rol anterior: {rolAnterior}. Rol nuevo: {nombreRolNuevo}."
            );

            TempData["Exito"] = "Rol actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}