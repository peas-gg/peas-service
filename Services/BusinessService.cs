using System;
using System.Configuration;
using AutoMapper;
using PEAS.Entities;
using PEAS.Entities.Authentication;
using PEAS.Entities.Site;
using PEAS.Helpers;
using PEAS.Models.Business;

namespace PEAS.Services
{
    public interface IBusinessService
    {
        Business AddBusiness(Account account, CreateBusiness model);
        Template AddTemplate();
        List<Template> GetTemplate();
    }

    public class BusinessService : IBusinessService
    {
        private readonly DataContext _context;
        private readonly IMapService _mapService;
        private readonly IMapper _mapper;
        private readonly ILogger<BusinessService> _logger;

        public BusinessService(DataContext context, IMapService mapService, IMapper mapper, ILogger<BusinessService> logger)
        {
            _context = context;
            _mapService = mapService;
            _mapper = mapper;
            _logger = logger;
        }

        public Business AddBusiness(Account account, CreateBusiness model)
        {
            try
            {
                validateModel(model);

                string location = _mapService.GetLocation(model.Latitude, model.Longitude).Result;
                string timeZone = _mapService.GetTimeZone(model.Latitude, model.Longitude).Result;

                var business = new Business
                {
                    Account = account,
                    Sign = model.Sign,
                    Name = model.Name,
                    Category = model.Category,
                    Color = model.Color,
                    Description = model.Description,
                    ProfilePhoto = model.ProfilePhoto,
                    Twitter = model.Twitter,
                    Instagram = model.Instagram,
                    Tiktok = model.Tiktok,
                    Location = location,
                    TimeZone = timeZone,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    IsActive = true,
                    Created = DateTime.UtcNow
                };

                _context.Businesses.Add(business);
                _context.SaveChanges();

                return business;
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        public List<Template> GetTemplate()
        {
            throw new NotImplementedException();
        }

        public Template AddTemplate()
        {
            throw new NotImplementedException();
        }

        private void validateModel(CreateBusiness model)
        {
            if (model.Sign.Length < 3)
            {
                throw new AppException("Invalid PEAS Sign: Your PEAS sign needs to be a minimum of 3 characters");
            }

            if (model.Name.Length < 5)
            {
                throw new AppException("Invalid Name: Your business name needs to be a minimum of 5 characters");
            }
        }
    }
}