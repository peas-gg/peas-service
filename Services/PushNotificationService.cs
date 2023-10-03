using Newtonsoft.Json;
using PEAS.Entities.Authentication;
using PEAS.Entities.Booking;
using CorePush.Apple;
using PEAS.Helpers;

namespace PEAS.Services
{
    public interface IPushNotificationService
    {
        void SendNewOrderPush(Account account, Order order);
    }

    public class PushNotificationService : IPushNotificationService
    {
        public struct AppleNotification
        {
            public struct Payload
            {
                public struct AlertBody
                {
                    public string Title { get; set; }
                    public string Body { get; set; }
                }

                public AlertBody Alert { get; set; }

                public string Sound { get; set; }

                [JsonProperty("mutable-content")]
                public int MutableContent { get; set; }

                [JsonProperty("content-available")]
                public int ContentAvailable { get; set; }
            }

            public Payload Aps { get; set; }
        }

        struct ApplePushSettings
        {
            public string P8Key { get; set; }
            public string P8KeyId { get; set; }
            public string TeamId { get; set; }
            public string AppIdentifier { get; set; }
            public string Environment { get; set; }
        }

        private readonly HttpClient _client;
        private readonly ApplePushSettings _applePushSettings;
        private readonly ILogger<PushNotificationService> _logger;

        public PushNotificationService(IConfiguration configuration, ILogger<PushNotificationService> logger)
        {
            _client = new HttpClient();
            _logger = logger;
            _applePushSettings = new ApplePushSettings
            {
                P8Key = configuration.GetSection("PushNotification:Apple:P8Key").Value ?? "",
                P8KeyId = configuration.GetSection("PushNotification:Apple:P8KeyId").Value ?? "",
                TeamId = configuration.GetSection("PushNotification:Apple:TeamId").Value ?? "",
                AppIdentifier = configuration.GetSection("PushNotification:Apple:AppIdentifier").Value ?? "",
                Environment = configuration.GetSection("Environment").Value ?? ""
            };
        }

        public void SendNewOrderPush(Account account, Order order)
        {
            try
            {
                Device? device = account.Devices?.FirstOrDefault();
                if (device != null)
                {
                    string title = "New Request";
                    string body = $"{order.Customer.FirstName} {order.Customer.LastName} is requesting a reservation for {order.Title}";

                    AppleNotification appleNotification = new AppleNotification
                    {
                        Aps = new AppleNotification.Payload
                        {
                            Alert = new AppleNotification.Payload.AlertBody
                            {
                                Title = title,
                                Body = body
                            },
                            Sound = "",
                            MutableContent = 1,
                            ContentAvailable = 0
                        }
                    };
                    sendApplePush(device.DeviceToken, appleNotification);
                }
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }

        private async void sendApplePush(string deviceToken, AppleNotification apsPayload)
        {
            try
            {
                var settings = new ApnSettings
                {
                    AppBundleIdentifier = _applePushSettings.AppIdentifier,
                    P8PrivateKey = _applePushSettings.P8Key,
                    P8PrivateKeyId = _applePushSettings.P8KeyId,
                    TeamId = _applePushSettings.TeamId,
                    ServerType = _applePushSettings.Environment == "Production" ? ApnServerType.Production : ApnServerType.Development
                };

                var apn = new ApnSender(settings, _client);
                var response = await apn.SendAsync(apsPayload, deviceToken);
                return;
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }
    }
}