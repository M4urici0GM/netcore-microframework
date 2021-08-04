
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace tests_socket_net
{
      public class WebSocketContext
    {
        private readonly Stream _connection;
        
        public WebSocketContext(Stream connection)
        {
            _connection = connection;
        }

        private async Task<WebSocket> AcceptWebsocketAsync(HttpContext httpContext)
        {
            string websocketKey = httpContext.Headers.GetValueOrDefault("Sec-WebSocket-Key");
            string composedKey = websocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
           
            using SHA1CryptoServiceProvider hashProvider = new SHA1CryptoServiceProvider();
            byte[] sha1AcceptKey = SHA1.Create().ComputeHash(httpContext.Encoding.GetBytes(composedKey));
            
            string base64AcceptKey = Convert.ToBase64String(sha1AcceptKey);
            Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                + "Connection: Upgrade" + Environment.NewLine
                + "Upgrade: websocket" + Environment.NewLine
                + $"Sec-WebSocket-Accept: {base64AcceptKey}"
                + Environment.NewLine
                + Environment.NewLine);
            
            await _connection.WriteAsync(response);
            WebSocket websocket = WebSocket.CreateFromStream(_connection, true, null, TimeSpan.FromMilliseconds(200));
            return websocket;
        }
    }
}