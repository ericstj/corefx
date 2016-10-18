namespace NCLFunctional.SecurityTests
{
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Threading.Tasks;

    [TestClass]
    public class SslProtocolsTest
    {
        [TestMethod]
        public void SecurityProtocolType_HttpUsingTls12_Ok()
        {
            if (!OSVersionHelper.IsAtLeast(OSVersion.Win7))
            {
                Assert.Inconclusive("Tls12 is not supported on Win7.");
            }

            TestHttpWebRequest(SecurityProtocolType.Tls12);
        }

        [TestMethod]
        public void SecurityProtocolType_HttpUsingTls11_Ok()
        {
            if (!OSVersionHelper.IsAtLeast(OSVersion.Win7))
            {
                Assert.Inconclusive("Tls11 is not supported on Win7.");
            }

            TestHttpWebRequest(SecurityProtocolType.Tls11);
        }

        [TestMethod]
        public void SecurityProtocolType_HttpUsingDefault_Ok()
        {
            TestHttpWebRequest(SecurityProtocolType.Tls | SecurityProtocolType.Ssl3);
        }

        [TestMethod]
        public void SecurityProtocolType_HttpUsingAllSupported_Ok()
        {
            SecurityProtocolType protocols = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            if (OSVersionHelper.IsAtLeast(OSVersion.Win7))
            {
                protocols =
                    SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            }

            TestHttpWebRequest(protocols);
        }

        [TestMethod]
        public void SslProtocols_SslStreamClientServerTls12_Ok()
        {
            if (!OSVersionHelper.IsAtLeast(OSVersion.Win7))
            {
                Assert.Inconclusive("Tls12 is not supported on Win7.");
            }

            TestSslStream(SslProtocols.Tls12);
        }

        [TestMethod]
        public void SslProtocols_SslStreamClientServerTls11_Ok()
        {
            if (!OSVersionHelper.IsAtLeast(OSVersion.Win7))
            {
                Assert.Inconclusive("Tls11 is not supported on Win7.");
            }

            TestSslStream(SslProtocols.Tls11);
        }

        [TestMethod]
        public void SslProtocols_SslStreamClientServerDefaults_Ok()
        {
            TestSslStream(SslProtocols.Tls | SslProtocols.Ssl3);
        }

        [TestMethod]
        public void SslProtocols_SslStreamClientServerAllSupported_Ok()
        {
            SslProtocols protocols = SslProtocols.Tls | SslProtocols.Ssl3;

            if (!OSVersionHelper.IsAtLeast(OSVersion.Win7))
            {
                protocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls |
                            SslProtocols.Ssl3;
            }

            TestSslStream(protocols);
        }

        public static void TestHttpWebRequest(SecurityProtocolType securityProtocols)
        {
            var server = new HttpsTestServer((SslProtocols)securityProtocols);
            server.StartServer();
            int port = server.Port;

            Task serverTask = server.RunTest();

            using (new SecurityProtocolIsolation())
            using (new DisableCertificateValidation())
            {
                ServicePointManager.SecurityProtocol = securityProtocols;

                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("https://localhost:" + port);
                HttpWebResponse response = (HttpWebResponse)httpRequest.GetResponse();

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }

            var sslProtocols = (SslProtocols)securityProtocols;
            if ((sslProtocols & SslProtocols.Tls12) == SslProtocols.Tls12)
            {
                Assert.AreEqual((int)SecurityProtocolType.Tls12, (int)server.SslProtocol);
            }
            else if ((sslProtocols & SslProtocols.Tls11) == SslProtocols.Tls11)
            {
                Assert.AreEqual((int)SecurityProtocolType.Tls11, (int)server.SslProtocol);
            }
            else if ((sslProtocols & SslProtocols.Tls) == SslProtocols.Tls)
            {
                Assert.AreEqual((int)SslProtocols.Tls, (int)server.SslProtocol);
            }
        }

        public static void TestSslStream(SslProtocols securityProtocols)
        {
            var server = new HttpsTestServer(securityProtocols);
            server.StartServer();
            int port = server.Port;

            Task serverTask = server.RunTest();

            using (var client = new TcpClient("localhost", port))
            {
                using (var ssl = new SslStream(
                    client.GetStream(),
                    false,
                    DisableCertificateValidation.SslStreamCallback))
                {
                    ssl.AuthenticateAsClient("localhost", null, securityProtocols, false);

                    if ((securityProtocols & SslProtocols.Tls12) == SslProtocols.Tls12)
                    {
                        Assert.AreEqual((int)SecurityProtocolType.Tls12, (int)ssl.SslProtocol);
                    }
                    else if ((securityProtocols & SslProtocols.Tls11) == SslProtocols.Tls11)
                    {
                        Assert.AreEqual((int)SecurityProtocolType.Tls11, (int)ssl.SslProtocol);
                    }
                    else if ((securityProtocols & SslProtocols.Tls) == SslProtocols.Tls)
                    {
                        Assert.AreEqual((int)SslProtocols.Tls, (int)ssl.SslProtocol);
                    }
                }
            }
        }
    }
}
