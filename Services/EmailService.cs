using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace GestionBudget_V2.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailTemplateService _templateService;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _templateService = new EmailTemplateService();
        }

        public async Task SendPasswordResetEmail(string email, string typeUtilisateur, string nouveauMotDePasse)
        {
            var subject = "🔒 Réinitialisation de votre mot de passe - Gestion Budget";
            var body = _templateService.GeneratePasswordResetEmail(typeUtilisateur, nouveauMotDePasse);

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendWelcomeEmail(string email, string nom, string prenom, string typeUtilisateur, string motDePasseTemporaire)
        {
            var subject = "👋 Bienvenue sur Gestion Budget - Votre compte a été créé";
            var body = _templateService.GenerateWelcomeEmail(nom, prenom, typeUtilisateur, motDePasseTemporaire);

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");

                var host = smtpSettings["Host"] ?? "smtp.ethereal.email";
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var username = smtpSettings["Username"] ?? string.Empty;
                var password = smtpSettings["Password"] ?? string.Empty;
                var fromEmail = smtpSettings["FromEmail"] ?? "noreply@gestionbudget.com";
                var fromName = smtpSettings["FromName"] ?? "Gestion Budget";

                var smtpClient = new SmtpClient(host)
                {
                    Port = port,
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Logger l'erreur
                Console.WriteLine($"Erreur d'envoi d'email: {ex.Message}");
                throw new Exception($"Erreur lors de l'envoi de l'email à {toEmail}: {ex.Message}");
            }
        }
    }
}