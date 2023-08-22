using System;
using System.Configuration;
using AutoMapper;
using PEAS.Entities;
using PEAS.Entities.Authentication;
using PEAS.Entities.Site;
using PEAS.Helpers;
using PEAS.Models.Account;
using PEAS.Models.Business;

namespace PEAS.Services
{
    public interface IBusinessService
    {
        BusinessResponse AddBusiness(Account account, CreateBusiness model);
        BusinessResponse UpdateBusiness(Account account, UpdateBusiness model);
        Template AddTemplate(CreateTemplate model);
        void DeleteTemplate(Guid id);
        List<Template> GetTemplates();
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

        public BusinessResponse AddBusiness(Account account, CreateBusiness model)
        {
            try
            {
                validateSign(model.Sign);
                validateSign(model.Name);

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

                var response = _mapper.Map<BusinessResponse>(business);

                return response;
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        public BusinessResponse UpdateBusiness(Account account, UpdateBusiness model)
        {
            try
            {
                var business = _context.Businesses.SingleOrDefault(x => x.Id == model.Id);

                if (business == null || business.Account != account)
                {
                    throw new AppException("Invalid Buisness Id");
                }

                if (model.Sign != null)
                {
                    validateSign(model.Sign);
                    business.Sign = model.Sign;
                }

                if (model.Name != null)
                {
                    validateName(model.Name);
                    business.Name = model.Name;
                }

                if (model.Color != null)
                {
                    business.Color = model.Color;
                }

                if (model.Description != null)
                {
                    business.Description = model.Description;
                }

                if (model.ProfilePhoto != null)
                {
                    business.ProfilePhoto = model.ProfilePhoto;
                }

                if (model.Twitter != null)
                {
                    business.Twitter = model.Twitter;
                }

                if (model.Instagram != null)
                {
                    business.Instagram = model.Instagram;
                }

                if (model.Tiktok != null)
                {
                    business.Tiktok = model.Tiktok;
                }

                if (model.Latitude != null && model.Longitude != null)
                {
                    string location = _mapService.GetLocation(model.Latitude.Value, model.Longitude.Value).Result;
                    string timeZone = _mapService.GetTimeZone(model.Latitude.Value, model.Longitude.Value).Result;
                    business.Location = location;
                    business.TimeZone = timeZone;
                }

                _context.Businesses.Update(business);
                _context.SaveChanges();

                var response = _mapper.Map<BusinessResponse>(business);

                return response;
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        public List<Template> GetTemplates()
        {
            try
            {
                return _context.Templates.ToList();
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        public Template AddTemplate(CreateTemplate model)
        {
            try
            {
                var business = _context.Businesses.Find(model.BusinessId);

                if (business == null)
                {
                    throw new AppException("Invalid Buisness Id");
                }
                Template template = new Template
                {
                    Category = model.Category,
                    Details = model.Details,
                    Photo = model.Photo,
                    Business = business
                };

                _context.Templates.Add(template);
                _context.SaveChanges();

                return template;
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        public void DeleteTemplate(Guid id)
        {
            try
            {
                var template = _context.Templates.Find(id);

                if (template == null)
                {
                    throw new AppException("Invalid Template Id");
                }

                _context.Templates.Remove(template);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        private void validateSign(string sign)
        {
            if (sign.Length < 3)
            {
                throw new AppException("Invalid PEAS Sign: Your PEAS sign needs to be a minimum of 3 characters");
            }
        }

        private void validateName(string name)
        {
            if (name.Length < 5)
            {
                throw new AppException("Invalid Name: Your business name needs to be a minimum of 5 characters");
            }
        }
    }
}