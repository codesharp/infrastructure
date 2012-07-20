using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// 服务信息
    /// <remarks>经过组织整理后的服务信息</remarks>
    /// </summary>
    [DataContract]
    [Serializable]
    [KnownType(typeof(List<ServiceConfig>))]
    public class ServiceInfo
    {
        /// <summary>
        /// 获取或设置服务全名 大小写敏感
        /// <remarks>如：Taobao.Service.RemoteService</remarks>
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// 获取或设置服务所在的程序集名称 大小写敏感
        /// </summary>
        [DataMember]
        public string AssemblyName { get; set; }
        /// <summary>
        /// 获取或设置该服务的负载均衡算法脚本
        /// </summary>
        [DataMember]
        public string LoadBalancingAlgorithm { get; set; }
        /// <summary>
        /// 获取或设置该服务可用的配置列表
        /// </summary>
        [DataMember]
        public ServiceConfig[] Configs { get; set; }

        /// <summary>
        /// 获取服务类型
        /// <remarks>若当前宿主不包含该服务的代理则返回Null</remarks>
        /// </summary>
        public Type Type
        {
            get
            {
                try
                {
                    return Assembly.Load(AssemblyName).GetType(this.Name, false);
                }
                catch { return null; }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public ServiceInfo()
        {
            this.Configs = new ServiceConfig[] { };
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="assemblyName"></param>
        public ServiceInfo(string name, string assemblyName)
            : this()
        {
            this.Name = name;
            this.AssemblyName = assemblyName;
        }

        /// <summary>
        /// 为服务增加服务配置，重复则忽略
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool AddServiceConfig(ServiceConfig config)
        {
            var temp = this.Configs.ToList();
            if (temp.Exists(o => o.Equals(config))) return false;
            temp.Add(config);
            this.Configs = temp.ToArray();
            return true;
        }
    }
}
