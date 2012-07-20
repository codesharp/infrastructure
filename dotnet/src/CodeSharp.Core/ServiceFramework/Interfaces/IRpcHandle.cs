using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.ServiceFramework.Interfaces
{
    /// <summary>
    /// 为服务节点提供相关远程通信处理功能
    /// </summary>
    public interface IRemoteHandle
    {
        /// <summary>
        /// 对当前节点外观进行暴露
        /// </summary>
        /// <param name="uri">暴露地址</param>
        void Expose(Uri uri);
        /// <summary>
        /// 尝试连接到指定服务节点
        /// </summary>
        /// <param name="uri">服务节点地址</param>
        /// <param name="timeout">超时设置</param>
        /// <param name="e">返回异常</param>
        /// <returns></returns>
        bool TryConnect(Uri uri, int? timeout, out Exception e);
        /// <summary>
        /// 向服务节点注册服务
        /// </summary>
        /// <param name="uri">服务节点地址</param>
        /// <param name="services">服务配置</param>
        void Register(Uri uri, ServiceConfig[] services);
        /// <summary>
        /// 获取服务节点的服务表版本
        /// </summary>
        /// <param name="uri">服务节点地址</param>
        /// <returns></returns>
        string GetVersion(Uri uri);
        /// <summary>
        /// 获取服务节点中的服务配置表
        /// </summary>
        /// <returns></returns>
        ServiceConfigTable GetServiceConfigs(Uri uri);
        /// <summary>
        /// 获取服务描述
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        string GetServiceDescription(ServiceConfig service);
        /// <summary>
        /// 执行服务调用
        /// </summary>
        /// <param name="call"></param>
        string Invoke(ServiceCall call);
        /// <summary>
        /// 向指定服务节点发送异步调用请求
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="call"></param>
        void SendAnAsyncCall(Uri uri, ServiceCall call);
    }
}
