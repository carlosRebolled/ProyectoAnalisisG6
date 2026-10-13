using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class UsuariosFiltroE2ETests : IAsyncLifetime
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
                SlowMo = 1500      // Pausa leve para observar la acción de filtrado
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
        public async Task Usuarios_FiltrarPorCedula_MuestraResultadoCorrecto()
        {
            // 1. Iniciar sesión como Administrador
            await IniciarSesionAdminAsync();

            // 2. Navegar al listado de gestión de usuarios
            await _page.ClickAsync("a[asp-controller='Usuarios'], a[href*='/Usuarios']");
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/Usuarios/Index");

            // 3. Escribir la cédula objetivo en el cuadro de búsqueda dinámica
            string cedulaObjetivo = "101110111";
            await _page.FillAsync("#buscarUsuario", cedulaObjetivo);

            // 4. Validar que el contador de registros se actualice a 1 (o que la fila que contiene la cédula sea visible)
            var filaResultado = _page.Locator($".fila-usuario:has-text('{cedulaObjetivo}')");
            await Expect(filaResultado).ToBeVisibleAsync();

            // 5. Validar que el contador muestre 1 registro encontrado
            var contador = _page.Locator("#contadorUsuarios");
            await Expect(contador).ToHaveTextAsync("1");
        }
    }
}