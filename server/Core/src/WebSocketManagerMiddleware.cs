// Author: Mohammad Reza Kani 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SocketNative.Models;

namespace SocketNative.Core
{
    /// <summary>
    /// WebSocket manager middleware
    /// </summary>
    public class WebSocketManagerMiddleware
    {
        private WebSocketHandler _webSocketHandler { get; set; }

        /// <summary>
        /// Constructor method
        /// </summary>
        /// <param name="webSocketHandler">WebSocketHandler class</param>
        public WebSocketManagerMiddleware(WebSocketHandler webSocketHandler)
        {
            _webSocketHandler = webSocketHandler;
        }

        private async Task Invoke(HttpContext context, WebSocket socket)
        {

            await _webSocketHandler.ProcessRequest(socket, context);
            ModuleSocket moduleSocket = StartupWebSocket.webSocketConnectionManager.GetModuleSocketById(socket);
            await _webSocketHandler.OnConnected(StartupWebSocket.webSocketConnectionManager.GetId(socket), moduleSocket);

            await Receive(socket, async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await _webSocketHandler.ReceiveAsync(socket, result, buffer);
                    return;
                }


                else if (result.MessageType == WebSocketMessageType.Close || socket.State == WebSocketState.CloseReceived)
                {
                    await _webSocketHandler.OnDisconnected(StartupWebSocket.webSocketConnectionManager.GetId(socket), moduleSocket);
                    return;
                }

            });
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            await Task.Run(async () =>
            {
                var buffer = new byte[StartupWebSocket.BufferSize];
                var inputSegment = new ArraySegment<byte>(buffer);


                while (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        var result = await socket.ReceiveAsync(inputSegment, CancellationToken.None);
                        handleMessage(result, buffer);
                    }
                    catch (WebSocketException webSocketException)
                    {
                        if (webSocketException.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                        {
                            //  await LogHelper.LogAsync("WebSocketManagerMiddleware->Receive->WebSocketError", LogHelper.LogModes.Debug);
                            ModuleSocket moduleSocket = StartupWebSocket.webSocketConnectionManager.GetModuleSocketById(socket);
                            await _webSocketHandler.OnDisconnected(StartupWebSocket.webSocketConnectionManager.GetId(socket), moduleSocket);
                        }
                    }
                   
                }
            });
        }

        /// <summary>
        /// It is responsible for communication from the received request of the socket type.
        /// </summary>
        /// <param name="httpContext">HttpContext received</param>
        /// <returns></returns>
        public async Task WebSocketRequest(HttpContext httpContext)
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                if (await _webSocketHandler.AuthorizeRequest(httpContext))
                {
                    
                    WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                    await Invoke(httpContext, webSocket);
                }
            }
            else
            {
                httpContext.Response.StatusCode = 400;
            }
        }
    }
}
