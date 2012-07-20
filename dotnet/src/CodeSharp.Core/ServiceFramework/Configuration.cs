using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.ServiceFramework.Interfaces;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// 提供服务框架配置
    /// </summary>
    public class Configuration
    {
        protected static Configuration _config;
        protected static Endpoint _endpoint;
        /// <summary>
        /// 获取服务框架配置实例
        /// </summary> 
        /// <returns></returns>
        public static Configuration Instance()
        {
            if (_config == null)
                throw new InvalidOperationException("请先调用Configure()进行初始化");
            return _config;
        }
        /// <summary>
        /// 初始化服务节点配置
        /// </summary>
        /// <returns></returns>
        public static Configuration Configure()
        {
            if (_config != null)
                throw new InvalidOperationException("不能重复初始化");

            var config = new Configuration();
            return config;
        }
        /// <summary>
        /// 卸载当前服务节点实例
        /// </summary>
        public static void Cleanup()
        {
            _config = null;
            _endpoint = null;
            //TODO:需要卸载各类静态实例和remoting等监听
        }

        /// <summary>
        /// 获取容器
        /// </summary>
        protected internal IContainer Container { get; protected set; }

        #region 基本配置项
        /// <summary>
        /// 获取心跳检查间隔 ms
        /// </summary>
        public double Interval { get; protected set; }
        /// <summary>
        /// 获取每次心跳检查完成后的暂停时间 ms
        /// </summary>
        public int Wait { get; protected set; }
        /// <summary>
        /// 获取服务节点的默认身份
        /// </summary>
        public Identity ID { get; protected set; }
        /// <summary>
        /// 获取服务节点的地址
        /// </summary>
        public Uri Uri { get; private set; }
        /// <summary>
        /// 获取关联服务节点地址
        /// </summary>
        public Uri AssociateUri { get; protected set; }
        /// <summary>
        /// 获取服务端异步接收节点地址
        /// </summary>
        public Uri AsyncReceiverUri { get; protected set; }
        /// <summary>
        /// 获取服务节点是否自托管
        /// </summary>
        public bool SelfHosting { get; protected set; }
        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        protected Configuration()
        {
            _config = this;
            //心跳参数不宜过短
            this.Interval = 10000;
            this.Wait = 1000;
            this.SelfHosting = true;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="config"></param>
        protected Configuration(Configuration config)
            : this()
        {
            this.AssociateUri = config.AssociateUri;
            this.AsyncReceiverUri = config.AsyncReceiverUri;
            this.Container = config.Container;
            this.ID = config.ID;
            this.Interval = config.Interval;
            this.Wait = config.Wait;
            this.SelfHosting = config.SelfHosting;

            this.SetUri(config.Uri);
        }

        /// <summary>
        /// 设置默认的服务访问身份
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Configuration Identity(Identity id)
        {
            if (id == null
                || string.IsNullOrEmpty(id.AuthKey)
                || string.IsNullOrEmpty(id.Source))
                throw new InvalidOperationException("id或AuthKey或Source不能为空，请正确初始化Configuration.Identity()");
            this.ID = id;
            return this;
        }
        /// <summary>
        /// 设置服务节点心跳检查参数
        /// </summary>
        /// <param name="interval">心跳间隔</param>
        /// <param name="wait">延迟</param>
        /// <returns></returns>
        public Configuration Heartbeat(double interval, int wait)
        {
            this.Wait = wait;
            this.Interval = interval;
            return this;
        }
        /// <summary>
        /// 设置与其关联的服务节点
        /// </summary>
        /// <param name="uri">服务节点地址</param>
        /// <returns></returns>
        public Configuration Associate(Uri uri)
        {
            if (this.AssociateUri != null)
                throw new InvalidOperationException("不可重复设置关联服务节点");
            if (uri.Equals(this.Uri))
                return this;
            //throw new InvalidOperationException("不能与当前节点关联");
            this.AssociateUri = uri;
            return this;
        }
        /// <summary>
        /// 指定服务端异步请求的接收者节点地址
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public Configuration AsyncReceiver(Uri uri)
        {
            if (this.AsyncReceiverUri != null)
                throw new InvalidOperationException("不可重复设置异步接收者节点");
            //if (uri.Equals(this.Uri))
            //    throw new InvalidOperationException("不能指定为当前节点");
            this.AsyncReceiverUri = uri;
            return this;
        }

        /// <summary>
        /// 启用默认的容器实现
        /// </summary>
        /// <returns></returns>
        public ConfigurationWithDefault Default()
        {
            return new ConfigurationWithDefault(this.SetContainer(new DefaultContainer()));
        }
        /// <summary>
        /// 设置容器
        /// </summary>
        /// <param name="container">容器实例</param>
        /// <returns></returns> 
        public Configuration SetContainer(IContainer container)
        {
            if (this.Container != null)
                throw new InvalidOperationException("不可重复设置容器");

            this.Container = container;
            return this;
        }

        /// <summary>
        /// 获取当前服务节点实例
        /// </summary>
        /// <returns></returns>
        public Endpoint Endpoint()
        {
            if (_endpoint != null) return _endpoint;

            _endpoint = new Endpoint(this);
            //将节点实例注册到容器
            this.Container.Register(typeof(Endpoint), _endpoint);
            return _endpoint;
        }
        /// <summary>
        /// 设置服务节点地址
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        protected Configuration SetUri(Uri uri)
        {
            if (uri == null) return this;
            if (this.Uri != null)
                throw new InvalidOperationException("不可重复设置服务节点地址");
            if (uri == this.AssociateUri)
                throw new InvalidOperationException("不能等于关联节点地址");
            this.Uri = uri;
            return this;
        }
    }
}