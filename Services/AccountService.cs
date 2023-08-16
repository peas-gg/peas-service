using AutoMapper;
using Microsoft.Extensions.Options;
using PEAS.Entities;
using PEAS.Helpers;
using PEAS.Models.Account;

namespace PEAS.Services
{
    public interface IAccountService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
    }

    public class AccountService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;

        private readonly ILogger<AccountService> _logger;

        public AccountService(
            DataContext context,
            IMapper mapper,
            IOptions<AppSettings> appSettings,
            ILogger<AccountService> logger)
        {
            _context = context;
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _logger = logger;
        }
    }
}