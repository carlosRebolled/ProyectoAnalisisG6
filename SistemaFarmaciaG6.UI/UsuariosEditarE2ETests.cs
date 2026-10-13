using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class UsuariosEditarE2ETests : IAsyncLifetime
    {
        private IPlaywright _playwright = null!;
        private IBrowser _browser = null!;
        private IPage _page = null!;

        public async Task InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false, // Muestra el navegador en vivo
                SlowMo = 1900      // Pausa leve para observar los pasos de edición
            });
            _page = await _browser.NewPageAsync();
        }

        public async Task DisposeAsync()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        // Método auxiliar para autenticarse previamente como Administrador
        private async Task IniciarSesionAdminAsync()
        {
            await _page.GotoAsync("https://localhost:7215/");
            await _page.FillAsync("input[name='correo']", "kevin.oviedovalverde@ucr.ac.cr");
            await _page.FillAsync("input[name='contrasena']", "AQAAAAIAAYagAAAAEO85ESyMMJ8kZpJThKEyz7I0u1TAB452ZegclJ+/1SRu5OU5Q8gSIbvcq+AS4K5hYA==");
            await _page.ClickAsync("button[type='submit']");
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/Home/Index");
        }

        [Fact]
        public async Task Usuarios_EditarInformacion_GuardaCambiosExitosamente()
        {
            // 1. Iniciar sesión con rol de Administrador
            await IniciarSesionAdminAsync();

            // 2. Navegar al módulo de gestión de usuarios desde el menú principal
            await _page.ClickAsync("a[asp-controller='Usuarios'], a[href*='/Usuarios']");

            // 3. Hacer clic en el botón "Editar" del primer usuario que aparece en la tabla
            // Buscamos el enlace o botón con la clase 'btn-primary' que contiene el texto "Editar"
            await _page.ClickAsync("table.usuarios-table tbody tr:first-child a.btn-primary");

            // 4. Validar que estamos en la vista de edición correcta
            await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Usuarios/Edit/\\d+"));

            // 5. Modificar los campos editables permitidos (por ejemplo, el apellido y teléfono)
            var inputApellido = _page.Locator("input[name='Apellido1']");
            await inputApellido.FillAsync("Rodríguez Actualizado");

            var inputTelefono = _page.Locator("input[name='Telefono']");
            await inputTelefono.FillAsync("87654321");

            // 6. Hacer clic en el botón de enviar "Guardar Cambios"
            await _page.ClickAsync("button[type='submit']:has-text('Guardar Cambios')");

            // 7. Validar que el sistema redirige de vuelta al listado general de usuarios tras el éxito
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/Usuarios/Index");

            // Opcional: Verificar que aparece la alerta de éxito en el listado
            var alertaExito = _page.Locator(".alert-success");
            await Expect(alertaExito).ToBeVisibleAsync();
        }
    }
}