using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;

namespace CodeSharp.ServiceFramework.NServiceBuses
{
    /// <summary>
    /// 调用请求
    /// </summary>
    [Serializable]
    public class RequestMessage : IMessage
    {
        /// <summary>
        /// 方法调用请求
        /// </summary>
        public ServiceCall Request { get; set; }
    }
}
