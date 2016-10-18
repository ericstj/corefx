namespace NCLFunctional.SecurityTests
{
    using Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Net.Security;
    using System.Net.Test.Common;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    [TestClass]
    public class SslStreamTest
    {
        [TestMethod]
        public async Task SslStream_DefaultTlsConfigurationSync_Ok()
        {
            using (var test = new SyncTest())
            {
                await test.RunTest();
            }
        }

        [TestMethod]
        public async Task SslStream_DefaultTlsConfigurationApm_Ok()
        {
            using (var test = new ApmTest())
            {
                await test.RunTest();
            }
        }

        [TestMethod]
        public async Task SslStream_DefaultTlsConfigurationAsync_Ok()
        {
            using (var test = new AsyncTest())
            {
                await test.RunTest();
            }
        }

        public abstract class TestBase : IDisposable
        {
            protected SslStream _clientStream;
            protected SslStream _serverStream;

            public TestBase()
            {
                var network = new VirtualNetwork();
                var clientNet = new VirtualNetworkStream(network, false);
                var serverNet = new VirtualNetworkStream(network, true);

                _clientStream = new SslStream(clientNet, false, DisableCertificateValidation.SslStreamCallback);
                _serverStream = new SslStream(serverNet, false);
            }

            public async Task RunTest()
            {
                X509Certificate2 serverCertificate = CertificateResource.GetServerCertificate();
                string serverHost = serverCertificate.GetNameInfo(X509NameType.SimpleName, false);
                X509CertificateCollection clientCertificates = new X509CertificateCollection();
                clientCertificates.Add(CertificateResource.GetClientCertificate());

                var tasks = new Task[2];
                tasks[0] = AuthenticateClient(serverHost, clientCertificates, checkCertificateRevocation: false);
                tasks[1] = AuthenticateServer(serverCertificate, clientCertificateRequired:true, checkCertificateRevocation:false);
                await Task.WhenAll(tasks);

                if (OSVersionHelper.IsAtLeast(OSVersion.Win10))
                {
                    Assert.IsTrue( _clientStream.HashAlgorithm == HashAlgorithmType.Sha256 ||
                                   _clientStream.HashAlgorithm == HashAlgorithmType.Sha384 ||
                                   _clientStream.HashAlgorithm == HashAlgorithmType.Sha512);
                }
            }

            protected abstract Task AuthenticateClient(string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation);

            protected abstract Task AuthenticateServer(X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation);

            public void Dispose()
            {
                if (_clientStream != null)
                {
                    _clientStream.Dispose();
                }

                if (_serverStream != null)
                {
                    _serverStream.Dispose();
                }
            }
        }
        
        public class SyncTest : TestBase
        {
            protected override Task AuthenticateClient(string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation)
            {
                return Task.Run( () => { _clientStream.AuthenticateAsClient(targetHost, clientCertificates, checkCertificateRevocation); });
            }

            protected override Task AuthenticateServer(X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation)
            {
                return Task.Run( () => { _serverStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired, checkCertificateRevocation); });
            }
        }

        public class ApmTest : TestBase
        {
            protected override Task AuthenticateClient(string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation)
            {
                return Task.Factory.FromAsync(
                    (callback, state) => _clientStream.BeginAuthenticateAsClient(targetHost, clientCertificates, checkCertificateRevocation, callback, state), 
                    _clientStream.EndAuthenticateAsClient, 
                    state:null);
            }

            protected override Task AuthenticateServer(X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation)
            {
                return Task.Factory.FromAsync(
                    (callback, state) => _serverStream.BeginAuthenticateAsServer(serverCertificate, clientCertificateRequired, checkCertificateRevocation, callback, state),
                    _serverStream.EndAuthenticateAsServer,
                    state:null);
            }
        }

        public class AsyncTest : TestBase
        {
            protected override Task AuthenticateClient(string targetHost, X509CertificateCollection clientCertificates, bool checkCertificateRevocation)
            {
                return _clientStream.AuthenticateAsClientAsync(targetHost, clientCertificates, checkCertificateRevocation);
            }

            protected override Task AuthenticateServer(X509Certificate serverCertificate, bool clientCertificateRequired, bool checkCertificateRevocation)
            {
                return _serverStream.AuthenticateAsServerAsync(serverCertificate, clientCertificateRequired, checkCertificateRevocation);
            }
        }
    }
}
