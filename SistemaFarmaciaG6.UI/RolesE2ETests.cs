using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class RolesE2ETests : IAsyncLifetime
    {
        private IPlaywright _playwright = null!;
        private IBrowser _browser = null!;
        private IPage _page = null!;

        public async Task InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false, // Muestra el navegador en pantalla
                SlowMo = 1100      // Pausa leve para observar los pasos de roles
            });
            _page = await _browser.NewPageAsync();
        }

        public async Task DisposeAsync()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        private async Task IniciarSesionAdminAsync()
        {
            await _page.GotoAsync("https://localhost:7215/");
            await _page.FillAsync("input[name='correo']", "kevin.oviedovalverde@ucr.ac.cr");
            await _page.FillAsync("input[name='contrasena']", "AQAAAAIAAYagAAAAEO85ESyMMJ8kZpJThKEyz7I0u1TAB452ZegclJ+/1SRu5OU5Q8gSIbvcq+AS4K5hYA==");
            await _page.ClickAsync("button[type='submit']");
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/Home/Index");
        }
        [Fact]
        public async Task Roles_FiltrarBusqueda_MuestraResultadoCorrecto()
        {
            // 1. Iniciar sesión como Administrador
            await IniciarSesionAdminAsync();

            // 2. Navegar al módulo de Roles
            await _page.ClickAsync("a[asp-controller='Roles'], a[href*='/Roles']");

            // 3. Escribir un criterio de búsqueda en el input correspondiente (asumiendo ID o clase similar de búsqueda)
            string textoBusqueda = "Decano";

            // Localizamos el campo de búsqueda de la vista de roles
            var inputBusqueda = _page.Locator("input[type='text'], input[placeholder*='buscar'], input[placeholder*='Buscar']");
            await inputBusqueda.FillAsync(textoBusqueda);

            // 4. Validar que el resultado filtrado se muestra correctamente en la tabla
            var filaResultado = _page.Locator($"tbody tr:has-text('{textoBusqueda}')");
            await Expect(filaResultado.First).ToBeVisibleAsync();
        }
        [Fact]
        public async Task Roles_ModificarRolUsuario_GuardaExitosamente()
        {
            // 1. Iniciar sesión como Administrador
            await IniciarSesionAdminAsync();

            // 2. Navegar al módulo de Roles haciendo clic en el menú correspondiente
            await _page.ClickAsync("a[asp-controller='Roles'], a[href*='/Roles']");

            // 3. Hacer clic en el botón o enlace de editar rol del primer registro de la tabla
            await _page.ClickAsync("table tbody tr:last-child a.btn-primary, table tbody tr:last-child a[href*='Edit']");

            // 4. Seleccionar un nuevo rol desde el menú desplegable (<select name="idRol">)
            // Seleccionamos por índice (por ejemplo, la segunda opción del combo)
            await _page.SelectOptionAsync("select[name='idRol']", new SelectOptionValue { Index = 3});

            // 5. Hacer clic en el botón de enviar "Guardar Rol"
            await _page.ClickAsync("button[type='submit']:has-text('Guardar Rol')");

            // 6. Validar que redirige de vuelta al listado del índice de roles y muestra éxito
            await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Roles/?(Index)?$"));
            var alertaExito = _page.Locator(".alert-success");
            await Expect(alertaExito).ToBeVisibleAsync();
        }

        
    }
}