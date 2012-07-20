using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using CodeSharp.ServiceFramework.Interfaces;
using CodeSharp.ServiceFramework.Remoting;

namespace CodeSharp.ServiceFramework.Castles
{
    /// <summary>
    /// 提供基于castle的服务框架配置
    /// </summary>
    public sealed class ConfigurationWithCastle : Configuration
    {
        /// <summary>
        /// 获取Windsor容器
        /// </summary>
        public IWindsorContainer WindsorContainer { get { return (this.Container as WindsorContainer)._container; } }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="config"></param>
        public ConfigurationWithCastle(Configuration config) : base(config) { }

        /// <summary>
        /// 设置与其关联的服务节点
        /// </summary>
        /// <param name="uri">服务节点地址</param>
        /// <returns></returns>
        public new ConfigurationWithCastle Associate(Uri uri)
        {
            return base.Associate(uri) as ConfigurationWithCastle;
        }
        /// <summary>
        /// 指定服务端异步请求的接收者节点地址
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public new ConfigurationWithCastle AsyncReceiver(Uri uri)
        {
            return base.AsyncReceiver(uri) as ConfigurationWithCastle;
        }
        /// <summary>
        /// 设置服务节点心跳检查参数
        /// </summary>
        /// <param name="interval">心跳间隔</param>
        /// <param name="wait">延迟</param>
        /// <returns></returns>
        public new ConfigurationWithCastle Heartbeat(double interval, int wait)
        {
            return base.Heartbeat(interval, wait) as ConfigurationWithCastle;
        }
        /// <summary>
        /// 设置默认的服务访问身份
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public new ConfigurationWithCastle Identity(Identity id)
        {
            return base.Identity(id) as ConfigurationWithCastle;
        }

        /// <summary>
        /// 自定义日志工厂
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public ConfigurationWithCastle LoggerFactory(ILoggerFactory factory)
        {
            this.WindsorContainer.Register(Component.For<ILoggerFactory>().Instance(factory));
            return this;
        }
        /// <summary>
        /// 自定义的身份实现
        /// </summary>
        /// <param name="auth"></param>
        /// <returns></returns>
        public ConfigurationWithCastle Authentication(IAuthentication auth)
        {
            this.WindsorContainer.Register(Component.For<IAuthentication>().Instance(auth));
            return this;
        }
        /// <summary>
        /// 自定义负载均衡实现
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public ConfigurationWithCastle LoadBalancing(ILoadBalancingHelper helper)
        {
            this.WindsorContainer.Register(Component.For<ILoadBalancingHelper>().Instance(helper));
            return this;
        }
        /// <summary>
        /// 使用自托管.Net Remoting
        /// </summary>
        /// <param name="uri">指定该服务节点的remoting地址</param>
        /// <returns></returns>
        public ConfigurationWithCastle Remoting(Uri uri)
        {
            return this.Remoting(uri, true);
        }
        /// <summary>
        /// 使用.Net Remoting通信
        /// </summary>
        /// <param name="uri">指定该服务节点的remoting地址</param>
        /// <param name="selfHosting">是否自托管，若要托管于外部宿主（如IIS），则设置为false</param>
        /// <returns></returns>
        public ConfigurationWithCastle Remoting(Uri uri, bool selfHosting)
        {
            this.SelfHosting = selfHosting;
            this.SetUri(uri);
            this.WindsorContainer
                .Register(Component.For<IRemoteHandle>()
                .ImplementedBy<RemotingHandle>());
            return this;
        }
        /// <summary>
        /// 自定义要使用的远程处理实现
        /// </summary>
        /// <typeparam name="T">远程处理实现类型</typeparam>
        /// <param name="uri">指定该服务节点的远程地址</param>
        /// <param name="selfHosting">是否自托管</param>
        /// <returns></returns>
        public ConfigurationWithCastle Remote<T>(Uri uri, bool selfHosting) where T : IRemoteHandle
        {
            this.SelfHosting = selfHosting;
            this.SetUri(uri);
            this.WindsorContainer
                .Register(Component.For<IRemoteHandle>()
                .ImplementedBy<T>());
            return this;
        }
        /// <summary>
        /// 启用Log4Net
        /// </summary>
        /// <param name="configure">是否需要对log4net进行初始化，若需要将使用XML方式</param>
        /// <returns></returns>
        public ConfigurationWithCastle Log4Net(bool configure)
        {
            if (configure)
                log4net.Config.XmlConfigurator.Configure();

            this.WindsorContainer.Register(Component
                .For<ILoggerFactory>()
                .ImplementedBy<Log4NetLoggerFactory>()
                .LifeStyle.Singleton);
            return this;
        }
    }
}