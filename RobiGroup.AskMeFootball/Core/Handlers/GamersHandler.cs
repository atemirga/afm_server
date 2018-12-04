using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RobiGroup.AskMeFootball.Core.Game;
using RobiGroup.Web.Common.Identity;
using WebSocketManager;
using WebSocketManager.Common;

namespace RobiGroup.AskMeFootball.Core.Handlers
{
    public class GamersHandler : WebSocketHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMatchManager _matchManager;

        public GamersHandler(WebSocketConnectionManager webSocketConnectionManager,
            IHttpContextAccessor httpContextAccessor,
            IMatchManager matchManager)
            : base(webSocketConnectionManager, new ControllerMethodInvocationStrategy())
        {
            _httpContextAccessor = httpContextAccessor;
            _matchManager = matchManager;
        }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);

            var socketId = WebSocketConnectionManager.GetId(socket);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            WebSocketConnectionManager.AddToGroup(socketId, userId);
            WebSocketConnectionManager.Connections[socketId].UserId = userId;
        }

        public override async Task OnDisconnected(WebSocket socket)
        {
            await base.OnDisconnected(socket);

            var socketId = WebSocketConnectionManager.GetId(socket);
            string groupId = WebSocketConnectionManager.GetSocketGroup(socketId);
            
            _//matchManager.SearchMatch( )
        }
    }
}