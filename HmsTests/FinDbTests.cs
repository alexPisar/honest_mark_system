using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HmsTests
{
    /// <summary>
    /// Сводное описание для CryptoTests
    /// </summary>
    [TestClass]
    public class FinDbTests
    {
        public FinDbTests()
        {
            //
            // TODO: добавьте здесь логику конструктора
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Получает или устанавливает контекст теста, в котором предоставляются
        ///сведения о текущем тестовом запуске и обеспечивается его функциональность.
        ///</summary>
        public TestContext TestContext
        {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Дополнительные атрибуты тестирования
        //
        // При написании тестов можно использовать следующие дополнительные атрибуты:
        //
        // ClassInitialize используется для выполнения кода до запуска первого теста в классе
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // TestInitialize используется для выполнения кода перед запуском каждого теста 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // TestCleanup используется для выполнения кода после завершения каждого теста
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CryptoPassTest()
        {
            var cipherPassword = "dcw2bcwl3\u0012sehxshm^92ur\\2iK2bz3U3j3dKzquulroh3xpr j";

            var ipAddress = "192.168.1.20";

            var cookieCollection = new System.Net.CookieCollection();
            cookieCollection.Add(new System.Net.Cookie("position", "5") { Domain = ipAddress });
            cookieCollection.Add(new System.Net.Cookie("shift", "17") { Domain = ipAddress });

            var encodedString = System.Net.WebUtility.UrlEncode(cipherPassword);
            var contentData = $"authorization={encodedString}";

            var serviceManager = new WebSystems.ServiceManager();
            string result = serviceManager.PostRequest($"http://{ipAddress}/upd/edi/Auth/index.php", contentData, cookieCollection);
        }

        [TestMethod]
        public void GetComissionDocumentTest()
        {
            var finDbClient = WebSystems.WebClients.FinDbWebClient.GetInstance();
            var result = finDbClient.GetComissionDocInfoByIdDocJournal(941319100);
        }
    }
}
