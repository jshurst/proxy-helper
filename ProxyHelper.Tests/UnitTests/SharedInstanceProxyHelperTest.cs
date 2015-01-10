using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyHelper.Local;
using ProxyHelper.Tests.Support;

namespace ProxyHelper.Tests.UnitTests
{
    [TestClass]
    public class SharedInstanceProxyHelperTest
    {
        private IUnityContainer _container;
        private IProxyHelper<IMyService> _proxyHelper;

        [TestInitialize]
        public void Init()
        {
            _container = BuildContainer();
            _proxyHelper = _container.Resolve<IProxyHelper<IMyService>>();
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
            
            Assert.IsNotNull(result);
            Assert.AreEqual(result, MyService.ResultMessage);
            Assert.AreEqual(result2, MyService.ResultMessage);
        }

        public static IUnityContainer BuildContainer()
        {
            var container = new UnityContainer();
            container.RegisterType(typeof (IProxyHelper<IMyService>), typeof (SharedInstanceProxyHelper<IMyService>),
                new InjectionConstructor(typeof(MyService)));

            return container;
        }

        [TestCleanup]
        public void CleanUp()
        {
            _container.Dispose();

            _proxyHelper = null;
            _container = null;
        }
    }
}
