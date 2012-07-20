using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting.Channels.Http;

using CodeSharp.ServiceFramework.Interfaces;

namespace CodeSharp.ServiceFramework.Remoting
{
    /// <summary>
    /// 基于.Net Remoting的节点通信处理
    /// 配合RemoteFacade使用
    /// <remarks>可通过派生此类</remarks>
    /// </summary>
    public class RemotingHandle : IRemoteHandle
    {
        private ILog _log;

        /// <summary>
        /// 获取要使用remoting暴露的远程类型
        /// </summary>
        protected virtual Type RemoteType { get { return typeof(RemoteFacade); } }

        public RemotingHandle(ILoggerFactory factory)
        {
            this._log = factory.Create(typeof(RemotingHandle));
        }

        public void Expose(Uri uri)
        {
            this.VerifyRemoteType();

            var properties = new Hashtable() { { "port", uri.Port }, { "name", "channel_" + uri.Port } };
            //TODO:设置windows权限
            if (uri.Scheme.Equals("tcp"))
            {
                var provider = new BinaryServerFormatterSinkProvider() { TypeFilterLevel = TypeFilterLevel.Full };
                ChannelServices.RegisterChannel(new TcpChannel(properties, null, provider), false);
            }
            else if (uri.Scheme.Equals("http"))
            {
                var provider = new SoapServerFormatterSinkProvider() { TypeFilterLevel = TypeFilterLevel.Full };
                ChannelServices.RegisterChannel(new HttpChannel(properties, null, provider), false);
            }
            else
                throw new InvalidOperationException("不支持该通道：" + uri);

            this.SetRemotingConfiguration();
            //facade无状态，使用singleton减少对象创建消耗
            //TODO:SAO+连接池对比singleton的高并发下连接创建性能？
            RemotingConfiguration.RegisterWellKnownServiceType(this.RemoteType
                , uri.LocalPath.TrimStart('/')
                , WellKnownObjectMode.Singleton);

            this._log.InfoFormat("将服务节点通过.Net Remoting暴露在地址{0}，类型为{1}", uri, this.RemoteType);
        }
        public bool TryConnect(Uri uri, int? timeout, out Exception e)
        {
            if (timeout.HasValue)
                return this.TryConnectTimeout(uri, timeout.Value, out e);

            try
            {
                var v = this.GetFacade(uri).GetVersion();
                e = null;
                return true;
            }
            catch (Exception ex)
            {
                e = ex;
                this._log.WarnFormat("连接到NSF服务节点{0}发生异常", uri, e);
                return false;
            }
        }
        public void Register(Uri uri, ServiceConfig[] services)
        {
            this.GetFacade(uri).Register(services);
        }
        public string GetVersion(Uri uri)
        {
            return this.GetFacade(uri).GetVersion();
        }
        public ServiceConfigTable GetServiceConfigs(Uri uri)
        {
            return this.GetFacade(uri).GetServiceConfigs();
        }
        public void SendAnAsyncCall(Uri uri, ServiceCall call)
        {
            this.GetFacade(uri).InvokeAsync(call);
        }

        public string GetServiceDescription(ServiceConfig service)
        {
            return this.GetFacade(service.HostUri).GetServiceDescription(service);
        }
        public string Invoke(ServiceCall call)
        {
            return this.GetFacade(call.Target.HostUri).Invoke(call);
        }

        private void VerifyRemoteType()
        {
            if (this.RemoteType != typeof(RemoteFacade)
                && !this.RemoteType.IsSubclassOf(typeof(RemoteFacade)))
                throw new InvalidCastException("RemoteType类型必须为CodeSharp.ServiceFramework.Remoting.RemoteFacade或从其派生");
        }
        //设置Remoting全局配置
        private void SetRemotingConfiguration()
        {
            //HACK:目前允许异常传播
            try
            {
                RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
            }
            catch (Exception e)
            {
                this._log.Info("设置RemotingConfiguration.CustomErrorsMode时失败", e);
            }
            RemotingConfiguration.CustomErrorsEnabled(false);
        }
        private RemoteFacade GetFacade(Uri uri)
        {
            return this.GetFacade(uri.ToString());
        }
        private RemoteFacade GetFacade(string uri)
        {
            return RemotingServices.Connect(typeof(RemoteFacade), uri) as RemoteFacade;
        }
        private bool TryConnectTimeout(Uri uri, int timeout, out Exception e)
        {
            //HACK:remoting没有提供tcp/http通道的connectionTimeout支持...

            Exception error = null;
            var t = new System.Threading.Thread(() =>
            {
                try { this.GetFacade(uri).GetVersion(); }
                catch (Exception ex) { error = ex; }
            });
            t.Start();

            if (t.Join(timeout))
                return (e = error) == null;
            else
            {
                e = new Exception(string.Format("连接到{0}时超时（{1}ms）", uri, timeout));
                return false;
            }
        }
    }
}