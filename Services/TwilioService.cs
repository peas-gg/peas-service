using System;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PEAS.Entities;
using PEAS.Helpers;
using Twilio;
using Twilio.Rest.Lookups.V2;

namespace PEAS.Services
{
    public interface ITwilioService
    {
        string? ValidatePhoneNumber(string number);
    }

    public class TwilioService : ITwilioService
    {
        private readonly IConfiguration Configuration;

        public TwilioService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string? ValidatePhoneNumber(string number)
        {
            string accountSid = Configuration.GetSection("Twilio:AccountSid").Value ?? "";
            string authToken = Configuration.GetSection("Twilio:AuthToken").Value ?? "";

            TwilioClient.Init(accountSid, authToken);

            var phoneNumber = PhoneNumberResource.Fetch(pathPhoneNumber: number);

            return (phoneNumber.Valid ?? false) ? phoneNumber.PhoneNumber.ToString() : null;
        }
    }
}