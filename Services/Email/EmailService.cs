using System;
using PEAS.Helpers;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace PEAS.Services.Email
{
    public interface IEmailService
    {
        void SendOrderEmail(string recipientEmail);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string sender = "PEAS";
        private readonly string senderEmailAddress = "hello@peas.gg";

        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _apiKey = _configuration.GetSection("SendGrid").Value ?? "";
            _logger = logger;
        }

        public async void SendOrderEmail(string recipientEmail)
        {
            try
            {
                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(senderEmailAddress, sender);
                var subject = "Reservation Request";
                var to = new EmailAddress(recipientEmail);
                var plainTextContent = "and easy to do anywhere, even with C#";
                var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                _ = await client.SendEmailAsync(msg);
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }
    }
}