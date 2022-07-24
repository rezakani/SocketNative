// Author: Mohammad Reza Kani 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using SocketNative.Core;


namespace SocketNative
{
    /// <summary>
    /// SocketNative initializer class
    /// </summary>
    public class StartupWebSocket
    {
        /// <summary>
        /// The main instance created is from the WebSocketConnectionManager class
        /// </summary>
        public static WebSocketConnectionManager webSocketConnectionManager;

        /// <summary>
        /// The main instance created is from the WebSocketHandler class
        /// </summary>
        public static WebSocketHandler socketHandler;

        /// <summary>
        /// The main instance created is from the WebSocketManagerMiddleware class
        /// </summary>
        public static WebSocketManagerMiddleware webSocketManagerMiddleware;


        /// <summary>
        /// Gets or sets the disconnect timeout
        /// The default is 10 seconds.
        /// </summary>
        public static TimeSpan DisconnectTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets the size of the protocol buffer
        /// The default is 4kb.
        /// </summary>
        public static int BufferSize { get; set; } = (4 * 1024);

        /// <summary>
        /// Initializes an instance of the WebSocketHandler class
        /// </summary>
        /// <typeparam name="T">WebSocketHandler class</typeparam>
        public static void Initialize<T>() where T : WebSocketHandler
        {
            webSocketConnectionManager = new WebSocketConnectionManager();
            socketHandler = Activator.CreateInstance(typeof(T), webSocketConnectionManager) as T;
            webSocketManagerMiddleware = new WebSocketManagerMiddleware(socketHandler);
            socketHandler.DisposeSockets();
        }
    }
}
