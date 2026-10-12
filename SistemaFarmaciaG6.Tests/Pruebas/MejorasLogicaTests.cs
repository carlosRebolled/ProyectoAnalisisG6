using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaFarmaciaG6.Controllers;
using SistemaFarmaciaG6.Models;
using SistemaFarmaciaG6.Tests.Infraestructura;

namespace SistemaFarmaciaG6.Tests.Pruebas;

/// <summary>
/// Reglas de negocio agregadas en la revisión de lógica:
/// validaciones antes de enviar, permisos de solo lectura, protección
/// para que el administrador no se bloquee a sí mismo, etc.
/// </summary>
public class MejorasLogicaTests : PruebaBase
{
    // ---------------- Informe docente: no se envía incompleto ----------------

    [Fact]
    public void InformeDocente_EnviarIncompleto_NoPasaAPendiente()
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoBorrador);   // vacío
        var controlador = Preparar(new InformeDocenteController(Db), "Docente", IdDocente);

        var resultado = controlador.EnviarConfirmado(informe.IdInformeDocente);

        Assert.Equal("Edit", Assert.IsType<RedirectToActionResult>(resultado).ActionName);
        Assert.Equal(EstadoBorrador, informe.IdEstado);
        Assert.Null(informe.FechaEnvio);
        Assert.Contains("sin completar", (string)controlador.TempData["Error"]!);
    }

    [Fact]
    public void InformeDocente_PantallaEnviarIncompleto_RedirigeAEditar()
    {
        var informe = AgregarInformeDocente(IdDocente, EstadoBorrador);

        var resultado = Preparar(new InformeDocenteController(Db), "Docente", IdDocente)
            .Enviar(informe.IdInformeDocente);

        Assert.Equal("Edit", Assert.IsType<RedirectToActionResult>(resultado).ActionName);
    }

    [Fact]
    public void InformeDocente_FormularioIncompleto_DetectaCampoFaltante()
    {
        var completo = InformeDocenteCompleto();
        Assert.False(InformeDocenteController.FormularioIncompleto(completo));

        completo.DetallePosgrado = " ";
        Assert.True(InformeDocenteController.FormularioIncompleto(completo));
    }

    // ---------------- Informe de dirección ----------------

    [Fact]
    public void InformeDireccion_CrearSinCursos_NoDejaUnInformeVacio()
    {
        var director = Preparar(new InformeDireccionController(Db), "Director", IdDirector);

        var resultado = CrearDireccion(director, cursos: new[] { 0 });

        Assert.Equal("Create", Assert.IsType<RedirectToActionResult>(resultado).ActionName);
        Assert.Equal("Debe agregar al menos un curso.", director.TempData["Error"]);
        Assert.Empty(Db.InformeDireccions.ToList());
    }

    [Fact]
    public void InformeDireccion_DespuesDeUnErrorSePuedeCrearCorrectamente()
    {
        CrearDireccion(Preparar(new InformeDireccionController(Db), "Director", IdDirector), cursos: Array.Empty<int>());

        var resultado = CrearDireccion(
            Preparar(new InformeDireccionController(Db), "Director", IdDirector),
            cursos: new[] { IdCursoEjemplo }, sesiones: 1);

        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultado).ActionName);
        var informe = Assert.Single(Db.InformeDireccions.ToList());
        Assert.Equal(EstadoBorrador, informe.IdEstado);
        Assert.Single(Db.CursosDireccions.ToList(), c => c.IdInformeDireccion == informe.IdInformeDireccion);
        Assert.Single(Db.SesionesDepartamentos.ToList(), s => s.IdInformeDireccion == informe.IdInformeDireccion);
    }

    [Fact]
    public void InformeDireccion_EnviarSinCursos_NoSePermite()
    {
        var informe = AgregarInformeDireccion(IdDirector, EstadoBorrador);
        var director = Preparar(new InformeDireccionController(Db), "Director", IdDirector);

        var resultado = director.EnviarConfirmado(informe.IdInformeDireccion);

        Assert.Equal("Edit", Assert.IsType<RedirectToActionResult>(resultado).ActionName);
        Assert.Equal(EstadoBorrador, informe.IdEstado);
        Assert.Equal("Debe registrar al menos un curso antes de enviar el informe.", director.TempData["Error"]);
    }

    // ---------------- Informe final: Visualizador solo consulta ----------------

    [Fact]
    public void InformeFinal_Visualizador_PuedeConsultar()
    {
        AgregarInformeFinal(EstadoFinalizado);

        var resultado = Preparar(new InformeFinalFacultadController(Db), "Visualizador", IdDocente).Index();

        Assert.IsType<ViewResult>(resultado);
    }

    [Fact]
    public void InformeFinal_Visualizador_NoPuedeCrearEditarNiFinalizar()
    {
        var informe = AgregarInformeFinal(EstadoBorrador, AnioActual - 1);
        InformeFinalFacultadController Visualizador() =>
            Preparar(new InformeFinalFacultadController(Db), "Visualizador", IdDocente);

        Assert.IsType<RedirectToActionResult>(Visualizador().Create());
        Visualizador().Create(new InformeFinalFacultad());
        Visualizador().Edit(informe.IdInformeFinal, new InformeFinalFacultad { Observaciones = "Cambio" });
        Visualizador().Finalizar(informe.IdInformeFinal);

        Assert.Single(Db.InformeFinalFacultads.ToList());
        Assert.Null(informe.Observaciones);
        Assert.Equal(EstadoBorrador, informe.IdEstado);
    }

    [Fact]
    public void InformeFinal_Jefatura_SiPuedeFinalizar()
    {
        var informe = AgregarInformeFinal(EstadoBorrador);

        Preparar(new InformeFinalFacultadController(Db), "Jefatura", IdDocente).Finalizar(informe.IdInformeFinal);

        Assert.Equal(EstadoFinalizado, informe.IdEstado);
    }

    // ---------------- Revisión: solo docentes activos ----------------

    [Fact]
    public void Revision_Director_SoloVeDocentesActivosDeSuDepartamento()
    {
        var vista = Assert.IsType<ViewResult>(
            Preparar(new RevisionInformesController(Db), "Director", IdDirector).Index());

        var usuarios = Assert.IsAssignableFrom<List<Usuario>>(vista.Model);
        Assert.Equal(new[] { IdDocente }, usuarios.Select(u => u.IdUsuario));
    }

    [Fact]
    public void Revision_Administrador_VeDocentesDeTodosLosDepartamentos()
    {
        var vista = Assert.IsType<ViewResult>(
            Preparar(new RevisionInformesController(Db), "Administrador", IdAdmin).Index());

        var ids = Assert.IsAssignableFrom<List<Usuario>>(vista.Model).Select(u => u.IdUsuario).OrderBy(i => i);
        Assert.Equal(new[] { IdDocente, IdDocenteOtroDep }, ids);
    }

    // ---------------- Administrador no se bloquea a sí mismo ----------------

    [Fact]
    public void Usuarios_AdministradorNoPuedeDesactivarSuPropiaCuenta()
    {
        var controlador = Preparar(new UsuariosController(Db), "Administrador", IdAdmin);
        var cambios = CambiosDeUsuario(estado: "Inactivo");

        var resultado = controlador.Edit(IdAdmin, cambios, RolAdministrador);

        Assert.IsType<ViewResult>(resultado);
        Assert.True(controlador.ModelState.ContainsKey("Estado"));
        Assert.Equal("Activo", Db.Usuarios.Find(IdAdmin)!.Estado);
    }

    [Fact]
    public void Usuarios_AdministradorNoPuedeQuitarseSuRol()
    {
        var controlador = Preparar(new UsuariosController(Db), "Administrador", IdAdmin);

        controlador.Edit(IdAdmin, CambiosDeUsuario(), RolDocente);

        Assert.True(controlador.ModelState.ContainsKey("idRol"));
        Assert.Equal(RolAdministrador, Db.UsuarioRols.Single(ur => ur.IdUsuario == IdAdmin).IdRol);
    }

    [Fact]
    public void Usuarios_RolInexistente_SeRechaza()
    {
        var controlador = Preparar(new UsuariosController(Db), "Administrador", IdAdmin);

        controlador.Edit(IdDocente, CambiosDeUsuario(), idRol: 99);

        Assert.Contains(controlador.ModelState["idRol"]!.Errors, e => e.ErrorMessage == "El rol seleccionado no existe.");
        Assert.Equal(RolDocente, Db.UsuarioRols.Single(ur => ur.IdUsuario == IdDocente).IdRol);
    }

    [Fact]
    public void Roles_AdministradorNoPuedeQuitarseSuRol()
    {
        var controlador = Preparar(new RolesController(Db), "Administrador", IdAdmin);

        controlador.Edit(IdAdmin, RolVisualizador);

        Assert.Equal("No puede quitarse a sí mismo el rol de Administrador.", controlador.TempData["Error"]);
        Assert.Equal(RolAdministrador, Db.UsuarioRols.Single(ur => ur.IdUsuario == IdAdmin).IdRol);
    }

    [Fact]
    public void Roles_RolInexistente_SeRechaza()
    {
        var controlador = Preparar(new RolesController(Db), "Administrador", IdAdmin);

        controlador.Edit(IdDocente, 99);

        Assert.Equal("El rol seleccionado no existe.", controlador.TempData["Error"]);
        Assert.Equal(RolDocente, Db.UsuarioRols.Single(ur => ur.IdUsuario == IdDocente).IdRol);
    }

    // ---------------- Correos sin importar mayúsculas ----------------

    [Fact]
    public void Login_CorreoConMayusculasYEspacios_PermiteIngresar()
    {
        var controlador = Preparar(new AccountController(Db, new EmailServiceFalso()));

        var resultado = controlador.Login("  Docente@UCR.ac.cr ", ClaveValida);

        Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(IdDocente, controlador.HttpContext.Session.GetInt32("IdUsuario"));
    }

    [Fact]
    public void Login_GuardaLaFechaDelUltimoInicioDeSesion()
    {
        Preparar(new AccountController(Db, new EmailServiceFalso())).Login("docente@ucr.ac.cr", ClaveValida);

        var ultimo = Db.Usuarios.Find(IdDocente)!.UltimoInicioSesion;
        Assert.NotNull(ultimo);
        Assert.True((DateTime.Now - ultimo!.Value).TotalMinutes < 1);
    }

    [Fact]
    public void CrearUsuario_CorreoSeGuardaEnMinusculas()
    {
        var controlador = Preparar(new UsuariosController(Db), "Administrador", IdAdmin);

        controlador.Create(UsuarioNuevo(correo: "  Laura.Vargas@UCR.ac.cr "), RolDocente);

        Assert.Contains(Db.Usuarios.ToList(), u => u.Correo == "laura.vargas@ucr.ac.cr");
    }

    [Fact]
    public void CrearUsuario_CorreoRepetidoConOtrasMayusculas_SeRechaza()
    {
        var controlador = Preparar(new UsuariosController(Db), "Administrador", IdAdmin);

        controlador.Create(UsuarioNuevo(correo: "DOCENTE@ucr.ac.cr"), RolDocente);

        Assert.Contains(controlador.ModelState["Correo"]!.Errors,
            e => e.ErrorMessage == "Ya existe un usuario con ese correo.");
    }

    // ---------------- Categorías sin nombres repetidos ----------------

    [Fact]
    public void Categoria_NombreRepetido_SeRechaza()
    {
        var controlador = Preparar(new CategoriasController(Db), "Administrador", IdAdmin);
        int antes = Db.Categorias.Count();

        var resultado = controlador.Create(new Categoria { NombreCategoria = "  instructor ", Estado = "Activo" });

        Assert.IsType<ViewResult>(resultado);
        Assert.Equal(antes, Db.Categorias.Count());
    }

    [Fact]
    public void Categoria_EditarConNombreDeOtra_SeRechaza()
    {
        var controlador = Preparar(new CategoriasController(Db), "Administrador", IdAdmin);

        controlador.Edit(CategoriaSinUsuarios,
            new Categoria { IdCategoria = CategoriaSinUsuarios, NombreCategoria = "Instructor", Estado = "Activo" });

        Assert.Equal("Catedrático", Db.Categorias.Find(CategoriaSinUsuarios)!.NombreCategoria);
    }

    [Fact]
    public void Categoria_EditarConservandoSuNombre_SePermite()
    {
        var controlador = Preparar(new CategoriasController(Db), "Administrador", IdAdmin);

        controlador.Edit(CategoriaSinUsuarios,
            new Categoria { IdCategoria = CategoriaSinUsuarios, NombreCategoria = "Catedrático", Estado = "Inactivo" });

        Assert.Equal("Inactivo", Db.Categorias.Find(CategoriaSinUsuarios)!.Estado);
    }

    // ---------------- Auxiliares ----------------

    private static IActionResult CrearDireccion(InformeDireccionController controlador, int[] cursos, int sesiones = 0)
    {
        int n = cursos.Length;
        var cantidades = new int?[n];
        var detalles = new string?[n];

        return controlador.Create(
            new InformeDireccion(),
            Enumerable.Repeat("Sesión 1", sesiones).ToArray(),
            Enumerable.Repeat(DateTime.Today, sesiones).ToArray(),
            Enumerable.Repeat("Puntos vistos", sesiones).ToArray(),
            cursos,
            cantidades, detalles,   // Coordinación
            cantidades, detalles,   // Colaboradores
            cantidades, detalles,   // Invitados
            cantidades, detalles,   // Experiencias prácticas
            cantidades, detalles,   // Actividades docencia integradas
            cantidades, detalles,   // Actividades análisis de contexto
            cantidades, detalles,   // Técnicas didácticas
            cantidades);            // Asistentes
    }

    private static Usuario CambiosDeUsuario(string estado = "Activo") => new()
    {
        Apellido1 = "Pérez",
        Apellido2 = "Mora",
        FechaNacimiento = new DateOnly(1985, 5, 20),
        Telefono = "88887777",
        IdGenero = 1,
        IdDepartamento = DepFarmacia,
        IdCategoria = CategoriaInstructor,
        IdTipoNombramiento = 1,
        Estado = estado
    };

    private static Usuario UsuarioNuevo(string correo) => new()
    {
        Cedula = "3-3333-3333",
        Nombre = "Laura",
        Apellido1 = "Vargas",
        FechaNacimiento = new DateOnly(1992, 3, 15),
        Correo = correo,
        Contrasena = "ClaveInicial1",
        IdGenero = 1,
        IdDepartamento = DepFarmacia,
        IdCategoria = CategoriaInstructor,
        IdTipoNombramiento = 1
    };
}
