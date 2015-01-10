using System.ServiceModel;

namespace ProxyHelper.Tests.Support
{
    [ServiceContract]
    public interface IMyService
    {
        [OperationContract]
        void DoWork();

        [OperationContract]
        string DoWorkAndReturnString();
    }
}