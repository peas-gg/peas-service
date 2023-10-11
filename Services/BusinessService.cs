using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PEAS.Entities;
using PEAS.Entities.Authentication;
using PEAS.Entities.Booking;
using PEAS.Entities.Site;
using PEAS.Helpers;
using PEAS.Helpers.Utilities;
using PEAS.Hubs;
using PEAS.Models;
using PEAS.Models.Business;
using PEAS.Models.Business.Order;
using PEAS.Services.Email;

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
        BusinessResponse SetSchedule(Account account, Guid businessId, List<ScheduleModel> model);
        List<DateRange> GetAvailablity(Guid businessId, Guid blockId, DateTime date);
        List<Customer> GetCustomers(Account account, Guid businessId);
        OrderResponseLite GetOrder(Guid orderId);
        List<OrderResponse> GetOrders(Account account, Guid businessId);
        OrderResponse CreateOrder(Guid businessId, OrderRequest model);
        OrderResponse RequestPayment(Account account, Guid businessId, PaymentRequest model);
        OrderResponse UpdateOrder(Account account, Guid businessId, UpdateOrderRequest model);
        WalletResponse GetWallet(Account account, Guid businessId);
        WalletResponse Withdraw(Account account, Guid businessId);
        EmptyResponse CompleteWithdraw(Guid withdrawalId);
        TemplateResponse AddTemplate(CreateTemplate model);
        void DeleteTemplate(Guid id);
        List<TemplateResponse> GetTemplates();
        Dictionary<string, string> GetColours();
    }

    public class BusinessService : IBusinessService
    {
        private readonly DataContext _context;
        private readonly IMapService _mapService;
        private readonly IEmailService _emailService;
        private readonly IHubContext<AppHub> _hubContext;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<BusinessService> _logger;

        public BusinessService(
                DataContext context,
                IMapService mapService,
                IEmailService emailService,
                IHubContext<AppHub> hubContext,
                IPushNotificationService pushNotificationService,
                IMapper mapper,
                ILogger<BusinessService> logger
            )
        {
            _context = context;
            _mapService = mapService;
            _emailService = emailService;
            _hubContext = hubContext;
            _pushNotificationService = pushNotificationService;
            _mapper = mapper;
            _logger = logger;
        }

        public BusinessResponse GetBusiness(string sign)
        {
            try
            {
                var business = _context.Businesses
                    .AsNoTracking()
                    .Include(x => x.Account)
                    .Include(x => x.Blocks).SingleOrDefault(x => x.Sign == sign && x.IsActive);

                if (business != null)
                {
                    return constructBusinessResponse(business, null, getCompletedOrderCount(business.Id));
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
                    .Include(x => x.Account)
                    .Include(x => x.Blocks)
                    .FirstOrDefault(x => x.Account == account && x.IsActive);

                if (business != null)
                {
                    return constructBusinessResponse(business, account, getCompletedOrderCount(business.Id));
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
                    Sign = model.Sign.Trim().ToLower(),
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

                return constructBusinessResponse(business, account, 0);
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
                var business = _context.Businesses.Include(x => x.Account).SingleOrDefault(x => x.Id == model.Id);

                if (business == null || business.Account.Id != account.Id)
                {
                    throw new AppException("Invalid Business Id");
                }

                if (model.Sign != null)
                {
                    validateSign(model.Sign);
                    business.Sign = model.Sign.Trim().ToLower();
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

                return constructBusinessResponse(business, account, getCompletedOrderCount(model.Id));
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

                return constructBusinessResponse(business, account, getCompletedOrderCount(businessId));
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

                var response = constructBusinessResponse(business, account, getCompletedOrderCount(businessId));

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

            business.Blocks.Remove(block);

            validateBlocks(business.Blocks);

            _context.Businesses.Update(business);
            _context.SaveChanges();
            return constructBusinessResponse(business, account, getCompletedOrderCount(businessId));
        }

        //Schedule
        public BusinessResponse SetSchedule(Account account, Guid businessId, List<ScheduleModel> model)
        {
            try
            {
                Business business = getBusiness(account, businessId);
                List<Schedule> schedules = new List<Schedule>();
                foreach (var schedule in model)
                {
                    if (schedule.StartTime > schedule.EndTime)
                    {
                        throw new AppException("Invalid Schedule: The start time for each day must be less that the end time.");
                    }

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

                business.Schedules?.Clear();
                business.Schedules = schedules;

                _context.Businesses.Update(business);
                _context.SaveChanges();
                return constructBusinessResponse(business, account, getCompletedOrderCount(businessId));
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
                    //Get the schedule for the date the user wants
                    DateTime startDate = date.ResetTimeToStartOfDay().Add(new TimeSpan(schedule.StartTime.Hour, schedule.StartTime.Minute, 0));
                    DateTime endDate = startDate + (schedule.EndTime - schedule.StartTime);
                    DateRange scheduleForTheDate = new DateRange(startDate, endDate);

                    //Get availability
                    List<Order>? ordersInTheDay = _context.Orders
                        .AsNoTracking()
                        .Where(x => x.Business.Id == businessId && x.OrderStatus != Order.Status.Declined && x.StartTime > DateTime.UtcNow)
                        .ToList();

                    //Get existing orders for the selected date
                    List<DateRange> ordersDateRanges = ordersInTheDay.Select(x => new DateRange(x.StartTime, x.EndTime)).ToList() ?? new List<DateRange>();
                    return DateRange.GetAvailability(scheduleForTheDate, new TimeSpan(0, 0, block.Duration), ordersDateRanges);
                }
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        public List<Customer> GetCustomers(Account account, Guid businessId)
        {
            try
            {
                Business? business = _context.Businesses
                    .AsNoTracking()
                    .Where(x => x.Id == businessId && x.Account.Id == account.Id)
                    .FirstOrDefault();

                if (business == null)
                {
                    throw new AppException("Invalid Business Id: Could not retrieve customers");
                }

                //Get customers from orders
                List<Customer> customers = _context.Orders
                    .AsNoTracking()
                    .Include(x => x.Customer)
                    .Where(x => x.Business.Id == businessId)
                    .Select(x => x.Customer)
                    .AsEnumerable()
                    .DistinctBy(x => x.Email)
                    .ToList();

                return customers;
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        //Orders
        public OrderResponseLite GetOrder(Guid orderId)
        {
            try
            {
                Order? order = _context.Orders.AsNoTracking().Include(x => x.Business).First(x => x.Id == orderId);
                if (order == null)
                {
                    throw new AppException("Invalid orderId");
                }

                return new OrderResponseLite {
                    BusinessSign = order.Business.Sign,
                    BusinessName = order.Business.Name,
                    BusinessProfilePhoto = order.Business.ProfilePhoto,
                    Currency = order.Currency,
                    Price = order.Price,
                    Title = order.Title,
                    Description = order.Description,
                    Image = order.Image,
                    StartTime = order.StartTime
                };
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        public List<OrderResponse> GetOrders(Account account, Guid businessId)
        {
            try
            {
                Business? business = _context.Businesses.Include(x => x.Account).First(x => x.Id == businessId);
                if (business == null || business.Account.Id != account.Id)
                {
                    throw new AppException("Invalid buinessId");
                }
                var orders = _context.Orders
                    .Include(x => x.Customer)
                    .Where(x => x.Business.Id == business.Id).ToList();

                return _mapper.Map<List<OrderResponse>>(orders);
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        public OrderResponse CreateOrder(Guid businessId, OrderRequest model)
        {
            try
            {
                //Get business
                Business? business = _context.Businesses
                    .Include(x => x.Blocks)
                    .Include(x => x.Account)
                    .ThenInclude(x => x.Devices)
                    .Where(x => x.Id == businessId)
                    .FirstOrDefault();

                if (business == null)
                {
                    throw new AppException("Invalid Business Id");
                }

                Block block = getBlock(business, model.BlockId);

                //Get Availability
                List<DateRange> availabilityDates = GetAvailablity(businessId, model.BlockId, model.Date);
                DateRange? timeSlot = availabilityDates.FirstOrDefault(x => x.Start == model.DateRange.Start);

                if (timeSlot == null)
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
                            FirstName = model.FirstName.Trim(),
                            LastName = model.Lastname.Trim(),
                            Email = model.Email.Trim(),
                            Phone = model.Phone.Trim()
                        };
                        _context.Customers.Add(newCustomer);
                        customer = newCustomer;
                    }
                    else
                    {
                        customer.FirstName = model.FirstName.Trim();
                        customer.LastName = model.Lastname.Trim();
                        customer.Phone = model.Phone.Trim();
                        _context.Customers.Update(customer);
                    }

                    //Create Order
                    DateTime dateNow = DateTime.UtcNow;
                    Order order = new Order
                    {
                        Business = business,
                        Customer = customer,
                        Currency = business.Currency,
                        Price = block.Price,
                        Title = block.Title,
                        Description = block.Description,
                        Image = block.Image,
                        Note = model.Note,
                        StartTime = timeSlot.Start,
                        EndTime = timeSlot.End,
                        OrderStatus = Order.Status.Pending,
                        Payment = null,
                        DidRequestPayment = false,
                        Created = dateNow,
                        LastUpdated = dateNow
                    };

                    _context.Orders.Add(order);
                    _context.SaveChanges();

                    //Send Email to user stating the reservation has been requested
                    _emailService.SendOrderEmail(order, business);

                    //Send to Hub
                    sendOrderRequestToHub(business.Account, order);

                    //Send Push Notification to the business owner
                    _pushNotificationService.SendNewOrderPush(business.Account, order);

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
                (Business business, Order order) = getOrder(account, businessId, model.OrderId);
                if (order.OrderStatus == Order.Status.Declined)
                {
                    throw new AppException("Cannot update a service that has been declined");
                }

                switch (model.OrderStatus)
                {
                    case Order.Status.Pending:
                        throw new AppException("Cannot set a service to pending");
                    case Order.Status.Approved:
                        if (order.StartTime < DateTime.UtcNow)
                        {
                            throw new AppException("This service cannot be approved because it's start time is in the past");
                        }
                        order.OrderStatus = Order.Status.Approved;
                        //Send email to customer
                        _emailService.SendOrderEmail(order, business);
                        break;
                    case Order.Status.Declined:
                        if (order.Payment != null)
                        {
                            throw new AppException("You cannot decline a service that has been paid for. Please reach out for help @ hello@peas.gg");
                        }
                        order.OrderStatus = Order.Status.Declined;
                        //Send email to the customer
                        _emailService.SendOrderEmail(order, business);
                        break;
                    case Order.Status.Completed:
                        if (order.OrderStatus != Order.Status.Approved)
                        {
                            throw new AppException("You cannot complete a service you did not approve");
                        }
                        order.OrderStatus = Order.Status.Completed;
                        break;
                }

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

        public OrderResponse RequestPayment(Account account, Guid businessId, PaymentRequest model)
        {
            try
            {
                (Business business, Order order) = getOrder(account, businessId, model.OrderId);

                if (order.OrderStatus != Order.Status.Approved)
                {
                    throw new AppException("Cannot request payment for a service that is not approved");
                }

                if (model.Price > Price.MaxPrice)
                {
                    throw new AppException($"Price cannot be more than {Price.Format(Price.MaxPrice)}");
                }

                if (model.Price > Price.FreePrice && model.Price < Price.MinPrice)
                {
                    throw new AppException($"The minimum price is ${Price.Format(Price.MinPrice)}");
                }

                order.Price = model.Price;

                //Send email to customer
                _emailService.SendPaymentEmail(order, business);

                order.DidRequestPayment = true;
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

        //Wallet
        public WalletResponse GetWallet(Account account, Guid businessId)
        {
            try
            {
                Business? business = _context.Businesses.AsNoTracking().Include(x => x.Account).Where(x => x.Id == businessId).FirstOrDefault();

                if (business == null || business.Account.Id != account.Id)
                {
                    throw new AppException("Invalid buinessId");
                }

                IEnumerable<Order> orders = _context.Orders.AsNoTracking().Include(x => x.Payment).Where(x => x.Business.Id == business.Id && x.Payment != null && x.Payment.Completed != null);
                IEnumerable<Withdrawal> withdrawals = _context.Withdrawals.AsNoTracking().Where(x => x.Business.Id == business.Id);

                long earnings = 0;
                long earningsWithdrawed = 0;
                long holdBalance = 0;

                DateTime maxWithdrawlDateDate = DateTime.UtcNow.AddDays(-2);

                WalletResponse walletResponse = new WalletResponse
                {
                    Balance = 0,
                    HoldBalance = 0,
                    Transactions = new List<WalletResponse.TransactionResponse>()
                };

                foreach (Order order in orders)
                {
                    if (order.Payment != null && order.Payment.Completed != null)
                    {
                        earnings += order.Payment.Total;
                        if (order.Payment.Completed > maxWithdrawlDateDate)
                        {
                            holdBalance += order.Payment.Total;
                        }
                    }
                    walletResponse.Transactions.Add(
                        new WalletResponse.TransactionResponse
                        {
                            TransactionType = WalletResponse.TransactionResponse.Type.Earning,
                            Earning = new EarningResponse
                            {
                                OrderId = order.Id,
                                Title = order.Title,
                                Base = order.Payment!.Base,
                                Deposit = order.Payment!.Deposit,
                                Tip = order.Payment!.Tip,
                                Fee = order.Payment!.Fee,
                                Total = order.Payment!.Total,
                                Created = order.Payment!.Created,
                                Completed = order.Payment!.Completed
                            }
                        }
                    );
                } 

                foreach (Withdrawal withdrawal in withdrawals)
                {
                    if (withdrawal.WithdrawalStatus != Withdrawal.Status.Failed)
                    {
                        earningsWithdrawed += withdrawal.Amount;
                    }
                    walletResponse.Transactions.Add(
                       new WalletResponse.TransactionResponse
                       {
                           TransactionType = WalletResponse.TransactionResponse.Type.Withdrawal,
                           Withdrawal = _mapper.Map<WithdrawalResponse>(withdrawal)
                       }
                   );
                }

                walletResponse.Balance = earnings - earningsWithdrawed;
                walletResponse.HoldBalance = holdBalance;

                return walletResponse;
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        public WalletResponse Withdraw(Account account, Guid businessId)
        {
            try
            {
                Business? business = _context.Businesses.AsNoTracking().Include(x => x.Account).Where(x => x.Id == businessId).FirstOrDefault();

                if (business == null || business.Account.Id != account.Id)
                {
                    throw new AppException("Invalid buinessId");
                }

                WalletResponse walletResponse = GetWallet(account, businessId);

                long amountToWithdraw = (walletResponse.Balance - walletResponse.HoldBalance);

                if (amountToWithdraw > 0)
                {
                    Withdrawal withdrawal = new Withdrawal
                    {
                        Business = business,
                        Amount = amountToWithdraw,
                        WithdrawalStatus = Withdrawal.Status.Pending,
                        Created = DateTime.UtcNow,
                    };

                    _context.Withdrawals.Add(withdrawal);
                    _context.SaveChanges();

                    _emailService.SendWithdrawEmailToAdmin(account, withdrawal);

                    return GetWallet(account, businessId);
                }
                else
                {
                    throw new AppException($"Cannot withdraw an invalid amount {amountToWithdraw}");
                }
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw AppException.ConstructException(e);
            }
        }

        public EmptyResponse CompleteWithdraw(Guid withdrawalId)
        {
            try
            {
                Withdrawal? withdrawal = _context.Withdrawals.Find(withdrawalId);

                if (withdrawal == null)
                {
                    throw new AppException("Invalid withdrawalId");
                }

                if (withdrawal.WithdrawalStatus == Withdrawal.Status.Succeeded)
                {
                    throw new AppException("Withdrawal has already been completed");
                }

                withdrawal.WithdrawalStatus = Withdrawal.Status.Succeeded;
                withdrawal.Completed = DateTime.UtcNow;

                _context.Withdrawals.Update(withdrawal);
                _context.SaveChanges();

                return new EmptyResponse();
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
                        Business = constructBusinessResponse(x.Business, null, 0)
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
                    Business = constructBusinessResponse(template.Business, null, 0)
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

        private async void sendOrderRequestToHub(Account account, Order order)
        {
            try
            {
                string message = $"Reservation request from {order.Customer.FirstName} {order.Customer.LastName} for {order.Title}";
                await _hubContext.Clients.Group(account.Id.ToString()).SendAsync("OrderReceived", message);
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }

        private int getCompletedOrderCount(Guid businessId)
        {
            return _context.Orders.AsNoTracking().Where(x => x.Business.Id == businessId && x.OrderStatus == Order.Status.Completed).Count();
        }

        private static BusinessResponse constructBusinessResponse(Business business, Account? account, int orderCount)
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
                OrderCount = orderCount,
                Twitter = business.Twitter,
                Instagram = business.Instagram,
                Tiktok = business.Tiktok,
                Location = business.Location,
                TimeZone = business.TimeZone,
                IsActive = business.IsActive,
                Blocks = business.Blocks,
                Schedules = null
            };

            if ((account != null) && (business.Account.Id == account.Id))
            {
                businessResponse.Latitude = business.Latitude;
                businessResponse.Longitude = business.Longitude;
                businessResponse.Schedules = business.Schedules == null ? null : mapScheduleModel(business.Schedules);
            }

            return businessResponse;
        }

        private static List<ScheduleModel> mapScheduleModel(List<Schedule> schedules)
        {
            List<ScheduleModel> response = new List<ScheduleModel>();
            foreach (var schedule in schedules)
            {
                response.Add(
                    new ScheduleModel
                    {
                        Id = schedule.Id,
                        DayOfWeek = schedule.DayOfWeek,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime
                    }
                );
            }
            return response;
        }

        private (Business, Order) getOrder(Account account, Guid businessId, Guid orderId)
        {
            Business? business = _context.Businesses
                    .Where(x => x.Account.Id == account.Id && x.Id == businessId)
                    .FirstOrDefault();

            if (business == null)
            {
                throw new AppException("Invalid business Id");
            }

            Order? order = _context.Orders
                .Include(x => x.Customer)
                .Where(x => x.Business.Id == businessId && x.Id == orderId)
                .FirstOrDefault();

            if (order == null)
            {
                throw new AppException("Invalid Order Id");
            }
            return (business, order);
        }

        private Business getBusiness(Account account, Guid businessId)
        {
            var business = _context.Businesses
                .Include(x => x.Account)
                .Include(x => x.Blocks)
                .SingleOrDefault(x => x.Id == businessId);

            if (business == null || business.Account.Id != account.Id)
            {
                throw new AppException("Invalid Business Id");
            }

            return business;
        }

        private Block getBlock(Business business, Guid blockId)
        {
            Block? block = business.Blocks.Where(x => x.Id == blockId).FirstOrDefault();

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

            blocks.Sort((x, y) => x.Index.CompareTo(y.Index));
            for(int i = 0; i < blocks.Count; i++)
            {
                blocks[i].Index = i;
                validateBlock(blocks[i]);
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
                throw new AppException($"Please enter a price below ${Price.Format(maxPrice)}");
            }

            if (block.Price < freePrice)
            {
                throw new AppException($"Enter a valid number for the price");
            }

            if (block.Price > freePrice && block.Price < minPrice)
            {
                throw new AppException($"The minimum price is ${Price.Format(minPrice)}");
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