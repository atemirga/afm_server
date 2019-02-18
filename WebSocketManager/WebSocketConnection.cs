using System;
using System.Net.WebSockets;

namespace WebSocketManager
{
    public class WebSocketConnection
    {
        public WebSocketConnection(string socketID, WebSocket webSocket):this()
        {
            Id = socketID;
            WebSocket = webSocket;
        }

        public WebSocketConnection()
        {
            ConnectedTime = DateTime.Now;
        }

        public string Id { get; set; }

        public string UserId { get; set; }

        public DateTime ConnectedTime { get; private set; }

        public WebSocket WebSocket { get; set; }

        public bool Away { get; set; }

        public bool IsBusy { get; set; }
    }
}