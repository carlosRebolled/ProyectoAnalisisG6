using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using SistemaFarmaciaG6.Data;
using SistemaFarmaciaG6.Models;

namespace SistemaFarmaciaG6.Tests.Infraestructura;

/// <summary>
/// Clase base de todas las pruebas.
/// Cada prueba recibe una base de datos en memoria NUEVA y vacía
/// (cargada con datos de ejemplo), así las pruebas no se afectan entre sí.
/// </summary>
public abstract class PruebaBase : IDisposable
{
    // ---- Roles (mismos Id que usa AdminController) ----
    public const int RolDocente = 1;
    public const int RolDirector = 2;
    public const int RolDecano = 3;
    public const int RolAdministrador = 4;
    public const int RolJefatura = 5;
    public const int RolVisualizador = 6;

    // ---- Estados de informe ----
    public const int EstadoBorrador = 1;
    public const int EstadoPendiente = 2;
    public const int EstadoDevuelto = 3;
    public const int EstadoAprobado = 4;
    public const int EstadoFinalizado = 5;

    // ---- Departamentos / catálogos ----
    public const int DepFarmacia = 1;
    public const int DepQuimica = 2;
    public const int CategoriaInstructor = 1;
    public const int CategoriaSinUsuarios = 2;

    // ---- Usuarios de ejemplo ----
    public const int IdAdmin = 1;
    public const int IdDocente = 2;          // Departamento Farmacia
    public const int IdDocenteOtroDep = 3;   // Departamento Química
    public const int IdDirector = 4;         // Departamento Farmacia
    public const int IdDecano = 5;
    public const int IdInactivo = 6;
    public const int IdSinRol = 7;

    public const string ClaveValida = "Clave123!";

    protected readonly DbFacultadFarmaciaContext Db;

    protected static int AnioActual => DateTime.Now.Year;

    protected PruebaBase()
    {
        var opciones = new DbContextOptionsBuilder<DbFacultadFarmaciaContext>()
            .UseInMemoryDatabase($"PruebasFarmacia_{Guid.NewGuid()}")
            .Options;

        Db = new DbFacultadFarmaciaContext(opciones);
        CargarDatosIniciales();
    }

    public void Dispose()
    {
        Db.Database.EnsureDeleted();
        Db.Dispose();
    }

    /// <summary>
    /// Prepara un controlador como si lo llamara un usuario con el rol indicado.
    /// Si rol/idUsuario son null, se simula un usuario sin sesión iniciada.
    /// </summary>
    protected T Preparar<T>(T controlador, string? rol = null, int? idUsuario = null)
        where T : Controller
    {
        var httpContext = new DefaultHttpContext
        {
            Session = new SesionFalsa()
        };

        if (idUsuario != null)
            httpContext.Session.SetInt32("IdUsuario", idUsuario.Value);

        if (rol != null)
            httpContext.Session.SetString("Rol", rol);

        controlador.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controlador.TempData = new TempDataDictionary(httpContext, new TempDataProviderFalso());

        return controlador;
    }

    protected static string Hash(Usuario usuario, string clave) =>
        new PasswordHasher<Usuario>().HashPassword(usuario, clave);

    protected InformeDocente AgregarInformeDocente(int idUsuario, int estado, int? anio = null)
    {
        var informe = new InformeDocente
        {
            IdUsuario = idUsuario,
            IdEstado = estado,
            Anio = anio ?? AnioActual,
            FechaCreacion = DateTime.Now,
            DetalleOtros = "No aplica"
        };
        Db.InformeDocentes.Add(informe);
        Db.SaveChanges();
        return informe;
    }

    protected InformeDireccion AgregarInformeDireccion(int idUsuario, int estado, int? anio = null)
    {
        var informe = new InformeDireccion
        {
            IdUsuario = idUsuario,
            IdEstado = estado,
            Anio = anio ?? AnioActual,
            FechaCreacion = DateTime.Now
        };
        Db.InformeDireccions.Add(informe);
        Db.SaveChanges();
        return informe;
    }

    protected InformeFinalFacultad AgregarInformeFinal(int estado, int? anio = null)
    {
        var informe = new InformeFinalFacultad
        {
            IdUsuarioGenera = IdDecano,
            IdEstado = estado,
            Anio = anio ?? AnioActual,
            FechaGeneracion = DateTime.Now
        };
        Db.InformeFinalFacultads.Add(informe);
        Db.SaveChanges();
        return informe;
    }

