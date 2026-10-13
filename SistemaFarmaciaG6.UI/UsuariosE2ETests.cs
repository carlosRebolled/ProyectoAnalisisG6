using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class UsuariosE2ETests : IAsyncLifetime
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
                SlowMo = 1900      // Pausa ligera para visualizar los pasos
            });
            _page = await _browser.NewPageAsync();
        }

        public async Task DisposeAsync()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        // Método auxiliar para autenticarnos previamente como Administrador
        private async Task IniciarSesionAdminAsync()
        {
            await _page.GotoAsync("https://localhost:7215/");
            await _page.FillAsync("input[name='correo']", "kevin.oviedovalverde@ucr.ac.cr");
            await _page.FillAsync("input[name='contrasena']", "AQAAAAIAAYagAAAAEDTei5xDd4zqnrB9SE/TFEffgUJo/OhUtW6K7ZWLr97K7/tEzCHm7MAHMyD9VhAJyA==");
            await _page.ClickAsync("button[type='submit']");
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/Home/Index");
        }

        [Fact]
        public async Task Usuarios_RegistrarNuevoUsuario_GuardaExitosamente()
        {
            // 1. Iniciar sesión primero para tener permisos de Administrador
            await IniciarSesionAdminAsync();

            // 2. Navegar al menú de Usuarios (haciendo clic en el enlace del menú lateral/superior de la layout)
            await _page.ClickAsync("a[asp-controller='Usuarios'], a[href*='/Usuarios']");

            // 3. Hacer clic en el botón "Crear Usuario"
            await _page.ClickAsync("a.usuarios-create-button");

            // 4. Rellenar el formulario de creación con datos de prueba
            await _page.FillAsync("input[name='Cedula']", "116540321");
            await _page.FillAsync("input[name='Nombre']", "María");
            await _page.FillAsync("input[name='Apellido1']", "Rodríguez");
            await _page.FillAsync("input[name='Apellido2']", "Mora");
            await _page.FillAsync("input[name='FechaNacimiento']", "1995-05-12");
            await _page.FillAsync("input[name='Correo']", "maria.rodriguez@ucr.ac.cr");
            await _page.FillAsync("input[name='Contrasena']", "Password123*");
            await _page.FillAsync("input[name='Telefono']", "88887777");

            // Seleccionar elementos de los desplegables (Combobox/Select)
            // Seleccionamos el primer género, departamento, categoría y rol disponible
            await _page.SelectOptionAsync("select[name='IdGenero']", new SelectOptionValue { Index = 1 });
            await _page.SelectOptionAsync("select[name='IdDepartamento']", new SelectOptionValue { Index = 1 });
            await _page.SelectOptionAsync("select[name='IdCategoria']", new SelectOptionValue { Index = 1 });
            await _page.SelectOptionAsync("select[name='IdTipoNombramiento']", new SelectOptionValue { Index = 1 });
            await _page.SelectOptionAsync("select[name='idRol']", new SelectOptionValue { Index = 1 });

            // 5. Enviar el formulario haciendo clic en "Guardar Usuario"
            await _page.ClickAsync("button[type='submit']:has-text('Guardar Usuario')");

            // 6. Validar que redirige al índice de usuarios y muestra el mensaje de éxito
            var alertaExito = _page.Locator(".alert-success");
            await Expect(alertaExito).ToBeVisibleAsync();
        }
    }
}