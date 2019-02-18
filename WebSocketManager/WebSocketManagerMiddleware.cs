using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebSocketManager.Common;
using WebSocketManager.Common.Json;

namespace WebSocketManager
{
    public class WebSocketManagerMiddleware
    {
        private readonly ILogger<WebSocketManagerMiddleware> _logger;
        private readonly string _authScheme;
        private readonly RequestDelegate _next;
        private WebSocketHandler _webSocketHandler { get; set; }

        private JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            SerializationBinder = new JsonBinderWithoutAssembly(),
            Converters = new List<JsonConverter>()
            {
                new DecimalConverter()
            }
        };

        public WebSocketManagerMiddleware(RequestDelegate next, ILogger<WebSocketManagerMiddleware> logger) : this(next, logger, null)
        {
        }

        public WebSocketManagerMiddleware(RequestDelegate next, ILogger<WebSocketManagerMiddleware> logger,
            WebSocketHandler webSocketHandler) : this(next, logger, webSocketHandler, null)
        {
        }

        public WebSocketManagerMiddleware(RequestDelegate next, ILogger<WebSocketManagerMiddleware> logger,
                                          WebSocketHandler webSocketHandler,
                                            string authScheme)
        {
            _jsonSerializerSettings.Converters.Insert(0, new PrimitiveJsonConverter());
            _next = next;
            _webSocketHandler = webSocketHandler;
            _logger = logger;
            _authScheme = authScheme;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    await _next.Invoke(context);
                    return;
                }

                var principal = new ClaimsPrincipal();
                var authResult = string.IsNullOrEmpty(_authScheme)
                                ? await context.AuthenticateAsync()
                                : await context.AuthenticateAsync(_authScheme);

                if (authResult?.Principal != null && authResult.Succeeded)
                {
                    principal.AddIdentities(authResult.Principal.Identities);
                }

                context.User = principal;
                if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
                {
                    _logger.LogInformation(" Socket connected: " + context.User.Identity.Name);
                    var socket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                    await _webSocketHandler.OnConnected(socket).ConfigureAwait(false);

                    await Receive(socket, async (result, serializedMessage) =>
                    {
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            try
                            {
                                _logger.LogInformation(serializedMessage);
                                Message message =
                                    JsonConvert.DeserializeObject<Message>(serializedMessage, _jsonSerializerSettings);
                                await _webSocketHandler.ReceiveAsync(socket, result, message).ConfigureAwait(false);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "ERROR ON RECEIVE!");
                            }
                            return;
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            try
                            {
                                await _webSocketHandler.OnDisconnected(socket);
                            }
                            catch (WebSocketException ex)
                            {
                                _logger.LogError(LoggingEvents.Exception, ex, "ERROR ON CONNECT");
                                //throw; //let's not swallow any exception for now
                            }

                            return;
                        }
                    });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ERROR ON INVOKE!");
            }
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, string> handleMessage)
        {
            while (socket.State == WebSocketState.Open)
            {
                ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[1024 * 4]);
                string message = null;
                WebSocketReceiveResult result = null;
                try
                {
                    using (var ms = new MemoryStream())
                    {
                        do
                        {
                            result = await socket.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                            ms.Write(buffer.Array, buffer.Offset, result.Count);
                        }
                        while (!result.EndOfMessage);

                        ms.Seek(0, SeekOrigin.Begin);

                        using (var reader = new StreamReader(ms, Encoding.UTF8))
                        {
                            message = await reader.ReadToEndAsync().ConfigureAwait(false);
                        }
                    }

                    handleMessage(result, message);
                }
                catch (WebSocketException e)
                {
                    _logger.LogError(LoggingEvents.Exception, e, "ERROR ON RECEIVE");
                    if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {
                        socket.Abort();
                    }
                }
            }

            await _webSocketHandler.OnDisconnected(socket);
        }
    }
}