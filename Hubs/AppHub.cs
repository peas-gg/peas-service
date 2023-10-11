using Microsoft.AspNetCore.SignalR;
using PEAS.Entities;
using PEAS.Entities.Authentication;
using PEAS.Entities.Booking;
using PEAS.Helpers;
using PEAS.Helpers.Utilities;

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

        public async Task Subscribe(Guid accountId)
        {
            try
            {
                Account? account = getAccount(accountId);
                if (account == null)
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("CouldNotSubscribe");
                }
                else
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, account.Id.ToString());
                }
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }

        public void UnSubscribe(Guid accountId)
        {
            try
            {
                Groups.RemoveFromGroupAsync(Context.ConnectionId, accountId.ToString());
            }
            catch (Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }

        private Account? getAccount(Guid id)
        {
            try
            {
                Account? account = _dataContext.Accounts.Find(id);
                return account;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}