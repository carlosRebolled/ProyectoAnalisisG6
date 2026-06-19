using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Models;
using SistemaFarmaciaG6.Helpers;

namespace SistemaFarmaciaG6.Controllers
{
    public class PerfilController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public PerfilController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var usuario = _context.Usuarios
                .Include(u => u.IdGeneroNavigation)
                .Include(u => u.IdDepartamentoNavigation)
                .Include(u => u.IdCategoriaNavigation)
                .Include(u => u.IdTipoNombramientoNavigation)
                .Include(u => u.UsuarioRols)
                    .ThenInclude(ur => ur.IdRolNavigation)
                .FirstOrDefault(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        public IActionResult Edit()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var usuario = _context.Usuarios.Find(idUsuario);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        [HttpPost]
        public IActionResult Edit(Usuario usuario)
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var usuarioBD = _context.Usuarios.Find(idUsuario);

            if (usuarioBD == null)
            {
                return NotFound();
            }

            string telefonoAnterior = usuarioBD.Telefono ?? "Sin teléfono";
            DateOnly fechaAnterior = usuarioBD.FechaNacimiento;

            ModelState.Clear();

            if (usuario.FechaNacimiento == default)
                ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento es obligatoria.");

            if (usuario.FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
                ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento no puede ser futura.");

            if (!string.IsNullOrWhiteSpace(usuario.Telefono) && usuario.Telefono.Length < 8)
                ModelState.AddModelError("Telefono", "El teléfono debe tener al menos 8 dígitos.");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revise los datos ingresados.";
                return View(usuarioBD);
            }

            usuarioBD.FechaNacimiento = usuario.FechaNacimiento;
            usuarioBD.Telefono = usuario.Telefono;

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "Usuarios",
                "Editar Perfil",
                $"El usuario {usuarioBD.Nombre} {usuarioBD.Apellido1} actualizó su perfil. Teléfono anterior: {telefonoAnterior}, teléfono actual: {usuarioBD.Telefono}. Fecha nacimiento anterior: {fechaAnterior}, fecha actual: {usuarioBD.FechaNacimiento}."
            );

            TempData["Exito"] = "Perfil actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}