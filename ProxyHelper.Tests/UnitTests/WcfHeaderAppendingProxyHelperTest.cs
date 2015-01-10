using System.ServiceModel;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyHelper.Tests.Support;
using ProxyHelper.Wcf;
using ProxyHelper.Wcf.Headers;

namespace ProxyHelper.Tests.UnitTests
{
    [TestClass]
    public class WcfHeaderAppendingProxyHelperTest
    {
        private ServiceHost _serviceHost = new ServiceHost(typeof(WindowsCredentialHeaderService));
        private const string ENDPOINT = "net.tcp://localhost:32065/WindowsCredentialHeaderService";
        private IProxyHelper<IMyService> _proxyHelper;
        private IUnityContainer _container;

        [TestInitialize]
        public void Init()
        {
            _container = BuildContainer();
            _proxyHelper = _container.Resolve<IProxyHelper<IMyService>>();

            _serviceHost = new ServiceHost(typeof(MyService));
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
            Assert.AreEqual(result, WindowsCredentialHeaderService.ResultMessage);
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

            Assert.IsNotNull(result);
            Assert.AreEqual(result, WindowsCredentialHeaderService.ResultMessage);
            Assert.AreEqual(result2, WindowsCredentialHeaderService.ResultMessage);
        }

        public static IUnityContainer BuildContainer()
        {
            var container = new UnityContainer();
            container.RegisterType<IMessageHeaderWriter, WindowsCredentialHeaderFactory>();
            container.RegisterType(typeof(IProxyHelper<IMyService>), typeof(WcfHeaderAppendingProxyHelper<IMyService>),
                new InjectionConstructor(NetTcpBindingFactory.GetBinding(), ENDPOINT, container.Resolve<IMessageHeaderWriter>()));

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
