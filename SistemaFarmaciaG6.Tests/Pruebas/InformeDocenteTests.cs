using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Controllers;
using SistemaFarmaciaG6.Models;
using SistemaFarmaciaG6.Tests.Infraestructura;

namespace SistemaFarmaciaG6.Tests.Pruebas;

/// <summary>
/// Ciclo de vida del informe docente:
/// Borrador (1) → Pendiente (2) → Devuelto (3) / Aprobado (4) → Finalizado (5).
/// </summary>
public class InformeDocenteTests : PruebaBase
{
    private InformeDocenteController Como(string rol, int idUsuario) =>
        Preparar(new InformeDocenteController(Db), rol, idUsuario);

    // ------------------------------ LISTADO ------------------------------

    [Fact]
    public void Index_SinSesion_RedirigeAlLogin()
    {
        var resultado = Preparar(new InformeDocenteController(Db)).Index();

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Login", redireccion.ActionName);
        Assert.Equal("Account", redireccion.ControllerName);
    }

    [Fact]
    public void Index_Docente_SoloVeSusPropiosInformes()
    {
        AgregarInformeDocente(IdDocente, EstadoBorrador);
        AgregarInformeDocente(IdDocenteOtroDep, EstadoBorrador);

        var vista = Assert.IsType<ViewResult>(Como("Docente", IdDocente).Index());

        var informes = Assert.IsAssignableFrom<List<InformeDocente>>(vista.Model);
        Assert.All(informes, i => Assert.Equal(IdDocente, i.IdUsuario));
        Assert.Single(informes);
    }

    [Fact]
    public void Index_Director_SoloVeInformesDeSuDepartamento()
    {
        AgregarInformeDocente(IdDocente, EstadoPendiente);          // Farmacia
        AgregarInformeDocente(IdDocenteOtroDep, EstadoPendiente);   // Química

        var vista = Assert.IsType<ViewResult>(Como("Director", IdDirector).Index());

        var informe = Assert.Single(Assert.IsAssignableFrom<List<InformeDocente>>(vista.Model));
        Assert.Equal(IdDocente, informe.IdUsuario);
    }

    [Theory]
    [InlineData("Administrador")]
    [InlineData("Decano")]
    [InlineData("Jefatura")]
    [InlineData("Visualizador")]
    public void Index_RolesConVistaGeneral_VenTodosLosInformes(string rol)
    {
        AgregarInformeDocente(IdDocente, EstadoPendiente);
        AgregarInformeDocente(IdDocenteOtroDep, EstadoPendiente);

        var vista = Assert.IsType<ViewResult>(Como(rol, IdAdmin).Index());

        Assert.Equal(2, Assert.IsAssignableFrom<List<InformeDocente>>(vista.Model).Count);
    }

    [Fact]
    public void Index_RolDesconocido_RedirigeAlInicio()
    {
        var resultado = Como("Invitado", IdDocente).Index();

        Assert.Equal("Home", Assert.IsType<RedirectToActionResult>(resultado).ControllerName);
    }

    // ------------------------------ CREAR ------------------------------

    [Fact]
    public void Crear_Docente_CreaBorradorDelAnioActualYAbreLaEdicion()
    {
        var resultado = Como("Docente", IdDocente).Create();

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Edit", redireccion.ActionName);

        var informe = Assert.Single(Db.InformeDocentes.ToList());
        Assert.Equal(IdDocente, informe.IdUsuario);
        Assert.Equal(EstadoBorrador, informe.IdEstado);
        Assert.Equal(AnioActual, informe.Anio);
        Assert.Null(informe.FechaEnvio);
        Assert.Contains(Db.Auditoria.ToList(), a => a.TablaAfectada == "InformeDocente" && a.Accion == "Crear");
    }

