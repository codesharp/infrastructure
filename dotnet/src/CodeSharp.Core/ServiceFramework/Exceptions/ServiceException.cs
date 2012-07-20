using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CodeSharp.ServiceFramework.Exceptions
{
    /// <summary>
    /// 服务异常
    /// </summary>
    /// <remarks>公开传播的异常</remarks>
    [Serializable]
    public class ServiceException : Exception
    {
        /// <summary>
        /// 异常的来源
        /// </summary>
        public string SourceUri { get; set; }
        /// <summary>
        /// 异常代码
        /// </summary>
        public ExceptionCode ErrorCode { get; set; }
        /// <summary>
        /// 初始化为未知的服务异常
        /// <remarks>ExceptionCode.Unknown</remarks>
        /// </summary>
        public ServiceException()
            : this(string.Empty)
        {
            this.ErrorCode = ExceptionCode.Unknown;
        }
        /// <summary>
        /// 初始化为未知的服务异常
        /// </summary>
        /// <param name="message">消息</param>
        public ServiceException(string message)
            : this(message, ExceptionCode.Unknown)
        {
            this.ErrorCode = ExceptionCode.Unknown;
        }
        /// <summary>
        /// 初始化为未知的服务异常
        /// </summary>
        /// <param name="message"></param>
        /// <param name="code"></param>
        public ServiceException(string message, ExceptionCode code)
            : this(message, code, null)
        {
            this.ErrorCode = ExceptionCode.Unknown;
        }
        /// <summary>
        /// 初始化为未知的服务异常
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public ServiceException(string message, Exception inner)
            : this(message, ExceptionCode.Unknown, inner)
        {
            this.ErrorCode = ExceptionCode.Unknown;
        }
        /// <summary>
        /// 初始化服务异常
        /// </summary>
        /// <param name="message"></param>
        /// <param name="code"></param>
        /// <param name="inner"></param>
        public ServiceException(string message, ExceptionCode code, Exception inner)
            : base(message, inner)
        {
            this.ErrorCode = code;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}