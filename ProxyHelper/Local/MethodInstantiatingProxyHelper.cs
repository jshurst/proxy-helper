using System;

namespace ProxyHelper.Local
{
    /// <summary>
    /// On each call to the CallService() method a new instance of the TInterface is created.
    /// 
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    public class MethodInstantiatingProxyHelper<TInterface> : IProxyHelper<TInterface> where TInterface : class
    {
        private readonly IResolver _resolver;

        public MethodInstantiatingProxyHelper(IResolver resolver)
        {
            _resolver = resolver;
        }

        public void CallService(Action<TInterface> action)
        {
            CallService(instance =>
                {
                    action(instance);
                    return "Simply calling Func.  Need a fake return statement.";
                });
        }

        public TResult CallService<TResult>(Func<TInterface, TResult> action)
        {
            TInterface instance = null;

            try
            {
                instance = _resolver.Resolve<TInterface>();
                return action(instance);
            }
            finally
            {
                var disposable = instance as IDisposable;
                if (disposable != null)
                    disposable.Dispose();

                instance = null;
            }
        }
    }
}