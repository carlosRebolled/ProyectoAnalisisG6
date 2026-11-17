using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class T02_UsuariosValidacionCamposE2ETests : PlaywrightBase
    {
        [Fact]
        public async Task Usuarios_CrearCamposVacios_MuestraMensajesValidacionRequeridos()
        {
            // 1. Iniciar sesión como Administrador (usa BaseUrl y admin1234 desde PlaywrightBase)
            await IniciarSesionAdminAsync();

            // 2. Navegar al módulo de Usuarios
            await Page.ClickAsync("a[asp-controller='Usuarios'], a[href*='/Usuarios']");

            // 3. Hacer clic en "Crear Usuario"
            await Page.ClickAsync("a.usuarios-create-button");

            // 4. Hacer clic directamente en "Guardar Usuario" sin llenar ningún campo
            await Page.ClickAsync("button[type='submit']:has-text('Guardar Usuario')");

            // 5. Validaciones (Aserciones):
            // a) Verificar que la URL permanece en la vista de creación (/Usuarios/Create) y no redirige
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/Usuarios/Create");

            // b) Verificar que aparecen mensajes de validación visibles para los campos obligatorios
            var mensajesError = Page.Locator(".field-validation-error, span.text-danger:not(:empty)");

            // Validamos que al menos un mensaje de campo requerido se despliegue en pantalla
            await Expect(mensajesError.First).ToBeVisibleAsync();
        }
    }
}