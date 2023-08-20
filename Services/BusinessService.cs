using System;
using System.Configuration;
using AutoMapper;
using PEAS.Entities;
using PEAS.Entities.Authentication;
using PEAS.Entities.Site;
using PEAS.Models.Business;

namespace PEAS.Services
{
    public interface IBusinessService
    {
        Business AddBusiness(Account account, CreateBusiness model);
        Template AddTemplate();
        List<Template> GetTemplate();
    }

    public class BusinessService : IBusinessService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        private readonly ILogger<BusinessService> _logger;

        public BusinessService(DataContext context, IMapper mapper, ILogger<BusinessService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public Business AddBusiness(Account account, CreateBusiness model)
        {
            throw new NotImplementedException();
        }

        public List<Template> GetTemplate()
        {
            throw new NotImplementedException();
        }

        public Template AddTemplate()
        {
            throw new NotImplementedException();
        }
    }
}