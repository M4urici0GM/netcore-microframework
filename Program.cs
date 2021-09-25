using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tests_socket_net
{
    class Program
    {
        static Task Main(string[] args)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eArgs) =>
            {
                eArgs.Cancel = true;
                cancellationTokenSource.Cancel(false);
            };
            
            ILogger serverLogger = new Logger();
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 3333);
            HttpServer httpServer = new HttpServer(
                ipEndPoint,
                Encoding.UTF8,
                serverLogger,
                10,
                4 * 1024);
            
            httpServer.Listen(cancellationTokenSource.Token)
                .Wait(cancellationTokenSource.Token);

            return Task.CompletedTask;
        }
    }
}
