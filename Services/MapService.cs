using System;
using Newtonsoft.Json;
using PEAS.Helpers;
using PEAS.Models.Maps;

namespace PEAS.Services
{
    public interface IMapService
    {
        Task<string> GetLocation(double latitude, double longitude);
    }

    public class MapService : IMapService
    {
        private readonly IConfiguration _configuration;
        private readonly string _key;
        private readonly HttpClient _client;
        private readonly ILogger<MapService> _logger;

        public MapService(IConfiguration configuration, ILogger<MapService> logger)
        {
            _configuration = configuration;
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

                return $"{address.Municipality}, {address.CountrySubdivision}. {address.Country}";
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException("Could not set your business location");
            }
        }
    }
}