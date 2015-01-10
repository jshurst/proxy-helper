using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ProxyHelper.Tests.Support
{
    /// <summary>
    /// This could be project or company specific.
    /// The point is that you can share this common binding file and not have to
    /// have the ServiceModel section.
    /// 
    /// You could even have a configuration switch that just said which binding to use.
    /// Having a factory isn't important...just share a class that returns a Binding,
    /// so that you can use this in your ServiceHost, and in your Client.
    /// 
    /// Once in production...how often do you really need to tweak your bindings???
    /// </summary>
    public class NetTcpBindingFactory
    {
       public static Binding GetBinding()
       {
           var binding = new NetTcpBinding();
           return binding;
       }
    }
}
