using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Controllers;
using SistemaFarmaciaG6.Models;
using SistemaFarmaciaG6.Tests.Infraestructura;

namespace SistemaFarmaciaG6.Tests.Pruebas;

/// <summary>
/// Revisión de informes: el Director revisa los informes docentes de su
/// departamento y el Decano revisa los informes de dirección.
/// </summary>
public class RevisionInformesTests : PruebaBase
{
    private RevisionInformesController Director() =>
        Preparar(new RevisionInformesController(Db), "Director", IdDirector);

    private RevisionInformeDireccionController Decano() =>
        Preparar(new RevisionInformeDireccionController(Db), "Decano", IdDecano);

    // ------------------- DIRECTOR → INFORMES DOCENTES -------------------

    [Theory]
    [InlineData("Docente")]
    [InlineData("Decano")]
    [InlineData("Visualizador")]
    public void RevisionDocente_RolSinPermiso_EsRedirigido(string rol)
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoPendiente);
        var controlador = Preparar(new RevisionInformesController(Db), rol, IdDocente);

        Assert.IsType<RedirectToActionResult>(controlador.Index());
        controlador.Aprobar(informe.IdInformeDocente);

        Assert.Equal(EstadoPendiente, informe.IdEstado);
    }

    [Fact]
    public void RevisionDocente_Index_MuestraSoloDocentesDelDepartamento()
    {
        var vista = Assert.IsType<ViewResult>(Director().Index());

        var usuarios = Assert.IsAssignableFrom<List<Usuario>>(vista.Model);
        Assert.All(usuarios, u => Assert.Equal(DepFarmacia, u.IdDepartamento));
        Assert.DoesNotContain(usuarios, u => u.IdUsuario == IdDocenteOtroDep);
    }

    [Fact]
    public void Aprobar_InformePendienteDelDepartamento_QuedaAprobado()
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoPendiente);

        Director().Aprobar(informe.IdInformeDocente);

        Assert.Equal(EstadoAprobado, informe.IdEstado);
        Assert.NotNull(informe.FechaAprobacion);
        Assert.Contains(Db.Auditoria.ToList(), a => a.Accion == "Aprobar" && a.IdUsuario == IdDirector);
    }

    [Fact]
    public void Aprobar_InformeDeOtroDepartamento_NoSePermite()
    {
        var informe = AgregarInformeDocente(IdDocenteOtroDep, EstadoPendiente);
        var controlador = Director();

        controlador.Aprobar(informe.IdInformeDocente);

        Assert.Equal(EstadoPendiente, informe.IdEstado);
        Assert.Equal("No puede aprobar informes de otro departamento.", controlador.TempData["Error"]);
    }

    [Fact]
    public void Aprobar_Administrador_PuedeAprobarCualquierDepartamento()
    {
        var informe = AgregarInformeDocente(IdDocenteOtroDep, EstadoPendiente);

        Preparar(new RevisionInformesController(Db), "Administrador", IdAdmin).Aprobar(informe.IdInformeDocente);

        Assert.Equal(EstadoAprobado, informe.IdEstado);
    }

    [Theory]
    [InlineData(EstadoBorrador)]
    [InlineData(EstadoDevuelto)]
    [InlineData(EstadoAprobado)]
    public void Aprobar_InformeQueNoEstaPendiente_NoCambia(int estado)
    {
        var informe = AgregarInformeDocente(IdDocente, estado);
        var controlador = Director();

        controlador.Aprobar(informe.IdInformeDocente);

        Assert.Equal(estado, informe.IdEstado);
        Assert.Equal("Solo se pueden aprobar informes pendientes.", controlador.TempData["Error"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Devolver_SinObservacion_NoSePermite(string observacion)
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoPendiente);
        var controlador = Director();

        var resultado = controlador.Devolver(informe.IdInformeDocente, observacion);

        Assert.Equal("Details", Assert.IsType<RedirectToActionResult>(resultado).ActionName);
        Assert.Equal(EstadoPendiente, informe.IdEstado);
        Assert.Empty(Db.Observaciones.ToList());
    }

    [Fact]
    public void Devolver_ConObservacion_QuedaDevueltoYGuardaLaObservacion()
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoPendiente);

        Director().Devolver(informe.IdInformeDocente, "Completar la sección de investigación");

        Assert.Equal(EstadoDevuelto, informe.IdEstado);
        var observacion = Assert.Single(Db.Observaciones.ToList());
        Assert.Equal(informe.IdInformeDocente, observacion.IdInformeDocente);
        Assert.Equal(IdDirector, observacion.IdUsuarioRealiza);
        Assert.Equal("Devolución", observacion.TipoObservacion);
    }

    [Fact]
    public void FlujoCompleto_DevueltoCorregidoYReenviado_TerminaAprobado()
    {
        var informe = AgregarInformeDocenteCompleto(IdDocente, EstadoBorrador);
        var docente = Preparar(new InformeDocenteController(Db), "Docente", IdDocente);

        docente.EnviarConfirmado(informe.IdInformeDocente);
        Assert.Equal(EstadoPendiente, informe.IdEstado);

        Director().Devolver(informe.IdInformeDocente, "Corregir docencia");
        Assert.Equal(EstadoDevuelto, informe.IdEstado);

        docente.Edit(informe.IdInformeDocente, InformeDocenteCompleto());
        docente.EnviarConfirmado(informe.IdInformeDocente);
        Assert.Equal(EstadoPendiente, informe.IdEstado);

        Director().Aprobar(informe.IdInformeDocente);
        Assert.Equal(EstadoAprobado, informe.IdEstado);

        // Una vez aprobado, el docente ya no lo puede modificar
        var cambios = InformeDocenteCompleto();
        cambios.DetalleDocencia = "Cambio no permitido";
        docente.Edit(informe.IdInformeDocente, cambios);
        Assert.NotEqual("Cambio no permitido", informe.DetalleDocencia);
    }

    // ------------------- DECANO → INFORMES DE DIRECCIÓN -------------------

    [Fact]
    public void RevisionDireccion_Director_NoPuedeRevisar()
    {
        var informe = AgregarInformeDireccion(IdDirector, EstadoPendiente);
        var controlador = Preparar(new RevisionInformeDireccionController(Db), "Director", IdDirector);

        Assert.IsType<RedirectToActionResult>(controlador.Index());
        controlador.Aprobar(informe.IdInformeDireccion);

        Assert.Equal(EstadoPendiente, informe.IdEstado);
    }

    [Fact]
    public void RevisionDireccion_Aprobar_InformePendiente()
    {
        var informe = AgregarInformeDireccion(IdDirector, EstadoPendiente);

        Decano().Aprobar(informe.IdInformeDireccion);

        Assert.Equal(EstadoAprobado, informe.IdEstado);
        Assert.NotNull(informe.FechaAprobacion);
    }

    [Fact]
    public void RevisionDireccion_Aprobar_InformeEnBorrador_NoSePermite()
    {
        var informe = AgregarInformeDireccion(IdDirector, EstadoBorrador);
        var controlador = Decano();

        controlador.Aprobar(informe.IdInformeDireccion);

        Assert.Equal(EstadoBorrador, informe.IdEstado);
        Assert.NotNull(controlador.TempData["Error"]);
    }

    [Fact]
    public void RevisionDireccion_Devolver_ConObservacion()
    {
        var informe = AgregarInformeDireccion(IdDirector, EstadoPendiente);

        Decano().Devolver(informe.IdInformeDireccion, "Agregar actas de sesiones");

        Assert.Equal(EstadoDevuelto, informe.IdEstado);
        var observacion = Assert.Single(Db.Observaciones.ToList());
        Assert.Equal(informe.IdInformeDireccion, observacion.IdInformeDireccion);
        Assert.Equal("Devolución Decano", observacion.TipoObservacion);
    }

    [Fact]
    public void RevisionDireccion_Devolver_SinObservacion_NoSePermite()
    {
        var informe = AgregarInformeDireccion(IdDirector, EstadoPendiente);

        Decano().Devolver(informe.IdInformeDireccion, "");

        Assert.Equal(EstadoPendiente, informe.IdEstado);
    }

    [Fact]
    public void RevisionDireccion_Detalle_Inexistente_DevuelveNotFound()
    {
        Assert.IsType<NotFoundResult>(Decano().Details(999));
    }
}
