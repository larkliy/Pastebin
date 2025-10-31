using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Pastebin.Services.Interfaces;

namespace Pastebin.Services.Implementations;

public class EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings) : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string message, CancellationToken cancellationToken = default)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(emailSettings.Value.SenderName, emailSettings.Value.SenderEmail));
        emailMessage.To.Add(new MailboxAddress("", toEmail));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("html") { Text = message };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            emailSettings.Value.SmtpServer, 
            emailSettings.Value.Port, 
            MailKit.Security.SecureSocketOptions.StartTls,
            cancellationToken);
            
        await client.AuthenticateAsync(
            emailSettings.Value.Username, 
            emailSettings.Value.Password, 
            cancellationToken);
        
        await client.SendAsync(emailMessage, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation($"Email sent to {toEmail}");
    }
}
