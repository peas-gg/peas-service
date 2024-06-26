﻿using AutoMapper;
using PEAS.Entities.Authentication;
using PEAS.Entities.Booking;
using PEAS.Entities.Site;
using PEAS.Models.Account;
using PEAS.Models.Business;
using PEAS.Models.Business.Order;
using PEAS.Models.Business.TimeBlock;

namespace PEAS.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Account, AuthenticateResponse>();
            CreateMap<RegisterRequest, Account>();
            CreateMap<Business, BusinessResponse>();
            CreateMap<Order, OrderResponse>();
            CreateMap<Withdrawal, WithdrawalResponse>();
            CreateMap<TimeBlock, TimeBlockResponse>();
        }
    }
}