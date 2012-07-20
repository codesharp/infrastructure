using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// 服务调用的完整声明
    /// </summary>
    [Serializable]
    [DataContract]
    [KnownType(typeof(ServiceConfig))]
    [KnownType(typeof(Identity))]
    [KnownType(typeof(ServiceCallArgument[]))]
    public class ServiceCall
    {
        /// <summary>
        /// 获取或设置调用目标的服务配置
        /// </summary>
        [DataMember]
        public ServiceConfig Target { get; set; }
        /// <summary>
        /// 获取或设置调用目标方法名
        /// </summary>
        [DataMember]
        public string TargetMethod { get; set; }
        /// <summary>
        /// 获取或设置调用的序列化参数组
        /// </summary>
        [DataMember]
        public ServiceCallArgument[] ArgumentCollection { get; set; }
        /// <summary>
        /// 调用者身份标识
        /// </summary>
        [DataMember]
        public Identity Identity { get; set; }
        /// <summary>
        /// 初始化
        /// </summary>
        public ServiceCall()
        {
            this.ArgumentCollection = new ServiceCallArgument[] { };
            this.Identity = new Identity();
        }
    }
    /// <summary>
    /// 参数
    /// </summary>
    [Serializable]
    [DataContract]
    public class ServiceCallArgument
    {
        /// <summary>
        /// 参数名
        /// </summary>
        [DataMember]
        public string Key { get; set; }
        /// <summary>
        /// 参数值
        /// <remarks>序列化后的文本</remarks>
        /// </summary>
        [DataMember]
        public string Value { get; set; }
        /// <summary>
        /// 初始化
        /// </summary>
        public ServiceCallArgument() { }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public ServiceCallArgument(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}