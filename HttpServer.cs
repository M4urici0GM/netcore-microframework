using System.Net.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using tests_socket_net.Interfaces;
using System.Diagnostics;
using System.Security.Authentication;

namespace tests_socket_net
{
    public class HttpServer
    {
        private readonly IPEndPoint _ipEndPoint;
        private readonly Encoding _defaultEncoding;
        private readonly Socket _serverSocket;
        private readonly ILogger _logger;
        private readonly int _maxBacklog;
        private readonly int _bufferSize;
        private readonly IHttpHandlerService _httpHandlerService;
        private readonly ICertificateHandler _certificateHandler;
        private readonly bool _sslEnabled;
    

        public HttpServer(
            IPEndPoint ipEndPoint,
            Encoding defaultEncoding,
            ILogger logger,
            int maxBacklog,
            int maxBufferSize,
            bool sslEnabled = false)
        {
            _bufferSize = maxBufferSize;
            _ipEndPoint = ipEndPoint;
            _maxBacklog = maxBacklog;
            _defaultEncoding = defaultEncoding;
            _logger = logger;
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _certificateHandler = new CertificateHandler(_logger, new ProcessStatusService());
            _sslEnabled = sslEnabled;
        }

        private bool IsValidRequestMethod(string requestStr)
        {
            return new Regex("(^GET)|(^POST)|(^DELETE)|(^PUT)").IsMatch(requestStr);
        }
        
        protected Dictionary<string, string> ParseRequestHeaders(byte[] bytes, Encoding encoding)
        {
            Dictionary<string, string> httpHeaders = new Dictionary<string, string>();
            string parsedString = encoding.GetString(bytes).Trim();
            string[] explodedStr = parsedString.Split('\n');
            if (explodedStr.Length == 0)
                throw new InvalidOperationException("Invalid request");
        
            foreach (string str in explodedStr)
            {
                string[] brokenStr = str.Split(":");
                if (brokenStr.Length == 0)
                    continue;
        
                string key = brokenStr[0].Trim().Normalize();
                if (IsValidRequestMethod(key))
                {
                    var brokenRequestMethodLine = key.Split(" ");
                    if (brokenRequestMethodLine.Length != 3)
                        throw new InvalidOperationException("Invalid Request Method/Version/Path");

                    httpHeaders.Add("path", brokenRequestMethodLine[1]);
                    httpHeaders.Add("http_version", brokenRequestMethodLine[2]);
                    httpHeaders.Add("http_method", brokenRequestMethodLine[0]);
                    
                    continue;
                }

                if (brokenStr.Length == 1)
                  continue;
        
                string value = brokenStr[1].Trim().Normalize();
                httpHeaders.Add(key, value);
            }

            return httpHeaders;
        }
        

        private async Task<Stream> AcceptSocketStreamAsync(Socket socket, CancellationToken cancellationToken)
        {
            Stream socketStream = new NetworkStream(socket, false);
            X509Certificate2 certificate = _certificateHandler.GetCertificate();
            
            SslStream sslStream = new SslStream(socketStream, false);
            try
            {
                await sslStream.AuthenticateAsServerAsync(
                    certificate,
                    false,
                    SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13,
                    false);
                _logger.LogInformation("Client connected.");
                return sslStream;
            } catch (Exception ex)
            {
                _logger.LogError($"Failed to accept socket connection: {ex.Message}");
                return socketStream;
            }
        }

        private async Task<HttpContext> AcceptHttpRequestAsync(Stream socketStream, CancellationToken cancellationToken)
        {
            try
            {
                byte[] buffer = new byte[_bufferSize];
                await socketStream.ReadAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (buffer.Length == 0)
                    throw new Exception("LOL");

                Dictionary<string, string> requestHeaders = ParseRequestHeaders(buffer, _defaultEncoding);
                HttpContext httpContext = new HttpContext(socketStream, _defaultEncoding, requestHeaders);

                return httpContext;
            }
            catch (Exception e)
            {
                HttpContext httpContext = new HttpContext(socketStream, _defaultEncoding);
                await httpContext.WriteResponseAsJsonAsync(HttpStatusCode.InternalServerError, new { teste = "aaa"});
                return null;
            }
        }


        protected async Task AcceptSocketAsync(CancellationToken cancellationToken)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Socket socket = await _serverSocket.AcceptAsync();
            Stream socketStream = await AcceptSocketStreamAsync(socket, cancellationToken);
            if (socketStream == null)
            {
                socket.Close();
                return;
            }

            HttpContext httpContext = await AcceptHttpRequestAsync(socketStream, cancellationToken);
            if (httpContext == null)
            {
                socket.Close();
                return;
            }
            
            await httpContext.WriteResponseAsJsonAsync(HttpStatusCode.OK, JsonConvert.SerializeObject(new {
                message = "Hello World",
            }));

            socket.Close();
            stopWatch.Stop();
            _logger.LogInformation($"{httpContext.Headers.GetValueOrDefault("http_method")} {httpContext.Path} {stopWatch.ElapsedMilliseconds}ms");
        }

        public async Task Listen(CancellationToken cancellationToken)
        {
            _serverSocket.Bind(_ipEndPoint);
            _serverSocket.Listen();
            _logger.LogInformation($"Server running at port {_ipEndPoint.Port}");

            while (!cancellationToken.IsCancellationRequested)
                await Task.Run(() => AcceptSocketAsync(cancellationToken), cancellationToken);
        } 
    }
}