    /// <summary>
    /// Informe docente con todos los campos obligatorios completos.
    /// </summary>
    protected static InformeDocente InformeDocenteCompleto() => new()
    {
        CantidadCongresosActivos = 1,
        DetalleCongresosActivos = "Congreso Centroamericano de Farmacia",
        CantidadCongresosPasivos = 0,
        DetalleCongresosPasivos = "No aplica",
        DetalleAccionSocial = "No aplica",
        DetalleInvestigacion = "Proyecto 123",
        DetalleDocencia = "Curso FA-101",
        DetallePublicaciones = "No aplica",
        DetalleCursosGrado = "FA-101",
        DetallePosgrado = "No aplica",
        DetalleRepresentacion = "No aplica",
        DetalleOtros = "No aplica",
        ObservacionesDocente = "Sin comentarios"
    };

    private void CargarDatosIniciales()
    {
        Db.Roles.AddRange(
            new Role { IdRol = RolDocente, NombreRol = "Docente", Estado = "Activo" },
            new Role { IdRol = RolDirector, NombreRol = "Director", Estado = "Activo" },
            new Role { IdRol = RolDecano, NombreRol = "Decano", Estado = "Activo" },
            new Role { IdRol = RolAdministrador, NombreRol = "Administrador", Estado = "Activo" },
            new Role { IdRol = RolJefatura, NombreRol = "Jefatura", Estado = "Activo" },
            new Role { IdRol = RolVisualizador, NombreRol = "Visualizador", Estado = "Activo" });

        Db.EstadosInformes.AddRange(
            new EstadosInforme { IdEstado = EstadoBorrador, NombreEstado = "Borrador" },
            new EstadosInforme { IdEstado = EstadoPendiente, NombreEstado = "Pendiente" },
            new EstadosInforme { IdEstado = EstadoDevuelto, NombreEstado = "Devuelto" },
            new EstadosInforme { IdEstado = EstadoAprobado, NombreEstado = "Aprobado" },
            new EstadosInforme { IdEstado = EstadoFinalizado, NombreEstado = "Finalizado" });

        Db.Departamentos.AddRange(
            new Departamento { IdDepartamento = DepFarmacia, NombreDepartamento = "Farmacia Industrial", Estado = "Activo" },
            new Departamento { IdDepartamento = DepQuimica, NombreDepartamento = "Química", Estado = "Activo" });

        Db.Generos.Add(new Genero { IdGenero = 1, NombreGenero = "Masculino", Estado = "Activo" });

        Db.Categorias.AddRange(
            new Categoria { IdCategoria = CategoriaInstructor, NombreCategoria = "Instructor", Estado = "Activo" },
            new Categoria { IdCategoria = CategoriaSinUsuarios, NombreCategoria = "Catedrático", Estado = "Activo" });

        Db.TiposNombramientos.Add(new TiposNombramiento { IdTipoNombramiento = 1, NombreNombramiento = "Propiedad", Estado = "Activo" });

        AgregarUsuario(IdAdmin, "Ana", "admin@ucr.ac.cr", DepFarmacia, RolAdministrador);
        AgregarUsuario(IdDocente, "Diego", "docente@ucr.ac.cr", DepFarmacia, RolDocente);
        AgregarUsuario(IdDocenteOtroDep, "Olga", "docente.quimica@ucr.ac.cr", DepQuimica, RolDocente);
        AgregarUsuario(IdDirector, "Daniel", "director@ucr.ac.cr", DepFarmacia, RolDirector);
        AgregarUsuario(IdDecano, "Elena", "decano@ucr.ac.cr", DepFarmacia, RolDecano);
        AgregarUsuario(IdInactivo, "Iván", "inactivo@ucr.ac.cr", DepFarmacia, RolDocente, estado: "Inactivo");
        AgregarUsuario(IdSinRol, "Sofía", "sinrol@ucr.ac.cr", DepFarmacia, rol: null);

        Db.SaveChanges();
    }

    private void AgregarUsuario(int id, string nombre, string correo, int departamento,
        int? rol, string estado = "Activo")
    {
        var usuario = new Usuario
        {
            IdUsuario = id,
            Cedula = $"1-0000-000{id}",
            Nombre = nombre,
            Apellido1 = "Pérez",
            Apellido2 = "Mora",
            FechaNacimiento = new DateOnly(1985, 5, 20),
            Correo = correo,
            Telefono = "88887777",
            IdGenero = 1,
            IdDepartamento = departamento,
            IdCategoria = CategoriaInstructor,
            IdTipoNombramiento = 1,
            Estado = estado,
            FechaRegistro = DateTime.Now
        };
        usuario.Contrasena = Hash(usuario, ClaveValida);

        Db.Usuarios.Add(usuario);

        if (rol != null)
            Db.UsuarioRols.Add(new UsuarioRol { IdUsuario = id, IdRol = rol.Value });
    }
}
