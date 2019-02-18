using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketManager
{
    public class WebSocketConnectionManager : WebSocketConnectionManager<CommonWebSocketConnection>
    {

    }

    public class CommonWebSocketConnection : WebSocketConnection {

        public CommonWebSocketConnection():base()
        {

        }

        public CommonWebSocketConnection(string socketID, WebSocket webSocket) : base(socketID, webSocket)
        {
        }
    }

    public class WebSocketConnectionManager<TConnection> where TConnection : WebSocketConnection, new()
    {
        private ConcurrentDictionary<string, TConnection> _connections = new ConcurrentDictionary<string, TConnection>();
        private ConcurrentDictionary<string, ConcurrentDictionary<string, TConnection>> _groups = new ConcurrentDictionary<string, ConcurrentDictionary<string, TConnection>>();
        private ConcurrentDictionary<string, string> _socketGroupMapping = new ConcurrentDictionary<string, string>();

        public ConcurrentDictionary<string, ConcurrentDictionary<string, TConnection>> Groups
        {
            get { return _groups; }
        }

        public ConcurrentDictionary<string, TConnection> Connections
        {
            get { return _connections; }
        }

        public TConnection GetSocketById(string id)
        {
            return _connections.FirstOrDefault(p => p.Key == id).Value;
        }

        public ConcurrentDictionary<string, TConnection> GetAll()
        {
            return _connections;
        }

        public ConcurrentDictionary<string, TConnection> GetAllFromGroup(string GroupID)
        {
            if (Groups.ContainsKey(GroupID))
            {
                return Groups[GroupID];
            }

            return default(ConcurrentDictionary<string, TConnection>);
        }

        public string GetId(WebSocket socket)
        {
            return _connections.FirstOrDefault(p => p.Value.WebSocket == socket).Key;
        }

        public string GetSocketGroup(string socketId)
        {
            if (_socketGroupMapping.ContainsKey(socketId))
            {
                return _socketGroupMapping[socketId];
            }

            return null;
        }

        public void AddSocket(WebSocket socket)
        {
            var connectionId = CreateConnectionId();
            _connections.TryAdd(connectionId, new TConnection(){ Id = connectionId, WebSocket = socket});
        }

        public void AddToGroup(string socketID, string groupID)
        {
            _socketGroupMapping.TryAdd(socketID, groupID);
            if (Groups.ContainsKey(groupID))
            {
                Groups[groupID].TryAdd(socketID, _connections[socketID]);
                return;
            }

            var webSocketConnections = new ConcurrentDictionary<string, TConnection>();
            webSocketConnections.TryAdd(socketID, _connections[socketID]);
            Groups.TryAdd(groupID, webSocketConnections);
        }

        public void RemoveFromGroup(string socketID, string groupID)
        {
            string removedGroup;
            _socketGroupMapping.TryRemove(socketID, out removedGroup);
            if (Groups.ContainsKey(groupID))
            {
                TConnection connection;
                Groups[groupID].TryRemove(socketID, out connection);

                if (!Groups[groupID].Any())
                {
                    ConcurrentDictionary<string, TConnection> connections;
                    Groups.TryRemove(groupID, out connections);
                }
            }
        }

        public async Task RemoveSocket(string id)
        {
            if (id == null) return;

            TConnection socket;
            _connections.TryRemove(id, out socket);

            foreach (var @group in Groups)
            {
                if (@group.Value.ContainsKey(id))
                {
                    RemoveFromGroup(id, @group.Key);
                }
            }


            if (socket.WebSocket.State != WebSocketState.Open) return;

            try
            {
                await socket.WebSocket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                    statusDescription: "Closed by the WebSocketManager",
                    cancellationToken: CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception e)
            {

            }
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}