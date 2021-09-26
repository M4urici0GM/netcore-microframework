using System.Security.Cryptography.X509Certificates;

namespace tests_socket_net
{
    public interface ICertificateHandler
    {
        X509Certificate2 GetCertificate();
        X509Certificate2 GetSelfSignedCertificate();
        X509Certificate2 ServerCertificateSelector(string name);
        X509Certificate2 AddCertificateToServer(string name, X509Certificate2 certificate);
    }
}