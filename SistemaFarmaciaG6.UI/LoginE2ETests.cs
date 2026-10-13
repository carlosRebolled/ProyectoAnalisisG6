using Microsoft.Playwright;
using Microsoft.Playwright.Xunit; // Asegura la extensión de aserciones de Playwright
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class LoginE2ETests : IAsyncLifetime
    {
        private IPlaywright _playwright = null!;
        private IBrowser _browser = null!;
        private IPage _page = null!;

        public async Task InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                SlowMo = 1900
            });
            _page = await _browser.NewPageAsync();
        }

        public async Task DisposeAsync()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        [Fact]
        public async Task Login_CredencialesValidas_AccedeExitosamente()
        {
            // 1. Navegar a la página de inicio (asegúrate de que el proyecto web esté corriendo en el puerto 7215)
            await _page.GotoAsync("https://localhost:7215/");

            // 2. Ingresar el correo utilizando el atributo id o name correcto de tu view ("correo")
            await _page.FillAsync("input[name='correo']", "kevin.oviedovalverde@ucr.ac.cr");

            // 3. Ingresar la contraseña utilizando el atributo correcto de tu view ("contrasena")
            await _page.FillAsync("input[name='contrasena']", "AQAAAAIAAYagAAAAEDTei5xDd4zqnrB9SE/TFEffgUJo/OhUtW6K7ZWLr97K7/tEzCHm7MAHMyD9VhAJyA==");

            // 4. Hacer clic en el botón de submit (Ingresar)
            await _page.ClickAsync("button[type='submit']");

            // 5. Validar que la interfaz redirige correctamente al panel principal
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/Home/Index");

        }
        // --- CASO 2: Validar error con credenciales incorrectas ---
        [Fact]
        public async Task Login_CredencialesInvalidas_MuestraMensajeError()
        {
            // 1. Navegar al login
            await _page.GotoAsync("https://localhost:7215/");

            // 2. Ingresar un correo o contraseña erróneos
            await _page.FillAsync("input[name='correo']", "correo.falso@ucr.ac.cr");
            await _page.FillAsync("input[name='contrasena']", "claveerronea123");

            // 3. Hacer clic en el botón de ingresar
            await _page.ClickAsync("button[type='submit']");

            // 4. Validar que la URL se mantiene en el login (o no redirige a Home)
            // Y verificar que el sistema muestra la alerta de error en pantalla
            var alertaError = _page.Locator(".alert-danger");
            await Expect(alertaError).ToBeVisibleAsync();
        }
    }
}