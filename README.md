# SocketNative
SocketNative is a library for ASP.NET developers that makes it incredibly simple to add real-time web functionality to your applications.
What is "real-time web" functionality? It's the ability to have your server-side code push content to the connected clients as it happens, in real-time.

# How to use this library

The first step is to add the library to the project using Nuget
## Get it on NuGet!

    Install-Package SocketNative
    
In the next step, create the handler class

```cs
public class SocketHandler : WebSocketHandler
    {
        public SocketHandler(WebSocketConnectionManager webSocketConnectionManager) : base(webSocketConnectionManager)
        {

        }

        public override Task<bool> AuthorizeRequest(HttpContext httpContext)
        {
            //Check the Authorization Request
            //return true or false
            return Task.FromResult(true);
        }

        public override async Task OnConnected(string socketId, ModuleSocket moduleSocket)
        {
            //The event that is called after the connection of the socket.
        }

        public override async Task OnReceived(string socketId, ModuleSocket moduleSocket, string data)
        {
            //This event is called after receiving the message from the client side
        }

        public override Task OnDisconnected(string socketId, ModuleSocket moduleSocket)
        {
            return base.OnDisconnected(socketId, moduleSocket);

            //An event that is called when a socket disconnects
        }

    }

```
And finally, make the settings of the startup class

## Startup.cs

```cs
      public Startup(IConfiguration configuration)
      {
         Configuration = configuration;

         StartupWebSocket.Initialize<SocketHandler>();
         StartupWebSocket.DisconnectTimeout = TimeSpan.FromSeconds(10);
      }
```

```cs
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
      {

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(10),
                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(webSocketOptions);

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                    if (context.WebSockets.IsWebSocketRequest)
                        await StartupWebSocket.webSocketManagerMiddleware.WebSocketRequest(context);
                    else
                        context.Response.StatusCode = 400;
                else
                    await next();
            });

      }
```

## Changelog
* **1.0.4**
    * Added support for .NET Core 2 and above
* **1.0.3**
    * Fix bug
* **1.0.2**
    * Fix bug
* **1.0.1**
    * Fix bug
* **1.0.0**
    * Initial release

## LICENSE
[Apache 2.0 License](https://github.com/rezakani/SocketNative/blob/master/LICENSE)
