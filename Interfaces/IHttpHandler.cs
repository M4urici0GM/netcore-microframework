using System.Threading.Tasks;

namespace tests_socket_net.Interfaces
{
    public interface IHttpHandlerService
    {
        Task<byte[]> HandleHttpRequest(HttpContext httpContext);
    }
}