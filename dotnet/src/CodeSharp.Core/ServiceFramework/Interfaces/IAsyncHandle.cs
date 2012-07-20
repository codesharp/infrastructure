using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.ServiceFramework.Interfaces
{
    /// <summary>
    /// 异步处理
    /// </summary>
    public interface IAsyncHandle
    {
        /// <summary>
        /// 客户端异步处理
        /// </summary>
        /// <param name="call"></param>
        void Handle(ServiceCall call);
        /// <summary>
        /// duplex处理
        /// </summary>
        /// <param name="call"></param>
        /// <param name="returnType"></param>
        /// <param name="callback"></param>
        void Handle(ServiceCall call, Type returnType, Action<object> callback);
    }
}
