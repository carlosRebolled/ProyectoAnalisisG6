using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SistemaFarmaciaG6.Services;

namespace SistemaFarmaciaG6.Tests.Infraestructura;

/// <summary>
/// Sesión en memoria que reemplaza a la sesión real de ASP.NET Core.
/// Permite simular un usuario logueado (IdUsuario, Rol, Nombre).
/// </summary>
public class SesionFalsa : ISession
{
    private readonly Dictionary<string, byte[]> _datos = new();

    public bool IsAvailable => true;
    public string Id { get; } = Guid.NewGuid().ToString();
    public IEnumerable<string> Keys => _datos.Keys;

    public void Clear() => _datos.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Remove(string key) => _datos.Remove(key);
    public void Set(string key, byte[] value) => _datos[key] = value;

    public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) =>
        _datos.TryGetValue(key, out value);
}

/// <summary>
/// Proveedor de TempData que no persiste nada (solo para pruebas).
/// </summary>
public class TempDataProviderFalso : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) =>
        new Dictionary<string, object>();

    public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
}

/// <summary>
/// Servicio de correo falso: en lugar de enviar el correo, lo guarda
/// en una lista para que la prueba pueda revisarlo.
/// </summary>
public class EmailServiceFalso : IEmailService
{
    public List<(string Destinatario, string Asunto, string Cuerpo)> Enviados { get; } = new();

    public Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        Enviados.Add((destinatario, asunto, cuerpoHtml));
        return Task.CompletedTask;
    }
}
