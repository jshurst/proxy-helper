using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using ProxyHelper.Wcf.Headers;

namespace ProxyHelper.Wcf
{
    public class WcfHeaderAppendingProxyHelper<TInterface> : WcfProxyHelper<TInterface> where TInterface : class
    {
        private readonly IMessageHeaderWriter _messageHeaderWriter;

        #region ctor

        public WcfHeaderAppendingProxyHelper(IMessageHeaderWriter messageHeaderWriter)
            : base()
        {
            _messageHeaderWriter = messageHeaderWriter;
        }

        public WcfHeaderAppendingProxyHelper(string endpointConfigurationName, IMessageHeaderWriter messageHeaderWriter)
            : base(endpointConfigurationName)
        {
            _messageHeaderWriter = messageHeaderWriter;
        }

        public WcfHeaderAppendingProxyHelper(string endpointConfigurationName, EndpointAddress remoteAddress, IMessageHeaderWriter messageHeaderWriter)
            : base(endpointConfigurationName, remoteAddress)
        {
            _messageHeaderWriter = messageHeaderWriter;
        }

        public WcfHeaderAppendingProxyHelper(Binding binding, IMessageHeaderWriter messageHeaderWriter)
            : base(binding)
        {
            _messageHeaderWriter = messageHeaderWriter;
        }

        public WcfHeaderAppendingProxyHelper(Binding binding, string remoteAddress, IMessageHeaderWriter messageHeaderWriter)
            : base(binding, remoteAddress)
        {
            _messageHeaderWriter = messageHeaderWriter;
        }

        public WcfHeaderAppendingProxyHelper(Binding binding, EndpointAddress remoteAddress, IMessageHeaderWriter messageHeaderWriter)
            : base(binding, remoteAddress)
        {
            _messageHeaderWriter = messageHeaderWriter;
        }

        public WcfHeaderAppendingProxyHelper(ServiceEndpoint endpoint, IMessageHeaderWriter messageHeaderWriter)
            : base(endpoint)
        {
            _messageHeaderWriter = messageHeaderWriter;
        }

        #endregion

        public override void CallService(Action<TInterface> action)
        {
            CallService(x =>
                {
                    action(x);
                    return true;
                });
        }

        public override TResult CallService<TResult>(Func<TInterface, TResult> action)
        {
            if (action == null) throw new ArgumentNullException("action");
            ChannelFactory<TInterface> channelFactory = null;

            try
            {
                using (channelFactory = CreateChannelFactoryInstance())
                {
                    using (var proxy = (IClientChannel)channelFactory.CreateChannel())
                    {
                        using (new OperationContextScope(proxy))
                        {
                            var header = _messageHeaderWriter.GenerateHeader();
                            OperationContext.Current.OutgoingMessageHeaders.Add(header);

                            return action((TInterface)proxy);
                        }
                    }
                }
            }
            finally
            {
                CloseConnection(channelFactory);
            }
        }
    }
}