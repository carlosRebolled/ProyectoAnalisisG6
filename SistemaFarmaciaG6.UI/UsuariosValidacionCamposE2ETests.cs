using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class UsuariosValidacionCamposE2ETests : IAsyncLifetime
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
                SlowMo = 1500      // Pausa leve para observar la interacción
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
        public async Task Usuarios_CrearCamposVacios_MuestraMensajesValidacionRequeridos()
        {
            // 1. Iniciar sesión como Administrador
            await IniciarSesionAdminAsync();

            // 2. Navegar al módulo de Usuarios
            await _page.ClickAsync("a[asp-controller='Usuarios'], a[href*='/Usuarios']");

            // 3. Hacer clic en "Crear Usuario"
            await _page.ClickAsync("a.usuarios-create-button");

            // 4. Hacer clic directamente en "Guardar Usuario" sin llenar ningún campo
            await _page.ClickAsync("button[type='submit']:has-text('Guardar Usuario')");

            // 5. Validaciones (Aserciones):
            // a) Verificar que la URL permanece en la vista de creación (/Usuarios/Create) y no redirige
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/Usuarios/Create");

            // b) Verificar que aparecen mensajes de validación visibles para los campos obligatorios
            var mensajesError = _page.Locator(".field-validation-error, span.text-danger:not(:empty)");

            // Validamos que al menos un mensaje de campo requerido se despliegue en pantalla
            await Expect(mensajesError.First).ToBeVisibleAsync();
        }
    }
}