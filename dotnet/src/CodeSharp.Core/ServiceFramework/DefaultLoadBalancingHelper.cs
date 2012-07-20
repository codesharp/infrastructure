using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.ServiceFramework.Interfaces;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// 负载均衡默认实现
    /// </summary>
    public class DefaultLoadBalancingHelper : Interfaces.ILoadBalancingHelper
    {
        private Random _rd;
        private static readonly string _function = "balance";
        private ILog _log;
        public DefaultLoadBalancingHelper(ILoggerFactory factory)
        {
            this._rd = new Random();
            this._log = factory.Create(typeof(DefaultLoadBalancingHelper));
        }
        public ServiceConfig GetServiceConfig(ServiceInfo service, string method, params ServiceCallArgument[] args)
        {
            //if (string.IsNullOrEmpty(service.LoadBalancingAlgorithm))
            return this.GetDefaultServiceConfig(service);
        }
        private ServiceConfig GetDefaultServiceConfig(ServiceInfo service)
        {
            //TODO:常用负载算法
            return service.Configs[this._rd.Next(0, service.Configs.Length)];
            //return service.Configs.First();
        }
    }
}