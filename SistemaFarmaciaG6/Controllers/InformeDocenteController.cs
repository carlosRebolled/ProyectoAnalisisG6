using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Helpers;
using SistemaFarmaciaG6.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


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

            int anioActual = DateTime.Now.Year;

            bool existe = _context.InformeDocentes.Any(i =>
                i.IdUsuario == idUsuario &&
                i.Anio == anioActual);

            if (existe)
            {
                TempData["Error"] =
                    $"Ya existe un informe docente para el año {anioActual}.";

                return RedirectToAction(nameof(Index));
            }

            var informe = new InformeDocente
            {
                IdUsuario = idUsuario.Value,
                IdEstado = 1,
                Anio = anioActual,
                FechaCreacion = DateTime.Now,
                FechaEnvio = null,
                FechaAprobacion = null
            };

            _context.InformeDocentes.Add(informe);
            _context.SaveChanges();

            AuditoriaHelper.Registrar(
                _context,
                HttpContext,
                "InformeDocente",
                "Crear",
                $"Se creó el informe docente #{informe.IdInformeDocente} del año {informe.Anio}."
            );

            return RedirectToAction(nameof(Edit), new
            {
                id = informe.IdInformeDocente
            });
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

        /*public IActionResult ExportarPdf2(int id)
        {
            if (!EsAdministrador())
            {
                return RedirectToAction("Index", "Home");
            }

            var informe = _context.InformeDocentes
                .Include(i => i.IdEstadoNavigation)
                .Include(i => i.IdUsuarioNavigation)
                .Include(i => i.DetalleInformeD)
                .FirstOrDefault(i => i.IdInformeFinal == id);

            if (informe == null)
            {
                return NotFound();
            }

            string nombreGenerador =
                $"{informe.IdUsuarioNavigation?.Nombre} " +
                $"{informe.IdUsuarioNavigation?.Apellido1} " +
                $"{informe.IdUsuarioNavigation?.Apellido2}";

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
        */
        [HttpGet]
        public IActionResult ExportarPdf(int id)
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
                var informe = _context.InformeDocentes
                    .Include(i => i.IdEstadoNavigation)
                    .Include(i => i.IdUsuarioNavigation)
                    .FirstOrDefault(i => i.IdInformeDocente == id);

                if (informe == null)
                {
                    TempData["Error"] = "El informe solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }

                string nombreGenerador =
                    $"{informe.IdUsuarioNavigation?.Nombre} " +
                    $"{informe.IdUsuarioNavigation?.Apellido1} " +
                    $"{informe.IdUsuarioNavigation?.Apellido2}".Trim();

                string nombreEstado =
                    informe.IdEstadoNavigation?.NombreEstado
                    ?? (informe.IdEstado == 1 ? "Borrador" : "Finalizado");

                // Generación del PDF
                byte[] pdf = Document.Create(document =>
                {
                    document.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);

                        page.DefaultTextStyle(t => t.FontSize(10).FontFamily("Arial"));

                        // Encabezado
                        page.Header()
                            .BorderBottom(2)
                            .BorderColor("#168F8D")
                            .PaddingBottom(12)
                            .Column(header =>
                            {
                                header.Item().Text("UNIVERSIDAD DE COSTA RICA")
                                    .FontSize(15).Bold().FontColor("#168F8D");

                                header.Item().Text("Facultad de Farmacia").FontSize(11);

                                header.Item().PaddingTop(8)
                                    .Text("INFORME DOCENTE")
                                    .FontSize(20).Bold();
                            });

                        // Contenido
                        page.Content().PaddingVertical(20).Column(contenido =>
                        {
                            contenido.Spacing(12);

                            contenido.Item().Background("#F3F7F8").Border(1)
                                .BorderColor("#D4E1E4").Padding(15).Column(datos =>
                                {
                                    datos.Spacing(7);

                                    datos.Item().Text(t =>
                                    {
                                        t.Span("Año: ").Bold();
                                        t.Span(informe.Anio.ToString());
                                    });

                                    datos.Item().Text(t =>
                                    {
                                        t.Span("Generado por: ").Bold();
                                        t.Span(nombreGenerador);
                                    });

                                    datos.Item().Text(t =>
                                    {
                                        t.Span("Estado: ").Bold();
                                        t.Span(nombreEstado);
                                    });

                                    datos.Item().Text(t =>
                                    {
                                        t.Span("Fecha de creación: ").Bold();
                                        t.Span(informe.FechaCreacion.ToString("dd/MM/yyyy HH:mm"));
                                    });

                                    datos.Item().Text(t =>
                                    {
                                        t.Span("Fecha de envío: ").Bold();
                                        t.Span(informe.FechaEnvio?.ToString("dd/MM/yyyy HH:mm") ?? "No enviado");
                                    });

                                    datos.Item().Text(t =>
                                    {
                                        t.Span("Fecha de aprobación: ").Bold();
                                        t.Span(informe.FechaAprobacion?.ToString("dd/MM/yyyy HH:mm") ?? "No aprobado");
                                    });
                                });

                            // Tabla de secciones del informe
                            contenido.Item().PaddingTop(8).Text("Detalle del Informe Docente")
                                .FontSize(14).Bold().FontColor("#168F8D");

                            contenido.Item().Table(tabla =>
                            {
                                tabla.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(3); // Nombre sección
                                    c.ConstantColumn(70); // Cantidad
                                    c.RelativeColumn(7); // Detalle
                                });

                                tabla.Header(h =>
                                {
                                    h.Cell().Element(EstiloEncabezado).Text("Sección").Bold();
                                    h.Cell().Element(EstiloEncabezado).AlignCenter().Text("Cantidad").Bold();
                                    h.Cell().Element(EstiloEncabezado).Text("Detalle").Bold();
                                });

                                // Cada sección del modelo
                                AgregarFila(tabla, "Congresos Activos", informe.CantidadCongresosActivos, informe.DetalleCongresosActivos);
                                AgregarFila(tabla, "Congresos Pasivos", informe.CantidadCongresosPasivos, informe.DetalleCongresosPasivos);
                                AgregarFila(tabla, "Acción Social", informe.CantidadAccionSocial, informe.DetalleAccionSocial);
                                AgregarFila(tabla, "Investigación", informe.CantidadInvestigacion, informe.DetalleInvestigacion);
                                AgregarFila(tabla, "Docencia", informe.CantidadDocencia, informe.DetalleDocencia);
                                AgregarFila(tabla, "Publicaciones", informe.CantidadPublicaciones, informe.DetallePublicaciones);
                                AgregarFila(tabla, "Cursos de Grado", informe.CantidadCursosGrado, informe.DetalleCursosGrado);
                                AgregarFila(tabla, "Posgrado", informe.CantidadPosgrado, informe.DetallePosgrado);
                                AgregarFila(tabla, "Representación", informe.CantidadRepresentacion, informe.DetalleRepresentacion);
                                AgregarFila(tabla, "Otros", null, informe.DetalleOtros);
                            });

                            // Observaciones
                            contenido.Item().PaddingTop(8).Text("Observaciones")
                                .FontSize(14).Bold().FontColor("#168F8D");

                            contenido.Item().Border(1).BorderColor("#D9E2E6").Padding(12)
                                .Text(string.IsNullOrWhiteSpace(informe.ObservacionesDocente)
                                    ? "Sin observaciones."
                                    : informe.ObservacionesDocente);
                        });

                        // Pie de página
                        page.Footer().BorderTop(1).BorderColor("#D9E2E6").PaddingTop(8)
                            .Row(footer =>
                            {
                                footer.RelativeItem()
                                    .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                                    .FontSize(8).FontColor(Colors.Grey.Darken1);

                                footer.RelativeItem().AlignRight().Text(t =>
                                {
                                    t.Span("Página ").FontSize(8);
                                    t.CurrentPageNumber().FontSize(8);
                                    t.Span(" de ").FontSize(8);
                                    t.TotalPages().FontSize(8);
                                });
                            });
                    });
                }).GeneratePdf();

                AuditoriaHelper.Registrar(
                    _context,
                    HttpContext,
                    "InformeDocente",
                    "Exportar PDF",
                    $"Se exportó a PDF el informe docente #{informe.IdInformeDocente} del año {informe.Anio}."
                );

                string nombreArchivo = $"Informe_Docente_{informe.Anio}.pdf";

                return File(pdf, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.InnerException?.Message ?? ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Método auxiliar para filas de la tabla
        private static void AgregarFila(TableDescriptor tabla, string seccion, int? cantidad, string? detalle)
        {
            tabla.Cell().Element(EstiloCelda).Text(seccion);
            tabla.Cell().Element(EstiloCelda).AlignCenter().Text(cantidad?.ToString() ?? "-");
            tabla.Cell().Element(EstiloCelda).Text(detalle ?? "No aplica");
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

                    return RedirectToAction(nameof(Edit), new { id });
                }

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

                TempData["LimpiarLocalStorage"] = true;
                TempData["Exito"] = "Informe guardado correctamente.";
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