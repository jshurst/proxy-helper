using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using ProxyHelper.Wcf.Headers;

namespace ProxyHelper.Tests.Support
{
    public interface IMessageHeaderReader<out T> where T : class
    {
        T RetrieveHeader();
    }

    public class WindowsCredentialHeaderFactory : IMessageHeaderWriter, IMessageHeaderReader<List<KeyValuePair<string, string>>> 
    {
        public const string HEADER_NAME = "message-header";
        public const string NS = "ns";

        public MessageHeader GenerateHeader()
        {
            var windowsIdentity = (Thread.CurrentPrincipal.Identity) as WindowsIdentity;
            if (windowsIdentity == null) throw new ApplicationException("Invalid Windows Identity.");

            var claims = new List<KeyValuePair<string, string>>
                {
                    //Should we encrypt the claims value???
                    new KeyValuePair<string, string>("windows-identity-name", windowsIdentity.Name)
                };
            
            var headerValue = new MessageHeader<List<KeyValuePair<string, string>>>(claims);
            var messageHeader = headerValue.GetUntypedHeader(HEADER_NAME, NS);

            return messageHeader;
        }

        public List<KeyValuePair<string,string>> RetrieveHeader()
        {
            return OperationContext.Current.IncomingMessageHeaders.FindHeader(HEADER_NAME, NS) > -1 
                ? OperationContext.Current.IncomingMessageHeaders.GetHeader<List<KeyValuePair<string,string>>>(HEADER_NAME, NS) 
                : null;
        }
    }
}