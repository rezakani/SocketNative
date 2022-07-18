using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using SocketNative.Models;
using Microsoft.AspNetCore.Http;

namespace SocketNative.Core
{
    /// <summary>
    /// Handles websocket type requests
    /// </summary>
    public abstract class WebSocketHandler
    {
        /// <summary>
        /// An instance is WebSocketConnectionManager
        /// </summary>
        protected WebSocketConnectionManager WebSocketConnectionManager { get; set; }

        /// <summary>
        /// Constructor method
        /// </summary>
        /// <param name="webSocketConnectionManager">An instance is WebSocketConnectionManager</param>
        public WebSocketHandler(WebSocketConnectionManager webSocketConnectionManager)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
        }

        /// <summary>
        /// The event that is called after the connection of the socket.
        /// </summary>
        /// <param name="socketId">The ID of the socket created</param>
        /// <param name="moduleSocket">Socket model carrier container</param>
        /// <returns></returns>
        public abstract Task OnConnected(string socketId, ModuleSocket moduleSocket);


        /// <summary>
        /// An event that is called when a socket disconnects
        /// </summary>
        /// <param name="socketId">The ID of the socket created</param>
        /// <param name="moduleSocket">Socket model carrier container</param>
        /// <returns></returns>
        public virtual async Task OnDisconnected(string socketId, ModuleSocket moduleSocket)
        {
            await WebSocketConnectionManager.RemoveSocketAsync(socketId);
        }

        /// <summary>
        /// This event is called before the socket is created
        /// </summary>
        /// <param name="socket">An example of a socket</param>
        /// <param name="httpContext">HttpContext received</param>
        /// <returns></returns>
        public virtual async Task ProcessRequest(WebSocket socket, HttpContext httpContext)
        {
            await Task.Run(() =>
            {
                WebSocketConnectionManager.AddSocket(socket, httpContext);
            });
        }

        /// <summary>
        /// Send a text message through an instance of the Socket class
        /// </summary>
        public async Task<bool> SendMessageAsync(WebSocket socket, string message)
        {
            try
            {
                if (socket.State != WebSocketState.Open)
                    return false;


                var encoded = Encoding.UTF8.GetBytes(message);
                var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);

                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Send a text message through an instance of the SocketId
        /// </summary>
        public async Task<bool> SendMessageAsync(string socketId, string message)
        {
            try
            {
                await SendMessageAsync(WebSocketConnectionManager.GetSocketById(socketId), message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Send a text message to all connected sockets
        /// </summary>
        public async Task SendBroadcastAsync(string message)
        {
            foreach (var pair in WebSocketConnectionManager.GetAll())
            {
                try
                {
                    if (pair.Value.Socket.State == WebSocketState.Open)
                        await SendMessageAsync(pair.Value.Socket, message);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Removal of discarded sockets instance
        /// </summary>
        public void DisposeSockets()
        {
            Task.Run(async () =>
           {
               while (true)
               {
                   Parallel.ForEach(WebSocketConnectionManager.GetAll(), async pair =>
                   {

                       try
                       {
                           if (pair.Value.Socket.State == WebSocketState.Open)
                               return;
                       }
                       catch (Exception)
                       {
                           await StartupWebSocket.socketHandler.OnDisconnected(pair.Key, pair.Value);
                       }
                   });

                   await Task.Delay(StartupWebSocket.DisconnectTimeout);
               }

           });
        }

        /// <summary>
        /// Checking the connectivity of a socket
        /// </summary>
        public bool IsConnected(string socketId)
        {
            try
            {
                if (StartupWebSocket.webSocketConnectionManager.GetSocketById(socketId).State == WebSocketState.Open)
                { return true; }
                else { return false; }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// This event is called when receiving a message from the client and its task is to call the corresponding socket.
        /// </summary>
        /// <param name="socket">An example of a socket</param>
        /// <param name="result">WebSocketReceiveResult received</param>
        /// <param name="buffer">Received data in bytes</param>
        /// <returns></returns>
        public virtual async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            try
            {
                await OnReceived(StartupWebSocket.webSocketConnectionManager.GetId(socket),
                     StartupWebSocket.webSocketConnectionManager.GetModuleSocketById(socket),
                     Encoding.UTF8.GetString(buffer, 0, result.Count));
            }
            catch { }
        }

        /// <summary>
        /// This event is called after receiving the message from the client side
        /// </summary>
        /// <param name="socketId">The ID of the socket created</param>
        /// <param name="moduleSocket">Socket model carrier container</param>
        /// <param name="data">Message received</param>
        /// <returns></returns>
        public abstract Task OnReceived(string socketId, ModuleSocket moduleSocket, string data);

        /// <summary>
        /// This event is called before establishing a socket connection and its task is to validate the received request.
        /// </summary>
        /// <param name="httpContext">HttpContext received</param>
        /// <returns></returns>
        public abstract Task<bool> AuthorizeRequest(HttpContext httpContext);
    }
}
