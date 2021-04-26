using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JordanDeBordProject3.Hubs
{
    public class GroceryListHub : Hub
    {
        public async Task SendMessageToAllAsync(string message) 
        {
            await Clients.All.SendAsync("Notification", message);
        }
    }
}
