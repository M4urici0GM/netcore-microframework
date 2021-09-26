using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microframework.Core;
using Microframework.Core.Interfaces;
using Microframework.Services.Services;

namespace Runner
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
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080);
            IProcessStatusService processStatusService = new ProcessStatusService();
            HttpServer httpServer = new HttpServer(
                ipEndPoint,
                Encoding.UTF8,
                serverLogger,
                10,
                4 * 1024,
                processStatusService);
            
            httpServer.Listen(cancellationTokenSource.Token)
                .Wait(cancellationTokenSource.Token);

            return Task.CompletedTask;
        }
    }
}