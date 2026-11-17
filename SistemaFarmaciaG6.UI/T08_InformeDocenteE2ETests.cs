using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace SistemaFarmaciaG6.UI
{
    public class T08_InformeDocenteE2ETests : PlaywrightBase
    {
        private async Task CompletarCamposFormularioInformeAsync()
        {
            await Page.FillAsync("#CantidadCongresosActivos", "1");
            await Page.FillAsync("#DetalleCongresosActivos", "Congreso Internacional de Ciencias Farmacéuticas");

            await Page.EvaluateAsync(@"() => {
                const idsNa = [
                    'naCongresosPasivos', 'naAccionSocial', 'naInvestigacion', 
                    'naDocencia', 'naPublicaciones', 'naCursosGrado', 
                    'naPosgrado', 'naRepresentacion', 'naOtros'
                ];

                idsNa.forEach(id => {
                    const chk = document.getElementById(id);
                    if (chk && !chk.checked) {
                        chk.checked = true;
                        chk.dispatchEvent(new Event('change'));
                    }
                });
            }");

            var txtObservaciones = Page.Locator("#ObservacionesDocente");
            if (await txtObservaciones.CountAsync() > 0)
            {
                await txtObservaciones.FillAsync("Informe completado y validado mediante Playwright E2E test.");
            }
        }

        private async Task AsegurarInformeBorradorAsync()
        {
            await Page.GotoAsync($"{BaseUrl}/InformeDocente/Index");
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/InformeDocente/Index");

            var botonEditar = Page.Locator("a:has-text('Editar'), a[href*='/InformeDocente/Edit']");
            if (await botonEditar.CountAsync() > 0)
            {
                return; // Ya existe un informe en borrador listo
            }

            var botonCrear = Page.Locator("a:has-text('Crear'), a:has-text('Nuevo'), a[href*='/InformeDocente/Create']");
            if (await botonCrear.CountAsync() > 0)
            {
                await botonCrear.First.ClickAsync();
            }
            else
            {
                await Page.GotoAsync($"{BaseUrl}/InformeDocente/Create");
            }

            await CompletarCamposFormularioInformeAsync();

            // Selector dinámico: busca "Guardar Borrador" (Creación) o "Guardar Cambios" (Edición automática)
            var botonGuardar = Page.Locator("button:has-text('Guardar Borrador'), button:has-text('Guardar Cambios'), button[type='submit']");
            await Expect(botonGuardar.First).ToBeVisibleAsync();
            await botonGuardar.First.ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/InformeDocente/Index");
        }

        [Fact]
        public async Task Step01_CrearBorrador_GuardaExitosamente()
        {
            await IniciarSesionDocenteAsync();

            // 1. Limpiar cualquier borrador previo para probar el flujo limpio
            await Page.GotoAsync($"{BaseUrl}/InformeDocente/Index");
            var botonEliminar = Page.Locator("a:has-text('Eliminar'), a[href*='/InformeDocente/Delete']");
            if (await botonEliminar.CountAsync() > 0)
            {
                Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
                await botonEliminar.First.ClickAsync();

                var btnConfirmar = Page.Locator("button[type='submit']:has-text('Eliminar'), input[value='Eliminar']");
                if (await btnConfirmar.CountAsync() > 0)
                    await btnConfirmar.First.ClickAsync();

                await Expect(Page).ToHaveURLAsync($"{BaseUrl}/InformeDocente/Index");
            }

            // 2. Hacer clic en Crear (si el sistema redirige a Edit porque detecta borrador pendiente, lo maneja el selector unificado)
            var botonCrear = Page.Locator("a:has-text('Crear'), a:has-text('Nuevo'), a[href*='/InformeDocente/Create']");
            if (await botonCrear.CountAsync() > 0)
            {
                await botonCrear.First.ClickAsync();
            }
            else
            {
                await Page.GotoAsync($"{BaseUrl}/InformeDocente/Create");
            }

            // 3. Completar todos los campos obligatorios
            await CompletarCamposFormularioInformeAsync();

            // 4. Hacer clic en el botón de guardar disponible (sea Guardar Borrador o Guardar Cambios)
            var botonGuardar = Page.Locator("button:has-text('Guardar Borrador'), button:has-text('Guardar Cambios'), button[type='submit']");
            await Expect(botonGuardar.First).ToBeVisibleAsync();
            await botonGuardar.First.ClickAsync();

            // 5. Validar redirección al Index y presencia del botón Editar
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/InformeDocente/Index");
            var botonEditar = Page.Locator("a:has-text('Editar'), a[href*='/InformeDocente/Edit']");
            await Expect(botonEditar.First).ToBeVisibleAsync();
        }

        [Fact]
        public async Task Step02_EditarYGuardarCambios_ActualizaExitosamente()
        {
            await IniciarSesionDocenteAsync();
            await AsegurarInformeBorradorAsync();

            await Page.Locator("a:has-text('Editar'), a[href*='/InformeDocente/Edit']").First.ClickAsync();
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/InformeDocente/Edit/\\d+"));

            await CompletarCamposFormularioInformeAsync();
            await Page.FillAsync("#ObservacionesDocente", "Actualización de observaciones desde prueba Edit con Playwright.");

            // Botón unificado de guardado
            var botonGuardar = Page.Locator("button:has-text('Guardar Cambios'), button:has-text('Guardar Borrador'), button[type='submit']");
            await botonGuardar.First.ClickAsync();

            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/InformeDocente/Index");
            var alertaExito = Page.Locator(".alert-success");
            if (await alertaExito.CountAsync() > 0)
            {
                await Expect(alertaExito).ToBeVisibleAsync();
            }
        }

        [Fact]
        public async Task Step03_VerDetalleYDescargarPdf_Exitoso()
        {
            await IniciarSesionDocenteAsync();
            await AsegurarInformeBorradorAsync();

            var botonDetalles = Page.Locator("a:has-text('Ver Detalle'), a[href*='/InformeDocente/Details']");
            await Expect(botonDetalles.First).ToBeVisibleAsync();
            await botonDetalles.First.ClickAsync();
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/InformeDocente/Details/\\d+"));

            await Page.ClickAsync("a:has-text('Volver'), a:has-text('Regresar'), a[href*='/InformeDocente/Index']");
            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/InformeDocente/Index");

            var botonPdf = Page.Locator("a:has-text('PDF'), a[href*='Pdf'], a[href*='Descargar']");
            if (await botonPdf.CountAsync() > 0)
            {
                var downloadTask = Page.WaitForDownloadAsync();
                await botonPdf.First.ClickAsync();
                var download = await downloadTask;
                Assert.NotNull(download);
            }
        }

        [Fact]
        public async Task Step04_EnviarInforme_BloqueaCorrectamente()
        {
            await IniciarSesionDocenteAsync();
            await AsegurarInformeBorradorAsync();

            var botonEnviar = Page.Locator("a:has-text('Enviar'), button:has-text('Enviar'), a[href*='/InformeDocente/Enviar']");
            await Expect(botonEnviar.First).ToBeVisibleAsync();

            Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
            await botonEnviar.First.ClickAsync();

            var botonConfirmarSi = Page.Locator("button:has-text('Sí'), input[value='Sí'], a:has-text('Sí')");
            if (await botonConfirmarSi.CountAsync() > 0)
            {
                await botonConfirmarSi.First.ClickAsync();
            }

            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/InformeDocente/Index");
            var botonEditarBloqueado = Page.Locator("a:has-text('Editar')");
            await Expect(botonEditarBloqueado).ToHaveCountAsync(0);
        }

        [Fact]
        public async Task Step05_EliminarInforme_Exitoso()
        {
            await IniciarSesionDocenteAsync();
            await AsegurarInformeBorradorAsync();

            var botonEliminar = Page.Locator("a:has-text('Eliminar'), a[href*='/InformeDocente/Delete']");
            await Expect(botonEliminar.First).ToBeVisibleAsync();

            Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
            await botonEliminar.First.ClickAsync();

            var botonConfirmarEliminar = Page.Locator("button[type='submit']:has-text('Eliminar'), input[value='Eliminar']");
            if (await botonConfirmarEliminar.CountAsync() > 0)
            {
                await botonConfirmarEliminar.First.ClickAsync();
            }

            await Expect(Page).ToHaveURLAsync($"{BaseUrl}/InformeDocente/Index");
        }
    }
}