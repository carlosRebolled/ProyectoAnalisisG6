using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class T03_UsuariosFiltroE2ETests : PlaywrightBase
    {
        [Fact]
        public async Task Usuarios_FiltrarPorCedula_MuestraResultadoCorrecto()
        {
            // 1. Iniciar sesión como Administrador (usa BaseUrl y admin1234 desde PlaywrightBase)
            await IniciarSesionAdminAsync();

            // 2. Navegar al listado de gestión de usuarios
            await Page.ClickAsync("a[asp-controller='Usuarios'], a[href*='/Usuarios']");
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/Usuarios/Index");

            // 3. Escribir la cédula objetivo en el cuadro de búsqueda dinámica
            string cedulaObjetivo = "101110111";
            await Page.FillAsync("#buscarUsuario", cedulaObjetivo);

            // 4. Validar que la fila que contiene la cédula sea visible
            var filaResultado = Page.Locator($".fila-usuario:has-text('{cedulaObjetivo}')");
            await Expect(filaResultado).ToBeVisibleAsync();

            // 5. Validar que el contador muestre 1 registro encontrado
            var contador = Page.Locator("#contadorUsuarios");
            await Expect(contador).ToHaveTextAsync("1");
        }
    }
}