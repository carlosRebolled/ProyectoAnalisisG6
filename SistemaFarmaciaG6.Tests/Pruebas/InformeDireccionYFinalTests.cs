using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Controllers;
using SistemaFarmaciaG6.Models;
using SistemaFarmaciaG6.Tests.Infraestructura;

namespace SistemaFarmaciaG6.Tests.Pruebas;

/// <summary>
/// Informe de Dirección (Director) e Informe Final de Facultad (Decano).
/// </summary>
public class InformeDireccionYFinalTests : PruebaBase
{
    private InformeDireccionController Direccion(string rol = "Director", int id = IdDirector) =>
        Preparar(new InformeDireccionController(Db), rol, id);

    private InformeFinalFacultadController Final(string rol = "Decano", int id = IdDecano) =>
        Preparar(new InformeFinalFacultadController(Db), rol, id);

    // ------------------------- INFORME DE DIRECCIÓN -------------------------

    [Fact]
    public void Direccion_Docente_NoTieneAcceso()
    {
        var resultado = Direccion("Docente", IdDocente).Index();

        Assert.Equal("Home", Assert.IsType<RedirectToActionResult>(resultado).ControllerName);
    }

    [Fact]
    public void Direccion_Index_DirectorSoloVeLosSuyos()
    {
        AgregarInformeDireccion(IdDirector, EstadoBorrador);
        AgregarInformeDireccion(IdAdmin, EstadoBorrador);

        var vista = Assert.IsType<ViewResult>(Direccion().Index());

        var informe = Assert.Single(Assert.IsAssignableFrom<List<InformeDireccion>>(vista.Model));
        Assert.Equal(IdDirector, informe.IdUsuario);
    }

    [Fact]
    public void Direccion_Crear_GeneraBorradorDelAnio()
    {
        var resultado = Direccion().Create();

        Assert.Equal("Edit", Assert.IsType<RedirectToActionResult>(resultado).ActionName);
        var informe = Assert.Single(Db.InformeDireccions.ToList());
        Assert.Equal(EstadoBorrador, informe.IdEstado);
        Assert.Equal(AnioActual, informe.Anio);
        Assert.Equal(IdDirector, informe.IdUsuario);
    }

    [Fact]
    public void Direccion_Crear_SoloUnoPorAnio()
    {
        AgregarInformeDireccion(IdDirector, EstadoBorrador);
        var controlador = Direccion();

        controlador.Create();

        Assert.Single(Db.InformeDireccions.ToList());
        Assert.Equal($"Ya existe un informe de dirección para el año {AnioActual}.", controlador.TempData["Error"]);
    }

    [Fact]
    public void Direccion_Enviar_PasaAPendiente()
    {
        var informe = AgregarInformeDireccion(IdDirector, EstadoBorrador);

        Direccion().EnviarConfirmado(informe.IdInformeDireccion);

        Assert.Equal(EstadoPendiente, informe.IdEstado);
        Assert.NotNull(informe.FechaEnvio);
    }

    [Fact]
    public void Direccion_EnviarInformeDeOtroDirector_NoSePermite()
    {
        var informe = AgregarInformeDireccion(IdAdmin, EstadoBorrador);
        var controlador = Direccion();

        controlador.EnviarConfirmado(informe.IdInformeDireccion);

        Assert.Equal(EstadoBorrador, informe.IdEstado);
        Assert.Equal("No puede enviar informes de otro director.", controlador.TempData["Error"]);
    }

    [Fact]
    public void Direccion_Eliminar_Borrador_SeElimina()
    {
        var informe = AgregarInformeDireccion(IdDirector, EstadoBorrador);

        Direccion().DeleteConfirmed(informe.IdInformeDireccion);

        Assert.Empty(Db.InformeDireccions.ToList());
    }

    [Theory]
    [InlineData(EstadoPendiente)]
    [InlineData(EstadoDevuelto)]
    [InlineData(EstadoAprobado)]
    public void Direccion_Eliminar_SoloSePermiteEnBorrador(int estado)
    {
        var informe = AgregarInformeDireccion(IdDirector, estado);
        var controlador = Direccion();

        controlador.DeleteConfirmed(informe.IdInformeDireccion);

        Assert.Single(Db.InformeDireccions.ToList());
        Assert.Equal("Solo se pueden eliminar informes en estado Borrador.", controlador.TempData["Error"]);
    }

