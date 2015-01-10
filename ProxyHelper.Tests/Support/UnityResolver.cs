using Microsoft.Practices.Unity;
using ProxyHelper.Local;

namespace ProxyHelper.Tests.Support
{
    public class UnityResolver : IResolver
    {
        private readonly IUnityContainer _container;

        public UnityResolver(IUnityContainer container)
        {
            _container = container;
        }

        public T Resolve<T>() where T : class
        {
            return _container.Resolve<T>();
        }
    }
}
