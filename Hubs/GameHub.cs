using Microsoft.AspNetCore.SignalR;
using pokerapi.Models;

namespace pokerapi.Hubs{
    public class GameHub : Hub
    {
        // Implement methods for game-related events
        // Example: Send player information to all connected clients
        public async Task SendPlayerInfo(Player player)
        {
            await Clients.All.SendAsync("ReceivePlayerInfo", player);
        }
    }
}