using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class T04_UsuariosCrearE2ETests : PlaywrightBase
    {
        [Fact]
        public async Task Usuarios_RegistrarNuevoUsuario_GuardaExitosamente()
        {
            // Iniciar sesión Administrador
            await IniciarSesionAdminAsync();

            // Navegar al menú de Usuarios (haciendo clic en el enlace del menú lateral/superior de la layout)
            await Page.ClickAsync("a[asp-controller='Usuarios'], a[href*='/Usuarios']");

            // Hacer clic en el botón "Crear Usuario"
            await Page.ClickAsync("a.usuarios-create-button");

            // Generar identificador único de 6 dígitos para evitar duplicados de Cédula/Correo en AWS
            string uid = Random.Shared.Next(100000, 999999).ToString();

            // Rellenar formulario
            await Page.FillAsync("input[name='Cedula']", $"116{uid}");
            await Page.FillAsync("input[name='Nombre']", "María");
            await Page.FillAsync("input[name='Apellido1']", "Rodríguez");
            await Page.FillAsync("input[name='Apellido2']", "Mora");
            await Page.FillAsync("input[name='FechaNacimiento']", "1995-05-12");
            await Page.FillAsync("input[name='Correo']", $"maria.{uid}@ucr.ac.cr");
            await Page.FillAsync("input[name='Contrasena']", "Password123*");
            await Page.FillAsync("input[name='Telefono']", "88887777");

            // Seleccionar elementos (Combobox/Select)
            // Seleccionamos el primer género, departamento, categoría y rol disponible
            await Page.SelectOptionAsync("select[name='IdGenero']", new SelectOptionValue { Index = 2 });
            await Page.SelectOptionAsync("select[name='IdDepartamento']", new SelectOptionValue { Index = 1 });
            await Page.SelectOptionAsync("select[name='IdCategoria']", new SelectOptionValue { Index = 15 });
            await Page.SelectOptionAsync("select[name='IdTipoNombramiento']", new SelectOptionValue { Index = 1 });
            await Page.SelectOptionAsync("select[name='idRol']", new SelectOptionValue { Index = 1 });

            // Enviar el formulario clic en "Guardar Usuario"
            await Page.ClickAsync("button[type='submit']:has-text('Guardar Usuario')");

            // Validar que redirige al índice de usuarios y muestra el mensaje de éxito
            var alertaExito = Page.Locator(".alert-success");
            await Expect(alertaExito).ToBeVisibleAsync();
        }
    }
}