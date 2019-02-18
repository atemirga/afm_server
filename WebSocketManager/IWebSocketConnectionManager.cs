using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebSocketManager
{
    public interface IWebSocketConnectionManager<TConnection> where TConnection : WebSocketConnection, new()
    {
        ConcurrentDictionary<string, TConnection> Connections { get; }
        ConcurrentDictionary<string, ConcurrentDictionary<string, TConnection>> Groups { get; }

        void AddSocket(WebSocket socket);
        void AddToGroup(string socketID, string groupID);
        ConcurrentDictionary<string, TConnection> GetAll();
        ConcurrentDictionary<string, TConnection> GetAllFromGroup(string GroupID);
        string GetId(WebSocket socket);
        TConnection GetSocketById(string id);
        string GetSocketGroup(string socketId);
        void RemoveFromGroup(string socketID, string groupID);
        Task RemoveSocket(string id);
    }
}