using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;

namespace CodeSharp.ServiceFramework.NServiceBuses
{
    /// <summary>
    /// 调用响应
    /// </summary>
    [Serializable]
    public class ResponseMessage : IMessage
    {
        /// <summary>
        /// 序列化结果
        /// </summary>
        public string Result { get; set; }
    }
}