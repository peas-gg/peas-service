using Newtonsoft.Json;
using PEAS.Helpers;
using PEAS.Models.Maps;

namespace PEAS.Services
{
    public interface IMapService
    {
        Task<string> GetLocation(double latitude, double longitude);
        Task<string> GetTimeZone(double latitude, double longitude);
    }

    public class MapService : IMapService
    {
        private readonly string _key;
        private readonly HttpClient _client;
        private readonly ILogger<MapService> _logger;

        public MapService(IConfiguration configuration, ILogger<MapService> logger)
        {
            _key = configuration.GetSection("AzureMap").Value ?? "";
            _client = new HttpClient();
            _logger = logger;
        }

        public async Task<string> GetLocation(double latitude, double longitude)
        {
            try
            {
                string coordinates = $"{latitude}" + "," + $"{longitude}";
                string endpoint = $"https://atlas.microsoft.com/search/address/reverse/json?api-version=1.0&query={coordinates}&subscription-key={_key}";

                var response = await _client.GetAsync(endpoint);
                string responseString = await response.Content.ReadAsStringAsync();
                LocationResponse jsonResponse = JsonConvert.DeserializeObject<LocationResponse>(responseString);
                LocationResponse.AddressExpanded address = jsonResponse.Addresses.FirstOrDefault().Address;

                if (address.Country == null)
                {
                    throw new Exception("Could not get location. Please verify the coordinates are accurate.");
                }

                return $"{address.Municipality}, {address.CountrySubdivision}. {address.Country}";
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException("Could not verify your business location");
            }
        }

        public async Task<string> GetTimeZone(double latitude, double longitude)
        {
            try
            {
                string coordinates = $"{latitude}" + "," + $"{longitude}";
                string endpoint = $"https://atlas.microsoft.com/timezone/byCoordinates/json?api-version=1.0&options=all&query={coordinates}&subscription-key={_key}";

                var response = await _client.GetAsync(endpoint);
                string responseString = await response.Content.ReadAsStringAsync();
                TimeZoneResponse jsonResponse = JsonConvert.DeserializeObject<TimeZoneResponse>(responseString);
                TimeZoneResponse.TimeZoneExpanded timeZone = jsonResponse.TimeZones.FirstOrDefault();

                return timeZone.Id;
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException("Could not set your business timezone");
            }
        }
    }
}