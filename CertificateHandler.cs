using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace tests_socket_net
{

    public enum ProcessPlatform
    {
        Windows,
        Unix
    }

    public interface IProcessStatusService
    {
        public ProcessPlatform CurrentOsArchitecture { get;  }
        
    }
    
    public class ProcessStatusService : IProcessStatusService
    {
        public ProcessPlatform CurrentOsArchitecture { get; }

        public ProcessStatusService()
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            CurrentOsArchitecture = isWindows ? ProcessPlatform.Windows : ProcessPlatform.Unix;
        }
    }

    public class CertificateHandler : ICertificateHandler
    {
        private readonly ILogger _logger;
        private readonly IProcessStatusService _processStatusService;

        private ConcurrentDictionary<string, X509Certificate2> _serverCertificates;
        
        public CertificateHandler(
            ILogger logger,
            IProcessStatusService processStatusService)
        {
            _logger = logger;
            _processStatusService = processStatusService;
            _serverCertificates = new ConcurrentDictionary<string, X509Certificate2>();
        }

        public X509Certificate2 AddCertificateToServer(string name, X509Certificate2 certificate)
        {
            bool alreadyHasCertificateName = _serverCertificates.ContainsKey(name);
            if (alreadyHasCertificateName)
                throw new InvalidOperationException("There's already a certificate with this name!");

            bool hasInserted = _serverCertificates.TryAdd(name, certificate);
            if (!hasInserted)
                throw new ApplicationException("Failed to insert item to store.");

            return certificate;
        }
        
        public X509Certificate2 GetSelfSignedCertificate()
        {
            var ecdsa = ECDsa.Create();
            if (ecdsa == null)
                return null;

            var req = new CertificateRequest("cn=foobar", ecdsa, HashAlgorithmName.SHA256);
            return req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
        }

        public X509Certificate2 GetDefaultServerCertificate()
        {
            
        }

        private X509Certificate2 ServerCertificateSelector(string name)
        {
            if (_serverCertificates is {Count: > 0})
            {
                foreach (var serverCertificate in _serverCertificates)
                {
                    _logger.LogDebug($"SNI Name: {name}");
                    if (_processStatusService.CurrentOsArchitecture == ProcessPlatform.Windows)
                        return serverCertificate.Value;

                    byte[] serverCertificateBuffer = serverCertificate.Value.Export(X509ContentType.Pkcs12);
                    return new X509Certificate2(serverCertificateBuffer);
                }
            }
            
            
            
            
            
            
        }
        

        public X509Certificate2 GetCertificate()
        {

            return X509Certificate2.CreateFromPemFile(
                "/home/m4urici0gm/certificates/craftworld.com.br/cert.pem",
                "/home/m4urici0gm/certificates/craftworld.com.br/privkey.pem");
        }
    }
}