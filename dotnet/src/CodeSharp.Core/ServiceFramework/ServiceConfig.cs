using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// 描述具体服务的相关配置信息
    /// <remarks>包含了服务的基本类型定义，服务节点地址等</remarks>
    /// </summary>
    [DataContract]
    [Serializable]
    public class ServiceConfig
    {
        /// <summary>
        /// 获取或设置服务宿主地址
        /// <remarks>
        /// 如：
        /// tcp://taobao-wf-app:9090/RemoteFacade
        /// http://taobao-wf-app/RemoteFacade
        /// </remarks>
        /// </summary>
        [DataMember]
        public string HostUri { get; set; }
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
        public ServiceConfig() { }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="assemblyName"></param>
        /// <param name="hostUri"></param>
        public ServiceConfig(string name, string assemblyName, string hostUri)
            : this()
        {
            this.Name = name;
            this.AssemblyName = assemblyName;
            this.HostUri = hostUri;
        }

        #region 重载服务相等的判断
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var config = obj as ServiceConfig;

            return this.Name.Equals(config.Name)
                && this.AssemblyName.Equals(config.AssemblyName)
                && this.HostUri.Equals(config.HostUri, StringComparison.InvariantCultureIgnoreCase);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator ==(ServiceConfig first, ServiceConfig second)
        {
            return Equals(first, second);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator !=(ServiceConfig first, ServiceConfig second)
        {
            return !(Equals(first, second));
        }
        #endregion
    }
}