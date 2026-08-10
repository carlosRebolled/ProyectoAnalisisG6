using System.Net;
using System.Net.Mail;

namespace SistemaFarmaciaG6.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task EnviarAsync(
        string destinatario,
        string asunto,
        string cuerpoHtml)
    {
        string servidor =
            _configuration["Email:SmtpServer"]!;

        int puerto =
            int.Parse(
                _configuration["Email:Port"]!
            );

        string usuario =
            _configuration["Email:Username"]!;

        string password =
            _configuration["Email:Password"]!;

        string remitente =
            _configuration["Email:From"]!;

        using var mensaje = new MailMessage
        {
            From = new MailAddress(
                remitente,
                "Facultad de Farmacia"
            ),

            Subject = asunto,

            Body = cuerpoHtml,

            IsBodyHtml = true
        };

        mensaje.To.Add(destinatario);

        using var cliente = new SmtpClient(
            servidor,
            puerto
        );

        cliente.Credentials =
            new NetworkCredential(
                usuario,
                password
            );

        cliente.EnableSsl = true;

        await cliente.SendMailAsync(mensaje);
    }
}