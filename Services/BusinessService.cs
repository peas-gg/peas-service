using System;
using System.Configuration;
using AutoMapper;
using PEAS.Entities;
using PEAS.Entities.Site;

namespace PEAS.Services
{
    public interface IBusinessService
    {
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

        public List<Template> GetTemplate()
        {
            throw new NotImplementedException();
        }
    }
}