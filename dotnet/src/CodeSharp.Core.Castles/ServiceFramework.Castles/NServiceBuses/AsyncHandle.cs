using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;
using CodeSharp.ServiceFramework.Interfaces;

namespace CodeSharp.ServiceFramework.NServiceBuses
{
    /// <summary>
    /// 基于NServiceBus的异步处理实现
    /// </summary>
    public class AsyncHandle : IAsyncHandle
    {
        private IBus _bus;
        private ILog _log;
        private ISerializer _serializer;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="bus"></param>
        public AsyncHandle(Endpoint endpoint, IBus bus)
            : this(endpoint
            , bus
            , endpoint.Resolve<ILoggerFactory>()
            , endpoint.Resolve<ISerializer>()) { }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="bus"></param>
        /// <param name="factory"></param>
        /// <param name="serializer"></param>
        public AsyncHandle(Endpoint endpoint, IBus bus, ILoggerFactory factory, ISerializer serializer)
        {
            this._bus = bus;
            this._log = factory.Create(typeof(AsyncHandle));
            this._serializer = serializer;
        }

        #region IAsyncHandle Members
        public void Handle(ServiceCall call, Type returnType, Action<object> callback)
        {
            if (this._bus == null)
                throw new InvalidOperationException("未初始化NServiceBus");
            if (callback != null)
                this._bus
                    .Send<RequestMessage>(m => m.Request = call)
                    .Register<ErrorCode>(o => callback(this._serializer.Deserialize(returnType, this.GetResult())));
            else
                this._bus.Send<RequestMessage>(m => m.Request = call);
        }
        public void Handle(ServiceCall call)
        {
            this.Handle(call, typeof(object), null);
        }
        #endregion

        private string GetResult()
        {
            return this._bus.CurrentMessageContext.Headers[RequestMessageHandler.HEADER];
        }
    }
}