using AutoMapper;
using PEAS.Entities.Authentication;
using PEAS.Models.Account;

namespace PEAS.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Account, AuthenticateResponse>();
            CreateMap<RegisterRequest, Account>();
        }
    }
}