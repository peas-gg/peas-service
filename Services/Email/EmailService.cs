using System.IO;
using PEAS.Entities.Booking;
using PEAS.Helpers;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace PEAS.Services.Email
{
    public interface IEmailService
    {
        void SendOrderEmail(Order order);
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

        public void SendOrderEmail(Order order)
        {
            try
            {
                //Set HTML Content
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services/Email/OrderEmailTemplate.html");
                string title = order.OrderStatus.ToString().ToUpper();
                string recipientName = order.Customer.FirstName + " " + order.Customer.LastName;
                string colour = "";
                string subtitle = "";
                Uri image = order.Image;
                switch (order.OrderStatus)
                {
                    case Order.Status.Pending:
                        colour = "#FFF7AD";
                        subtitle = $"You requested a reservation with {order.Business.Name}. You will receive an email when your reservation is approved";
                        break;
                    case Order.Status.Approved:
                        colour = "#C4FFBC";
                        subtitle = $"Your reservation with {order.Business.Name} has been approved";
                        break;
                    case Order.Status.Declined:
                        colour = "#FF7A7A";
                        subtitle = $"Your reservation with {order.Business.Name} was declined. Please feel free to make another reservation.";
                        break;
                    case Order.Status.Completed:
                        throw new AppException("Cannot send emails for completed order status");
                }
                var htmlString = File
                    .ReadAllText(templatePath)
                    .Replace("#Title#", title)
                    .Replace("#RecipientName#", recipientName)
                    .Replace("#Color#", colour)
                    .Replace("#SubTitle#", subtitle);
                sendEmail($"Reservation With {order.Business.Name}", order.Customer.Email, htmlString);
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }

        public void SendPaymentEmail(Order order)
        {
            try
            {
                //Set HTML Content
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services/Email/OrderEmailTemplate.html");
                var htmlString = File
                    .ReadAllText(templatePath)
                    .Replace("#Title#", "This is a very serious email");
                sendEmail("Payment Request", order.Customer.Email, htmlString);
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }

        private async void sendEmail(string subject, string recipient, string htmlContent)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(senderEmailAddress, sender);
            var to = new EmailAddress(recipient);
            var plainTextContent = "and easy to do anywhere, even with C#";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            _ = await client.SendEmailAsync(msg);
        }
    }
}