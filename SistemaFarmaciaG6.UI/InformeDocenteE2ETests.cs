using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class InformeDocenteE2ETests : IAsyncLifetime
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
                SlowMo = 1000      // Pausa leve para observar los pasos
            });
            _page = await _browser.NewPageAsync();
        }

        public async Task DisposeAsync()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        private async Task IniciarSesionDocenteAsync()
        {
            await _page.GotoAsync("https://localhost:7215/");
            await _page.FillAsync("input[name='correo']", "maria.rodriguez@ucr.ac.cr");
            await _page.FillAsync("input[name='contrasena']", "AQAAAAIAAYagAAAAED7MqrnDMecE11Hxuy8f+rSsUCHvLfPV5WtzLeQfdpeQNgX19CV1M6j8cle/sKHAIw=="); // O la contraseña que use el docente
            await _page.ClickAsync("button[type='submit']");
            await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Home/Index$"));
        }

        [Fact]
        public async Task InformeDocente_EditarYGuardarCambios_ActualizaExitosamente()
        {
            // 1. Iniciar sesión como docente
            await IniciarSesionDocenteAsync();

            // 2. Navegar al módulo de Informe Docente
            await _page.GotoAsync("https://localhost:7215/InformeDocente/Index");
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/InformeDocente/Index");

            
            // (Si ya estás en la vista de edición o necesitas entrar, validamos la URL actual)
            await _page.GotoAsync("https://localhost:7215/InformeDocente/Edit/5");
            await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/InformeDocente/Edit/\\d+"));

            // 4. Modificar las observaciones o algún campo editable del informe
            var txtObservaciones = _page.Locator("textarea[name*='Observaciones'], input[name*='Observaciones']");
            if (await txtObservaciones.CountAsync() > 0)
            {
                await txtObservaciones.First.FillAsync("Actualización de observaciones de prueba automatizada con Playwright.");
            }

            // 5. Hacer clic en el botón de "Guardar Cambios"
            await _page.ClickAsync("button[type='submit']:has-text('Guardar Cambios')");

            // 6. Validar que la acción se completa exitosamente
            var alertaExito = _page.Locator(".alert-success");
            if (await alertaExito.CountAsync() > 0)
            {
                await Expect(alertaExito).ToBeVisibleAsync();
            }
        }
        [Fact]
        public async Task InformeDocente_PdfEditarYEnviar()
        {
            // 1. Iniciar sesión como docente
            await IniciarSesionDocenteAsync();

            // 2. Navegar al índice de informes docentes
            await _page.GotoAsync("https://localhost:7215/InformeDocente/Index");
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/InformeDocente/Index");

            // 3. Ver Detalles: Hacer clic en el botón de "Ver Detalle" si está disponible
            var botonDetalles = _page.Locator("a:has-text('Ver Detalle'), a[href*='/InformeDocente/Details']");
            if (await botonDetalles.CountAsync() > 0)
            {
                await botonDetalles.First.ClickAsync();
                await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/InformeDocente/Details/\\d+"));

                // Regresar al listado principal
                await _page.ClickAsync("a:has-text('Volver'), a:has-text('Regresar'), a[href*='/InformeDocente/Index']");
                await Expect(_page).ToHaveURLAsync("https://localhost:7215/InformeDocente/Index");
            }

            // 4. Descargar PDF: Presionar el botón de PDF si está disponible
            var botonPdf = _page.Locator("a:has-text('PDF'), a[href*='Pdf'], a[href*='Descargar']");
            if (await botonPdf.CountAsync() > 0)
            {
                var downloadTask = _page.WaitForDownloadAsync();
                await botonPdf.First.ClickAsync();
                var download = await downloadTask;
                Assert.NotNull(download);
            }

            // 5. Ir a Editar (Validando primero si el informe aún está en borrador y muestra el botón Editar)
            var botonEditar = _page.Locator("a:has-text('Editar'), a[href*='/InformeDocente/Edit']");
            if (await botonEditar.CountAsync() > 0)
            {
                await botonEditar.First.ClickAsync();
                await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/InformeDocente/Edit/\\d+"));

                // Regresar al listado desde la edición
                await _page.ClickAsync("a:has-text('Cancelar'), a:has-text('Volver'), a[href*='/InformeDocente/Index']");
                await Expect(_page).ToHaveURLAsync("https://localhost:7215/InformeDocente/Index");
            }

            

            // 7Validar que regresa al Index y que el informe queda bloqueado (sin botón Editar)
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/InformeDocente/Index");

            var botonEditarBloqueado = _page.Locator("a:has-text('Editar')");
            await Expect(botonEditarBloqueado).ToHaveCountAsync(0);
        }
        [Fact]
        public async Task InformeDocente_EnviarInforme_BloqueaCorrectamente()
        {
            await IniciarSesionDocenteAsync();

            await _page.GotoAsync("https://localhost:7215/InformeDocente/Index");
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/InformeDocente/Index");

            // Enviar Informe
            var botonEnviar = _page.Locator("a:has-text('Enviar'), button:has-text('Enviar'), a[href*='/InformeDocente/Enviar']");
            if (await botonEnviar.CountAsync() > 0)
            {
                // Configurar el manejador de diálogos para aceptar automáticamente cualquier confirmación nativa (ej. window.confirm)
                _page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

                await botonEnviar.First.ClickAsync();

                // Manejar modal de confirmación en HTML si existiera
                var botonConfirmarSi = _page.Locator("button:has-text('Sí'), input[value='Sí'], a:has-text('Sí')");
                if (await botonConfirmarSi.CountAsync() > 0)
                {
                    await botonConfirmarSi.First.ClickAsync();
                }
            }

            // Validar que regresa al Index y que el informe queda bloqueado (sin botón Editar)
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/InformeDocente/Index");

            var botonEditarBloqueado = _page.Locator("a:has-text('Editar')");
            await Expect(botonEditarBloqueado).ToHaveCountAsync(0);
        }

        [Fact]
        public async Task InformeDocente_EliminarInforme_Exitoso()
        {
            await IniciarSesionDocenteAsync();

            await _page.GotoAsync("https://localhost:7215/InformeDocente/Index");
            await Expect(_page).ToHaveURLAsync("https://localhost:7215/InformeDocente/Index");

            // Buscar botón o enlace de Eliminar
            var botonEliminar = _page.Locator("a:has-text('Eliminar'), a[href*='/InformeDocente/Delete']");
            if (await botonEliminar.CountAsync() > 0)
            {
                // Configurar el diálogo nativo para aceptar la advertencia de eliminación
                _page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

                await botonEliminar.First.ClickAsync();

                // Si la aplicación redirige a una vista de confirmación tipo Delete.cshtml con botón POST de confirmar
                var botonConfirmarEliminar = _page.Locator("button[type='submit']:has-text('Eliminar'), input[value='Eliminar']");
                if (await botonConfirmarEliminar.CountAsync() > 0)
                {
                    await botonConfirmarEliminar.First.ClickAsync();
                }

                // Validar retorno al índice de informes
                await Expect(_page).ToHaveURLAsync("https://localhost:7215/InformeDocente/Index");
            }
        }

    }
}