using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Controllers;
using SistemaFarmaciaG6.Models;
using SistemaFarmaciaG6.Tests.Infraestructura;

namespace SistemaFarmaciaG6.Tests.Pruebas;

/// <summary>
/// Inicio de sesión, cierre de sesión y recuperación de contraseña.
/// </summary>
public class AccountControllerTests : PruebaBase
{
    private readonly EmailServiceFalso _email = new();

    private AccountController Crear() =>
        Preparar(new AccountController(Db, _email));

    // ---------------------------- LOGIN ----------------------------

    [Fact]
    public void Login_CredencialesValidas_IniciaSesionYRedirigeAlInicio()
    {
        var controlador = Crear();

        var resultado = controlador.Login("docente@ucr.ac.cr", ClaveValida);

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redireccion.ActionName);
        Assert.Equal("Home", redireccion.ControllerName);

        var sesion = controlador.HttpContext.Session;
        Assert.Equal(IdDocente, sesion.GetInt32("IdUsuario"));
        Assert.Equal("Docente", sesion.GetString("Rol"));
        Assert.Equal("Diego Pérez Mora", sesion.GetString("Nombre"));
    }

    [Fact]
    public void Login_CredencialesValidas_RegistraAuditoria()
    {
        Crear().Login("docente@ucr.ac.cr", ClaveValida);

        var auditoria = Assert.Single(Db.Auditoria.ToList());
        Assert.Equal("Login", auditoria.Accion);
        Assert.Equal(IdDocente, auditoria.IdUsuario);
    }

    [Fact]
    public void Login_ContrasenaIncorrecta_MuestraError()
    {
        var controlador = Crear();

        var resultado = controlador.Login("docente@ucr.ac.cr", "otraClave");

        Assert.IsType<ViewResult>(resultado);
        Assert.Equal("Correo o contraseña incorrectos", (string?)controlador.ViewBag.Error);
        Assert.Null(controlador.HttpContext.Session.GetInt32("IdUsuario"));
    }

    [Fact]
    public void Login_CorreoNoRegistrado_MuestraError()
    {
        var controlador = Crear();

        var resultado = controlador.Login("noexiste@ucr.ac.cr", ClaveValida);

        Assert.IsType<ViewResult>(resultado);
        Assert.Equal("Correo o contraseña incorrectos", (string?)controlador.ViewBag.Error);
    }

    [Fact]
    public void Login_UsuarioInactivo_NoPermiteIngresar()
    {
        var controlador = Crear();

        var resultado = controlador.Login("inactivo@ucr.ac.cr", ClaveValida);

        Assert.IsType<ViewResult>(resultado);
        Assert.Contains("inactiva", (string)controlador.ViewBag.Error);
        Assert.Null(controlador.HttpContext.Session.GetInt32("IdUsuario"));
    }

    [Fact]
    public void Login_UsuarioSinRol_MuestraError()
    {
        var controlador = Crear();

        var resultado = controlador.Login("sinrol@ucr.ac.cr", ClaveValida);

        Assert.IsType<ViewResult>(resultado);
        Assert.Equal("El usuario no tiene un rol asignado", (string?)controlador.ViewBag.Error);
    }

    [Fact]
    public void Login_ContrasenaAntiguaEnTextoPlano_PermiteIngresarYLaConvierteAHash()
    {
        var usuario = Db.Usuarios.Find(IdDocente)!;
        usuario.Contrasena = "claveVieja";
        Db.SaveChanges();

        var resultado = Crear().Login("docente@ucr.ac.cr", "claveVieja");

        Assert.IsType<RedirectToActionResult>(resultado);
        Assert.NotEqual("claveVieja", usuario.Contrasena);
        var verificacion = new PasswordHasher<Usuario>()
            .VerifyHashedPassword(usuario, usuario.Contrasena, "claveVieja");
        Assert.Equal(PasswordVerificationResult.Success, verificacion);
    }

    [Fact]
    public void Logout_LimpiaLaSesionYRedirigeAlLogin()
    {
        var controlador = Preparar(new AccountController(Db, _email), "Docente", IdDocente);

        var resultado = controlador.Logout();

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Login", redireccion.ActionName);
        Assert.Empty(controlador.HttpContext.Session.Keys);
        Assert.Contains(Db.Auditoria.ToList(), a => a.Accion == "Logout" && a.IdUsuario == IdDocente);
    }

    // -------------------- RECUPERACIÓN DE CONTRASEÑA --------------------

    [Fact]
    public async Task OlvideContrasena_CorreoNoRegistrado_MensajeGenericoYNoEnviaCorreo()
    {
        var controlador = Crear();

        var resultado = await controlador.OlvideContrasena("nadie@ucr.ac.cr");

        Assert.IsType<ViewResult>(resultado);
        Assert.NotNull(controlador.TempData["Mensaje"]);
        Assert.Empty(_email.Enviados);
        Assert.Empty(Db.RecuperacionesContrasena.ToList());
    }

    [Fact]
    public async Task OlvideContrasena_CorreoRegistrado_GuardaCodigoYEnviaCorreo()
    {
        var resultado = await Crear().OlvideContrasena("  DOCENTE@ucr.ac.cr ");

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("VerificarCodigo", redireccion.ActionName);

        var correo = Assert.Single(_email.Enviados);
        Assert.Equal("docente@ucr.ac.cr", correo.Destinatario);
        Assert.Matches(@"\d{6}", correo.Cuerpo);

        var recuperacion = Assert.Single(Db.RecuperacionesContrasena.ToList());
        Assert.Equal(IdDocente, recuperacion.IdUsuario);
        Assert.False(recuperacion.Utilizado);
        Assert.Equal(0, recuperacion.Intentos);
        Assert.True(recuperacion.FechaExpiracion > DateTime.Now.AddMinutes(9));
        Assert.True(recuperacion.FechaExpiracion <= DateTime.Now.AddMinutes(10));
    }

    [Fact]
    public void VerificarCodigoGet_SinCorreo_RedirigeAOlvideContrasena()
    {
        var resultado = Crear().VerificarCodigo("");

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("OlvideContrasena", redireccion.ActionName);
    }

    [Fact]
    public async Task VerificarCodigo_CodigoIncorrecto_SumaUnIntento()
    {
        var recuperacion = AgregarRecuperacion("123456");
        var controlador = Crear();

        var resultado = await controlador.VerificarCodigo("docente@ucr.ac.cr", "999999");

        Assert.IsType<ViewResult>(resultado);
        Assert.Equal("Código incorrecto.", (string?)controlador.ViewBag.Error);
        Assert.Equal(1, recuperacion.Intentos);
    }

    [Fact]
    public async Task VerificarCodigo_CodigoVencido_MuestraError()
    {
        AgregarRecuperacion("123456", expira: DateTime.Now.AddMinutes(-1));
        var controlador = Crear();

        await controlador.VerificarCodigo("docente@ucr.ac.cr", "123456");

        Assert.Contains("vencido", (string)controlador.ViewBag.Error);
        Assert.Null(controlador.HttpContext.Session.GetInt32("IdUsuarioRecuperacion"));
    }

    [Fact]
    public async Task VerificarCodigo_CincoIntentosFallidos_BloqueaElCodigo()
    {
        AgregarRecuperacion("123456", intentos: 5);
        var controlador = Crear();

        // Aunque el código sea correcto, ya no se acepta
        await controlador.VerificarCodigo("docente@ucr.ac.cr", "123456");

        Assert.Contains("máximo de intentos", (string)controlador.ViewBag.Error);
        Assert.Null(controlador.HttpContext.Session.GetInt32("IdUsuarioRecuperacion"));
    }

    [Fact]
    public async Task VerificarCodigo_CodigoCorrecto_AutorizaElCambioDeContrasena()
    {
        var recuperacion = AgregarRecuperacion("123456");
        var controlador = Crear();

        var resultado = await controlador.VerificarCodigo("docente@ucr.ac.cr", "123456");

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("RestablecerContrasena", redireccion.ActionName);
        Assert.Equal(IdDocente, controlador.HttpContext.Session.GetInt32("IdUsuarioRecuperacion"));
        Assert.Equal(recuperacion.IdRecuperacion, controlador.HttpContext.Session.GetInt32("IdRecuperacion"));
    }

    [Fact]
    public void RestablecerContrasenaGet_SinVerificarCodigo_RedirigeAOlvideContrasena()
    {
        var resultado = Crear().RestablecerContrasena();

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("OlvideContrasena", redireccion.ActionName);
    }

    [Theory]
    [InlineData("", "", "Debe ingresar una nueva contraseña.")]
    [InlineData("corta", "corta", "La contraseña debe tener al menos 8 caracteres.")]
    [InlineData("NuevaClave1", "NuevaClave2", "Las contraseñas no coinciden.")]
    public async Task RestablecerContrasena_DatosInvalidos_MuestraError(
        string nueva, string confirmar, string errorEsperado)
    {
        var recuperacion = AgregarRecuperacion("123456");
        var controlador = Crear();
        controlador.HttpContext.Session.SetInt32("IdUsuarioRecuperacion", IdDocente);
        controlador.HttpContext.Session.SetInt32("IdRecuperacion", recuperacion.IdRecuperacion);

        var resultado = await controlador.RestablecerContrasena(nueva, confirmar);

        Assert.IsType<ViewResult>(resultado);
        Assert.Equal(errorEsperado, (string?)controlador.ViewBag.Error);
        Assert.False(recuperacion.Utilizado);
    }

    [Fact]
    public async Task FlujoCompleto_RecuperarContrasena_PermiteIngresarConLaNueva()
    {
        // 1. Solicita el código
        var controlador = Crear();
        await controlador.OlvideContrasena("docente@ucr.ac.cr");
        string codigo = Regex.Match(_email.Enviados.Single().Cuerpo, @"\d{6}").Value;

        // 2. Verifica el código (misma sesión)
        await controlador.VerificarCodigo("docente@ucr.ac.cr", codigo);

        // 3. Cambia la contraseña
        var resultado = await controlador.RestablecerContrasena("NuevaClave2026", "NuevaClave2026");

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Login", redireccion.ActionName);
        Assert.True(Db.RecuperacionesContrasena.Single().Utilizado);
        Assert.Null(controlador.HttpContext.Session.GetInt32("IdUsuarioRecuperacion"));

        // 4. La contraseña vieja ya no sirve y la nueva sí
        var login = Crear();
        Assert.IsType<ViewResult>(login.Login("docente@ucr.ac.cr", ClaveValida));
        Assert.IsType<RedirectToActionResult>(Crear().Login("docente@ucr.ac.cr", "NuevaClave2026"));
    }

    [Fact]
    public async Task CodigoDeRecuperacion_NoSePuedeReutilizar()
    {
        var controlador = Crear();
        await controlador.OlvideContrasena("docente@ucr.ac.cr");
        string codigo = Regex.Match(_email.Enviados.Single().Cuerpo, @"\d{6}").Value;
        await controlador.VerificarCodigo("docente@ucr.ac.cr", codigo);
        await controlador.RestablecerContrasena("NuevaClave2026", "NuevaClave2026");

        var otroIntento = Crear();
        var resultado = await otroIntento.VerificarCodigo("docente@ucr.ac.cr", codigo);

        Assert.IsType<ViewResult>(resultado);
        Assert.Equal("Código inválido o vencido.", (string?)otroIntento.ViewBag.Error);
    }

    private RecuperacionContrasena AgregarRecuperacion(string codigo, DateTime? expira = null, int intentos = 0)
    {
        var recuperacion = new RecuperacionContrasena
        {
            IdUsuario = IdDocente,
            FechaCreacion = DateTime.Now,
            FechaExpiracion = expira ?? DateTime.Now.AddMinutes(10),
            Utilizado = false,
            Intentos = intentos
        };
        recuperacion.CodigoHash = new PasswordHasher<RecuperacionContrasena>()
            .HashPassword(recuperacion, codigo);

        Db.RecuperacionesContrasena.Add(recuperacion);
        Db.SaveChanges();
        return recuperacion;
    }
}
