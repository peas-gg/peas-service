using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using PEAS.Entities;
using PEAS.Entities.Authentication;
using PEAS.Helpers;
using PEAS.Models;
using PEAS.Models.Account;
using BC = BCrypt.Net.BCrypt;

namespace PEAS.Services
{
    public interface IAccountService
    {
        AuthenticateResponse? Authenticate(AuthenticateRequest model, string ipAddress);
        AuthenticateResponse Register(RegisterRequest model, string ipAddress);
        string? ValidatePhoneNumber(string phoneNumber);
        EmptyResponse RequestVerificationCode(string phoneNumber);
        EmptyResponse RequestPasswordReset(string email);
        EmptyResponse ResetPassword(ResetPasswordRequest model);
    }

    public class AccountService : IAccountService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly ITwilioService _twilioService;
        private readonly IMapper _mapper;

        private readonly ILogger<AccountService> _logger;

        public AccountService(
            DataContext context,
            IConfiguration configuration,
            ITwilioService twilioService,
            IMapper mapper,
            ILogger<AccountService> logger)
        {
            _context = context;
            _configuration = configuration;
            _twilioService = twilioService;
            _mapper = mapper;
            _logger = logger;
        }

        public AuthenticateResponse? Authenticate(AuthenticateRequest model, string ipAddress)
        {
            try
            {
                var account = _context.Accounts.SingleOrDefault(x => x.Email == model.Email);

                if (account == null)
                {
                    throw new AppException("Email or password is incorrect");
                }

                string accountLockedMessage = "Your account is locked due to multiple incorrect passwords. Please reset your password";
                //Check if account is locked
                if (account.IsLocked)
                {
                    throw new AppException(accountLockedMessage);
                }

                //Check Login Attempt and set maximum of 10 attempts then lock
                if (!BC.Verify(model.Password, account.Password))
                {
                    account.LoginAttempt++;
                    if (account.LoginAttempt >= 10)
                    {
                        account.IsLocked = true;
                        //Save changes to db
                        _context.Update(account);
                        _context.SaveChanges();
                        throw new AppException(accountLockedMessage);
                    }
                    //Save changes to db
                    _context.Update(account);
                    _context.SaveChanges();
                    throw new AppException("Email or password is incorrect");
                }

                //Reset login attempt to zero after successful login
                account.LoginAttempt = 0;


                //Check Otp
                if (model.Code == null)
                {
                    //Save changes to db
                    _context.Update(account);
                    _context.SaveChanges();

                    _ = RequestVerificationCode(account.Phone);
                    return null;
                }
                else
                {
                    //Check Twilio
                    _ = validateVerificationCode(account.Phone, model.Code);

                    // authentication successful so generate jwt and refresh tokens
                    var jwtToken = generateJwtToken(account);
                    var refreshToken = generateRefreshToken(account, ipAddress);


                    // remove old refresh tokens from account
                    removeOldRefreshTokens(account);

                    account.RefreshTokens?.Add(refreshToken);

                    //Save changes to db
                    _context.Update(account);
                    _context.SaveChanges();

                    var response = _mapper.Map<AuthenticateResponse>(account);
                    response.JwtToken = jwtToken;
                    response.RefreshToken = refreshToken.Token;
                    return response;
                }
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        public AuthenticateResponse Register(RegisterRequest model, string ipAddress)
        {
            try
            {
                string validatedPhoneNumber = ValidatePhoneNumber(model.Phone) ?? "";
                model.Phone = validatedPhoneNumber;

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

                //Save changes to db
                _context.Accounts.Add(account);
                _context.SaveChanges();

                var response = _mapper.Map<AuthenticateResponse>(account);
                response.JwtToken = jwtToken;
                response.RefreshToken = refreshToken.Token;
                return response;
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        public EmptyResponse RequestPasswordReset(string email)
        {
            try
            {
                var account = _context.Accounts.SingleOrDefault(x => x.Email == email);
                if (account != null)
                {
                    _twilioService.RequestCode(account.Phone);
                }
                return new EmptyResponse();
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                return new EmptyResponse();
            }
        }

        public EmptyResponse ResetPassword(ResetPasswordRequest model)
        {
            try
            {
                var account = _context.Accounts.SingleOrDefault(x => x.Email == model.Email);

                if (account == null)
                {
                    throw new AppException("Please ensure your email address is correct");
                }

                //Validate Otp Code
                _ = validateVerificationCode(account.Phone, model.Code);

                //Update password
                account.Password = BC.HashPassword(model.Password);

                //Remove old refresh tokens from account
                removeOldRefreshTokens(account);

                _context.Accounts.Update(account);
                _context.SaveChanges();
                return new EmptyResponse();
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        public EmptyResponse RequestVerificationCode(string phoneNumber)
        {
            try
            {
                string? validatedPhoneNumber = ValidatePhoneNumber(phoneNumber);

                if (validatedPhoneNumber == null)
                {
                    throw new AppException("Invalid Phone Number");
                }

                _twilioService.RequestCode(validatedPhoneNumber);
                return new EmptyResponse();
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
        }

        public string? ValidatePhoneNumber(string phoneNumber)
        {
            return _twilioService.ValidatePhoneNumber(phoneNumber);
        }

        private void validateRegisterModel(RegisterRequest model)
        {
            if (!isValidEmail(model.Email))
            {
                throw new AppException("Invalid Email");
            }

            if (!isNameValid(model.FirstName) || !isNameValid(model.LastName))
            {
                throw new AppException("Invalid first name or last name");
            }

            if (string.IsNullOrEmpty(model.Phone))
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
            _ = validateVerificationCode(model.Phone, model.Code);
        }

        private EmptyResponse validateVerificationCode(string phoneNumber, string code)
        {
            try
            {
                bool result = _twilioService.IsCodeValid(phoneNumber, code);

                if (!result)
                {
                    throw new AppException("Invalid Code");
                }

                return new EmptyResponse();
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
                throw new AppException(e.Message);
            }
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
            return password.Trim().Length >= 6;
        }

        private bool isNameValid(string name)
        {
            string namePattern = @"^[A-Z][a-zA-Z]*$";
            if (string.IsNullOrEmpty(name) || name.Length > 20)
                return false;

            Regex regex = new Regex(namePattern);
            return regex.IsMatch(name);
        }

        private string generateJwtToken(Account account)
        {
            int expiryMinutes = _configuration.GetSection("Environment").Value == "Development" ? 30 : 10;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("Secret").Value!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", account.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
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

        private void removeOldRefreshTokens(Account account)
        {
            //Remove all refresh tokens. This should prevent the user from having two active sessions at a time
            account.RefreshTokens?.RemoveAll(x => x.Created <= DateTime.UtcNow);
        }
    }
}