namespace NCLFunctional.SecurityTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Win32;
    using NCLFunctional.Common;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;

    [TestClass]
    public class SystemDefaultTlsVersionsTest
    {
        private const string SystemDefaultTlsVersionsKeyName = "SystemDefaultTlsVersions";

        [TestMethod]
        public void Security_SystemDefaultTlsVersions_DefaultNet463_Enabled()
        {
            using (AppDomainHelper appDomainHelper = new AppDomainHelper(AppDomainHelper.GetExecutingAssemblyTargetFramework()))
            {
                appDomainHelper.ExecuteNoResult<bool>(Security_SystemDefaultTlsVersions_DefaultNet463_AppDomainEntry, true);
            }
        }

        public static void Security_SystemDefaultTlsVersions_DefaultNet463_AppDomainEntry(bool unused)
        {
            Assert.AreEqual(SecurityProtocolType.SystemDefault, ServicePointManager.SecurityProtocol);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;

            TestHttpWebRequest(OptOutSystemDefaultTlsVersions());
            TestSslStream(OptOutSystemDefaultTlsVersions());
        }

        [TestMethod]
        public void Security_SystemDefaultTlsVersions_Net462_RegKey()
        {
            using (AppDomainHelper appDomainHelper = new AppDomainHelper(".NETFramework,Version=v4.6.2"))
            {
                appDomainHelper.ExecuteNoResult<bool>(Security_SystemDefaultTlsVersions_Net462_RegKey_AppDomainEntry, true);
            }
        }

        public static void Security_SystemDefaultTlsVersions_Net462_RegKey_AppDomainEntry(bool unused)
        {
            Assert.AreEqual(SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls, ServicePointManager.SecurityProtocol);

            // This is allowed in .Net 4.6.2 and below but should fail deeper within the stack.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            
            TestHttpWebRequest(!OptInSystemDefaultTlsVersions());
            TestSslStream(!OptInSystemDefaultTlsVersions());
        }

        public static void TestHttpWebRequest(bool expectFail)
        {
            WebException exception = null;

            HttpsTestServer server = CreateHttpServer(expectFail);

            try
            {
                using (new DisableCertificateValidation())
                {
                    HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("https://localhost:" + server.Port);
                    HttpWebResponse response = (HttpWebResponse)httpRequest.GetResponse();
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                }
            }
            catch (WebException wex)
            {
                exception = wex;
            }

            if (expectFail)
            {
                Assert.IsNotNull(exception);
                Assert.IsNotNull(exception.InnerException);
                Assert.AreEqual(typeof(ArgumentException), exception.InnerException.GetType());
            }
            else
            {
                if (OSVersionHelper.IsAtLeast(OSVersion.Win8))
                {
                    Assert.AreEqual((int)SslProtocols.Tls12, (int)server.SslProtocol);
                }
                else
                {
                    Assert.AreEqual((int)SslProtocols.Tls, (int)server.SslProtocol);
                }
            }
        }

        public static void TestSslStream(bool expectFail)
        {
            ArgumentException exception = null;

            HttpsTestServer server = CreateHttpServer(expectFail);

            try
            {
                using (var client = new TcpClient("localhost", server.Port))
                using (var ssl = new SslStream(
                    client.GetStream(),
                    false,
                    DisableCertificateValidation.SslStreamCallback))
                {
                    ssl.AuthenticateAsClient("localhost", null, SslProtocols.None, false);
                }
                
            }
            catch (ArgumentException ex)
            {
                exception = ex;
            }

            if (expectFail)
            {
                Assert.IsNotNull(exception);
            }
        }

        // .Net 4.6.3 and above registry key behavior.
        private static bool OptOutSystemDefaultTlsVersions()
        {
            return NetRegistryConfiguration.CheckRegistryKeyHasExactValue(SystemDefaultTlsVersionsKeyName, 0);
        }

        // .Net 4.6.2 and below registry key behavior.
        private static bool OptInSystemDefaultTlsVersions()
        {
            return NetRegistryConfiguration.CheckRegistryKeyHasExactValue(SystemDefaultTlsVersionsKeyName, 1);
        }

        private static HttpsTestServer CreateHttpServer(bool expectFail)
        {
            SslProtocols serverSecurityProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
            if (!expectFail)
            {
                serverSecurityProtocols = SslProtocols.None;
            }

            var server = new HttpsTestServer(serverSecurityProtocols);
            server.StartServer();
            var serverTask = server.RunTest();

            return server;
        }
    }
}