    [Fact]
    public void Crear_YaExisteInformeEsteAnio_NoCreaOtro()
    {
        AgregarInformeDocente(IdDocente, EstadoBorrador);
        var controlador = Como("Docente", IdDocente);

        var resultado = controlador.Create();

        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultado).ActionName);
        Assert.Single(Db.InformeDocentes.ToList());
        Assert.Equal($"Ya existe un informe docente para el año {AnioActual}.", controlador.TempData["Error"]);
    }

    [Fact]
    public void Crear_InformeDeAnioAnterior_PermiteCrearElDeEsteAnio()
    {
        AgregarInformeDocente(IdDocente, EstadoAprobado, AnioActual - 1);

        Como("Docente", IdDocente).Create();

        Assert.Equal(2, Db.InformeDocentes.Count(i => i.IdUsuario == IdDocente));
    }

    [Theory]
    [InlineData("Director")]
    [InlineData("Decano")]
    [InlineData("Visualizador")]
    public void Crear_RolSinPermiso_NoCreaInforme(string rol)
    {
        var resultado = Como(rol, IdDirector).Create();

        Assert.Equal("Home", Assert.IsType<RedirectToActionResult>(resultado).ControllerName);
        Assert.Empty(Db.InformeDocentes.ToList());
    }

    [Fact]
    public void CrearPost_FormularioIncompleto_NoGuarda()
    {
        var controlador = Como("Docente", IdDocente);
        var informe = InformeDocenteCompleto();
        informe.DetalleInvestigacion = "   ";

        controlador.Create(informe);

        Assert.Empty(Db.InformeDocentes.ToList());
        Assert.Contains("espacios obligatorios", (string)controlador.TempData["Error"]!);
    }

    [Fact]
    public void CrearPost_FormularioCompleto_GuardaComoBorrador()
    {
        var controlador = Como("Docente", IdDocente);

        controlador.Create(InformeDocenteCompleto());

        var informe = Assert.Single(Db.InformeDocentes.ToList());
        Assert.Equal(EstadoBorrador, informe.IdEstado);
        Assert.Equal(IdDocente, informe.IdUsuario);
        Assert.Equal("Proyecto 123", informe.DetalleInvestigacion);
    }

    // ------------------------------ EDITAR ------------------------------

    [Fact]
    public void Editar_BorradorPropio_GuardaLosCambios()
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoBorrador);
        var cambios = InformeDocenteCompleto();
        cambios.CantidadPublicaciones = 3;
        cambios.DetallePublicaciones = "Tres artículos";

        var resultado = Como("Docente", IdDocente).Edit(informe.IdInformeDocente, cambios);

        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultado).ActionName);
        Assert.Equal(3, informe.CantidadPublicaciones);
        Assert.Equal("Tres artículos", informe.DetallePublicaciones);
    }

    [Fact]
    public void Editar_InformeDevuelto_SePuedeCorregir()
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoDevuelto);

        Como("Docente", IdDocente).Edit(informe.IdInformeDocente, InformeDocenteCompleto());

        Assert.Equal("Proyecto 123", informe.DetalleInvestigacion);
    }

    [Fact]
    public void Editar_InformeDeOtroDocente_NoSePermite()
    {
        var informe = AgregarInformeDocente(IdDocenteOtroDep, EstadoBorrador);
        var controlador = Como("Docente", IdDocente);

        controlador.Edit(informe.IdInformeDocente, InformeDocenteCompleto());

        Assert.Equal("No puede editar informes de otro usuario.", controlador.TempData["Error"]);
        Assert.Null(informe.DetalleInvestigacion);
    }

    [Theory]
    [InlineData(EstadoPendiente)]
    [InlineData(EstadoAprobado)]
    [InlineData(EstadoFinalizado)]
    public void Editar_InformeYaEnviadoOAprobado_NoSePermite(int estado)
    {
        var informe = AgregarInformeDocente(IdDocente, estado);
        var controlador = Como("Docente", IdDocente);

        controlador.Edit(informe.IdInformeDocente, InformeDocenteCompleto());

        Assert.Equal("Solo se pueden editar informes en estado Borrador o Devuelto.", controlador.TempData["Error"]);
        Assert.Null(informe.DetalleInvestigacion);
    }

    [Fact]
    public void Editar_FormularioIncompleto_NoGuarda()
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoBorrador);
        var cambios = InformeDocenteCompleto();
        cambios.DetalleOtros = null;

        var resultado = Como("Docente", IdDocente).Edit(informe.IdInformeDocente, cambios);

        Assert.Equal("Edit", Assert.IsType<RedirectToActionResult>(resultado).ActionName);
        Assert.Null(informe.DetalleInvestigacion);
    }

    [Fact]
    public void Editar_InformeInexistente_DevuelveNotFound()
    {
        Assert.IsType<NotFoundResult>(Como("Docente", IdDocente).Edit(999, InformeDocenteCompleto()));
    }

    // ------------------------------ ENVIAR ------------------------------

    [Theory]
    [InlineData(EstadoBorrador)]
    [InlineData(EstadoDevuelto)]
    public void Enviar_BorradorODevuelto_PasaAPendiente(int estadoInicial)
    {
        var informe = AgregarInformeDocenteCompleto(IdDocente, estadoInicial);

        Como("Docente", IdDocente).EnviarConfirmado(informe.IdInformeDocente);

        Assert.Equal(EstadoPendiente, informe.IdEstado);
        Assert.NotNull(informe.FechaEnvio);
        Assert.Contains(Db.Auditoria.ToList(), a => a.Accion == "Enviar");
    }

    [Fact]
    public void Enviar_InformeYaAprobado_NoCambiaDeEstado()
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoAprobado);
        var controlador = Como("Docente", IdDocente);

        controlador.EnviarConfirmado(informe.IdInformeDocente);

        Assert.Equal(EstadoAprobado, informe.IdEstado);
        Assert.NotNull(controlador.TempData["Error"]);
    }

    [Fact]
    public void Enviar_InformeDeOtroDocente_NoSePermite()
    {
        var informe = AgregarInformeDocente(IdDocenteOtroDep, EstadoBorrador);

        Como("Docente", IdDocente).EnviarConfirmado(informe.IdInformeDocente);

        Assert.Equal(EstadoBorrador, informe.IdEstado);
    }

    // ------------------------------ ELIMINAR ------------------------------

    [Fact]
    public void Eliminar_BorradorPropio_SeElimina()
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoBorrador);

        Como("Docente", IdDocente).DeleteConfirmed(informe.IdInformeDocente);

        Assert.Empty(Db.InformeDocentes.ToList());
    }

    [Fact]
    public void Eliminar_InformePendiente_NoSeElimina()
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoPendiente);

        Como("Docente", IdDocente).DeleteConfirmed(informe.IdInformeDocente);

        Assert.Single(Db.InformeDocentes.ToList());
    }

    [Fact]
    public void Eliminar_InformeDeOtroDocente_NoSeElimina()
    {
        var informe = AgregarInformeDocente(IdDocenteOtroDep, EstadoBorrador);

        Como("Docente", IdDocente).DeleteConfirmed(informe.IdInformeDocente);

        Assert.Single(Db.InformeDocentes.ToList());
    }

    // ------------------------------ REABRIR ------------------------------

    [Theory]
    [InlineData(EstadoPendiente)]
    [InlineData(EstadoAprobado)]
    [InlineData(EstadoFinalizado)]
    public void Reabrir_Administrador_DevuelveElInformeABorrador(int estadoInicial)
    {
        var informe = AgregarInformeDocente(IdDocente, estadoInicial);
        informe.FechaEnvio = DateTime.Now;
        informe.FechaAprobacion = DateTime.Now;
        Db.SaveChanges();

        Como("Administrador", IdAdmin).Reabrir(informe.IdInformeDocente);

        Assert.Equal(EstadoBorrador, informe.IdEstado);
        Assert.Null(informe.FechaEnvio);
        Assert.Null(informe.FechaAprobacion);
    }

    [Fact]
    public void Reabrir_Docente_NoTienePermiso()
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoAprobado);

        Como("Docente", IdDocente).Reabrir(informe.IdInformeDocente);

        Assert.Equal(EstadoAprobado, informe.IdEstado);
    }

    [Fact]
    public void Reabrir_InformeEnBorrador_MuestraError()
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoBorrador);
        var controlador = Como("Administrador", IdAdmin);

        controlador.Reabrir(informe.IdInformeDocente);

        Assert.Equal("Solo se pueden reabrir informes enviados, aprobados o finalizados.", controlador.TempData["Error"]);
    }

    // ------------------------------ DETALLE / PDF ------------------------------

    [Fact]
    public void Detalle_DocenteViendoInformeAjeno_EsRedirigido()
    {
        var informe = AgregarInformeDocente(IdDocenteOtroDep, EstadoPendiente);

        var resultado = Como("Docente", IdDocente).Details(informe.IdInformeDocente);

        Assert.Equal("Home", Assert.IsType<RedirectToActionResult>(resultado).ControllerName);
    }

    [Fact]
    public void Detalle_DirectorDeOtroDepartamento_EsRedirigido()
    {
        var informe = AgregarInformeDocente(IdDocenteOtroDep, EstadoPendiente);

        var resultado = Como("Director", IdDirector).Details(informe.IdInformeDocente);

        Assert.IsType<RedirectToActionResult>(resultado);
    }

    [Fact]
    public void Detalle_DirectorDelMismoDepartamento_PuedeVerloConObservaciones()
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoDevuelto);
        Db.Observaciones.Add(new Observacione
        {
            IdInformeDocente = informe.IdInformeDocente,
            IdUsuarioRealiza = IdDirector,
            Comentario = "Falta detalle",
            Fecha = DateTime.Now
        });
        Db.SaveChanges();
        var controlador = Como("Director", IdDirector);

        var vista = Assert.IsType<ViewResult>(controlador.Details(informe.IdInformeDocente));

        Assert.Same(informe, vista.Model);
        Assert.Single((List<Observacione>)controlador.ViewBag.Observaciones);
    }

    [Fact]
    public void ExportarPdf_GeneraUnArchivoPdf()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var informe = AgregarInformeDocente(IdDocente, EstadoAprobado);
        var controlador = Como("Docente", IdDocente);

        var resultado = controlador.ExportarPdf(informe.IdInformeDocente);

        var archivo = Assert.IsType<FileContentResult>(resultado);
        Assert.Equal("application/pdf", archivo.ContentType);
        Assert.Equal($"Informe_Docente_{AnioActual}.pdf", archivo.FileDownloadName);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(archivo.FileContents, 0, 4));
    }
}
