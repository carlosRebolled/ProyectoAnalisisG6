using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class T01_LoginE2ETests : PlaywrightBase
    {
        // --- CASO 1: Validar acceso exitoso con credenciales válidas ---
        [Fact]
        public async Task Login_CredencialesValidas_AccedeExitosamente()
        {
            // 1. Navegar a la página de inicio en AWS
            await Page.GotoAsync($"{BaseUrl}/");

            // 2. Ingresar el correo válido
            await Page.FillAsync("input[name='correo']", "kevin.oviedovalverde@ucr.ac.cr");

            // 3. Ingresar la contraseña en texto plano (corrige el hash que estaba antes)
            await Page.FillAsync("input[name='contrasena']", "admin1234");

            // 4. Hacer clic en el botón de submit (Ingresar)
            await Page.ClickAsync("button[type='submit']");

            // 5. Validar que la interfaz redirige correctamente al panel principal
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/Home/Index");
        }

        // --- CASO 2: Validar error con credenciales incorrectas ---
        [Fact]
        public async Task Login_CredencialesInvalidas_MuestraMensajeError()
        {
            // 1. Navegar al login en AWS
            await Page.GotoAsync($"{BaseUrl}/");

            // 2. Ingresar un correo o contraseña erróneos
            await Page.FillAsync("input[name='correo']", "correo.falso@ucr.ac.cr");
            await Page.FillAsync("input[name='contrasena']", "claveerronea123");

            // 3. Hacer clic en el botón de ingresar
            await Page.ClickAsync("button[type='submit']");

            // 4. Verificar que el sistema muestra la alerta de error en pantalla
            var alertaError = Page.Locator(".alert-danger");
            await Expect(alertaError).ToBeVisibleAsync();
        }
    }
}