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
                <div style=""
  font-family: 'Courier New', Courier, monospace;
  background-color: #d9d9d9;
  margin: 0;
  padding: 40px 20px;
"">
  <!-- Conteneur principal -->
    <div style=""
    max-width: 520px;
    margin: 0 auto;
    background-color: #d9d9d9;
    border: 3px solid #2d2d2d;
    padding: 45px 35px;
    box-shadow: 8px 8px 0px rgba(45, 45, 45, 0.25);
  "">

    <!-- En-tête / Brand -->
    <div style=""text-align: center; margin-bottom: 30px;"">
      <span style=""
        font-size: 10px;
        color: #2d2d2d;
        letter-spacing: 2px;
      "">■</span>
      <span style=""
        font-size: 24px;
        color: #2d2d2d;
        letter-spacing: 4px;
        font-weight: bold;
      "">&nbsp;C&#x2E31;R&#x2E31;E&#x2E31;P&#x2E31;S&#x2E31;T&nbsp;</span>
      <span style=""
        font-size: 10px;
        color: #2d2d2d;
        letter-spacing: 2px;
      "">■</span>
    </div>

    <!-- Séparateur pointillé -->
    <div style=""border-top: 2px dotted #2d2d2d; margin: 0 0 30px 0;""></div>

    <!-- Titre de section -->
    <div style=""
      font-size: 14px;
      color: #2d2d2d;
      letter-spacing: 1px;
      margin-bottom: 20px;
    "">■ CONFIRMATION DE COURRIEL ■</div>

    <!-- Message principal avec barre gauche -->
    <div style=""
      border-left: 6px solid #2d2d2d;
      padding-left: 25px;
      margin-bottom: 35px;
    "">
      <h1 style=""
        font-family: 'Courier New', Courier, monospace;
        font-size: 22px;
        font-weight: normal;
        color: #2d2d2d;
        margin: 0 0 15px 0;
        line-height: 1.5;
      "">Bienvenue dans le collectif</h1>

      <p style=""
        font-size: 15px;
        color: #2d2d2d;
        line-height: 1.7;
        margin: 0;
        opacity: 0.85;
      "">
        Merci de vous être inscrit. Pour activer votre compte, veuillez confirmer votre adresse courriel en cliquant sur le bouton ci-dessous.
      </p>
    </div>

    <!-- Bouton CTA -->
    <div style=""text-align: center; margin-bottom: 35px;"">
      <a href=""{confirmationLink}"" style=""
        font-family: 'Courier New', Courier, monospace;
        font-size: 16px;
        font-weight: bold;
        color: #d9d9d9;
        background-color: #2d2d2d;
        border: 2px solid #2d2d2d;
        padding: 14px 40px;
        text-decoration: none;
        display: inline-block;
        letter-spacing: 1px;
      "">[ Confirmer mon courriel ]</a>
    </div>

    <!-- Instructions -->
    <div style=""
      border: 2px solid #2d2d2d;
      padding: 20px;
      margin-bottom: 25px;
    "">
      <div style=""
        font-size: 14px;
        color: #2d2d2d;
        line-height: 1.7;
        margin-bottom: 10px;
      "">
        <span style=""margin-right: 10px;"">&#x2192;</span>
        Le lien expire dans <strong>24 heures</strong>.
      </div>
      <div style=""
        font-size: 14px;
        color: #2d2d2d;
        line-height: 1.7;
        margin-bottom: 0;
      "">
        <span style=""margin-right: 10px;"">&#x2192;</span>
        Si vous n'avez pas cr&eacute;&eacute; de compte, ignorez ce courriel.
      </div>
    </div>

    <!-- Avertissement lien de secours -->
    <div style=""
      background-color: rgba(45, 45, 45, 0.08);
      border-left: 4px solid #2d2d2d;
      padding: 12px 16px;
      margin-bottom: 30px;
    "">
      <p style=""
        font-size: 13px;
        color: #2d2d2d;
        line-height: 1.6;
        margin: 0 0 8px 0;
      "">
        <span style=""font-size: 10px; margin-right: 8px;"">&#9632;</span>
        Si le bouton ne fonctionne pas, copiez et collez ce lien dans votre navigateur :
      </p>
      <p style=""
        font-size: 12px;
        color: #2d2d2d;
        word-break: break-all;
        margin: 0;
        opacity: 0.7;
        text-decoration: underline;
      "">{confirmationLink}</p>
    </div>

    <!-- Séparateur pointillé -->
    <div style=""border-top: 2px dotted #2d2d2d; margin: 0 0 25px 0;""></div>

    <!-- Footer -->
    <div style=""text-align: center;"">
      <p style=""
        font-size: 13px;
        color: #2d2d2d;
        opacity: 0.6;
        font-style: italic;
        line-height: 1.6;
        margin: 0;
      "">
        Collectif de Recherche et d'Exp&eacute;rimentation Philosophique<br>
        sur le devenir du Sujet &agrave; l'&egrave;re Technique
      </p>
    </div>

  </div>
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
