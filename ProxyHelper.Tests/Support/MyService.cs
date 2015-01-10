namespace ProxyHelper.Tests.Support
{
    public class MyService : IMyService
    {
        public static string ResultMessage = "Do something.";

        public void DoWork()
        {
            var result = ResultMessage;
        }

        public string DoWorkAndReturnString()
        {
            return ResultMessage;
        }
    }
}
