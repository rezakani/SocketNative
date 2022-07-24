// Author: Mohammad Reza Kani 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using SocketNative.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SocketNative.Core
{
    /// <summary>
    /// Manage websocket connection
    /// </summary>
    public class WebSocketConnectionManager 
    {
        private ConcurrentDictionary<string, ModuleSocket> _sockets = new ConcurrentDictionary<string, ModuleSocket>();

        /// <summary>
        /// Execute a condition inside your module sockets list.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        public IEnumerable<KeyValuePair<string, ModuleSocket>> Find(Func<KeyValuePair<string, ModuleSocket>, bool> predicate)
        {
            return _sockets.Where(predicate).ToList();
        }

        /// <summary>
        /// Update module socket
        /// </summary>
        public bool UpdateValueByConnectionId(string socketId, ModuleSocket moduleSocket)
        {
            if (_sockets.ContainsKey(socketId))
            {
                _sockets[socketId] = moduleSocket;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns an instance of the WebSocket class</returns>
        public WebSocket GetSocketById(string id)
        {
            return _sockets.FirstOrDefault(p => p.Key.Equals(id)).Value.Socket;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns an instance of the WebSocket class</returns>
        public ModuleSocket GetModuleSocketById(string id)
        {
            return _sockets.FirstOrDefault(p => p.Key.Equals(id)).Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns an instance of the ModuleSocket class</returns>
        public ModuleSocket GetModuleSocketById(WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value.Socket == socket).Value;
        }

        /// <summary>
        /// Get query string with id
        /// </summary>
        public Dictionary<string, string> GetRequestQueryStringById(string id)
        {
            return _sockets.FirstOrDefault(p => p.Key.Equals(id)).Value.RequestQueryString;
        }

        /// <summary>
        /// Add a query string to the socket
        /// </summary>
        public void AddRequestQueryStringById(string id, string key, string value)
        {
            if (!(String.IsNullOrEmpty(key) || String.IsNullOrEmpty(value)))
                _sockets.FirstOrDefault(p => p.Key.Equals(id)).Value.RequestQueryString.Add(key, value);
        }

        /// <summary>
        /// Get all sockets
        /// </summary>
        public ConcurrentDictionary<string, ModuleSocket> GetAll()
        {
            return _sockets;
        }

        /// <summary>
        /// Number of sockets
        /// </summary>
        public int Count
        {
            get { return _sockets.Count; }
        }

        /// <summary>
        /// Get ID with socket object
        /// </summary>
        public string GetId(WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value.Socket == socket).Key;
        }
        /// <summary>
        /// Add a new socket to the list of sockets
        /// </summary>
        public void AddSocket(WebSocket socket, HttpContext context)
        {
            try
            {
                ModuleSocket moduleSocket = new ModuleSocket();
                moduleSocket.Socket = socket;
                var dic = new Dictionary<string, string>();

                context.Request.Query.Keys.ToList().ForEach(x =>
                {
                    dic.Add(x, context.Request.Query[x].ToString());
                });
                moduleSocket.RequestQueryString = dic;
                _sockets.TryAdd(CreateConnectionId(), moduleSocket);
            }
            catch { }
        }

        /// <summary>
        /// Remove a set of sockets with a single transaction
        /// </summary>
        public async Task RemoveSocketAsync(params string[] connectionIds)
        {
            foreach (var item in connectionIds)
            {
                _sockets.TryRemove(item, out ModuleSocket moduleSocket);

                try
                {
                    await moduleSocket.Socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                      statusDescription: "Closed by the WebSocketManager",
                                      cancellationToken: CancellationToken.None);
                }
                catch (Exception)
                {
                   
                }
            }
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
