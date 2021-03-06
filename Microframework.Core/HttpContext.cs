using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microframework.Core
{
    public class HttpContext
    {
        private readonly Stream _connection;

        public Encoding Encoding { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public bool IsWebsocket { get => Headers.ContainsKey("Sec-WebSocket-Key"); private set => IsWebsocket = value; }
        public string Path { get => Headers.GetValueOrDefault("path", "/"); private set => Path = value; }

        public HttpContext()
        {}
        
        public HttpContext(
            Stream connection,
            Encoding defaultEncoding,
            Dictionary<string, string> httpHeaders = null)
        {
            Headers = httpHeaders ?? new Dictionary<string, string>();
            Encoding = defaultEncoding;
            _connection = connection;
        }
    
        private async Task<byte[]> AcceptDataAsync()
        {
            byte[] buffer = new byte[4 * 1024];
            await _connection.ReadAsync(new ArraySegment<byte>(buffer));

            return buffer;
        }


        public Task WriteResponseAsJsonAsync(HttpStatusCode status, object data)
        {
            string responseStr = JsonSerializer.Serialize(data);
            return WriteResponseAsJsonAsync(status, responseStr);
        }

        public async Task WriteResponseAsJsonAsync(HttpStatusCode statusCode, string data)
        {
            Dictionary<string, string> responseData = new()
            {
                { "Connection", "close" },
                { "Content-Type", "application/json" },
                { "Content-Length", Convert.ToString(data.Length) },
            };
            int status = (int)statusCode;
            string finalResponse = $"HTTP/2.0 {status} {Enum.GetName(statusCode)}\n";
            foreach (var keyValue in responseData)
                finalResponse += $"{keyValue.Key}: {keyValue.Value}\n";

            finalResponse += "\n";
            finalResponse += data;


            byte[] responseBuffer = Encoding.GetBytes(finalResponse);
            await _connection.WriteAsync(responseBuffer, 0, finalResponse.Length);
            await _connection.FlushAsync();
            _connection.Close();
        }

    }
}