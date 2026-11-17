using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class T05_UsuariosEditarE2ETests : PlaywrightBase
    {
        [Fact]
        public async Task Usuarios_EditarInformacion_GuardaCambiosExitosamente()
        {
            // 1. Iniciar sesión con rol de Administrador (usa BaseUrl y admin1234 desde PlaywrightBase)
            await IniciarSesionAdminAsync();

            // 2. Navegar al módulo de gestión de usuarios desde el menú principal
            await Page.ClickAsync("a[asp-controller='Usuarios'], a[href*='/Usuarios']");

            // 3. Hacer clic en el botón "Editar" del último usuario de la tabla
            // (Evitamos editar first-child para no alterar al Administrador con el que iniciamos sesión)
            await Page.ClickAsync("table.usuarios-table tbody tr:last-child a.btn-primary");

            // 4. Validar que estamos en la vista de edición correcta
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Usuarios/Edit/\\d+"));

            // 5. Modificar los campos editables permitidos (apellido y teléfono)
            var inputApellido = Page.Locator("input[name='Apellido1']");
            await inputApellido.FillAsync("Rodríguez Camilo");

            var inputTelefono = Page.Locator("input[name='Telefono']");
            await inputTelefono.FillAsync("69898980");

            // 6. Hacer clic en el botón de enviar "Guardar Cambios"
            await Page.ClickAsync("button[type='submit']:has-text('Guardar Cambios')");

            // 7. Validar que el sistema redirige de vuelta al listado general de usuarios en AWS
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/Usuarios/Index");

            // 8. Verificar que aparece la alerta de éxito en el listado
            var alertaExito = Page.Locator(".alert-success");
            await Expect(alertaExito).ToBeVisibleAsync();
        }
    }
}