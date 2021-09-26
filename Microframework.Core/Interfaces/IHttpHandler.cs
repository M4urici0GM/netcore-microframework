using System.Threading.Tasks;

namespace Microframework.Core.Interfaces
{
    public interface IHttpHandlerService
    {
        Task<byte[]> HandleHttpRequest(HttpContext httpContext);
    }
}