using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PEAS.Entities;
using PEAS.Entities.Authentication;
using PEAS.Helpers;
using PEAS.Models.Account;
using BC = BCrypt.Net.BCrypt;

namespace PEAS.Services
{
    public interface IAccountService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
        AuthenticateResponse Register(RegisterRequest model, string ipAddress);
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

        public AuthenticateResponse Register(RegisterRequest model, string ipAddress)
        {
            try
            {
                //Validate model
                validateRegisterModel(model);

                if (_context.Accounts.Any(x => x.Email == model.Email))
                {
                    throw new AppException("Account exists. Please login");
                }

                //Map model to new account object
                Account account = _mapper.Map<Account>(model);

                account.Role = Role.User;
                account.Created = DateTime.UtcNow;

                //Hash password
                account.Password = BC.HashPassword(model.PasswordText);

                //Generate jwt and refresh tokens for authentication
                var jwtToken = generateJwtToken(account);
                var refreshToken = generateRefreshToken(account, ipAddress);

                //Add Refresh Token
                account.RefreshTokens = new List<RefreshToken>
                {
                    refreshToken
                };

                //Save account
                _context.Accounts.Add(account);

                //Save changes to db
                _context.Update(account);
                _context.SaveChanges();

                var response = _mapper.Map<AuthenticateResponse>(account);
                response.JwtToken = jwtToken;
                response.RefreshToken = refreshToken.Token;
                return response;
            }
            catch(Exception e)
            {
                throw new AppException(e.Message);
            }
        }

        private void validateRegisterModel(RegisterRequest model)
        {
            if (!isValidEmail(model.Email))
            {
                throw new AppException("Invalid Email");
            }

            if (String.IsNullOrWhiteSpace(model.FirstName) || String.IsNullOrWhiteSpace(model.LastName))
            {
                throw new AppException("Invalid first name or last name");
            }

            if (String.IsNullOrWhiteSpace(model.Phone))
            {
                throw new AppException("Invalid phone number");
            }

            if (!isPasswordValid(model.PasswordText))
            {
                throw new AppException("Password should be at least 6 characters");
            }

            if (!model.AcceptTerms)
            {
                throw new AppException("Please accept the terms and conditions to continue");
            }

            //Validate the OTPCode
        }

        private bool isValidEmail(string email)
        {
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            if (string.IsNullOrEmpty(email))
                return false;

            Regex regex = new Regex(emailPattern);
            return regex.IsMatch(email);
        }

        private bool isPasswordValid(string password)
        {
            return password.Trim().Length > 6;
        }

        private string generateJwtToken(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", account.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken generateRefreshToken(Account account, string ipAddress)
        {
            return new RefreshToken
            {
                Account = account,
                Token = randomTokenString(),
                Expires = DateTime.UtcNow.AddDays(30),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        private string randomTokenString()
        {
            using var rng = RandomNumberGenerator.Create();
            var randomNumber = new byte[40];
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}