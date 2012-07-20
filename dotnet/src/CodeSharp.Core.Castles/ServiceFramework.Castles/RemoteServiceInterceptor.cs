using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting;
using System.Text;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using CodeSharp.ServiceFramework.Async;
using CodeSharp.ServiceFramework.Exceptions;
using CodeSharp.ServiceFramework.Interfaces;
using CodeSharp.ServiceFramework.Remoting;

namespace CodeSharp.ServiceFramework.Castles
{
    /// <summary>
    /// 远程服务拦截
    /// <remarks>
    /// 任何使用了该拦截器的服务都被认为是运行在远程
    /// 异步方式的选择由拦截器该决定（目前支持提供服务端异步）
    /// 拦截器中只处理调用逻辑而不包含失败保护等检测逻辑
    /// </remarks>
    /// </summary>
    public class RemoteServiceInterceptor : IInterceptor
    {
        private ILog _log;
        private Endpoint _endpoint;
        private ISerializer _serializer;
        private ILoadBalancingHelper _loadBalancingHelper;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="endpoint"></param>
        public RemoteServiceInterceptor(Endpoint endpoint)
            : this(endpoint
            , endpoint.Resolve<ILoggerFactory>()
            , endpoint.Resolve<ISerializer>()
            , endpoint.Resolve<ILoadBalancingHelper>()) { }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="factory"></param>
        /// <param name="serializer"></param>
        /// <param name="loadBalancingHelper"></param>
        public RemoteServiceInterceptor(Endpoint endpoint
            , ILoggerFactory factory
            , ISerializer serializer
            , ILoadBalancingHelper loadBalancingHelper)
        {
            this._endpoint = endpoint;
            this._log = factory.Create(typeof(RemoteServiceInterceptor));
            this._serializer = serializer;
            this._loadBalancingHelper = loadBalancingHelper;
        }

        #region IInterceptor Members
        public void Intercept(IInvocation invocation)
        {
            //HACK:接口代理类型自动查找第一个接口作为服务类型
            var serviceType = invocation.TargetType ?? invocation.Proxy.GetType().GetInterfaces()[0];
            var service = this._endpoint.ServiceTable.Services.FirstOrDefault(o => o.Type != null && o.Type.Equals(serviceType));
            //TODO:优化查找
            //重试一次
            if (service == null)
                service = this._endpoint.ServiceTable.Services.FirstOrDefault(o => o.Type != null && o.Type.Equals(serviceType));
            if (service == null)
                throw new ServiceException(
                    string.Format("当前服务节点不存在类型为{0}的服务配置信息", serviceType.FullName));
            //生成ServiceCall
            var call = this.ParseCall(service, invocation);
            //调用模式
            var mode = ServiceAsync.Mode();
            //同步调用
            if (!ServiceAsync.IsAsync())
                invocation.ReturnValue = this._endpoint.Invoke(call, invocation.Method.ReturnType);
            else
            {
                //HACK:必须设置默认值，否则值类型会出现异常
                //TODO:优化此处异步调用默认值的反射处理
                if (invocation.Method.ReturnType.IsValueType && invocation.Method.ReturnType != typeof(void))
                    invocation.ReturnValue = Activator.CreateInstance(invocation.Method.ReturnType);
                //客户端异步 无回调
                if (mode == ServiceAsyncMode.Client && invocation.Method.ReturnType.Equals(typeof(void)))
                    this._endpoint.InvokeAsync(call);
                //客户端异步 回调 双工 TODO:使用Saga模式 不支持longrunning
                else if (mode == ServiceAsyncMode.Client)
                    this._endpoint.InvokeAsync(call, invocation.Method.ReturnType, ServiceAsync.Callback());
                //服务器端异步 通过异步接收节点中转
                else
                    this._endpoint.AsyncInvokeAt(mode == ServiceAsyncMode.Server
                        ? new Uri(call.Target.HostUri)
                        : this._endpoint.AsyncReceiverUri
                        , call);
            }
            this._log.InfoFormat("{0}调用服务节点:{1}，服务：{2}.{3}，身份：{4}|{5}，参数：{6}，返回：{7}"
                , ServiceAsync.IsAsync() ? "以" + mode + "异步模式" : "同步"
                , call.Target.HostUri
                , call.Target.Name
                , call.TargetMethod
                , call.Identity.Source
                , call.Identity.AuthKey
                , string.Join("$", invocation.Arguments.Select(o => (o ?? "null").ToString()).ToArray())
                , ServiceAsync.IsAsync() ? "异步调用无返回值" : invocation.ReturnValue);
        }
        #endregion

        private ServiceCall ParseCall(ServiceInfo service, IInvocation invocation)
        {
            var call = new ServiceCall()
            {
                TargetMethod = invocation.Method.Name,
                Identity = this._endpoint.Configuration.ID
            };
            //填充参数
            var i = -1;
            var parameters = invocation.Method.GetParameters();
            var args = new List<ServiceCallArgument>();
            invocation.Arguments.ToList().ForEach(o =>
            {
                i++;
                args.Add(new ServiceCallArgument(parameters[i].Name
                    , this._serializer.Serialize(o, parameters[i].ParameterType)));//多态序列化
            });
            call.ArgumentCollection = args.ToArray();
            //根据负载算法选取适合的服务配置
            call.Target = this.Routing(service, call);
            return call;
        }

        /// <summary>
        /// 服务地址路由，可重写此方法以完成服务路由
        /// </summary>
        /// <param name="service">服务信息</param>
        /// <param name="call">调用信息</param>
        /// <returns></returns>
        protected virtual ServiceConfig Routing(ServiceInfo service, ServiceCall call)
        {
            return this._loadBalancingHelper.GetServiceConfig(service, call.TargetMethod, call.ArgumentCollection);
        }
    }
}