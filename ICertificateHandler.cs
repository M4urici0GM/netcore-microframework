using System.Security.Cryptography.X509Certificates;

namespace tests_socket_net
{
    public interface ICertificateHandler
    {
        X509Certificate2 GetCertificate();
        X509Certificate2 GetSelfSignedCertificate();
    }
}