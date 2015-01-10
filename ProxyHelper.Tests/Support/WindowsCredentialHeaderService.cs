using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Security;
using ProxyHelper.Wcf.Headers;

namespace ProxyHelper.Tests.Support
{
    public class WindowsCredentialHeaderService : IMyService
    {
        public static string ResultMessage = "Do something.";

        public void DoWork()
        {
            ValidateHeader();
            var result = ResultMessage;
        }

        public string DoWorkAndReturnString()
        {
            ValidateHeader();
            return ResultMessage;
        }

        private void ValidateHeader()
        {
            IMessageHeaderReader<List<KeyValuePair<string, string>>> reader = new WindowsCredentialHeaderFactory();
            List<KeyValuePair<string, string>> header = reader.RetrieveHeader();

            if (header.All(x => x.Key != "windows-identity-name"))
                throw new SecurityAccessDeniedException();
        }
    }
}
