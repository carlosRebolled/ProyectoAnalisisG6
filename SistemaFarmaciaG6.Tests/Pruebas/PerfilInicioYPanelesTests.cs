using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using SistemaFarmaciaG6.Controllers;
using SistemaFarmaciaG6.Helpers;
using SistemaFarmaciaG6.Models;
using SistemaFarmaciaG6.Tests.Infraestructura;

namespace SistemaFarmaciaG6.Tests.Pruebas;

/// <summary>
/// Perfil del usuario, página de inicio, paneles (dashboards) y helpers.
/// </summary>
public class PerfilInicioYPanelesTests : PruebaBase
{
    // ------------------------------ INICIO ------------------------------

    [Fact]
    public void Inicio_SinSesion_RedirigeAlLogin()
    {
        var controlador = Preparar(new HomeController(NullLogger<HomeController>.Instance));

        var redireccion = Assert.IsType<RedirectToActionResult>(controlador.Index());

        Assert.Equal("Login", redireccion.ActionName);
    }

    [Fact]
    public void Inicio_ConSesion_MuestraNombreYRol()
    {
        var controlador = Preparar(new HomeController(NullLogger<HomeController>.Instance), "Decano", IdDecano);
        controlador.HttpContext.Session.SetString("Nombre", "Elena Pérez");

        Assert.IsType<ViewResult>(controlador.Index());
        Assert.Equal("Decano", (string?)controlador.ViewBag.Rol);
        Assert.Equal("Elena Pérez", (string?)controlador.ViewBag.Nombre);
    }

    // ------------------------------ PERFIL ------------------------------

    [Fact]
    public void Perfil_SinSesion_RedirigeAlLogin()
    {
        var resultado = Preparar(new PerfilController(Db)).Index();

        Assert.Equal("Login", Assert.IsType<RedirectToActionResult>(resultado).ActionName);
    }

    [Fact]
    public void Perfil_MuestraLosDatosDelUsuarioEnSesion()
    {
        var vista = Assert.IsType<ViewResult>(Preparar(new PerfilController(Db), "Docente", IdDocente).Index());

        Assert.Equal(IdDocente, Assert.IsType<Usuario>(vista.Model).IdUsuario);
    }

    [Fact]
    public void Perfil_Editar_DatosValidos_SeGuardan()
    {
        var controlador = Preparar(new PerfilController(Db), "Docente", IdDocente);

        var resultado = controlador.Edit(new Usuario { Telefono = "60606060", FechaNacimiento = new DateOnly(1980, 12, 1) });

        Assert.IsType<RedirectToActionResult>(resultado);
        var usuario = Db.Usuarios.Find(IdDocente)!;
        Assert.Equal("60606060", usuario.Telefono);
        Assert.Equal(new DateOnly(1980, 12, 1), usuario.FechaNacimiento);
        Assert.Contains(Db.Auditoria.ToList(), a => a.Accion == "Editar Perfil");
    }

    [Fact]
    public void Perfil_Editar_TelefonoCorto_SeRechaza()
    {
        var controlador = Preparar(new PerfilController(Db), "Docente", IdDocente);

        var resultado = controlador.Edit(new Usuario { Telefono = "1234", FechaNacimiento = new DateOnly(1980, 1, 1) });

        Assert.IsType<ViewResult>(resultado);
        Assert.True(controlador.ModelState.ContainsKey("Telefono"));
        Assert.Equal("88887777", Db.Usuarios.Find(IdDocente)!.Telefono);
    }

    [Fact]
    public void Perfil_Editar_FechaFutura_SeRechaza()
    {
        var controlador = Preparar(new PerfilController(Db), "Docente", IdDocente);

        controlador.Edit(new Usuario { FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddYears(1)) });

