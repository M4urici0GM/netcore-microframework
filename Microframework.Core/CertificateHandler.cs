using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microframework.Core.Enums;
using Microframework.Core.Interfaces;

namespace Microframework.Core
{
    

    public class CertificateHandler : ICertificateHandler
    {
        private const string ServerAuthenticationOid = "1.3.6.1.5.5.7.3.1";

        private readonly ILogger _logger;
        private readonly IProcessStatusService _processStatusService;

        
        private readonly ConcurrentDictionary<string, X509Certificate2> _serverCertificates;
        
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
            var ecdsa = RSA.Create();

            var req = new CertificateRequest("cn=foobar", ecdsa, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            var certificate = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
            return _processStatusService.CurrentOsArchitecture == ProcessPlatform.Windows
                ? certificate
                : new X509Certificate2(certificate.Export(X509ContentType.Pkcs12));
        }

        public X509Certificate2 ServerCertificateSelector(string name)
        {
            
            if (!_serverCertificates.ContainsKey(name))
                throw new InvalidOperationException($"No certificate found with name {name}");

            bool hasRetrieved = _serverCertificates.TryGetValue(name, out var certificate);
            if (!hasRetrieved)
                throw new Exception($"Failed to get certificate with name {name}");

            return certificate;
        }
        

        public X509Certificate2 GetCertificate()
        {
            using var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);

            // if (certStore.Certificates.Count == 0)
            //     throw new Exception("No certificates found!");
            //
            // var certificates = certStore.Certificates.Find(X509FindType.FindBySerialNumber, "72D431BD04C267CC", true);
            var x509Certificate2 = certStore.Certificates[1];

            return x509Certificate2;
        }
    }
}