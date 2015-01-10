# I.	Introduction

This library creates proxies on the fly.  I got tired of creating proxy classes for every WCF service and of seeing these proxies not disposed of properly.
This library is not limitied to WCF though, you can also use it for object instantiation, so you can very easily switch out local "services" with WCF services (and hopefully soon REST services - although, I haven't decided if this even makes sense) 
without making modifications to your code (you only modify your bootstrapper/container registrations).

This code does not depend on any external libraries outside of the .Net Framework.  In addition one of the goals was to make sure that the ProxyHelper is unit testable and mockable.

__Please note that the examples below come from the source code and unit tests.  I STRONGLY encourage you to read them to see how this can be used.__

# II.	How To

Regardless of the ProxyHelper that you choose you should be able to write the same code in your class.

Consider the following (taken from one of the unit tests).

```    
	public class FakeController
    {   
        private readonly IProxyHelper<IMyService> _proxyHelper;   

        public FakeController(IProxyHelper<IMyService> proxyHelper)  
        {  
            _proxyHelper = proxyHelper;  
        }  

        public string GetSomeData()  
        {  
            var result = _proxyHelper.CallService(proxy => proxy.DoWorkAndReturnString());  
            return result;  
        }  
	}  
```  

From looking at this you can't really tell what IProxyHelper< MyService> is calling.  It might be a WCF service, or even perhaps just a local object/method that just implements the IMyService contract and it's good that it isn't married to WCF...our container will tell us what's actually registered.

So let's look in the container...

```	  
	var container = new UnityContainer();  
    container.RegisterType(typeof(IProxyHelper<IMyService>), typeof(WcfProxyHelper<IMyService>));
```  

Here we can see it is a WcfProxyHelper.

This is the basic pattern for all proxy helpers in this library.  The difference is just in the constructors for each ProxyHelper implementation.

...and don't worry, as you'll see below using Unity (or any IoC Container for that matter) is totally optional.


##A.	Types of ProxyHelpers (at the time of this writing)


I might add new ones in the future (they are very easy to extend).

###1.	WcfProxyHelper
	
Under the covers it just builds a ChannelFactory.  Because of this it has constructors to match those of the ChannelFactory ( http://msdn.microsoft.com/en-us/library/ms576132.aspx ).

If you are interested, under the covers the proxyhelper is simply doing this:

```  	 
    var channelFactory = CreateChannelFactoryInstance();

    try
    {
        return action(channelFactory.CreateChannel());
    }
    finally
    {
        CloseConnection(channelFactory);
    }  
```

I won't elaborate on the private methods here (read the source code if you are interested).

And as mentioned above you can use it (like all proxyhelpers) like this.

```  
	_proxyHelper.CallService(proxy => proxy.DoWork());  
```

Or, if you want to share an open channel for multiple calls, then you can do this.

```  
	string result = null;  
    string result2 = null;  
            
    _proxyHelper.CallService(proxy =>
        {
            proxy.DoWork();
            result = proxy.DoWorkAndReturnString();
            result2 = proxy.DoWorkAndReturnString();
        });  
```

###2.	HeaderAppendingWcfProxyHelper

Often times I find myself needing to pass information in WCF Headers.  This can be very useful for a lot of scenarios.
A common one that I've run across is needing to pass Claims over which can be very useful in a Trusted-Subsystem pattern (if you don't want to use delegation).

There is no change in how you would write your code from the regular WCF Proxyhelper, however the constructors are different.

You still have all of the regular ChannelFactory constructors, however I've added an IMessageHeaderWriter parameter to each of them.  

Let's look at this interface.

```
	public interface IMessageHeaderWriter  
    {  
		MessageHeader GenerateHeader();  
    }  
```

After the channel is constructed, a header is appended to the outgoing request.  By passing in an interface you can easily extend this to pass whatever headers you want.

Here is an example of passing a username in a keyvalue pair.


```  
	public MessageHeader GenerateHeader()  
	{  
		var windowsIdentity = (Thread.CurrentPrincipal.Identity) as WindowsIdentity;  
		if (windowsIdentity == null) throw new ApplicationException("Invalid Windows Identity.");  

		var claims = new List<KeyValuePair<string, string>>  
			{  
				new KeyValuePair<string, string>("windows-identity-name", windowsIdentity.Name)  
			};  
            
		var headerValue = new MessageHeader<List<KeyValuePair<string, string>>>(claims);  
		var messageHeader = headerValue.GetUntypedHeader("my-header", "ns");  

		return messageHeader;  
	}  
```

Of course you'll probably want to read this (it's just a standard header).