    [Fact]
    public void Direccion_Reabrir_SoloAdministrador()
    {
        var informe = AgregarInformeDireccion(IdDirector, EstadoAprobado);

        Direccion().Reabrir(informe.IdInformeDireccion);
        Assert.Equal(EstadoAprobado, informe.IdEstado);

        Direccion("Administrador", IdAdmin).Reabrir(informe.IdInformeDireccion);
        Assert.Equal(EstadoBorrador, informe.IdEstado);
        Assert.Null(informe.FechaAprobacion);
    }

    // ------------------------- INFORME FINAL DE FACULTAD -------------------------

    [Theory]
    [InlineData("Docente")]
    [InlineData("Director")]
    public void Final_RolSinPermiso_EsRedirigido(string rol)
    {
        var resultado = Final(rol, IdDocente).Index();

        Assert.Equal("Home", Assert.IsType<RedirectToActionResult>(resultado).ControllerName);
    }

    [Fact]
    public void Final_Crear_IncluyeSoloInformesDeDireccionAprobadosDelAnio()
    {
        AgregarInformeDireccion(IdDirector, EstadoAprobado);
        AgregarInformeDireccion(IdAdmin, EstadoPendiente);                    // no aprobado
        AgregarInformeDireccion(IdDecano, EstadoAprobado, AnioActual - 1);   // otro año

        var resultado = Final().Create(new InformeFinalFacultad { Observaciones = "Informe anual" });

        Assert.IsType<RedirectToActionResult>(resultado);
        var informe = Assert.Single(Db.InformeFinalFacultads.ToList());
        Assert.Equal(EstadoBorrador, informe.IdEstado);
        Assert.Equal(IdDecano, informe.IdUsuarioGenera);

        var detalle = Assert.Single(Db.DetalleInformeFinals.ToList());
        Assert.Equal(informe.IdInformeFinal, detalle.IdInformeFinal);
        Assert.Contains("Daniel Pérez", detalle.DetalleActividad);
    }

    [Fact]
    public void Final_Crear_SoloUnoPorAnio()
    {
        AgregarInformeFinal(EstadoBorrador);
        var controlador = Final();

        controlador.Create(new InformeFinalFacultad());

        Assert.Single(Db.InformeFinalFacultads.ToList());
        Assert.Equal($"Ya existe un informe final para el año {AnioActual}.", controlador.TempData["Error"]);
    }

    [Fact]
    public void Final_Editar_BorradorActualizaObservaciones()
    {
        var informe = AgregarInformeFinal(EstadoBorrador);

        Final().Edit(informe.IdInformeFinal, new InformeFinalFacultad { Observaciones = "Revisado" });

        Assert.Equal("Revisado", informe.Observaciones);
    }

    [Fact]
    public void Final_Editar_Finalizado_NoSePermite()
    {
        var informe = AgregarInformeFinal(EstadoFinalizado);

        Final().Edit(informe.IdInformeFinal, new InformeFinalFacultad { Observaciones = "Cambio" });

        Assert.Null(informe.Observaciones);
    }

    [Fact]
    public void Final_Finalizar_Borrador_QuedaFinalizado()
    {
        var informe = AgregarInformeFinal(EstadoBorrador);

        Final().Finalizar(informe.IdInformeFinal);

        Assert.Equal(EstadoFinalizado, informe.IdEstado);
        Assert.NotNull(informe.FechaAprobacion);
    }

    [Fact]
    public void Final_Finalizar_YaFinalizado_MuestraError()
    {
        var informe = AgregarInformeFinal(EstadoFinalizado);
        var controlador = Final();

        controlador.Finalizar(informe.IdInformeFinal);

        Assert.Equal("Solo se pueden finalizar informes en estado Borrador.", controlador.TempData["Error"]);
    }

    [Fact]
    public void Final_Reabrir_DecanoNoPuede_AdministradorSi()
    {
        var informe = AgregarInformeFinal(EstadoFinalizado);

        Final().Reabrir(informe.IdInformeFinal);
        Assert.Equal(EstadoFinalizado, informe.IdEstado);

        Final("Administrador", IdAdmin).Reabrir(informe.IdInformeFinal);
        Assert.Equal(EstadoBorrador, informe.IdEstado);
        Assert.Null(informe.FechaAprobacion);
    }

    [Fact]
    public void Final_Reabrir_InformeEnBorrador_MuestraError()
    {
        var informe = AgregarInformeFinal(EstadoBorrador);
        var controlador = Final("Administrador", IdAdmin);

        controlador.Reabrir(informe.IdInformeFinal);

        Assert.Equal("Solo se pueden reabrir informes finalizados.", controlador.TempData["Error"]);
    }
}
