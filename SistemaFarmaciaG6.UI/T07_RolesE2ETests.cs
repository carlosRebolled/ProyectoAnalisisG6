using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class T07_RolesE2ETests : PlaywrightBase
    {
        [Fact]
        public async Task Roles_FiltrarBusqueda_MuestraResultadoCorrecto()
        {
            // 1. Iniciar sesión como Administrador (usa BaseUrl y admin1234 desde PlaywrightBase)
            await IniciarSesionAdminAsync();

            // 2. Navegar al módulo de Roles
            await Page.ClickAsync("a[asp-controller='Roles'], a[href*='/Roles']");

            // 3. Escribir un criterio de búsqueda en el input correspondiente
            string textoBusqueda = "Decano";

            var inputBusqueda = Page.Locator("input[type='text'], input[placeholder*='buscar'], input[placeholder*='Buscar']");
            await inputBusqueda.FillAsync(textoBusqueda);

            // 4. Validar que el resultado filtrado se muestra correctamente en la tabla
            var filaResultado = Page.Locator($"tbody tr:has-text('{textoBusqueda}')");
            await Expect(filaResultado.First).ToBeVisibleAsync();
        }

        [Fact]
        public async Task Roles_ModificarRolUsuario_GuardaExitosamente()
        {
            // 1. Iniciar sesión como Administrador
            await IniciarSesionAdminAsync();

            // 2. Navegar al módulo de Roles haciendo clic en el menú correspondiente
            await Page.ClickAsync("a[asp-controller='Roles'], a[href*='/Roles']");

            // 3. Hacer clic en el botón de editar rol del último registro de la tabla
            await Page.ClickAsync("table tbody tr:last-child a.btn-primary, table tbody tr:last-child a[href*='Edit']");

            // 4. Seleccionar un nuevo rol desde el menú desplegable (<select name="idRol">)
            await Page.SelectOptionAsync("select[name='idRol']", new SelectOptionValue { Index = 3 });

            // 5. Hacer clic en el botón de enviar "Guardar Rol"
            await Page.ClickAsync("button[type='submit']:has-text('Guardar Rol')");

            // 6. Validar que redirige de vuelta al listado del índice de roles y muestra éxito
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Roles/?(Index)?$"));
            var alertaExito = Page.Locator(".alert-success");
            await Expect(alertaExito).ToBeVisibleAsync();
        }
    }
}