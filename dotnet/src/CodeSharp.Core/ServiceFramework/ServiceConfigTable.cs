using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// 服务表
    /// </summary>
    [Serializable]
    [DataContract]
    public class ServiceConfigTable
    {
        /// <summary>
        /// 获取或设置服务配置表来源地址
        /// <remarks>
        /// 如：
        /// tcp://taobao-wf-app:9090/RemoteFacade
        /// http://taobao-wf-app/RemoteFacade
        /// </remarks>
        /// </summary>
        [DataMember]
        public string HostUri { get; set; }
        /// <summary>
        /// 获取或设置版本
        /// </summary>
        [DataMember]
        public string Version { get; set; }
        /// <summary>
        /// 获取或设置配置列表
        /// </summary>
        [DataMember]
        public ServiceConfig[] Configs { get; set; }
        /// <summary>
        /// 获取或设置服务列表
        /// </summary>
        [DataMember]
        public ServiceInfo[] Services { get; set; }
        /// <summary>
        /// 初始化
        /// </summary>
        public ServiceConfigTable()
        {
            //HACK:取消自动版本的赋值，会导致节点和关联中心的服务版本意外的出现相等的情况
            //this.Version = DateTime.Now.ToString("yyyyMMddHHmmss");
            this.Version = string.Empty;
            this.Configs = new ServiceConfig[] { };
            this.Services = new ServiceInfo[] { };
        }

        /// <summary>
        /// 向表中增加服务配置，重复则忽略
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
        /// <summary>
        /// 向表中增加服务信息
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public void AddServiceInfo(ServiceInfo service)
        { 
            var temp = this.Services.ToList();
            temp.Add(service);
            this.Services = temp.ToArray();
        }
    }
}
