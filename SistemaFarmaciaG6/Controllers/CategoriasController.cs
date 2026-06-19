using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Models;
using System.Linq;

namespace SistemaFarmaciaG6.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public CategoriasController(DbFacultadFarmaciaContext context)
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

        // Acción para listar las categorías
        public IActionResult Index()
        {
            if (!EsAdministrador())
            {
                return RedirigirNoAutorizado();
            }

            // Consulta LINQ equivalente al SELECT TOP (1000)
            var categorias = _context.Categorias
                                     .Take(1000)
                                     .ToList();

            return View(categorias);
        }

        // CREAR (GET)
        public IActionResult Create()
        {
            if (!EsAdministrador())
            {
                return RedirigirNoAutorizado();
            }

            return View();
        }

        // CREAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Categoria categoria)
        {
            if (!EsAdministrador())
            {
                return RedirigirNoAutorizado();
            }

            if (ModelState.IsValid)
            {
                _context.Categorias.Add(categoria);
                _context.SaveChanges();

                TempData["Exito"] = "Categoría creada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Revise los datos ingresados.";

            return View(categoria);
        }

        // EDITAR (GET)
        public IActionResult Edit(int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirNoAutorizado();
            }

            var categoria = _context.Categorias.Find(id);

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // EDITAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Categoria categoria)
        {
            if (!EsAdministrador())
            {
                return RedirigirNoAutorizado();
            }

            if (id != categoria.IdCategoria)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(categoria);
                _context.SaveChanges();

                TempData["Exito"] = "Categoría actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Revise los datos ingresados.";

            return View(categoria);
        }

        // ELIMINAR (GET)
        public IActionResult Delete(int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirNoAutorizado();
            }

            var categoria = _context.Categorias.Find(id);

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // ELIMINAR (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!EsAdministrador())
            {
                return RedirigirNoAutorizado();
            }

            var categoria = _context.Categorias.Find(id);

            if (categoria == null)
            {
                return NotFound();
            }

            // Verificar si existen usuarios con esta categoría
            bool tieneUsuarios = _context.Usuarios
                                         .Any(u => u.IdCategoria == id);

            if (tieneUsuarios)
            {
                TempData["Error"] =
                    "No se puede eliminar la categoría porque existen usuarios asociados a ella.";

                return RedirectToAction(nameof(Index));
            }

            _context.Categorias.Remove(categoria);
            _context.SaveChanges();

            TempData["Exito"] =
                "Categoría eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}