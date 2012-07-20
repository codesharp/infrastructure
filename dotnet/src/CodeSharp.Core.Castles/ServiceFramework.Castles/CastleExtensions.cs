using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CodeSharp.ServiceFramework.Interfaces;

namespace CodeSharp.ServiceFramework.Castles
{
    /// <summary>
    /// 为ServiceFramework增加基于Castle容器的开源实现的扩展
    /// </summary>
    public static class CastleExtensions
    {
        /// <summary>
        /// 使用Castle容器实现
        /// </summary>
        /// <param name="config"></param>
        /// <param name="container">Windsor容器，由于容器限制，传入的容器实例必须启用NamingSubSystem以支持本地/远程服务间切换</param>
        /// <returns></returns>
        public static ConfigurationWithCastle Castle(this Configuration config, IWindsorContainer container)
        {
            return Castle<RemoteServiceInterceptor>(config, container);
        }
        /// <summary>
        /// 使用Castle容器实现
        /// </summary>
        /// <typeparam name="interceptor">声明自定义的拦截器</typeparam>
        /// <param name="config"></param>
        /// <param name="container">Windsor容器，由于容器限制，传入的容器实例必须启用NamingSubSystem以支持本地/远程服务间切换</param>
        /// <returns></returns>
        public static ConfigurationWithCastle Castle<interceptor>(this Configuration config, IWindsorContainer container)
            where interceptor : RemoteServiceInterceptor
        {
            return new ConfigurationWithCastle(config.SetContainer(new WindsorContainer(container, typeof(interceptor))));
        }
    }
}