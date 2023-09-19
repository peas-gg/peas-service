using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PEAS.Entities;
using PEAS.Entities.Authentication;
using PEAS.Entities.Booking;
using PEAS.Entities.Site;
using PEAS.Helpers;
using PEAS.Helpers.Utilities;
using PEAS.Models.Business;
using PEAS.Models.Business.Order;
using PEAS.Models.Business.Schedule;

namespace PEAS.Services
{
    public interface IBusinessService
    {
        BusinessResponse GetBusiness(string sign);
        BusinessResponse GetBusiness(Account account);
        string GetLocation(double latitude, double longitude);
        BusinessResponse AddBusiness(Account account, CreateBusiness model);
        BusinessResponse UpdateBusiness(Account account, UpdateBusiness model);
        BusinessResponse AddBlock(Account account, Guid businessId, Block model);
        BusinessResponse UpdateBlock(Account account, Guid businessId, UpdateBlock model);
        BusinessResponse DeleteBlock(Account account, Guid businessId, Guid blockId);
        BusinessResponse SetSchedule(Account account, Guid businessId, List<ScheduleRequest> model);
        List<DateRange> GetAvailablity(Guid businessId, Guid blockId, DateTime date);
        OrderResponse CreateOrder(Guid businessId, OrderRequest model);
        OrderResponse UpdateOrder(Account account, Guid businessId, UpdateOrderRequest model);
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
                    .Include(x => x.Blocks).SingleOrDefault(x => x.Sign == sign && x.IsActive);

