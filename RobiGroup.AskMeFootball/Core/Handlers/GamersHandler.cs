using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RobiGroup.Web.Common.Identity;
using WebSocketManager;
using WebSocketManager.Common;

namespace RobiGroup.AskMeFootball.Core.Handlers
{
    public class GamersHandler : WebSocketHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GamersHandler(WebSocketConnectionManager webSocketConnectionManager,
            IHttpContextAccessor httpContextAccessor)
            : base(webSocketConnectionManager, new ControllerMethodInvocationStrategy())
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);

            var socketId = WebSocketConnectionManager.GetId(socket);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            WebSocketConnectionManager.AddToGroup(socketId, userId);
            WebSocketConnectionManager.Connections[socketId].UserId = userId;
        }
    }
}