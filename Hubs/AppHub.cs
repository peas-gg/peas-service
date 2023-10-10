using Microsoft.AspNetCore.SignalR;
using PEAS.Entities;

namespace PEAS.Hubs
{
    public class AppHub : Hub
    {
        private readonly ILogger<AppHub> _logger;
        private readonly DataContext _dataContext;

        public AppHub(ILogger<AppHub> logger, DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
        }
    }
}