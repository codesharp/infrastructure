using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// 默认异步处理实现
    /// </summary>
    public class DefaultAsyncHandle : Interfaces.IAsyncHandle
    {
        public void Handle(ServiceCall call)
        {
            throw new NotImplementedException("未提供异步处理实现");
        }

        public void Handle(ServiceCall call, Type returnType, Action<object> callback)
        {
            throw new NotImplementedException("未提供带回调的异步处理实现");
        }
    }
}