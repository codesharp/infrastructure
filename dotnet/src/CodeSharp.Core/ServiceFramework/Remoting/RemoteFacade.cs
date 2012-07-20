using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CodeSharp.ServiceFramework.Exceptions;
using CodeSharp.ServiceFramework.Interfaces;

namespace CodeSharp.ServiceFramework.Remoting
{
    /// <summary>
    /// 默认的服务节点的远程外观
    /// </summary>
    public class RemoteFacade : MarshalByRefObject
    {
        private Endpoint _endpoint { get { return Configuration.Instance().Endpoint(); } }
        private ILog _log { get { return this._endpoint.Resolve<ILoggerFactory>().Create(typeof(RemoteFacade)); } }

        /// <summary>
        /// 获取服务配置表版本
        /// </summary>
        /// <returns></returns>
        public virtual string GetVersion()
        {
            return this._endpoint.ServiceTable.Version;
        }
        /// <summary>
        /// 获取服务节点中的服务配置表
        /// </summary>
        /// <returns></returns>
        public virtual ServiceConfigTable GetServiceConfigs()
        {
            return this._endpoint.ServiceTable;
        }
        /// <summary>
        /// 获取服务的定义描述文本
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public virtual string GetServiceDescription(ServiceConfig service)
        {
            return this._endpoint.GetServiceDescription(service);
        }
        /// <summary>
        /// 向服务节点注册服务
        /// </summary>
        /// <param name="services">服务配置</param>
        public void Register(ServiceConfig[] services)
        {
            this._endpoint.Register(services);
        }
        /// <summary>
        /// 执行服务调用
        /// </summary>
        /// <returns>返回结果的JSON</returns>
        public virtual string Invoke(ServiceCall call)
        {
            try
            {
                return this._endpoint.InvokeSerialized(call);
            }
            catch (Exception e)
            {
                if (e is ServiceException)
                {
                    this._log.Warn("服务调用发生异常", e);
                    throw e;
                }
                this._log.Error("服务调用发生异常", e);
                throw new ServiceException(e.Message, e);
            }
        }
        /// <summary>
        /// 异步执行服务调用
        /// </summary>
        /// <param name="call"></param>
        public virtual void InvokeAsync(ServiceCall call)
        {
            try
            {
                this._endpoint.InvokeAsync(call);
            }
            catch (Exception e)
            {
                if (e is ServiceException) throw e;
                throw new ServiceException(e.Message, e);
            }
        }
    }
}