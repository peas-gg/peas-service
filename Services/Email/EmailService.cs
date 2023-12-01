using PEAS.Entities.Authentication;
using PEAS.Entities.Booking;
using PEAS.Entities.Site;
using PEAS.Helpers;
using PEAS.Helpers.Utilities;
using SendGrid;
using SendGrid.Helpers.Mail;
using TimeZoneConverter;
using Newtonsoft.Json;


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
        private readonly string senderEmailAddress = "hello@peas.gg";
        private readonly string siteUrl;
        private readonly HttpClient _client;

        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _apiKey = _configuration.GetSection("SendGrid").Value ?? "";
            _logger = logger;
            siteUrl = (configuration.GetSection("Environment").Value ?? "") == "Production" ? "https://peas.gg/" : "https://dev.peas.gg/";
        }

        public async void SendOrderEmail(Order order, Business business)
        {
            try
            {
                //Set HTML Content
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services/Email/OrderStatus.html");
                string title = order.OrderStatus.ToString().ToUpper();
                string recipientName = order.Customer.FirstName + " " + order.Customer.LastName;
                string calenderDateFormat = "yyyyMMddTHHmmssZ";
                string googleCalenderLink = "";
                string appleCalenderLink = "";
                string colour = "";
                string subtitle = "";
                string businessName = $"\"{business.Name}\"";
                Uri image = order.Image;
                DateTime orderDate = getBusinessTime(order.StartTime, business.TimeZone);
                string time = orderDate.ToString("h:mm tt");
                string day = $"{orderDate:ddd}, {orderDate:MMM} {orderDate.Day}";
                string startTime = order.StartTime.ToString(calenderDateFormat);
                string endTime = order.EndTime.ToString(calenderDateFormat);
                string addToCalenderDisplay = "none";

                switch (order.OrderStatus)
                {
                    case Order.Status.Pending:
                        colour = "#FFF7AD";
                        subtitle = $"You requested a reservation with {businessName}. You will receive an email when your reservation is approved";
                        break;
                    case Order.Status.Approved:
                        CalenderLinksResponse calenderLinksResponse = await getCalenderLinks($"{order.Title} with {business.Name}", startTime, endTime);

                        colour = "#C4FFBC";
                        subtitle = $"Your reservation with {businessName} has been approved";
                        addToCalenderDisplay = "block";
                        googleCalenderLink = calenderLinksResponse.Links.Google;
                        appleCalenderLink = calenderLinksResponse.Links.Apple;
                        
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
                    .Replace("#Day#", day)
                    .Replace("#StartTime#", startTime)
                    .Replace("#EndTime#", endTime)
                    .Replace("#AddToCalenderDisplay#", addToCalenderDisplay)
                    .Replace("#GoogleCalenderLink#", googleCalenderLink)
                    .Replace("#AppleCalenderLink#", appleCalenderLink);

                sendEmail(business.Name, $"{title} - Reservation #{order.Id.ToString()[..5].ToUpper()}", order.Customer.Email, "", htmlString);
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

                sendEmail(business.Name, $"Payment request #{order.Id.ToString()[..5].ToUpper()}", order.Customer.Email, "", htmlString);
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
                sendEmail("PEAS", $"New withdrawal request # {withdrawal.Id.ToString()[..5].ToUpper()}", "Kingsley@peas.gg", $"{account.FirstName} {account.LastName} is requesting a withdrawal of ${Price.Format(withdrawal.Amount)}", "");
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }

        private async void sendEmail(string sender, string subject, string recipient, string plainTextContent, string htmlContent)
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
            TimeZoneInfo businessTimeZone = TZConvert.GetTimeZoneInfo(timeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, businessTimeZone);
        }


        public EmailService()
        {
            _client = new HttpClient();
        }



        private async Task<CalenderLinksResponse> getCalenderLinks(string title, string startTime, string endTime)
        {
            try
            {
                string endpoint = "https://calndr.link/api/events";
                var values = new Dictionary<string, string>
                {
                    { "title", $"{title}" },
                    { "start", $"{startTime}" },
                    { "end", $"{endTime}" }
                };

                var content = new FormUrlEncodedContent(values);

                var response = await _client.PostAsync(endpoint, content);
                string responseString = await response.Content.ReadAsStringAsync();
                CalenderLinksResponse jsonResponse = JsonConvert.DeserializeObject<CalenderLinksResponse>(responseString);

                return jsonResponse;
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException("Could not get Calender Links");
            }
        }
    }
}