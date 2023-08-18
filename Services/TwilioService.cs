using System;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using PEAS.Entities;
using PEAS.Helpers;
using Twilio;
using Twilio.Rest.Lookups.V2;
using Twilio.Rest.Verify.V2.Service;

namespace PEAS.Services
{
    public interface ITwilioService
    {
        string? ValidatePhoneNumber(string number);
        void RequestCode(string number);
        bool IsCodeValid(string number, string code);
    }

    public class TwilioService : ITwilioService
    {
        private readonly IConfiguration _configuration;
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _pathServiceId;

        public TwilioService(IConfiguration configuration)
        {
            _configuration = configuration;
            _accountSid = _configuration.GetSection("Twilio:AccountSid").Value ?? "";
            _authToken = _configuration.GetSection("Twilio:AuthToken").Value ?? "";
            _pathServiceId = _configuration.GetSection("Twilio:PathServiceSid").Value ?? "";
        }

        public string? ValidatePhoneNumber(string number)
        {
            TwilioClient.Init(_accountSid, _authToken);

            var phoneNumber = PhoneNumberResource.Fetch(pathPhoneNumber: number);

            return (phoneNumber.Valid ?? false) ? phoneNumber.PhoneNumber.ToString() : null;
        }

        public void RequestCode(string number)
        {
            TwilioClient.Init(_accountSid, _authToken);
            _ = VerificationResource.Create(
                to: number,
                channel: "sms",
                pathServiceSid: _pathServiceId
            );
        }

        public bool IsCodeValid(string number, string code)
        {
            TwilioClient.Init(_accountSid, _authToken);

            var verificationCheck = VerificationCheckResource.Create(
                to: number,
                code: code,
                pathServiceSid: _pathServiceId
            );

            return verificationCheck.Status == "approved";
        }
    }
}