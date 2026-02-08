using Backend.Services.Interfaces;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using System.Threading.Tasks;
using BrevoTask = sib_api_v3_sdk.Model.Task;
namespace Backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async System.Threading.Tasks.Task SendConfirmationEmailAsync(string toEmail, string confirmationToken)
        {
            var apiKey = _configuration["Email:BrevoApiKey"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"] ?? "Crepst";
            var frontendUrl = _configuration["FrontendUrl"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("La clé API Brevo n'est pas configurée.");
            }

            // Créer le lien de confirmation
            var confirmationLink = $"{frontendUrl}/confirm-email?token={confirmationToken}";

            // Configuration de l'API Brevo
            Configuration.Default.ApiKey["api-key"] = apiKey;

            var apiInstance = new TransactionalEmailsApi();

            var htmlContent = $@"
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
            ";

            // Créer l'objet SendSmtpEmail avec la syntaxe correcte
            var sendSmtpEmail = new SendSmtpEmail
            {
                Sender = new SendSmtpEmailSender(fromName, fromEmail),
                To = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(toEmail) },
                HtmlContent = htmlContent,
                Subject = "Confirmez votre inscription"
            };

            try
            {
                var result = await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
                Console.WriteLine($"✅ Email envoyé avec succès via l'API Brevo ! Message ID: {result.MessageId}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors de l'envoi via l'API Brevo : {ex.Message}", ex);
            }
        }
    }
}