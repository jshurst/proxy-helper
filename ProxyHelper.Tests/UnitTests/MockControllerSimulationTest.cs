using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyHelper.Tests.Support;
using Moq;

namespace ProxyHelper.Tests.UnitTests
{
    [TestClass]
    public class MockControllerSimulationTest
    {
        [TestMethod]
        public void MockTheProxyHelperAndMakeSingleCall()
        {
            const string EXPECTED = "hey";
            var mockProxyHelper = new Mock<IProxyHelper<IMyService>>();
            mockProxyHelper.Setup(proxy => proxy.CallService(It.IsAny<Func<IMyService, string>>()))
                .Returns(EXPECTED);

            var result = new FakeController(mockProxyHelper.Object).CallASingleServiceMethod();

           Assert.AreEqual(result, EXPECTED);
        }
    }

    public class FakeController
    {
        private readonly IProxyHelper<IMyService> _proxyHelper;

        public FakeController(IProxyHelper<IMyService> proxyHelper)
        {
            _proxyHelper = proxyHelper;
        }

        public string CallASingleServiceMethod()
        {
            var result = _proxyHelper.CallService(proxy => proxy.DoWorkAndReturnString());
            return result;
        }

        public string CallMultipleMethodsWithTheSameProxy()
        {
            string result = null;
                
                _proxyHelper.CallService(proxy =>
                    {
                        var internalResult = proxy.DoWorkAndReturnString();
                        var internalResult2 = proxy.DoWorkAndReturnString();
                        result = internalResult + internalResult2;
                    });

            return result;
        }
    }
}
