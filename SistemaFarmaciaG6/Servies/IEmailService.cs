namespace SistemaFarmaciaG6.Services;

public interface IEmailService
{
    Task EnviarAsync(
        string destinatario,
        string asunto,
        string cuerpoHtml
    );
}