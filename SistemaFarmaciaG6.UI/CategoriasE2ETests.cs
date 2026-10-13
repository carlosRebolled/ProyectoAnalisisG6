using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class CategoriasE2ETests : IAsyncLifetime
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
                SlowMo = 400      // Pausa leve para observar la ejecución de los pasos
            });
            _page = await _browser.NewPageAsync();
        }

        public async Task DisposeAsync()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        // Método auxiliar para iniciar sesión como Administrador
        private async Task IniciarSesionAdminAsync()
        {
            await _page.GotoAsync("https://localhost:7215/");
            await _page.FillAsync("input[name='correo']", "kevin.oviedovalverde@ucr.ac.cr");
            await _page.FillAsync("input[name='contrasena']", "AQAAAAIAAYagAAAAEO85ESyMMJ8kZpJThKEyz7I0u1TAB452ZegclJ+/1SRu5OU5Q8gSIbvcq+AS4K5hYA==");
            await _page.ClickAsync("button[type='submit']");
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/Home/Index");
        }

        [Fact]
        public async Task Categorias_CrearNuevaCategoria_GuardaExitosamente()
        {
            // 1. Autenticación como Administrador
            await IniciarSesionAdminAsync();

            // 2. Navegar al módulo de Categorías desde el menú principal
            // (Asegúrate de que el enlace en tu navbar apunte al controlador Categorias)
            await _page.ClickAsync("a[asp-controller='Categorias'], a[href*='/Categorias']");
            await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/?(Index)?$"));

            // 3. Hacer clic en el botón "Crear Nueva Categoría"
            await _page.ClickAsync("a.categorias-create-button");
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/Categorias/Create");

            // 4. Rellenar los campos del formulario
            await _page.FillAsync("input[name='NombreCategoria']", "Categoría Automatizada E2E");
            await _page.SelectOptionAsync("select[name='Estado']", "Activo");

            // 5. Hacer clic en el botón de guardar
            await _page.ClickAsync("button[type='submit']:has-text('Guardar')");

            // 6. Validar redirección al listado y mensaje de éxito
            await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/?(Index)?$"));
            var alertaExito = _page.Locator(".alert-success");
            await Expect(alertaExito).ToBeVisibleAsync();
        }

        [Fact]
        public async Task Categorias_EditarCategoriaExistente_ActualizaExitosamente()
        {
            // 1. Autenticación
            await IniciarSesionAdminAsync();

            // 2. Navegar a Categorías
            await _page.ClickAsync("a[asp-controller='Categorias'], a[href*='/Categorias']");

            // 3. Hacer clic en el botón "Editar" de la primera categoría de la tabla
            await _page.ClickAsync("table.categorias-table tbody tr:first-child a.btn-warning");

            // 4. Modificar el nombre de la categoría
            var inputNombre = _page.Locator("input[name='NombreCategoria']");
            await inputNombre.FillAsync("Categoría Actualizada Playwright");

            // 5. Hacer clic en "Actualizar"
            await _page.ClickAsync("button[type='submit']:has-text('Actualizar')");

            // 6. Validar retorno al listado y éxito
            await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/?(Index)?$"));
            var alertaExito = _page.Locator(".alert-success");
            await Expect(alertaExito).ToBeVisibleAsync();
        }

        [Fact]
        public async Task Categorias_FiltrarPorNombre_MuestraResultadoCorrecto()
        {
            // 1. Autenticación
            await IniciarSesionAdminAsync();

            // 2. Navegar a Categorías
            await _page.ClickAsync("a[asp-controller='Categorias'], a[href*='/Categorias']");

            // 3. Escribir texto en el buscador dinámico de categorías (#buscarCategoria)
            string textoFiltro = "Categoría";
            await _page.FillAsync("#buscarCategoria", textoFiltro);

            // 4. Validar que al menos una fila visible contenga el texto buscado
            var filaResultado = _page.Locator($".fila-categoria:has-text('{textoFiltro}')");
            await Expect(filaResultado.First).ToBeVisibleAsync();

            // 5. Validar que el contador superior refleje resultados
            var contador = _page.Locator("#contadorCategorias");
            await Expect(contador).Not.ToHaveTextAsync("0");
        }
        [Fact]
        public async Task Categorias_EliminarUltimaCategoria_EliminaExitosamente()
        {
            // 1. Autenticación como Administrador
            await IniciarSesionAdminAsync();

            // 2. Navegar al módulo de Categorías
            await _page.ClickAsync("a[asp-controller='Categorias'], a[href*='/Categorias']");
            await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/?(Index)?$"));

            // 3. Hacer clic en el botón "Eliminar" del último registro disponible en la tabla
            await _page.ClickAsync("table.table tbody tr:last-child a.btn-danger");

            // 4. Validar que estamos en la vista de confirmación de eliminación
            await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/Delete/\\d+"));

            // 5. Hacer clic en el botón de confirmación "Eliminar" del formulario POST
            await _page.ClickAsync("form[asp-action='Delete'] button[type='submit'], button.btn-danger:has-text('Eliminar')");

            // 6. Validar que redirige de vuelta al listado general y muestra el mensaje de éxito
            await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/?(Index)?$"));
            var alertaExito = _page.Locator(".alert-success");
            await Expect(alertaExito).ToBeVisibleAsync();
        }

        [Fact]
        public async Task Categorias_VisualizarDetalles_MuestraInformacionCorrectamente()
        {
            // 1. Autenticación como Administrador
            await IniciarSesionAdminAsync();

            // 2. Navegar al módulo de Categorías
            await _page.ClickAsync("a[asp-controller='Categorias'], a[href*='/Categorias']");
            await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/?(Index)?$"));

            // 3. Hacer clic en el botón "Detalles" de la primera categoría de la tabla
            await _page.ClickAsync("table.table tbody tr:first-child a.btn-info, table.table tbody tr:first-child a:has-text('Detalles')");

            // 4. Validar que la URL corresponde a la vista de detalles de una categoría específica (/Categorias/Details/{id})
            await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Categorias/Details/\\d+"));

            // 5. Validar que el título o la tarjeta con la información de la categoría se muestre correctamente en pantalla
            var tituloDetalles = _page.Locator("h2:has-text('Detalles de Categoría')");
            await Expect(tituloDetalles).ToBeVisibleAsync();

            var tarjetaContenido = _page.Locator(".card");
            await Expect(tarjetaContenido).ToBeVisibleAsync();
        }
    }
}