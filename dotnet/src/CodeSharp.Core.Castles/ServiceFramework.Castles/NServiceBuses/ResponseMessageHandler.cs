using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;
using System.Runtime.Remoting;
using System.Net;

namespace CodeSharp.ServiceFramework.NServiceBuses
{
    /// <summary>
    /// 响应消息处理
    /// <remarks>
    /// 非必须
    /// 应用可实现自己的Handler
    /// </remarks>
    /// </summary>
    public class ResponseMessageHandler : IHandleMessages<ResponseMessage>
    { 
        public IBus Bus { get; set; }
        public void Handle(ResponseMessage message) { }
    }
}
