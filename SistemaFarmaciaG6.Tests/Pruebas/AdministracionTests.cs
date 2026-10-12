using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Controllers;
using SistemaFarmaciaG6.Models;
using SistemaFarmaciaG6.Tests.Infraestructura;

namespace SistemaFarmaciaG6.Tests.Pruebas;

/// <summary>
/// Módulos exclusivos del Administrador: usuarios, roles, categorías y auditoría.
/// </summary>
public class AdministracionTests : PruebaBase
{
    private static void VerificarRedirigeAlInicio(IActionResult resultado)
    {
        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redireccion.ActionName);
        Assert.Equal("Home", redireccion.ControllerName);
    }

    // -------------------- SEGURIDAD: SOLO ADMINISTRADOR --------------------

    [Theory]
    [InlineData(null)]
    [InlineData("Docente")]
    [InlineData("Director")]
    [InlineData("Decano")]
    [InlineData("Jefatura")]
    [InlineData("Visualizador")]
    public void ModulosDeAdministrador_UsuarioNoAdministrador_EsRedirigido(string? rol)
    {
        VerificarRedirigeAlInicio(Preparar(new UsuariosController(Db), rol, IdDocente).Index(null!, null!));
        VerificarRedirigeAlInicio(Preparar(new UsuariosController(Db), rol, IdDocente).Create());
        VerificarRedirigeAlInicio(Preparar(new RolesController(Db), rol, IdDocente).Index());
        VerificarRedirigeAlInicio(Preparar(new CategoriasController(Db), rol, IdDocente).Index());
        VerificarRedirigeAlInicio(Preparar(new AuditoriaController(Db), rol, IdDocente).Index());
        VerificarRedirigeAlInicio(Preparar(new AdminController(Db), rol, IdDocente).Dashboard());
    }

    [Fact]
    public void Usuarios_NoAdministradorNoPuedeCrearUsuarios()
    {
        var controlador = Preparar(new UsuariosController(Db), "Docente", IdDocente);
        int antes = Db.Usuarios.Count();

        var resultado = controlador.Create(UsuarioNuevo(), RolDocente);

        VerificarRedirigeAlInicio(resultado);
        Assert.Equal(antes, Db.Usuarios.Count());
    }

    // ------------------------------ USUARIOS ------------------------------

    [Fact]
    public void CrearUsuario_DatosValidos_GuardaUsuarioConRolYContrasenaEncriptada()
    {
        var controlador = Preparar(new UsuariosController(Db), "Administrador", IdAdmin);
        var nuevo = UsuarioNuevo();

        var resultado = controlador.Create(nuevo, RolDirector);

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redireccion.ActionName);

        var guardado = Db.Usuarios.Single(u => u.Cedula == "2-2222-2222");
        Assert.Equal("Activo", guardado.Estado);
        Assert.True(guardado.CambiarContrasena);
        Assert.NotEqual("ClaveInicial1", guardado.Contrasena);
        Assert.Equal(PasswordVerificationResult.Success,
            new PasswordHasher<Usuario>().VerifyHashedPassword(guardado, guardado.Contrasena, "ClaveInicial1"));

        var rol = Assert.Single(Db.UsuarioRols.ToList(), ur => ur.IdUsuario == guardado.IdUsuario);
        Assert.Equal(RolDirector, rol.IdRol);
        Assert.Contains(Db.Auditoria.ToList(), a => a.TablaAfectada == "Usuarios" && a.Accion == "Crear");
    }

    [Fact]
    public void CrearUsuario_CorreoQueNoEsUcr_SeRechaza()
    {
        var controlador = Preparar(new UsuariosController(Db), "Administrador", IdAdmin);
        var nuevo = UsuarioNuevo();
        nuevo.Correo = "persona@gmail.com";

        var resultado = controlador.Create(nuevo, RolDocente);

        Assert.IsType<ViewResult>(resultado);
        Assert.True(controlador.ModelState.ContainsKey("Correo"));
        Assert.DoesNotContain(Db.Usuarios.ToList(), u => u.Correo == "persona@gmail.com");
    }

    [Fact]
    public void CrearUsuario_CedulaRepetida_SeRechaza()
    {
        var controlador = Preparar(new UsuariosController(Db), "Administrador", IdAdmin);
        var nuevo = UsuarioNuevo();
        nuevo.Cedula = Db.Usuarios.Find(IdDocente)!.Cedula;

        var resultado = controlador.Create(nuevo, RolDocente);

        Assert.IsType<ViewResult>(resultado);
        Assert.Contains(controlador.ModelState["Cedula"]!.Errors,
            e => e.ErrorMessage == "Ya existe un usuario con esa cédula.");
    }

    [Fact]
    public void CrearUsuario_CorreoRepetido_SeRechaza()
    {
        var controlador = Preparar(new UsuariosController(Db), "Administrador", IdAdmin);
        var nuevo = UsuarioNuevo();
        nuevo.Correo = "docente@ucr.ac.cr";

        controlador.Create(nuevo, RolDocente);

        Assert.Contains(controlador.ModelState["Correo"]!.Errors,
            e => e.ErrorMessage == "Ya existe un usuario con ese correo.");
    }

    [Fact]
    public void CrearUsuario_FechaDeNacimientoFutura_SeRechaza()
    {
        var controlador = Preparar(new UsuariosController(Db), "Administrador", IdAdmin);
        var nuevo = UsuarioNuevo();
        nuevo.FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        var resultado = controlador.Create(nuevo, RolDocente);

        Assert.IsType<ViewResult>(resultado);
        Assert.True(controlador.ModelState.ContainsKey("FechaNacimiento"));
    }

    [Fact]
    public void CrearUsuario_CamposObligatoriosVacios_ReportaCadaError()
    {
        var controlador = Preparar(new UsuariosController(Db), "Administrador", IdAdmin);
        var vacio = new Usuario { Cedula = "", Nombre = "", Apellido1 = "", Correo = "", Contrasena = "" };

        var resultado = controlador.Create(vacio, idRol: 0);

        Assert.IsType<ViewResult>(resultado);
        foreach (var campo in new[] { "Cedula", "Nombre", "Apellido1", "Correo", "Contrasena",
                     "FechaNacimiento", "IdGenero", "IdDepartamento", "IdCategoria",
                     "IdTipoNombramiento", "idRol" })
        {
            Assert.True(controlador.ModelState.ContainsKey(campo), $"Falta validar el campo {campo}");
        }
        Assert.Equal("Revise los datos ingresados.", controlador.TempData["Error"]);
    }

    [Fact]
    public void ListarUsuarios_FiltraPorEstadoYNombre()
    {
        // Se usa un controlador por llamada: ViewResult.Model lee el ViewData del
        // controlador, así que una segunda llamada sobrescribiría el primer modelo.
        var inactivos = Assert.IsType<ViewResult>(
            Preparar(new UsuariosController(Db), "Administrador", IdAdmin).Index("Inactivo", null!));
        var porNombre = Assert.IsType<ViewResult>(
            Preparar(new UsuariosController(Db), "Administrador", IdAdmin).Index(null!, "Olga"));

        Assert.Equal(IdInactivo, Assert.Single((List<Usuario>)inactivos.Model!).IdUsuario);
        Assert.Equal(IdDocenteOtroDep, Assert.Single((List<Usuario>)porNombre.Model!).IdUsuario);
    }

    [Fact]
    public void EditarUsuario_DatosValidos_ActualizaDatosYRol()
    {
        var controlador = Preparar(new UsuariosController(Db), "Administrador", IdAdmin);
        var cambios = new Usuario
        {
            Apellido1 = "Solano",
            Apellido2 = "Rojas",
            FechaNacimiento = new DateOnly(1990, 1, 1),
            Telefono = "22223333",
            IdGenero = 1,
            IdDepartamento = DepQuimica,
            IdCategoria = CategoriaInstructor,
            IdTipoNombramiento = 1,
            Estado = "Inactivo"
        };

        var resultado = controlador.Edit(IdDocente, cambios, RolDirector);

        Assert.IsType<RedirectToActionResult>(resultado);
        var usuario = Db.Usuarios.Find(IdDocente)!;
        Assert.Equal("Solano", usuario.Apellido1);
        Assert.Equal(DepQuimica, usuario.IdDepartamento);
        Assert.Equal("Inactivo", usuario.Estado);
        Assert.Equal(RolDirector, Db.UsuarioRols.Single(ur => ur.IdUsuario == IdDocente).IdRol);
    }

    [Fact]
    public void EditarUsuario_Inexistente_DevuelveNotFound()
    {
        var controlador = Preparar(new UsuariosController(Db), "Administrador", IdAdmin);

        Assert.IsType<NotFoundResult>(controlador.Edit(999));
    }

    // -------------------------------- ROLES --------------------------------

    [Fact]
    public void CambiarRol_SinSeleccionarRol_MuestraError()
    {
        var controlador = Preparar(new RolesController(Db), "Administrador", IdAdmin);

        var resultado = controlador.Edit(IdDocente, idRol: 0);

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Edit", redireccion.ActionName);
        Assert.Equal("Debe seleccionar un rol.", controlador.TempData["Error"]);
        Assert.Equal(RolDocente, Db.UsuarioRols.Single(ur => ur.IdUsuario == IdDocente).IdRol);
    }

    [Fact]
    public void CambiarRol_UsuarioConRol_ActualizaElRolYAudita()
    {
        var controlador = Preparar(new RolesController(Db), "Administrador", IdAdmin);

        controlador.Edit(IdDocente, RolJefatura);

        Assert.Equal(RolJefatura, Db.UsuarioRols.Single(ur => ur.IdUsuario == IdDocente).IdRol);
        var auditoria = Db.Auditoria.Single(a => a.Accion == "Cambiar Rol");
        Assert.Contains("Rol anterior: Docente", auditoria.Descripcion);
        Assert.Contains("Rol nuevo: Jefatura", auditoria.Descripcion);
    }

    [Fact]
    public void CambiarRol_UsuarioSinRol_LeAsignaUnoNuevo()
    {
        var controlador = Preparar(new RolesController(Db), "Administrador", IdAdmin);

        controlador.Edit(IdSinRol, RolVisualizador);

        Assert.Equal(RolVisualizador, Db.UsuarioRols.Single(ur => ur.IdUsuario == IdSinRol).IdRol);
    }

    // ------------------------------ CATEGORÍAS ------------------------------

    [Fact]
    public void CrearCategoria_DatosValidos_SeGuarda()
    {
        var controlador = Preparar(new CategoriasController(Db), "Administrador", IdAdmin);

        var resultado = controlador.Create(new Categoria { NombreCategoria = "Interino", Estado = "Activo" });

        Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Contains(Db.Categorias.ToList(), c => c.NombreCategoria == "Interino");
        Assert.Equal("Categoría creada correctamente.", controlador.TempData["Exito"]);
    }

    [Fact]
    public void CrearCategoria_ModeloInvalido_NoSeGuarda()
    {
        var controlador = Preparar(new CategoriasController(Db), "Administrador", IdAdmin);
        controlador.ModelState.AddModelError("NombreCategoria", "Requerido");
        int antes = Db.Categorias.Count();

        var resultado = controlador.Create(new Categoria { NombreCategoria = "", Estado = "Activo" });

        Assert.IsType<ViewResult>(resultado);
        Assert.Equal(antes, Db.Categorias.Count());
    }

    [Fact]
    public void EditarCategoria_IdNoCoincide_DevuelveNotFound()
    {
        var controlador = Preparar(new CategoriasController(Db), "Administrador", IdAdmin);

        var resultado = controlador.Edit(CategoriaInstructor,
            new Categoria { IdCategoria = CategoriaSinUsuarios, NombreCategoria = "X", Estado = "Activo" });

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public void EditarCategoria_DatosValidos_SeActualiza()
    {
        var controlador = Preparar(new CategoriasController(Db), "Administrador", IdAdmin);

        controlador.Edit(CategoriaSinUsuarios,
            new Categoria { IdCategoria = CategoriaSinUsuarios, NombreCategoria = "Catedrático Pleno", Estado = "Inactivo" });

        var categoria = Db.Categorias.Find(CategoriaSinUsuarios)!;
        Assert.Equal("Catedrático Pleno", categoria.NombreCategoria);
        Assert.Equal("Inactivo", categoria.Estado);
    }

    [Fact]
    public void EliminarCategoria_ConUsuariosAsociados_NoSeElimina()
    {
        var controlador = Preparar(new CategoriasController(Db), "Administrador", IdAdmin);

        controlador.DeleteConfirmed(CategoriaInstructor);

        Assert.NotNull(Db.Categorias.Find(CategoriaInstructor));
        Assert.Contains("usuarios asociados", (string)controlador.TempData["Error"]!);
    }

    [Fact]
    public void EliminarCategoria_SinUsuarios_SeElimina()
    {
        var controlador = Preparar(new CategoriasController(Db), "Administrador", IdAdmin);

        controlador.DeleteConfirmed(CategoriaSinUsuarios);

        Assert.Null(Db.Categorias.Find(CategoriaSinUsuarios));
        Assert.Contains(Db.Auditoria.ToList(), a => a.TablaAfectada == "Categorias" && a.Accion == "Eliminar");
    }

    [Fact]
    public void DetalleCategoria_Inexistente_DevuelveNotFound()
    {
        var controlador = Preparar(new CategoriasController(Db), "Administrador", IdAdmin);

        Assert.IsType<NotFoundResult>(controlador.Details(999));
    }

    // ------------------------------ AUDITORÍA ------------------------------

    [Fact]
    public void Auditoria_Administrador_VeLosRegistrosDelMasRecienteAlMasAntiguo()
    {
        Db.Auditoria.AddRange(
            new Auditorium { IdUsuario = IdAdmin, TablaAfectada = "T", Accion = "Viejo", Fecha = DateTime.Now.AddDays(-1) },
            new Auditorium { IdUsuario = IdAdmin, TablaAfectada = "T", Accion = "Nuevo", Fecha = DateTime.Now });
        Db.SaveChanges();

        var vista = Assert.IsType<ViewResult>(Preparar(new AuditoriaController(Db), "Administrador", IdAdmin).Index());

        var registros = Assert.IsAssignableFrom<List<Auditorium>>(vista.Model);
        Assert.Equal(new[] { "Nuevo", "Viejo" }, registros.Select(r => r.Accion));
    }

    private static Usuario UsuarioNuevo() => new()
    {
        Cedula = "2-2222-2222",
        Nombre = "Laura",
        Apellido1 = "Vargas",
        Apellido2 = "Soto",
        FechaNacimiento = new DateOnly(1992, 3, 15),
        Correo = "laura.vargas@ucr.ac.cr",
        Contrasena = "ClaveInicial1",
        Telefono = "88880000",
        IdGenero = 1,
        IdDepartamento = DepFarmacia,
        IdCategoria = CategoriaInstructor,
        IdTipoNombramiento = 1
    };
}