```  
	var header = OperationContext.Current.IncomingMessageHeaders.GetHeader<List<KeyValuePair<string,string>>>("my-header", "ns");	

	if (header.All(x => x.Key != "windows-identity-name"))  
		throw new SecurityAccessDeniedException();  
```

You could even put this in an Attribute and decorate your WCF headers with it.


###3.	SharedInstanceProxyHelper

Many times in development you don't need your layers behind an actual service, however if you are building a system that might need to scale, you might want to leave it open to be easily changed to use a service should the need arise.

This ProxyHelper (along with the MethodInstantiatingProxyHelper) will simply invoke methods from a local object instance.

Here is how you would register it using Unity (it could just as easily be insantiated by you, instead of a container).

```  
	var container = new UnityContainer();  
	container.RegisterType(typeof (IProxyHelper<IMyService>), typeof (SharedInstanceProxyHelper<IMyService>),
		new InjectionConstructor(typeof(MyService)));
```

This ProxyHelper will not dispose of the instance of MyService, so the service can be used across multiple calls of the ProxyHelper.

###4.	MethodInstantiatingProxyHelper

In contrast to the SharedInstanceProxyHelper, the MethodInstantiatingProxyHelper will create a new instance of your class each time a method is called.  It will then dispose of the instance.

A registration might look like the following.

```   
  var container = new UnityContainer();  
  container.RegisterType<IMyService, MyService>();  
  container.RegisterType<IResolver, UnityResolver>(new InjectionConstructor(container));  
  container.RegisterType(typeof (IProxyHelper<IMyService>), typeof(MethodInstantiatingProxyHelper<IMyService>));  
```

Notice that in this registration we are registering a type of IResolver that maps to UnityResolver.  The IResolver interface is included in this library and is passed to the MethodInstantiatingProxyHelper in its constructor.

The IResolver inteface looks like this.

```   
	public interface IResolver  
    {  
        T Resolve<T>() where T : class;  
    }  
```

It only has one method contract and under the covers of the MethodInstantiatingProxyHelper, the internal code is doing this.

```    
	IResolver _resolver;  
	...  

	instance = _resolver.Resolve<TInterface>();  
    return action(instance);  
```

So, you can choose how to resolve the instance any method you choose (even Activator.CreateInstance).

Here is an example of an IResolver using Unity.

```  
	public class UnityResolver : IResolver  
    {  
        private readonly IUnityContainer _container;  

        public UnityResolver(IUnityContainer container)  
        {  
            _container = container;  
        }  

        public T Resolve<T>() where T : class  
        {  
            return _container.Resolve<T>();  
        }  
    }    
```  

# III.	Unit Testing and Mocking
One of my goals was for this to be unit testable.  If you look above to the Controller example you'll see a very familiar pattern.  We want to pass a ProxyHelper interface into the the controller's constructor and be able to write a test against it.

Using Moq, we can do the following:

```  
	[TestMethod]  
    public void MockTheProxyHelperAndMakeSingleCall()  
    {  
        const string EXPECTED = "hey";  
        var mockProxyHelper = new Mock<IProxyHelper<IMyService>>();  
        mockProxyHelper.Setup(proxy => proxy.CallService(It.IsAny<Func<IMyService, string>>()))  
            .Returns(EXPECTED);  

        var result = new FakeController(mockProxyHelper.Object).CallASingleServiceMethod();  

        Assert.AreEqual(result, EXPECTED);  
    }  
```