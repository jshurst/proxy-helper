using System;

namespace ProxyHelper.Local
{
    /// <summary>
    /// The instance is either passed in, or created from a passed in type, in the constructor,
    /// so it's shared between different method calls in the same proxyHelper instance.
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    public class SharedInstanceProxyHelper<TInterface> : IProxyHelper<TInterface> where TInterface : class
    {
        private readonly TInterface _instance;
        
        public SharedInstanceProxyHelper(TInterface instance)
        {
            _instance = instance;
        }
        
        public void CallService(Action<TInterface> action)
        {
            CallService(instance =>
                {
                    action(instance);
                    return "Simply calling Func.  Need a fake return statement.";;
                });
        }

        public TResult CallService<TResult>(Func<TInterface, TResult> action)
        {
            return action(_instance);
        }
    }
}