                if (business != null)
                {
                    return constructBusinessResponse(business, null);
                }
                else
                {
                    throw new AppException("Could not find the business you are looking for");
                }
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        public BusinessResponse GetBusiness(Account account)
        {
            try
            {
                var business = _context.Businesses
                    .AsNoTracking()
                    .Include(x => x.Blocks)
                    .FirstOrDefault(x => x.Account == account && x.IsActive);

                if (business != null)
                {
                    return constructBusinessResponse(business, account);
                }
                else
                {
                    throw new AppException("No business found. Please create a business");
                }
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        public string GetLocation(double latitude, double longitude)
        {
            try
            {
                return _mapService.GetLocation(latitude, longitude).Result;
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        public BusinessResponse AddBusiness(Account account, CreateBusiness model)
        {
            try
            {
                validateSign(model.Sign);
                validateName(model.Name);
                validateBlocks(model.Blocks);

                if (_context.Businesses.Any(x => x.Account == account && x.IsActive) && account.Role != Role.Admin)
                {
                    throw new AppException("You already have an existing business please login");
                }

                string location = _mapService.GetLocation(model.Latitude, model.Longitude).Result;
                string timeZone = _mapService.GetTimeZone(model.Latitude, model.Longitude).Result;

                var business = new Business
                {
                    Account = account,
                    Sign = model.Sign.Trim(),
                    Name = model.Name,
                    Category = model.Category,
                    Currency = Currency.CAD,
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
                    Blocks = model.Blocks,
                    Schedules = null
                };

                _context.Businesses.Add(business);
                _context.SaveChanges();

                return constructBusinessResponse(business, account);
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
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

                return constructBusinessResponse(business, account);
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        //Block
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

                return constructBusinessResponse(business, account);
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
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
                    block.Price = (int)model.Price;
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

                var response = constructBusinessResponse(business, account);

                return response;

            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
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

            block.Deleted = DateTime.Now;

            validateBlocks(business.Blocks);

            _context.Businesses.Update(business);
            _context.SaveChanges();
            return constructBusinessResponse(business, account);
        }

        //Schedule
        public BusinessResponse SetSchedule(Account account, Guid businessId, List<ScheduleRequest> model)
        {
            try
            {
                Business business = getBusiness(account, businessId);
                List<Schedule> schedules = new List<Schedule>();
                foreach (var schedule in model)
                {
                    schedules
                        .Add(
                            new Schedule
                            {
                                Business = business,
                                DayOfWeek = schedule.DayOfWeek,
                                StartTime = schedule.StartTime,
                                EndTime = schedule.EndTime
                            }
                        );
                }

                business.Schedules = schedules;
                _context.Businesses.Update(business);
                _context.SaveChanges();
                return constructBusinessResponse(business, account);
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        public List<DateRange> GetAvailablity(Guid businessId, Guid blockId, DateTime date)
        {
            try
            {
                //Get business
                Business? business = _context.Businesses
                    .AsNoTracking()
                    .Include(x => x.Blocks)
                    .Include(x => x.Schedules)
                    .Where(x => x.Id == businessId)
                    .FirstOrDefault();

                if (business == null)
                {
                    throw new AppException("Invalid Business Id");
                }

                Block block = getBlock(business, blockId);

                //Get availability for the day
                Schedule? schedule = business.Schedules?.Where(x => x.DayOfWeek == date.DayOfWeek).FirstOrDefault();

                if (schedule == null)
                {
                    return new List<DateRange>();
                }
                else
                {
                    //Get availability
                    List<Order>? ordersInTheDay = _context.Orders
                        .AsNoTracking()
                        .Where(x => x.OrderStatus != Order.Status.Declined && x.StartTime.Day == date.Day)
                        .ToList();
                    //Get existing orders for the selected date
                    List<DateRange> ordersDateRanges = ordersInTheDay.Select(x => new DateRange(x.StartTime, x.EndTime)).ToList() ?? new List<DateRange>();
                    return DateRange.GetAvailability(new DateRange(schedule.StartTime, schedule.EndTime), new TimeSpan(0, 0, block.Duration), ordersDateRanges);
                }
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        //Orders
        public OrderResponse CreateOrder(Guid businessId, OrderRequest model)
        {
            try
            {
                //Get business
                Business? business = _context.Businesses
                    .Include(x => x.Blocks)
                    .Where(x => x.Id == businessId)
                    .FirstOrDefault();

                if (business == null)
                {
                    throw new AppException("Invalid Business Id");
                }

                Block block = getBlock(business, model.BlockId);

                //Get Availability
                List<DateRange> availabilityDates = GetAvailablity(businessId, model.BlockId, model.DateRange.Start);

                if (!availabilityDates.Contains(model.DateRange))
                {
                    throw new AppException("The selected time is unavilable. Please select a different time.");
                }
                else
                {
                    //Create Customer
                    Customer? customer = _context.Customers.Where(x => x.Email == model.Email).FirstOrDefault();
                    if (customer == null)
                    {
                        Customer newCustomer = new Customer
                        {
                            FirstName = model.FirstName,
                            LastName = model.Lastname,
                            Email = model.Email,
                            Phone = model.Phone
                        };
                        _context.Customers.Add(newCustomer);
                        customer = newCustomer;
                    }
                    else
                    {
                        customer.FirstName = model.Email;
                        customer.LastName = model.Lastname;
                        customer.Phone = model.Phone;
                        _context.Customers.Update(customer);
                    }

                    //Create Order
                    DateTime dateNow = DateTime.UtcNow;
                    Order order = new Order
                    {
                        Business = business,
                        Block = block,
                        Customer = customer,
                        Currency = business.Currency,
                        Price = block.Price,
                        Title = block.Title,
                        Description = block.Description,
                        Image = block.Image,
                        Note = model.Note,
                        StartTime = model.DateRange.Start,
                        EndTime = model.DateRange.End,
                        OrderStatus = Order.Status.Pending,
                        Payment = null,
                        Created = dateNow,
                        LastUpdated = dateNow
                    };

                    _context.Orders.Add(order);
                    _context.SaveChanges();

                    return _mapper.Map<OrderResponse>(order);
                }
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        public OrderResponse UpdateOrder(Account account, Guid businessId, UpdateOrderRequest model)
        {
            try
            {
                Business? business = _context.Businesses
                    .Where(x => x.Account == account && x.Id == businessId)
                    .FirstOrDefault();

                if (business == null)
                {
                    throw new AppException("Invalid business Id");
                }

                Order? order = _context.Orders
                    .Where(x => x.Business.Id == businessId && x.Id == model.OrderId)
                    .FirstOrDefault();

                if (order == null)
                {
                    throw new AppException("Invalid Order Id");
                }

                order.OrderStatus = model.OrderStatus;
                order.LastUpdated = DateTime.UtcNow;

                _context.Orders.Update(order);
                _context.SaveChanges();

                return _mapper.Map<OrderResponse>(order);
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        //Templates
        public List<TemplateResponse> GetTemplates()
        {
            try
            {
                var templates = _context.Templates
                    .Include(x => x.Business)
                    .AsNoTracking()
                    .Select(x => new TemplateResponse
                    {
                        Id = x.Id,
                        Category = x.Category,
                        Details = x.Details,
                        Photo = x.Photo,
                        Business = constructBusinessResponse(x.Business, null)
                    })
                    .ToList();

                return templates;
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        public TemplateResponse AddTemplate(CreateTemplate model)
        {
            try
            {
                var business = _context.Businesses
                    .Include(x => x.Account)
                    .Where(x => x.Id == model.BusinessId)
                    .FirstOrDefault();

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
                    Business = constructBusinessResponse(template.Business, null)
                };
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
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
                throw AppException.ConstructException(e);
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
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException("Error loading business colors");
            }
        }

        private static BusinessResponse constructBusinessResponse(Business business, Account? account)
        {
            var businessResponse = new BusinessResponse
            {
                Id = business.Id,
                Sign = business.Sign,
                Name = business.Name,
                Category = business.Category,
                Color = business.Color,
                Description = business.Description,
                ProfilePhoto = business.ProfilePhoto,
                Twitter = business.Twitter,
                Instagram = business.Instagram,
                Tiktok = business.Tiktok,
                Location = business.Location,
                TimeZone = business.TimeZone,
                IsActive = business.IsActive,
                Blocks = business.Blocks
            };

            if ((account != null) && (business.Account == account))
            {
                businessResponse.Latitude = business.Latitude;
                businessResponse.Longitude = business.Longitude;
            }

            return businessResponse;
        }

        private Business getBusiness(Account account, Guid businessId)
        {
            var business = _context.Businesses.Include(x => x.Blocks.Where(y => !y.IsDeleted)).SingleOrDefault(x => x.Id == businessId);

            if (business == null || business.Account != account)
            {
                throw new AppException("Invalid Business Id");
            }

            return business;
        }

        private Block getBlock(Business business, Guid blockId)
        {
            Block? block = business.Blocks.Where(x => x.Id == blockId && !x.IsDeleted).FirstOrDefault();

            if (block == null)
            {
                throw new AppException("Invalid Block Id");
            }

            return block;
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

            foreach (Block block in blocks)
            {
                validateBlock(block);
            }
        }

        private void validateBlock(Block block)
        {
            int freePrice = Price.FreePrice;
            int minPrice = Price.MinPrice;
            int maxPrice = Price.MaxPrice;

            if (!Enum.IsDefined(typeof(Block.Type), block.BlockType))
            {
                throw new AppException($"Invalid block type {block.BlockType}");
            }

            if (block.Price > maxPrice)
            {
                throw new AppException($"Please enter a price below {maxPrice}");
            }

            if (block.Price < freePrice)
            {
                throw new AppException($"Enter a valid number for the price");
            }

            if (block.Price > freePrice && block.Price < minPrice)
            {
                throw new AppException($"The minimum price is {minPrice}");
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

            if (sign.Any(char.IsWhiteSpace))
            {
                throw new AppException("Invalid PEAS Sign: Your PEAS sign cannot contain white spaces");
            }

            if (_context.Businesses.AsNoTracking().Any(x => x.Sign == sign.Trim()))
            {
                throw new AppException($"PEAS Sign \"{sign}\" taken. Please choose another sign");
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