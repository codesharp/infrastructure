using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CodeSharp.ServiceFramework.Async
{
    /// <summary>
    /// 异步调用声明
    /// </summary>
    public class ServiceAsync : IDisposable
    {
        private static readonly string _slot_server = "____ServiceAsync_Server";
        private static readonly string _slot_mode = "____ServiceAsync_Mode";
        private static ServiceAsyncMode DEFAULT = ServiceAsyncMode.Sync;

        private ServiceAsyncMode _prev;
        /// <summary>
        /// 服务端异步调用声明，默认以集中式服务端异步
        /// </summary>
        public ServiceAsync() : this(ServiceAsyncMode.Centralized) { }
        /// <summary>
        /// 服务端异步调用声明
        /// </summary>
        /// <param name="mode">异步模式</param>
        public ServiceAsync(ServiceAsyncMode mode)
        {
            this._prev = Mode();
            Thread.SetData(Thread.GetNamedDataSlot(_slot_mode), mode);
        }
        /// <summary>
        /// 释放声明
        /// </summary>
        public void Dispose()
        {
            Thread.SetData(Thread.GetNamedDataSlot(_slot_mode), this._prev);
        }

        /// <summary>
        /// 是否声明为异步调用
        /// </summary>
        /// <returns></returns>
        public static bool IsAsync()
        {
            //兼容原_slot_server的声明方式
            return Convert.ToBoolean(Thread.GetData(Thread.GetNamedDataSlot(_slot_server)))
                || ServiceAsync.Mode() != ServiceAsyncMode.Sync;
        }
        /// <summary>
        /// 获取异步调用请求接收节点地址
        /// </summary>
        /// <returns></returns>
        public static ServiceAsyncMode Mode()
        {
            var mode = Thread.GetData(Thread.GetNamedDataSlot(_slot_mode));
            return mode == null ? ServiceAsync.DEFAULT : (ServiceAsyncMode)mode;
        }
        /// <summary>
        /// 获取声明的回调
        /// </summary>
        public static Action<object> Callback()
        {
            return new Action<object>(o => { });
        }
    }

    /// <summary>
    /// 异步模式
    /// </summary>
    public enum ServiceAsyncMode
    {
        /// <summary>
        /// 同步
        /// </summary>
        Sync = 0,
        /// <summary>
        /// 集中式异步，在专用异步节点
        /// </summary>
        Centralized,
        /// <summary>
        /// 服务端异步，在目标服务端
        /// </summary>
        Server,
        /// <summary>
        /// 客户端异步，在当前节点
        /// </summary>
        Client
    }
}