using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.ServiceFramework.Interfaces
{
    /// <summary>
    /// 为服务节点提供相关http处理功能
    /// </summary>
    public interface IHttpHandle
    {
        /// <summary>
        /// 执行服务调用
        /// </summary>
        /// <param name="call"></param>
        /// <returns></returns>
        string Invoke(ServiceCall call);
        /// <summary>
        /// 向目标地址注册服务
        /// </summary>
        /// <param name="target"></param>
        /// <param name="configs"></param>
        void Register(string target, IEnumerable<ServiceConfig> configs);
        /// <summary>
        /// 检查服务表版本
        /// </summary>
        /// <param name="target"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        bool CheckVersion(string target, string version);
        /// <summary>
        /// 获取服务表
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        ServiceConfigTable GetServiceTable(string target);
    }
}
