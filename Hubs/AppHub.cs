﻿using Microsoft.AspNetCore.SignalR;
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

        public async Task Subscribe()
        {
            try
            {
                Account? account = (Account?)Context.GetHttpContext()?.Items["Account"];
                if (account == null)
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("UnAuthorized");
                }
                else
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, account.Id.ToString());
                }
            }
            catch(Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }

        public async Task UnSubscribe(Guid accountId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, accountId.ToString());
            }
            catch(Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }

        public void OrderReceived(Order order, Account account)
        {
            try
            {
                string message = $"Reservation request from {order.Customer.FirstName} {order.Customer.LastName} for {order.Title}";
                Clients.User(account.Id.ToString()).SendAsync("OrderReceived", message);
            }
            catch(Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }

        public void PaymentReceived(Order order, Account account)
        {
            try
            {
                string message = $"Payment received from {order.Customer.FirstName} {order.Customer.LastName} (${Price.Format(order.Payment!.Total)})";
                Clients.User(account.Id.ToString()).SendAsync("PaymentReceived", message);
            }
            catch(Exception e)
            {
                AppLogger.Log(_logger, e);
            }
        }
    }
}