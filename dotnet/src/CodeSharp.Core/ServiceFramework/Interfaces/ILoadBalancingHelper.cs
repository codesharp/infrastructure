using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.ServiceFramework.Interfaces
{
    /// <summary>
    /// 提供服务地址负载均衡功能
    /// </summary>
    public interface ILoadBalancingHelper
    {
        /// <summary>
        /// 获取服务宿主地址
        /// </summary>
        /// <param name="service"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        ServiceConfig GetServiceConfig(ServiceInfo service, string method, params ServiceCallArgument[] args);
    }
}