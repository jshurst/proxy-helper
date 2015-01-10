Please note that in these tests I'm using the Unity IoC Container to resolve the IProxyHelper.

This is not required, but it does show that the ProxyHelper can be used with a container (it obviously can be 
instantiated outside of a container) and that all of the different proxy helpers can be instantiated from a container.

I did this specifically to show that your internal code doesn't need to change to switch from local instantiation of classes or WCF 
(your client doesn't care what is running the code as long as it runs).

You should be able to use your container of choice...or directly instantiate it.