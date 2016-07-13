using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;
using UnicornHack.Models.GameState;

namespace UnicornHack.Hubs
{
    [HubName("gameHub")]
    public class GameHub : Hub<IGameClient>
    {
        public void Send(string name, string message)
        {
            Clients.All.AddNewMessageToPage(name + " " + Context.User.Identity.Name, message);
        }

        public Task PerformAction(string action, string target)
        {
            return Task.FromResult(0);
        }

        public override Task OnConnected()
        {
            // record ConnectionId
            var query = Context.Request.Query;
            string userId = query["userId"];
            var user = Context.User.Identity;
            //var cookies = Context.Request.Cookies;
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            // ensure ConnectionId is recorded
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            // remove ConnectionId
            return base.OnDisconnected(stopCalled);
        }
    }

    public interface IGameClient
    {
        void AddNewMessageToPage(string name, string message);
        void UpdateGameState(PlayerCharacter playerCharacter);
    }
}