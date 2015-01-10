using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyHelper.Local
{
    public interface IResolver
    {
        T Resolve<T>() where T : class;
    }
}
