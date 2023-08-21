using AutoMapper;
using PEAS.Entities.Authentication;
using PEAS.Entities.Site;
using PEAS.Models.Account;
using PEAS.Models.Business;

namespace PEAS.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Account, AuthenticateResponse>();
            CreateMap<RegisterRequest, Account>();
            CreateMap<Business, BusinessResponse>();
        }
    }
}