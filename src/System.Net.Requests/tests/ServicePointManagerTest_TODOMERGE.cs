namespace NCLFunctional.SecurityTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Net;
    using System.Net.Security;

    [TestClass]
    public class ServicePointManagerTest
    {
        public void ServicePointManager_CheckDefaults_AllExpected()
        {
            // This documents the defaults against accidental changes, 
            // as well as warns us if any tests change the defaults.
            Assert.IsFalse(ServicePointManager.CheckCertificateRevocationList);
            Assert.AreEqual(2, ServicePointManager.DefaultConnectionLimit);
            Assert.AreEqual(2 * 60 * 1000, ServicePointManager.DnsRefreshTimeout); // 2 minutes
            Assert.IsFalse(ServicePointManager.EnableDnsRoundRobin);
            Assert.AreEqual(EncryptionPolicy.RequireEncryption, ServicePointManager.EncryptionPolicy);
            Assert.IsTrue(ServicePointManager.Expect100Continue);
            Assert.AreEqual(100 * 1000, ServicePointManager.MaxServicePointIdleTime); // 100s
            Assert.AreEqual(0, ServicePointManager.MaxServicePoints); // No limit
            Assert.IsTrue(ServicePointManager.UseNagleAlgorithm);
        }

        [TestMethod]
        public void ServicePointManager_CheckAllowedProtocols_AllAllowed_Success()
        {
            using (new SecurityProtocolIsolation())
            {
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls
                    | SecurityProtocolType.Ssl3;
            }
        }

        [TestMethod]
        public void ServicePointManager_CheckAllowedProtocols_Ssl2_Fails()
        {
            const int ssl2Client = 0x00000008;
            const int ssl2Server = 0x00000004;

            SecurityProtocolType ssl2 = (SecurityProtocolType)(ssl2Client | ssl2Server);

            using (new SecurityProtocolIsolation())
            {
                try
                {
                    ServicePointManager.SecurityProtocol = ssl2;
                    Assert.Fail("ServicePointManager.SecurityProtocol should not allow Ssl2");
                }
                catch (NotSupportedException)
                { }

                try
                {
                    ServicePointManager.SecurityProtocol = ssl2 | SecurityProtocolType.Ssl3;
                    Assert.Fail("ServicePointManager.SecurityProtocol should not allow Ssl2|Ssl3");
                }
                catch (NotSupportedException)
                { }
            }
        }

        [TestMethod]
        public void ServicePointManager_CheckAllowedProtocols_SystemDefault_Allowed()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
        }
    }
}
