using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Models;
using SistemaFarmaciaG6.Helpers;
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

                AuditoriaHelper.Registrar(
                    _context,
                    HttpContext,
                    "Categorias",
                    "Crear",
                    $"Se creó la categoría {categoria.NombreCategoria} con estado {categoria.Estado}."
                );

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
                var categoriaBD = _context.Categorias.Find(id);

                if (categoriaBD == null)
                {
                    return NotFound();
                }

                string nombreAnterior = categoriaBD.NombreCategoria;
                string estadoAnterior = categoriaBD.Estado;

                categoriaBD.NombreCategoria = categoria.NombreCategoria;
                categoriaBD.Estado = categoria.Estado;

                _context.SaveChanges();

                AuditoriaHelper.Registrar(
                    _context,
                    HttpContext,
                    "Categorias",
                    "Editar",
                    $"Se editó la categoría #{categoriaBD.IdCategoria}. Nombre anterior: {nombreAnterior}, nombre actual: {categoriaBD.NombreCategoria}. Estado anterior: {estadoAnterior}, estado actual: {categoriaBD.Estado}."
                );

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

            string nombreCategoria = categoria.NombreCategoria;
            int idCategoria = categoria.IdCategoria;

            _context.Categorias.Remove(categoria);
            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "Categorias",
                "Eliminar",
                $"Se eliminó la categoría #{idCategoria}: {nombreCategoria}."
            );

            TempData["Exito"] =
                "Categoría eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // DETALLES (GET)
        public IActionResult Details(int id)
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
    }
}