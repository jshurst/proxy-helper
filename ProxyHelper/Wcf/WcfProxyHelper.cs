using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace ProxyHelper.Wcf
{
    public class WcfProxyHelper<TInterface> : IProxyHelper<TInterface> where TInterface : class
    {
        #region Props, Variables, and Enums

        private enum ChannelFactoryConstructorParams
        {
            Binding = 2,
            EndpointConfigurationName = 4,
            ServiceEndpoint = 6,
            EndpointConfigurationNameAndRemoteAddress = 8,
            BindingAndRemoteAddressString = 10,
            BindingAndEndpointAddress = 12,
            None = 14
        }

        private readonly ChannelFactoryConstructorParams _channelFactoryConstructorParams;
        private readonly Binding _binding;
        private readonly string _endpointConfigurationName;
        private readonly EndpointAddress _remoteAddress;
        private readonly string _remoteAddressString;
        private readonly ServiceEndpoint _serviceEndpoint;

        #endregion

        #region ctor

        public WcfProxyHelper()
        {
            _channelFactoryConstructorParams = ChannelFactoryConstructorParams.None;
        }

        public WcfProxyHelper(string endpointConfigurationName)
        {
            _channelFactoryConstructorParams = ChannelFactoryConstructorParams.EndpointConfigurationName;
            _endpointConfigurationName = endpointConfigurationName;
        }

        public WcfProxyHelper(string endpointConfigurationName, EndpointAddress remoteAddress)
        {
            _channelFactoryConstructorParams = ChannelFactoryConstructorParams.EndpointConfigurationNameAndRemoteAddress;
            _endpointConfigurationName = endpointConfigurationName;
            _remoteAddress = remoteAddress;
        }

        public WcfProxyHelper(Binding binding)
        {
            _channelFactoryConstructorParams = ChannelFactoryConstructorParams.Binding;
            _binding = binding;
        }

        public WcfProxyHelper(Binding binding, string remoteAddress)
        {
            _channelFactoryConstructorParams = ChannelFactoryConstructorParams.BindingAndRemoteAddressString;
            _binding = binding;
            _remoteAddressString = remoteAddress;
        }

        public WcfProxyHelper(Binding binding, EndpointAddress remoteAddress)
        {
            _channelFactoryConstructorParams = ChannelFactoryConstructorParams.BindingAndEndpointAddress;
            _binding = binding;
            _remoteAddress = remoteAddress;
        }

        public WcfProxyHelper(ServiceEndpoint endpoint)
        {
            _channelFactoryConstructorParams = ChannelFactoryConstructorParams.ServiceEndpoint;
            _serviceEndpoint = endpoint;
        }

        #endregion

        public virtual void CallService(Action<TInterface> action)
        {
            CallService(x =>
            {
                action(x);
                return true;
            });
        }

        public virtual TResult CallService<TResult>(Func<TInterface, TResult> action)
        {
            if (action == null) throw new ArgumentNullException("action");
            var channelFactory = CreateChannelFactoryInstance();

            try
            {
                return action(channelFactory.CreateChannel());
            }
            finally
            {
                CloseConnection(channelFactory);
            }
        }

        protected virtual ChannelFactory<TInterface> CreateChannelFactoryInstance()
        {
            ChannelFactory<TInterface> channelFactory;

            switch (_channelFactoryConstructorParams)
            {
                case ChannelFactoryConstructorParams.Binding:
                    channelFactory = new ChannelFactory<TInterface>(_binding);
                    break;
                case ChannelFactoryConstructorParams.BindingAndEndpointAddress:
                    channelFactory = new ChannelFactory<TInterface>(_binding, _remoteAddress);
                    break;
                case ChannelFactoryConstructorParams.BindingAndRemoteAddressString:
                    channelFactory = new ChannelFactory<TInterface>(_binding, _remoteAddressString);
                    break;
                case ChannelFactoryConstructorParams.EndpointConfigurationName:
                    channelFactory = new ChannelFactory<TInterface>(_endpointConfigurationName);
                    break;
                case ChannelFactoryConstructorParams.EndpointConfigurationNameAndRemoteAddress:
                    channelFactory = new ChannelFactory<TInterface>(_endpointConfigurationName, _remoteAddress);
                    break;
                case ChannelFactoryConstructorParams.ServiceEndpoint:
                    channelFactory = new ChannelFactory<TInterface>(_serviceEndpoint);
                    break;
                case ChannelFactoryConstructorParams.None:
                    channelFactory = new ChannelFactory<TInterface>(typeof(TInterface).ToString());
                    break;
                default:
                    throw new ApplicationException("ChannelFactory instantiation parameters not found.");
            }


            return channelFactory;
        }

        protected virtual void CloseConnection(ICommunicationObject communicationObject)
        {
            if (communicationObject.State == CommunicationState.Faulted)
            {
                communicationObject.Abort();
            }
            else
            {
                try
                {
                    communicationObject.Close();
                }
                catch
                {
                    communicationObject.Abort();
                }
            }
        }
    }
}