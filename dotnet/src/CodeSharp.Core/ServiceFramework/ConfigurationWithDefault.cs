using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.ServiceFramework.Interfaces;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// 提供基于默认容器的服务框架配置
    /// <remarks>该实现目前只支持单例服务</remarks>
    /// </summary>
    public sealed class ConfigurationWithDefault : Configuration
    {
        /// <summary>
        /// 获取默认容器
        /// </summary>
        public DefaultContainer DefaultContainer { get { return this.Container as DefaultContainer; } }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="config"></param>
        public ConfigurationWithDefault(Configuration config) : base(config) { }
        /// <summary>
        /// 向容器中注册服务类型
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="classType">必须有公开的无参构造函数</param>
        /// <returns></returns>
        public ConfigurationWithDefault Register(Type serviceType, Type classType)
        {
            this.DefaultContainer.Register(serviceType, classType);
            return this;
        }
        /// <summary>
        /// 向容器中注册服务类型
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public ConfigurationWithDefault Register(Type serviceType, object instance)
        {
            this.DefaultContainer.Register(serviceType, instance);
            return this;
        }
        /// <summary>
        /// 自定义日志工厂
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public ConfigurationWithDefault LoggerFactory(ILoggerFactory factory)
        {
            this.DefaultContainer.Register(typeof(ILoggerFactory), factory);
            return this;
        }
        /// <summary>
        /// 自定义的身份实现
        /// </summary>
        /// <param name="auth"></param>
        /// <returns></returns>
        public ConfigurationWithDefault Authentication(IAuthentication auth)
        {
            this.DefaultContainer.Register(typeof(IAuthentication), auth);
            return this;
        }
        /// <summary>
        /// 自定义负载均衡实现
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public ConfigurationWithDefault LoadBalancing(ILoadBalancingHelper helper)
        {
            this.DefaultContainer.Register(typeof(ILoadBalancingHelper), helper);
            return this;
        }

        /// <summary>
        /// 启用Remoting
        /// </summary>
        /// <param name="uri">指定该服务节点的remoting地址</param>
        /// <returns></returns>
        public ConfigurationWithDefault Remoting(Uri uri)
        {
            return this.SetUri(uri) as ConfigurationWithDefault;
        }
        /// <summary>
        /// 启用Remoting
        /// </summary>
        /// <param name="uri">指定该服务节点的remoting地址</param>
        /// <param name="selfHosting">是否自托管，若要托管于外部宿主（如IIS），则设置为false</param>
        /// <returns></returns>
        public ConfigurationWithDefault Remoting(Uri uri, bool selfHosting)
        {
            this.SelfHosting = selfHosting;
            return this.SetUri(uri) as ConfigurationWithDefault;
        }
    }
}