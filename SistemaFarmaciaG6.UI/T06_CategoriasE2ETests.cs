using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class T06_CategoriasE2ETests : PlaywrightBase
    {
        [Fact]
        public async Task Categorias_CrearNuevaCategoria_GuardaExitosamente()
        {
            // 1. Autenticación como Administrador3
            await IniciarSesionAdminAsync();

            // 2. Navegar al módulo de Categorías desde el menú principal
            await Page.ClickAsync("a[asp-controller='Categorias'], a[href*='/Categorias']");
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/?(Index)?$"));

            // 3. Hacer clic en el botón "Crear Nueva Categoría"
            await Page.ClickAsync("a.categorias-create-button");
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/Categorias/Create");

            // 4. Rellenar los campos del formulario con sufijo único para AWS
            string sufijo = DateTime.Now.ToString("HHmmss");
            await Page.FillAsync("input[name='NombreCategoria']", $"Categoría Automatizada E2E {sufijo}");
            await Page.SelectOptionAsync("select[name='Estado']", "Activo");

            // 5. Hacer clic en el botón de guardar
            await Page.ClickAsync("button[type='submit']:has-text('Guardar')");

            // 6. Validar redirección al listado y mensaje de éxito
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/?(Index)?$"));
            var alertaExito = Page.Locator(".alert-success");
            await Expect(alertaExito).ToBeVisibleAsync();
        }

        [Fact]
        public async Task Categorias_EditarCategoriaExistente_ActualizaExitosamente()
        {
            // 1. Autenticación
            await IniciarSesionAdminAsync();

            // 2. Navegar a Categorías
            await Page.ClickAsync("a[asp-controller='Categorias'], a[href*='/Categorias']");

            // 3. Hacer clic en el botón "Editar" de la última categoría de la tabla
            // (Usamos last-child para no sobrescribir las categorías base del sistema en AWS)
            await Page.ClickAsync("table.categorias-table tbody tr:last-child a.btn-warning");

            // 4. Modificar el nombre de la categoría
            string sufijo = DateTime.Now.ToString("HHmmss");
            var inputNombre = Page.Locator("input[name='NombreCategoria']");
            await inputNombre.FillAsync($"Categoría Actualizada Playwright {sufijo}");

            // 5. Hacer clic en "Actualizar"
            await Page.ClickAsync("button[type='submit']:has-text('Actualizar')");

            // 6. Validar retorno al listado y éxito
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/?(Index)?$"));
            var alertaExito = Page.Locator(".alert-success");
            await Expect(alertaExito).ToBeVisibleAsync();
        }

        [Fact]
        public async Task Categorias_FiltrarPorNombre_MuestraResultadoCorrecto()
        {
            // 1. Autenticación
            await IniciarSesionAdminAsync();

            // 2. Navegar a Categorías
            await Page.ClickAsync("a[asp-controller='Categorias'], a[href*='/Categorias']");

            // 3. Escribir texto en el buscador dinámico de categorías (#buscarCategoria)
            string textoFiltro = "Categoría";
            await Page.FillAsync("#buscarCategoria", textoFiltro);

            // 4. Validar que al menos una fila visible contenga el texto buscado
            var filaResultado = Page.Locator($".fila-categoria:has-text('{textoFiltro}')");
            await Expect(filaResultado.First).ToBeVisibleAsync();

            // 5. Validar que el contador superior refleje resultados
            var contador = Page.Locator("#contadorCategorias");
            await Expect(contador).Not.ToHaveTextAsync("0");
        }

        [Fact]
        public async Task Categorias_EliminarUltimaCategoria_EliminaExitosamente()
        {
            // 1. Autenticación como Administrador
            await IniciarSesionAdminAsync();

            // 2. Navegar al módulo de Categorías
            await Page.ClickAsync("a[asp-controller='Categorias'], a[href*='/Categorias']");
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/?(Index)?$"));

            // 3. Hacer clic en el botón "Eliminar" del último registro disponible en la tabla
            await Page.ClickAsync("table.table tbody tr:last-child a.btn-danger");

            // 4. Validar que estamos en la vista de confirmación de eliminación
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/Delete/\\d+"));

            // 5. Hacer clic en el botón de confirmación "Eliminar" del formulario POST
            await Page.ClickAsync("form[asp-action='Delete'] button[type='submit'], button.btn-danger:has-text('Eliminar')");

            // 6. Validar que redirige de vuelta al listado general y muestra el mensaje de éxito
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/?(Index)?$"));
            var alertaExito = Page.Locator(".alert-success");
            await Expect(alertaExito).ToBeVisibleAsync();
        }

        [Fact]
        public async Task Categorias_VisualizarDetalles_MuestraInformacionCorrectamente()
        {
            // 1. Autenticación como Administrador
            await IniciarSesionAdminAsync();

            // 2. Navegar al módulo de Categorías
            await Page.ClickAsync("a[asp-controller='Categorias'], a[href*='/Categorias']");
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/?(Index)?$"));

            // 3. Hacer clic en el botón "Detalles" de la primera categoría de la tabla
            await Page.ClickAsync("table.table tbody tr:first-child a.btn-info, table.table tbody tr:first-child a:has-text('Detalles')");

            // 4. Validar que la URL corresponde a la vista de detalles de una categoría específica (/Categorias/Details/{id})
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/Details/\\d+"));

            // 5. Validar que el título o la tarjeta con la información de la categoría se muestre correctamente en pantalla
            var tituloDetalles = Page.Locator("h2:has-text('Detalles de Categoría')");
            await Expect(tituloDetalles).ToBeVisibleAsync();

            var tarjetaContenido = Page.Locator(".card");
            await Expect(tarjetaContenido).ToBeVisibleAsync();
        }
    }
}
