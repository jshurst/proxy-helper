using System;

namespace ProxyHelper
{
    public interface IProxyHelper<TInterface> where TInterface : class
    {
        void CallService(Action<TInterface> action);
        TResult CallService<TResult>(Func<TInterface, TResult> action);
    }
}
