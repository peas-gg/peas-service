using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PEAS.Entities;
using PEAS.Entities.Authentication;
using PEAS.Entities.Site;
using PEAS.Helpers;
using PEAS.Models.Business;

namespace PEAS.Services
{
    public interface IBusinessService
    {
        BusinessResponse GetBusiness(string sign);
        BusinessResponse AddBusiness(Account account, CreateBusiness model);
        BusinessResponse UpdateBusiness(Account account, UpdateBusiness model);
        BusinessResponse AddBlock(Account account, Guid businessId, Block model);
        BusinessResponse UpdateBlock(Account account, Guid businessId, UpdateBlock model);
        BusinessResponse DeleteBlock(Account account, Guid businessId, Guid blockId);
        TemplateResponse AddTemplate(CreateTemplate model);
        void DeleteTemplate(Guid id);
        List<TemplateResponse> GetTemplates();
        Dictionary<string, string> GetColours();
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

        public BusinessResponse GetBusiness(string sign)
        {
            try
            {
                var business = _context.Businesses
                    .AsNoTracking()
                    .Include(x => x.Blocks).SingleOrDefault(x => x.Sign == sign);

                if (business != null)
                {
                    return ConstructBusinessResponse(business);
                } else
                {
                    throw new AppException("Could not find the business you are looking for");
                }
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        public BusinessResponse AddBusiness(Account account, CreateBusiness model)
        {
            try
            {
                validateSign(model.Sign);
                validateName(model.Name);
                validateBlocks(model.Blocks);

                if (_context.Businesses.Any(x => x.Account == account && x.IsActive))
                {
                    throw new AppException("You already have an existing business please login");
                }

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
                    Created = DateTime.UtcNow,
                    Blocks = model.Blocks
                };

                _context.Businesses.Add(business);
                _context.SaveChanges();

                return ConstructBusinessResponse(business, false);
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
                    throw new AppException("Invalid Business Id");
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

                return ConstructBusinessResponse(business, false);
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        public BusinessResponse AddBlock(Account account, Guid businessId, Block model)
        {
            try
            {
                Business business = getBusiness(account, businessId);

                validateBlock(model);

                business.Blocks.Add(model);

                validateBlocks(business.Blocks);

                _context.Businesses.Update(business);
                _context.SaveChanges();

                return ConstructBusinessResponse(business, false);
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        public BusinessResponse UpdateBlock(Account account, Guid businessId, UpdateBlock model)
        {
            try
            {
                Business business = getBusiness(account, businessId);
                Block? block = business.Blocks.Where(x => x.Id == model.Id).FirstOrDefault();

                if (block == null)
                {
                    throw new AppException("Invalid Block Id");
                }

                if (model.Image != null)
                {
                    block.Image = model.Image;
                }

                if (model.Price != null)
                {
                    block.Price = (double)model.Price;
                }

                if (model.Duration != null)
                {
                    block.Duration = (int)model.Duration;
                }

                if (model.Title != null)
                {
                    block.Title = model.Title;
                }

                if (model.Description != null)
                {
                    block.Description = model.Description;
                }

                validateBlocks(business.Blocks);

                _context.Businesses.Update(business);
                _context.SaveChanges();

                var response = ConstructBusinessResponse(business, false);

                return response;

            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        public BusinessResponse DeleteBlock(Account account, Guid businessId, Guid blockId)
        {
            Business business = getBusiness(account, businessId);
            Block? block = business.Blocks.Where(x => x.Id == blockId).FirstOrDefault();

            if (block == null)
            {
                throw new AppException("Invalid Block Id");
            }

            business.Blocks.Remove(block);

            validateBlocks(business.Blocks);

            _context.Businesses.Update(business);
            _context.SaveChanges();

            var response = _mapper.Map<BusinessResponse>(business);

            return response;
        }

        public List<TemplateResponse> GetTemplates()
        {
            try
            {
                var templates = _context.Templates
                    .Include(x => x.Business)
                    .AsNoTracking()
                    .Select(x => new TemplateResponse {
                        Id = x.Id,
                        Category = x.Category,
                        Details = x.Details,
                        Photo = x.Photo,
                        Business = ConstructBusinessResponse(x.Business, true)
                    })
                    .ToList();

                return templates;
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        public TemplateResponse AddTemplate(CreateTemplate model)
        {
            try
            {
                var business = _context.Businesses.Find(model.BusinessId);

                if (business == null)
                {
                    throw new AppException("Invalid Business Id");
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

                return new TemplateResponse
                {
                    Id = template.Id,
                    Category = template.Category,
                    Details = template.Details,
                    Photo = template.Photo,
                    Business = ConstructBusinessResponse(template.Business)
                };
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

        public Dictionary<string, string> GetColours()
        {
            try
            {
                var serializer = new JsonSerializer();
                using var streamReader = new StreamReader("Models/Colours.json");
                using var textReader = new JsonTextReader(streamReader);
                return serializer.Deserialize<Dictionary<string, string>>(textReader)!;
            }
            catch(Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException("Error loading business colors");
            }
        }

        private BusinessResponse ConstructBusinessResponse(Business business, bool isPublic = true)
        {
            var businessResponse = _mapper.Map<BusinessResponse>(business);
            if (isPublic)
            {
                businessResponse.Latitude = null;
                businessResponse.Longitude = null;
            }
            return businessResponse;
        }

        private Business getBusiness(Account account, Guid businessId)
        {
            var business = _context.Businesses.Include(x => x.Blocks).SingleOrDefault(x => x.Id == businessId);

            if (business == null || business.Account != account)
            {
                throw new AppException("Invalid Business Id");
            }

            return business;
        }

        private void validateBlocks(List<Block> blocks)
        {
            if (blocks == null || blocks.Count == 0)
            {
                throw new AppException("You must have at least 1 block at any given time");
            }

            if (blocks.Count > 5)
            {
                throw new AppException("You are only allowed to have 5 blocks at this moment");
            }

            foreach(Block block in blocks)
            {
                validateBlock(block);
            }
        }

        private void validateBlock(Block block)
        {
            double maxPrice = 5000.00;

            if (!Enum.IsDefined(typeof(Block.Type), block.BlockType))
            {
                throw new AppException($"Invalid block type {block.BlockType}");
            }

            if (block.Price > maxPrice)
            {
                throw new AppException($"Please type a price below {maxPrice}");
            }

            if (block.Price < 0.00)
            {
                throw new AppException($"Enter a valid number for the price");
            }

            if (string.IsNullOrEmpty(block.Title))
            {
                throw new AppException($"Please enter a Title");
            }

            if (string.IsNullOrEmpty(block.Description))
            {
                throw new AppException($"Please enter a Description");
            }
        }

        private void validateSign(string sign)
        {
            if (sign.Length < 3)
            {
                throw new AppException("Invalid PEAS Sign: Your PEAS sign needs to be a minimum of 3 characters");
            }

            if (sign.Length > 16)
            {
                throw new AppException("Invalid PEAS Sign: Your PEAS sign needs to be a maximum of 16 characters");
            }

            if (_context.Businesses.AsNoTracking().Any(x => x.Sign == sign))
            {
                throw new AppException("PEAS Sign taken. Please choose another sign");
            }
        }

        private void validateName(string name)
        {
            if (name.Length < 1)
            {
                throw new AppException("Invalid Name: Your business name needs to be a minimum of 1 character");
            }

            if (name.Length > 30)
            {
                throw new AppException("Invalid Name: Your business name needs to be a maximum of 30 characters");
            }
        }
    }
}