        Assert.True(controlador.ModelState.ContainsKey("FechaNacimiento"));
        Assert.Equal(new DateOnly(1985, 5, 20), Db.Usuarios.Find(IdDocente)!.FechaNacimiento);
    }

    [Fact]
    public void Perfil_Editar_SoloModificaAlUsuarioDeLaSesion()
    {
        var controlador = Preparar(new PerfilController(Db), "Docente", IdDocente);

        // Aunque se envíe otro IdUsuario en el formulario, se usa el de la sesión
        controlador.Edit(new Usuario { IdUsuario = IdAdmin, Telefono = "70707070", FechaNacimiento = new DateOnly(1980, 1, 1) });

        Assert.Equal("70707070", Db.Usuarios.Find(IdDocente)!.Telefono);
        Assert.Equal("88887777", Db.Usuarios.Find(IdAdmin)!.Telefono);
    }

    // ------------------------------ PANELES ------------------------------

    [Fact]
    public void PanelAdministrador_CuentaUsuariosEInformes()
    {
        AgregarInformeDocente(IdDocente, EstadoBorrador);
        AgregarInformeDocente(IdDocenteOtroDep, EstadoPendiente);
        AgregarInformeDocente(IdDirector, EstadoAprobado);
        var controlador = Preparar(new AdminController(Db), "Administrador", IdAdmin);

        Assert.IsType<ViewResult>(controlador.Dashboard());

        Assert.Equal(7, (int)controlador.ViewBag.TotalUsuarios);
        Assert.Equal(6, (int)controlador.ViewBag.UsuariosActivos);
        Assert.Equal(1, (int)controlador.ViewBag.UsuariosInactivos);
        Assert.Equal(2, (int)controlador.ViewBag.TotalDocentes);  // el inactivo no cuenta
        Assert.Equal(1, (int)controlador.ViewBag.TotalDirectores);
        Assert.Equal(3, (int)controlador.ViewBag.TotalInformes);
        Assert.Equal(1, (int)controlador.ViewBag.InformesBorrador);
        Assert.Equal(1, (int)controlador.ViewBag.InformesPendientes);
        Assert.Equal(1, (int)controlador.ViewBag.InformesAprobados);
    }

    [Fact]
    public void PanelDocente_SoloCuentaSusInformes()
    {
        AgregarInformeDocente(IdDocente, EstadoDevuelto);
        AgregarInformeDocente(IdDocente, EstadoAprobado, AnioActual - 1);
        AgregarInformeDocente(IdDocenteOtroDep, EstadoPendiente);
        var controlador = Preparar(new DocenteController(Db), "Docente", IdDocente);

        controlador.Dashboard();

        Assert.Equal(2, (int)controlador.ViewBag.TotalInformes);
        Assert.Equal(1, (int)controlador.ViewBag.Devueltos);
        Assert.Equal(1, (int)controlador.ViewBag.Aprobados);
        Assert.Equal(0, (int)controlador.ViewBag.Pendientes);
    }

    [Fact]
    public void PanelDirector_SoloCuentaInformesDeSuDepartamento()
    {
        AgregarInformeDocente(IdDocente, EstadoPendiente);
        AgregarInformeDocente(IdDocenteOtroDep, EstadoPendiente);
        var controlador = Preparar(new DirectorController(Db), "Director", IdDirector);

        controlador.Dashboard();

        Assert.Equal(1, (int)controlador.ViewBag.Total);
        Assert.Equal(1, (int)controlador.ViewBag.Pendientes);
    }

    [Theory]
    [InlineData("Administrador")]
    [InlineData("Docente")]
    public void Paneles_RolEquivocado_EsRedirigido(string rol)
    {
        Assert.IsType<RedirectToActionResult>(Preparar(new DecanoController(Db), rol, IdAdmin).Dashboard());
        Assert.IsType<RedirectToActionResult>(Preparar(new DirectorController(Db), rol, IdAdmin).Dashboard());
    }

    [Fact]
    public void PanelDocente_RolDirector_EsRedirigido()
    {
        Assert.IsType<RedirectToActionResult>(Preparar(new DocenteController(Db), "Director", IdDirector).Dashboard());
    }

    // ------------------------------ HELPERS ------------------------------

    [Theory]
    [InlineData("Administrador", true)]
    [InlineData("Docente", false)]
    [InlineData("administrador", false)]
    [InlineData(null, false)]
    public void SeguridadHelper_EsAdministrador(string? rol, bool esperado)
    {
        var contexto = new DefaultHttpContext { Session = new SesionFalsa() };
        if (rol != null) contexto.Session.SetString("Rol", rol);

        Assert.Equal(esperado, SeguridadHelper.EsAdministrador(contexto));
    }

    [Fact]
    public void AuditoriaHelper_ConUsuarioEnSesion_GuardaElRegistro()
    {
        var contexto = new DefaultHttpContext { Session = new SesionFalsa() };
        contexto.Session.SetInt32("IdUsuario", IdAdmin);

        AuditoriaHelper.Registrar(Db, contexto, "Usuarios", "Crear", "Prueba");

        var registro = Assert.Single(Db.Auditoria.ToList());
        Assert.Equal(IdAdmin, registro.IdUsuario);
        Assert.Equal("Usuarios", registro.TablaAfectada);
        Assert.Equal("Crear", registro.Accion);
        Assert.Equal("Prueba", registro.Descripcion);
        Assert.True((DateTime.Now - registro.Fecha).TotalMinutes < 1);
    }

    [Fact]
    public void AuditoriaHelper_SinUsuarioEnSesion_NoGuardaNada()
    {
        var contexto = new DefaultHttpContext { Session = new SesionFalsa() };

        AuditoriaHelper.Registrar(Db, contexto, "Usuarios", "Crear", "Prueba");

        Assert.Empty(Db.Auditoria.ToList());
    }
}
