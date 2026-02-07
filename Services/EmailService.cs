using Backend.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string confirmationToken)
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPass = _configuration["Email:SmtpPass"];
            var fromEmail = _configuration["Email:FromEmail"];
            var frontendUrl = _configuration["FrontendUrl"];

            // Créer le lien de confirmation
            var confirmationLink = $"{frontendUrl}/confirm-email?token={confirmationToken}";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = "Confirmez votre inscription",
                Body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>Bienvenue ! 🎉</h2>
                        <p style='color: #555; font-size: 16px;'>
                            Merci de vous être inscrit. Veuillez confirmer votre adresse email en cliquant sur le bouton ci-dessous :
                        </p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationLink}' 
                               style='background-color: #007bff; color: white; padding: 14px 30px; 
                                      text-decoration: none; border-radius: 5px; display: inline-block;
                                      font-weight: bold; font-size: 16px;'>
                                Confirmer mon email
                            </a>
                        </div>
                        
                        <p style='color: #666; font-size: 13px; margin-top: 20px;'>
                            Si le bouton ne fonctionne pas, copiez et collez ce lien dans votre navigateur :
                        </p>
                        <p style='color: #007bff; font-size: 12px; word-break: break-all;'>
                            {confirmationLink}
                        </p>
                        
                        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                        
                        <p style='color: #999; font-size: 11px;'>
                            ⏰ Ce lien expire dans 24 heures.<br>
                            Si vous n'avez pas créé de compte, ignorez cet email.
                        </p>
                    </div>
                ",
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}