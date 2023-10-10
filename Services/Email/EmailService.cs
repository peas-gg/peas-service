using PEAS.Entities.Authentication;
using PEAS.Entities.Booking;
using PEAS.Entities.Site;
using PEAS.Helpers;
using PEAS.Helpers.Utilities;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace PEAS.Services.Email
{
    public interface IEmailService
    {
        void SendOrderEmail(Order order, Business business);
        void SendPaymentEmail(Order order, Business business);
        void SendWithdrawEmailToAdmin(Account account, Withdrawal withdrawal);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string sender = "PEAS";
        private readonly string senderEmailAddress = "hello@peas.gg";
        private readonly string siteUrl;

        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _apiKey = _configuration.GetSection("SendGrid").Value ?? "";
            _logger = logger;
            siteUrl = (configuration.GetSection("Environment").Value ?? "") == "Production" ? "https://peas.gg/" : "https://dev.peas.gg/";
        }

        public void SendOrderEmail(Order order, Business business)
        {
            try
            {
                //Set HTML Content
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services/Email/OrderStatus.html");
                string title = order.OrderStatus.ToString().ToUpper();
                string recipientName = order.Customer.FirstName + " " + order.Customer.LastName;
                string colour = "";
                string subtitle = "";
                string businessName = $"\"{business.Name}\"";
                Uri image = order.Image;
                DateTime orderDate = getBusinessTime(order.StartTime, business.TimeZone);
                string time = orderDate.ToString("h:mm tt");
                string day = $"{orderDate:ddd}, {orderDate:MMM} {orderDate.Day}";
                switch (order.OrderStatus)
                {
                    case Order.Status.Pending:
                        colour = "#FFF7AD";
                        subtitle = $"You requested a reservation with {businessName}. You will receive an email when your reservation is approved";
                        break;
                    case Order.Status.Approved:
                        colour = "#C4FFBC";
                        subtitle = $"Your reservation with {businessName} has been approved";
                        break;
                    case Order.Status.Declined:
                        colour = "#FF7A7A";
                        subtitle = $"Your reservation with {businessName} was declined. Please feel free to make another reservation.";
                        break;
                    case Order.Status.Completed:
                        throw new AppException("Cannot send emails for completed order status");
                }
                var htmlString = File
                    .ReadAllText(templatePath)
                    .Replace("#Title#", title)
                    .Replace("#RecipientName#", recipientName)
                    .Replace("#Color#", colour)
                    .Replace("#Image#", image.ToString())
                    .Replace("#SubTitle#", subtitle)
                    .Replace("#OrderTitle#", order.Title)
                    .Replace("#Price#", $"${Price.Format(order.Price)}")
                    .Replace("#Time#", time)
                    .Replace("#Day#", day);

                sendEmail($"{title} - Reservation With {business.Name}  #{order.Id.ToString()[..5].ToUpper()}", order.Customer.Email, "", htmlString);
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }

        public void SendPaymentEmail(Order order, Business business)
        {
            try
            {
                //Set HTML Content
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services/Email/PaymentRequest.html");

                string subtitle = $"{business.Name} is requesting payment";
                Uri image = order.Image;
                DateTime orderDate = getBusinessTime(order.StartTime, business.TimeZone);
                string time = $"{orderDate:h:mm tt} @ {orderDate:ddd}, {orderDate:MMM} {orderDate.Day}";

                var htmlString = File
                    .ReadAllText(templatePath)
                    .Replace("#Image#", image.ToString())
                    .Replace("#SubTitle#", subtitle)
                    .Replace("#OrderTitle#", order.Title)
                    .Replace("#PaymentLink#", $"{siteUrl}pay/{order.Id}")
                    .Replace("#Price#", $"${Price.Format(order.Price)}")
                    .Replace("#Time#", time);

                sendEmail($"Payment request from {business.Name} #{order.Id.ToString()[..5].ToUpper()}", order.Customer.Email, "", htmlString);
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }

        public void SendWithdrawEmailToAdmin(Account account, Withdrawal withdrawal)
        {
            try
            {
                sendEmail($"New withdrawal request # {withdrawal.Id.ToString()[..5].ToUpper()}", "Kingsley@peas.gg", $"{account.FirstName} {account.LastName} is requesting a withdrawal of ${withdrawal.Amount}", "");
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }

        private async void sendEmail(string subject, string recipient, string plainTextContent, string htmlContent)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(senderEmailAddress, sender);
            var to = new EmailAddress(recipient);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            _ = await client.SendEmailAsync(msg);
        }

        private static DateTime getBusinessTime(DateTime dateTime, string timeZone)
        {
            //Convert to business time
            TimeZoneInfo businessTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, businessTimeZone);
        }
    }
}