using System.ServiceModel.Channels;

namespace ProxyHelper.Wcf.Headers
{
    public interface IMessageHeaderWriter
    {
        MessageHeader GenerateHeader();
    }
}