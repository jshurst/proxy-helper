using System.ServiceModel;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyHelper.Tests.Support;
using ProxyHelper.Wcf;

namespace ProxyHelper.Tests.UnitTests
{
    [TestClass]
    public class WcfProxyHelperTest
    {
        private ServiceHost _serviceHost;
        private const string ENDPOINT = "net.tcp://localhost:32065/MyService";
        private IProxyHelper<IMyService> _proxyHelper;
        private IUnityContainer _container;
        
        [TestInitialize]
        public void Init()
        {
            _container = BuildContainer();
            _proxyHelper = _container.Resolve<IProxyHelper<IMyService>>();

            _serviceHost = new ServiceHost(typeof (MyService));
            _serviceHost.AddServiceEndpoint(typeof(IMyService), NetTcpBindingFactory.GetBinding(), ENDPOINT);
            _serviceHost.Open();
        }

        [TestMethod]
        public void TestServiceWithVoidMethod()
        {
            _proxyHelper.CallService(proxy => proxy.DoWork());

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestServiceWithMethodReturningAString()
        {
            string result = _proxyHelper.CallService(proxy => proxy.DoWorkAndReturnString());

            Assert.IsNotNull(result);
            Assert.AreEqual(result, MyService.ResultMessage);
        }

        [TestMethod]
        public void TestSingleInstanceOfTheProxyHelperWithSynchronousCalls()
        {
            string result = null;
            string result2 = null;
            
            _proxyHelper.CallService(proxy =>
                {
                    proxy.DoWork();
                    result = proxy.DoWorkAndReturnString();
                    result2 = proxy.DoWorkAndReturnString();
                });
            _serviceHost.Close();
            
            Assert.IsNotNull(result);
            Assert.AreEqual(result, MyService.ResultMessage);
            Assert.AreEqual(result2, MyService.ResultMessage);
        }

        public static IUnityContainer BuildContainer()
        {
            var container = new UnityContainer();
            container.RegisterType(typeof(IProxyHelper<IMyService>), typeof(WcfProxyHelper<IMyService>), 
                new InjectionConstructor(NetTcpBindingFactory.GetBinding(),ENDPOINT));

            return container;
        }

        [TestCleanup]
        public void CleanUp()
        {
            _serviceHost.Close();
            _container.Dispose();

            _serviceHost = null;
            _proxyHelper = null;
            _container = null;
        }
    }
}
