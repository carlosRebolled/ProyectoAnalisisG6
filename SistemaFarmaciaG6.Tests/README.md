# Pruebas automáticas – Sistema Facultad de Farmacia (G6)

Proyecto de pruebas unitarias con **xUnit** para `SistemaFarmaciaG6`.

Las pruebas **no necesitan SQL Server**: cada prueba crea su propia base de datos
en memoria (`Microsoft.EntityFrameworkCore.InMemory`) con datos de ejemplo, así que
se pueden ejecutar en cualquier computadora y no tocan la base de datos real.
El envío de correos también se simula (`EmailServiceFalso`), no se envía nada.

## Cómo ejecutarlas

**Visual Studio:** abrir `SistemaFarmaciaG6/SistemaFarmaciaG6.sln` →
menú **Prueba → Explorador de pruebas** → **Ejecutar todas las pruebas** (Ctrl+R, A).

**Consola** (desde la carpeta raíz del repositorio):

```bash
dotnet test SistemaFarmaciaG6.Tests
```

**GitHub:** el workflow `.github/workflows/pruebas.yml` las ejecuta solo en cada
`push` a `main` y en cada pull request (pestaña *Actions* del repositorio).

## Estructura

| Carpeta / archivo | Contenido |
|---|---|
| `Infraestructura/PruebaBase.cs` | Base de datos en memoria, datos de ejemplo (roles, estados, usuarios) y método `Preparar(...)` que simula la sesión de un usuario con un rol |
| `Infraestructura/Fakes.cs` | Sesión, TempData y servicio de correo falsos |
| `Pruebas/AccountControllerTests.cs` | Login, logout, usuario inactivo / sin rol, migración de contraseñas a hash, recuperación de contraseña por código (vencimiento, intentos, reutilización) |
| `Pruebas/AdministracionTests.cs` | Acceso exclusivo del administrador; CRUD de usuarios (validaciones, correo @ucr.ac.cr, duplicados), roles, categorías y auditoría |
| `Pruebas/InformeDocenteTests.cs` | Ciclo del informe docente: crear, editar, enviar, eliminar, reabrir, permisos por rol y exportar PDF |
| `Pruebas/RevisionInformesTests.cs` | Director aprueba/devuelve informes de su departamento; Decano revisa informes de dirección; flujo completo |
| `Pruebas/InformeDireccionYFinalTests.cs` | Informe de dirección e Informe Final de Facultad (consolidación, finalizar, reabrir) |
| `Pruebas/PerfilInicioYPanelesTests.cs` | Perfil, página de inicio, paneles por rol y helpers de seguridad/auditoría |

## Estados de informe usados

1 Borrador · 2 Pendiente · 3 Devuelto · 4 Aprobado · 5 Finalizado

## Agregar una prueba nueva

```csharp
public class MiModuloTests : PruebaBase
{
    [Fact]
    public void Accion_Condicion_ResultadoEsperado()
    {
        var controlador = Preparar(new CategoriasController(Db), "Administrador", IdAdmin);

        var resultado = controlador.Index();

        Assert.IsType<ViewResult>(resultado);
    }
}
```
