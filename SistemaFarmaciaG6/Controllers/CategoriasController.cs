using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Models; 
using System.Linq;

public class CategoriasController : Controller
{
    private readonly DbFacultadFarmaciaContext _context;

    public CategoriasController(DbFacultadFarmaciaContext context)
    {
        _context = context;
    }

    // Acción para listar las categorías
    public IActionResult Index()
    {
        // Consulta LINQ equivalente al SELECT TOP (1000)
        var categorias = _context.Categorias
                                 .Take(1000)
                                 .ToList();

        return View(categorias);
    }
    // CREAR (GET)
    public IActionResult Create()
    {
        return View();
    }

    // CREAR (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Categoria categoria)
    {
        if (ModelState.IsValid)
        {
            _context.Categorias.Add(categoria);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        return View(categoria);
    }

    // EDITAR (GET)
    public IActionResult Edit(int id)
    {
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
        if (id != categoria.IdCategoria) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(categoria);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        return View(categoria);
    }

    // ELIMINAR (GET)
    public IActionResult Delete(int id)
    {
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
        var categoria = _context.Categorias.Find(id);
        if (categoria != null)
        {
            _context.Categorias.Remove(categoria);
            _context.SaveChanges();
        }
        return RedirectToAction(nameof(Index));
    }
    // DETALLES (GET)
    public IActionResult Details(int id)
    {
        var categoria = _context.Categorias.Find(id);
        if (categoria == null)
        {
            return NotFound();
        }
        return View(categoria);
    }

}

