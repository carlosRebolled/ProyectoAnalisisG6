using Microsoft.Playwright;
using Xunit;

namespace SistemaFarmaciaG6.UI;

public abstract class PlaywrightBase : IAsyncLifetime
{
    protected IPlaywright PlaywrightInstance { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    // Lee la variable de entorno PLAYWRIGHT_BASE_URL; si no existe, usa el servidor AWS
    protected static string BaseUrl =>
        (Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "http://18.118.254.83")
        .TrimEnd('/');

    // En GitHub Actions (CI=true) corre en modo headless; en local muestra el navegador
    protected static bool IsHeadless =>
        string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase);

    public async Task InitializeAsync()
    {
        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = IsHeadless,
            SlowMo = IsHeadless ? 0 : 800
        });

        // IgnoreHTTPSErrors evita bloqueos si luego agregas un certificado autofirmado
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });

        Page = await context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await Browser.CloseAsync();
        PlaywrightInstance.Dispose();
    }

    protected async Task IniciarSesionAdminAsync()
    {
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.FillAsync("input[name='correo']", "kevin.oviedovalverde@ucr.ac.cr");
        await Page.FillAsync("input[name='contrasena']", "admin1234");
        await Page.ClickAsync("button[type='submit']");
        await Assertions.Expect(Page).ToHaveURLAsync($"{BaseUrl}/Home/Index");
    }
    protected async Task IniciarSesionDocenteAsync()
    {
        await Page.GotoAsync($"{BaseUrl}/");
        await Page.FillAsync("input[name='correo']", "maria.573400@ucr.ac.cr"); // Verifica que coincida el correo real del docente en AWS
        await Page.FillAsync("input[name='contrasena']", "Password123*"); // <-- Verifica que coincida con la clave real del docente en AWS
        await Page.ClickAsync("button[type='submit']");
        await Assertions.Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Home/Index$"));
    }
}