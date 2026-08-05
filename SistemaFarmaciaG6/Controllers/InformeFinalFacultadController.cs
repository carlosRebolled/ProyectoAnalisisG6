using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Helpers;
using SistemaFarmaciaG6.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace SistemaFarmaciaG6.Controllers
{
    public class InformeFinalFacultadController : Controller
    {
        private readonly DbFacultadFarmaciaContext _context;

        public InformeFinalFacultadController(DbFacultadFarmaciaContext context)
        {
            _context = context;
        }

        private string RolSesion()
        {
            return HttpContext.Session.GetString("Rol") ?? "";
        }

        private int? IdUsuarioSesion()
        {
            return HttpContext.Session.GetInt32("IdUsuario");
        }

        private bool PuedeGestionar()
        {
            var rol = RolSesion();

            return rol == "Decano" ||
                   rol == "Administrador" ||
                   rol == "Jefatura" ||
                   rol == "Visualizador";
        }

        private bool EsAdministrador()
        {
            return RolSesion() == "Administrador";
        }

        public IActionResult Index()
        {
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informes = _context.InformeFinalFacultads
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioGeneraNavigation)
                .OrderByDescending(i => i.Anio)
                .ToList();

            return View(informes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(InformeFinalFacultad informe)
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

            bool existe = _context.InformeFinalFacultads.Any(i =>
                i.Anio == anioActual);

            if (existe)
            {
                TempData["Error"] = $"Ya existe un informe final para el año {anioActual}.";
                return RedirectToAction(nameof(Index));
            }

            informe.IdUsuarioGenera = idUsuario.Value;
            informe.IdEstado = 1;
            informe.Anio = anioActual;
            informe.FechaGeneracion = DateTime.Now;
            informe.FechaAprobacion = null;

            _context.InformeFinalFacultads.Add(informe);
            _context.SaveChanges();

            var informesDireccion = _context.InformeDireccions
                .Include(i => i.IdUsuarioNavigation)
                .Where(i => i.Anio == anioActual && i.IdEstado == 4)
                .ToList();

            foreach (var item in informesDireccion)
            {
                var detalle = new DetalleInformeFinal
                {
                    IdInformeFinal = informe.IdInformeFinal,
                    TipoActividad = "Informe Dirección",
                    Cantidad = 1,
                    DetalleActividad =
                        $"Informe Dirección #{item.IdInformeDireccion} generado por {item.IdUsuarioNavigation.Nombre} {item.IdUsuarioNavigation.Apellido1}"
                };

                _context.DetalleInformeFinals.Add(detalle);
            }

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeFinalFacultad",
                "Crear",
                $"Se creó el informe final de Facultad #{informe.IdInformeFinal} del año {informe.Anio}."
            );

            TempData["Exito"] = "Informe final creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informe = _context.InformeFinalFacultads
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioGeneraNavigation)
                .Include(i => i.DetalleInformeFinals)
                .FirstOrDefault(i => i.IdInformeFinal == id);

            if (informe == null)
            {
                return NotFound();
            }

            var informesDireccion = _context.InformeDireccions
                .Include(i => i.IdUsuarioNavigation)
                    .ThenInclude(u => u.IdDepartamentoNavigation)
                .Include(i => i.IdEstadoNavigation)
                .Where(i =>
                    i.Anio == informe.Anio &&
                    i.IdEstado == 4)
                .OrderBy(i => i.IdUsuarioNavigation.IdDepartamentoNavigation.NombreDepartamento)
                .ToList();

            ViewBag.InformesDireccion = informesDireccion;

            var informesDocentes = _context.InformeDocentes
                .Include(i => i.IdUsuarioNavigation)
                    .ThenInclude(u => u.IdDepartamentoNavigation)
                .Include(i => i.IdEstadoNavigation)
                .Where(i =>
                    i.Anio == informe.Anio &&
                    i.IdEstado == 4)
                .OrderBy(i => i.IdUsuarioNavigation.Apellido1)
                .ThenBy(i => i.IdUsuarioNavigation.Apellido2)
                .ThenBy(i => i.IdUsuarioNavigation.Nombre)
                .ToList();

            ViewBag.InformesDocentes = informesDocentes;

            return View(informe);
        }

        [HttpGet]
        public IActionResult ExportarPdf(int id)
        {
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informe = _context.InformeFinalFacultads
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioGeneraNavigation)
                .Include(i => i.DetalleInformeFinals)
                .FirstOrDefault(i => i.IdInformeFinal == id);

            if (informe == null)
            {
                return NotFound();
            }

            string nombreGenerador =
                $"{informe.IdUsuarioGeneraNavigation?.Nombre} " +
                $"{informe.IdUsuarioGeneraNavigation?.Apellido1} " +
                $"{informe.IdUsuarioGeneraNavigation?.Apellido2}";

            nombreGenerador = nombreGenerador.Trim();

            string nombreEstado =
                informe.IdEstadoNavigation?.NombreEstado
                ?? (informe.IdEstado == 1 ? "Borrador" : "Finalizado");

            byte[] pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);

                    page.Margin(40);

                    page.DefaultTextStyle(text =>
                        text.FontSize(10)
                            .FontFamily("Arial")
                    );

                    page.Header()
                        .BorderBottom(2)
                        .BorderColor("#168F8D")
                        .PaddingBottom(12)
                        .Column(header =>
                        {
                            header.Item()
                                .Text("UNIVERSIDAD DE COSTA RICA")
                                .FontSize(15)
                                .Bold()
                                .FontColor("#168F8D");

                            header.Item()
                                .Text("Facultad de Farmacia")
                                .FontSize(11);

                            header.Item()
                                .PaddingTop(8)
                                .Text("INFORME FINAL DE FACULTAD")
                                .FontSize(20)
                                .Bold();
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(contenido =>
                        {
                            contenido.Spacing(12);

                            contenido.Item()
                                .Background("#F3F7F8")
                                .Border(1)
                                .BorderColor("#D4E1E4")
                                .Padding(15)
                                .Column(datos =>
                                {
                                    datos.Spacing(7);

                                    datos.Item().Text(texto =>
                                    {
                                        texto.Span("Año: ").Bold();
                                        texto.Span(informe.Anio.ToString());
                                    });

                                    datos.Item().Text(texto =>
                                    {
                                        texto.Span("Generado por: ").Bold();
                                        texto.Span(nombreGenerador);
                                    });

                                    datos.Item().Text(texto =>
                                    {
                                        texto.Span("Estado: ").Bold();
                                        texto.Span(nombreEstado);
                                    });

                                    datos.Item().Text(texto =>
                                    {
                                        texto.Span("Fecha de generación: ").Bold();

                                        texto.Span(
                                            informe.FechaGeneracion
                                                .ToString("dd/MM/yyyy HH:mm")
                                        );
                                    });

                                    datos.Item().Text(texto =>
                                    {
                                        texto.Span("Fecha de finalización: ").Bold();

                                        texto.Span(
                                            informe.FechaAprobacion.HasValue
                                                ? informe.FechaAprobacion.Value
                                                    .ToString("dd/MM/yyyy HH:mm")
                                                : "No finalizado"
                                        );
                                    });
                                });

                            contenido.Item()
                                .PaddingTop(8)
                                .Text("Observaciones generales")
                                .FontSize(14)
                                .Bold()
                                .FontColor("#168F8D");

                            contenido.Item()
                                .Border(1)
                                .BorderColor("#D9E2E6")
                                .Padding(12)
                                .Text(
                                    string.IsNullOrWhiteSpace(informe.Observaciones)
                                        ? "Sin observaciones."
                                        : informe.Observaciones
                                );

                            if (informe.DetalleInformeFinals != null &&
                                informe.DetalleInformeFinals.Any())
                            {
                                contenido.Item()
                                    .PaddingTop(8)
                                    .Text("Detalle del informe")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor("#168F8D");

                                contenido.Item().Table(tabla =>
                                {
                                    tabla.ColumnsDefinition(columnas =>
                                    {
                                        columnas.RelativeColumn(2);
                                        columnas.ConstantColumn(70);
                                        columnas.RelativeColumn(4);
                                    });

                                    tabla.Header(encabezado =>
                                    {
                                        encabezado.Cell()
                                            .Element(EstiloEncabezado)
                                            .Text("Tipo de actividad")
                                            .Bold();

                                        encabezado.Cell()
                                            .Element(EstiloEncabezado)
                                            .AlignCenter()
                                            .Text("Cantidad")
                                            .Bold();

                                        encabezado.Cell()
                                            .Element(EstiloEncabezado)
                                            .Text("Detalle")
                                            .Bold();
                                    });

                                    foreach (var detalle in informe.DetalleInformeFinals)
                                    {
                                        tabla.Cell()
                                            .Element(EstiloCelda)
                                            .Text(detalle.TipoActividad ?? "");

                                        tabla.Cell()
                                            .Element(EstiloCelda)
                                            .AlignCenter()
                                            .Text(
                                                detalle.Cantidad?.ToString() ?? "-"
                                            );

                                        tabla.Cell()
                                            .Element(EstiloCelda)
                                            .Text(
                                                detalle.DetalleActividad
                                                ?? "Sin detalle"
                                            );
                                    }
                                });
                            }
                        });

                    page.Footer()
                        .BorderTop(1)
                        .BorderColor("#D9E2E6")
                        .PaddingTop(8)
                        .Row(footer =>
                        {
                            footer.RelativeItem()
                                .Text(
                                    $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}"
                                )
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);

                            footer.RelativeItem()
                                .AlignRight()
                                .Text(texto =>
                                {
                                    texto.Span("Página ").FontSize(8);
                                    texto.CurrentPageNumber().FontSize(8);
                                    texto.Span(" de ").FontSize(8);
                                    texto.TotalPages().FontSize(8);
                                });
                        });
                });
            }).GeneratePdf();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeFinalFacultad",
                "Exportar PDF",
                $"Se exportó a PDF el informe final " +
                $"#{informe.IdInformeFinal} del año {informe.Anio}."
            );

            string nombreArchivo =
                $"Informe_Final_Facultad_{informe.Anio}.pdf";

            return File(
                pdf,
                "application/pdf",
                nombreArchivo
            );
        }

        private static IContainer EstiloEncabezado(IContainer container)
        {
            return container
                .Background("#5C778B")
                .PaddingVertical(8)
                .PaddingHorizontal(6)
                .DefaultTextStyle(text =>
                    text.FontColor(Colors.White)
                        .FontSize(9)
                );
        }

        private static IContainer EstiloCelda(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor("#D9E2E6")
                .PaddingVertical(7)
                .PaddingHorizontal(6)
                .DefaultTextStyle(text =>
                    text.FontSize(9)
                );
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informe = _context.InformeFinalFacultads.Find(id);

            if (informe == null)
            {
                return NotFound();
            }

            if (informe.IdEstado != 1)
            {
                TempData["Error"] = "Solo se pueden editar informes en estado Borrador.";
                return RedirectToAction(nameof(Index));
            }

            return View(informe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, InformeFinalFacultad informe)
        {
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informeBD = _context.InformeFinalFacultads.Find(id);

            if (informeBD == null)
            {
                return NotFound();
            }

            if (informeBD.IdEstado != 1)
            {
                TempData["Error"] = "Solo se pueden editar informes en estado Borrador.";
                return RedirectToAction(nameof(Index));
            }

            informeBD.Observaciones = informe.Observaciones;

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeFinalFacultad",
                "Editar",
                $"Se editó el informe final de Facultad #{informeBD.IdInformeFinal} del año {informeBD.Anio}."
            );

            TempData["Exito"] = "Informe final actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Finalizar(int id)
        {
            if (!PuedeGestionar())
            {
                return RedirectToAction("Index", "Home");
            }

            var informe = _context.InformeFinalFacultads.Find(id);

            if (informe == null)
            {
                return NotFound();
            }

            if (informe.IdEstado != 1)
            {
                TempData["Error"] = "Solo se pueden finalizar informes en estado Borrador.";
                return RedirectToAction(nameof(Index));
            }

            informe.IdEstado = 5;
            informe.FechaAprobacion = DateTime.Now;

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeFinalFacultad",
                "Finalizar",
                $"Se finalizó el informe final de Facultad #{informe.IdInformeFinal} del año {informe.Anio}."
            );

            TempData["Exito"] = "Informe final finalizado correctamente.";
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

            var informe = _context.InformeFinalFacultads.Find(id);

            if (informe == null)
            {
                return NotFound();
            }

            if (informe.IdEstado != 5)
            {
                TempData["Error"] = "Solo se pueden reabrir informes finalizados.";
                return RedirectToAction(nameof(Index));
            }

            informe.IdEstado = 1;
            informe.FechaAprobacion = null;

            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeFinalFacultad",
                "Reabrir",
                $"El administrador reabrió el informe final de Facultad #{informe.IdInformeFinal} del año {informe.Anio}."
            );

            TempData["Exito"] = "Informe final reabierto correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}