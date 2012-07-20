using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Net;
using System.IO;
using CodeSharp.ServiceFramework.Interfaces;
using CodeSharp.ServiceFramework.Exceptions;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// 服务节点
    /// </summary>
    /// <remarks>
    /// HACK:服务节点共享服务配置表 节点+关联节点
    /// </remarks>
    public class Endpoint
    {
        //连接超时ms 不能设置过短，容易造成服务在不可服务的情况下意外刷新
        private static readonly int _connectTimeout = 5000;
        private static bool _internalDebug;
        private static object _timerLock = new object();
        private static object _serviceTableLock = new object();
        private System.Timers.Timer _refreshTimer;
        private bool _refreshFlag = false;

        private IContainer _container { get { return this.Configuration.Container; } }
        private ILog _log { get { return this.Resolve<ILoggerFactory>().Create(this.GetType()); } }
        private ISerializer _serializer { get { return this.Resolve<ISerializer>(); } }
        private IRemoteHandle _remoteHandle { get { return this.Resolve<IRemoteHandle>(); } }

        /// <summary>
        /// 服务节点配置实例
        /// </summary>
        public Configuration Configuration { get; private set; }
        /// <summary>
        /// 获取服务节点地址
        /// </summary>
        public Uri Uri { get { return this.Configuration.Uri; } }
        /// <summary>
        /// 获取关联服务节点地址
        /// </summary>
        public Uri AssociateUri { get { return this.Configuration.AssociateUri; } }
        /// <summary>
        /// 获取默认的服务端异步请求接收节点地址
        /// </summary>
        public Uri AsyncReceiverUri { get { return this.Configuration.AsyncReceiverUri ?? this.AssociateUri; } }

        #region ServiceTable
        /// <summary>
        /// 本地服务配置列表
        /// </summary>
        private List<ServiceConfig> _localTable;
        /// <summary>
        /// 来自其他节点的服务配置列表
        /// </summary>
        private List<ServiceConfig> _clientTable;
        /// <summary>
        /// 来自关联节点服务表
        /// </summary>
        private ServiceConfigTable _associateTable;
        
        //汇总服务表=本地服务表+关联节点服务表+客户端节点服务表
        private ServiceConfigTable _serviceTable;
        /// <summary>
        /// 获取该节点所有服务的配置表
        /// <remarks>包含：该节点已注册的本地服务，关联节点的服务，其他节点注册在该节点的服务</remarks>
        /// </summary>
        public ServiceConfigTable ServiceTable
        {
            get
            {
                if (this._serviceTable != null)
                    return this._serviceTable;

                lock (_serviceTableLock)
                {
                    if (this._serviceTable != null)
                        return this._serviceTable;

                    var table = new ServiceConfigTable()
                    {
                        HostUri = this.Uri == null ? string.Empty : this.Uri.ToString(),
                        Version = DateTime.Now.ToString("yyyyMMddHHmmss")//设置版本
                    };
                    this.PrepareServiceTable(table, this._localTable);
                    this.PrepareServiceTable(table, this._associateTable.Configs.ToList());
                    this.PrepareServiceTable(table, this._clientTable);
                    //替换，避免在填充过程获取到未填充完整的集合
                    return this._serviceTable = table;
                }
            }
        }
        #endregion

#if DEBUG
        static Endpoint()
        {
            _internalDebug = true;
        }
#endif
        /// <summary>
        /// 初始化
        /// </summary>
        internal Endpoint(Configuration configuration)
        {
            this.Configuration = configuration;

            if (this.Configuration.ID == null)
                this.Configuration.Identity(new Identity()
                {
                    Source = System.Environment.MachineName,
                    //TODO:修改为外部指定
                    AuthKey = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(System.Environment.MachineName, "MD5")
                });

            this._localTable = new List<ServiceConfig>();
            this._clientTable = new List<ServiceConfig>();
            this._associateTable = new ServiceConfigTable();
        }

        /// <summary>
        /// 尝试强制刷新服务配置表
        /// </summary>
        public void Refresh()
        {
            if (this.RefreshAssociate())
                this.ClearServiceTable();
            if (this.RefreshClientTable())
                this.ClearServiceTable();
        }
        /// <summary>
        /// 向该节点添加本地服务
        /// <remarks>重复则忽略</remarks>
        /// </summary>
        /// <param name="serviceTypes">服务类型</param>
        public Endpoint Add(params Type[] serviceTypes)
        {
            if (this.Uri == null)
                throw new InvalidOperationException("没有为当前节点设置暴露方式，不能添加服务");
            if (serviceTypes != null)
                serviceTypes.ToList().ForEach(o => this.Add(o, string.Empty));
            return this;
        }
        /// <summary>
        /// 向该节点添加本地服务
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="loadBalancingAlgorithm">指定服务的负载均衡算法</param>
        /// <returns>返回是否添加成功</returns>
        public Endpoint Add(Type serviceType, string loadBalancingAlgorithm)
        {
            if (this.Uri == null)
                throw new InvalidOperationException("没有为当前节点设置暴露方式，不能添加服务");

            if (serviceType == null) return this;

            if (!this.AddIfNotExist(this._localTable
                , new ServiceConfig(serviceType.FullName
                , serviceType.Assembly.FullName
                , this.Uri.ToString()) { LoadBalancingAlgorithm = loadBalancingAlgorithm })) return this;

            this._log.InfoFormat("在当前节点添加本地服务：{0},{1}"
                , serviceType.FullName
                , serviceType.Assembly.FullName);

            this.ClearServiceTable();

            return this;
        }
        /// <summary>
        /// 向该节点注册远程服务
        /// <remarks>重复则忽略</remarks>
        /// </summary>
        /// <param name="services">服务配置</param>
        public void Register(params ServiceConfig[] services)
        {
            if (services == null) return;

            var flag = false;
            //过滤重复Name+AssemblyName+HostUri
            services.ToList().ForEach(o =>
            {
                //排除意外出现的本地服务
                if (this.IsLocal(o.HostUri)) return;
                //若不重复则注册
                if (this.AddIfNotExist(this._clientTable, o))
                {
                    flag = true;
                    this._log.InfoFormat("在当前服务节点注册了远程服务：{0},{1} {2}", o.Name, o.AssemblyName, o.HostUri);
                }
            });
            if (flag)
                this.ClearServiceTable();
        }
        /// <summary>
        /// 获取指定服务的描述
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public string GetServiceDescription(ServiceConfig service)
        {
            return this.IsLocal(service.HostUri)
                ? this.Resolve<IServiceDescriptionRender>().Render(service)
                : this._remoteHandle.GetServiceDescription(service);
        }
        /// <summary>
        /// 获取组件可用实例
        /// <remarks>
        /// 不存在则返回Null
        /// 框架内以及依赖于此框架容器的类型获取组件应都使用此方法
        /// </remarks>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>() where T : class
        {
            return this._container.Resolve<T>();
        }

        /// <summary>
        /// 执行服务调用
        /// <remarks>自动根据服务位置进行调用</remarks>
        /// </summary>
        /// <typeparam name="T">期望的返回值类型</typeparam>
        /// <param name="call">调用声明</param>
        /// <returns></returns>
        /// <exception cref="ServiceException"></exception>
        public T Invoke<T>(ServiceCall call)
        {
            return (T)this.Invoke(call, typeof(T));
        }
        /// <summary>
        /// 执行服务调用
        /// <remarks>自动根据服务位置进行调用</remarks>
        /// </summary>
        /// <param name="call">调用声明</param>
        /// <param name="returnType">期望的返回值类型</param>
        /// <returns></returns>
        /// <exception cref="ServiceException"></exception>
        public object Invoke(ServiceCall call, Type returnType)
        {
            Type type;
            return this.IsLocal(call.Target.HostUri)
                ? this.InvokeLocal(call, out type)
                : this._serializer.Deserialize(returnType, this.InvokeRemote(call));
        }
        /// <summary>
        /// 执行服务调用，返回序列化结果
        /// </summary>
        /// <param name="call">调用声明</param>
        /// <returns></returns> 
        public string InvokeSerialized(ServiceCall call)
        {
            Type returnType;
            return this.IsLocal(call.Target.HostUri)
                ? this._serializer.Serialize(this.InvokeLocal(call, out returnType), returnType)
                : this.InvokeRemote(call);
        }
        /// <summary>
        /// 异步执行服务调用
        /// <remarks>客户端/节点内异步</remarks>
        /// </summary>
        /// <param name="call"></param>
        /// <exception cref="ServiceException"></exception>
        public void InvokeAsync(ServiceCall call)
        {
            this.Resolve<IAsyncHandle>().Handle(call);

            this._log.DebugFormat("在当前节点发起对服务{0}|{1}|{2}的异步调用请求"
              , call.Target.Name
              , call.TargetMethod
              , call.Target.HostUri);
        }
        /// <summary>
        /// 异步执行服务调用 duplex
        /// <remarks>默认使用客户端异步模式</remarks>
        /// </summary>
        /// <param name="call"></param>
        /// <param name="returnType">期望的返回值类型</param>
        /// <param name="callback">回调</param>
        /// <exception cref="ServiceException"></exception>
        [Obsolete("暂不使用，非可靠")]
        public void InvokeAsync(ServiceCall call, Type returnType, Action<object> callback)
        {
            this.Resolve<IAsyncHandle>().Handle(call, returnType, callback);
        }
        /// <summary>
        /// 在指定节点执行异步调用
        /// </summary>
        /// <param name="at">指定执行该异步调用的节点地址</param>
        /// <param name="call">调用声明</param>
        public void AsyncInvokeAt(Uri at, ServiceCall call)
        {
            this.Resolve<IRemoteHandle>().SendAnAsyncCall(at, call);

            this._log.DebugFormat("向节点{0}发送对服务{1}|{2}|{3}的异步调用请求"
                , at
                , call.Target.Name
                , call.TargetMethod
                , call.Target.HostUri);
        }

        /// <summary>
        /// 生成服务调用声明
        /// <remarks>用于本地存在目标服务代理的调用声明，支持多态参数</remarks>
        /// </summary>
        /// <param name="serviceType">目标服务类型</param>
        /// <param name="methodName">目标方法名</param>
        /// <param name="arguments">参数组</param>
        /// <returns></returns>
        public ServiceCall GenerateServiceCall(Type serviceType, string methodName, params Tuple<string, Type, object>[] arguments)
        {
            ServiceInfo service = this.ServiceTable.Services.FirstOrDefault(o => 
                o.Type != null && o.Type.Equals(serviceType));

            if (service == null)
                throw new ServiceException(string.Format(
                    "当前服务节点不存在类型为{0}的服务配置信息"
                    , serviceType.FullName));

            var call = new ServiceCall() { TargetMethod = methodName, Identity = this.Configuration.ID };

            if (arguments != null)
            {
                var args = new List<ServiceCallArgument>();
                //多态序列化
                arguments.ToList().ForEach(o => 
                    args.Add(new ServiceCallArgument(o.V1, this._serializer.Serialize(o.V3, o.V2))));
                call.ArgumentCollection = args.ToArray();
            }
            //根据负载算法选取适合的服务配置
            call.Target = this.Resolve<ILoadBalancingHelper>().GetServiceConfig(service
                , call.TargetMethod
                , call.ArgumentCollection);
            return call;
        }
        /// <summary>
        /// 生成服务调用声明
        /// <remarks>本地可无需存在目标服务的代理，不支持多态</remarks>
        /// </summary>
        /// <param name="serviceTypeName">目标服务类型全名</param>
        /// <param name="methodName">目标方法名</param>
        /// <param name="arguments">参数组</param>
        /// <returns></returns>
        public ServiceCall GenerateServiceCall(string serviceTypeName, string methodName, IDictionary<string, object> arguments)
        {
            ServiceInfo service = this.ServiceTable.Services.FirstOrDefault(o =>
                o.Name.Equals(serviceTypeName, StringComparison.InvariantCultureIgnoreCase));

            if (service == null)
                throw new ServiceException(string.Format(
                    "当前服务节点不存在类型为{0}的服务配置信息"
                    , serviceTypeName));

            var call = new ServiceCall() { TargetMethod = methodName, Identity = this.Configuration.ID };

            if (arguments != null)
            {
                var args = new List<ServiceCallArgument>();
                arguments.ToList().ForEach(o => args.Add(new ServiceCallArgument(o.Key, this._serializer.Serialize(o.Value))));
                call.ArgumentCollection = args.ToArray();
            }
            //根据负载算法选取适合的服务配置
            call.Target = this.Resolve<ILoadBalancingHelper>().GetServiceConfig(service
                , call.TargetMethod
                , call.ArgumentCollection);
            return call;
        }

        /// <summary>
        /// 运行
        /// </summary>
        public void Run()
        {
            //检查基本服务依赖是否齐全
            this.Resolve<ISerializer>();
            this.Resolve<IAuthentication>();
            this.Resolve<ILoadBalancingHelper>();

            //打印服务节点参数
            this.RenderParameters();

            if (this.Uri == null)
                this._log.Info("没有为当前节点指定宿主方式，在当前节点注册的服务将不可用");
            else if (!this.Configuration.SelfHosting)
                this._log.Info("由于禁用了自托管，请确认您通过其他方式将服务节点进行托管");
            else
                this._remoteHandle.Expose(this.Uri);
            //刷新服务配置表
            this.Refresh();
            //定时检查配置表各个服务是否健康
            this.PrepareTimer();
        }

        //工作线程执行刷新任务，同一时刻只允许一个任务执行
        private void DoRefreshTask()
        {
            if (this._refreshFlag) return;

            lock (_timerLock)
            {
                if (this._refreshFlag) return;
                this._refreshFlag = true;
            }

            ThreadPool.QueueUserWorkItem(o =>
            {
                this.Refresh();
                Thread.Sleep(this.Configuration.Wait);
                this._refreshFlag = false;
            });
        }
        /// <summary>
        /// 尝试刷新关联节点配置，返回是否需要刷新服务表
        /// </summary>
        /// <returns>是否需要刷新服务表_serviceTable</returns>
        private bool RefreshAssociate()
        {
            if (this.AssociateUri == null)
                return false;

            //以下代码段均在线程安全前提下

            #region 检查关联节点是否可连接
            Exception ex;
            if (!this._remoteHandle.TryConnect(this.AssociateUri, null, out ex))
            {
                //仅移除来自关联节点的服务配置
                this._associateTable.Services.ToList().ForEach(o =>
                    o.Configs = o.Configs.Where(p => !p.HostUri.Equals(this.AssociateUri)).ToArray());
                //仅移除没有可用服务配置的服务
                this._container.ClearRemoteServices(this._associateTable.Services
                    .Where(o => o.Configs.Length == 0)
                    .Select(o => o.Type)
                    .ToArray());
                //关联节点不存在后保留原始配置可以进行正常调用
                this._log.Warn("连接关联节点时发生异常，来自该节点内的远程服务已被卸载，其余服务保留", ex);
                return true;
            }
            #endregion

            #region 检查版本
            try
            {
                if (this._associateTable.Version.Equals(this._remoteHandle.GetVersion(this.AssociateUri)))
                    return false;
            }
            catch (Exception e)
            {
                this._log.Warn("检查关联节点服务表版本时发生异常", e);
                return false;
            }
            #endregion

            #region 重新向本地容器注册关联节点的远程服务并清空配置表
            try
            {
                var configs = this._remoteHandle.GetServiceConfigs(this.AssociateUri);
                //重试一次，防止意外情况造成的空列表
                if (configs == null || configs.Services.Length == 0)
                    configs = this._remoteHandle.GetServiceConfigs(this.AssociateUri);
                if (configs == null || configs.Services.Length == 0)
                    this._log.InfoFormat("从关联节点获取到空列表");
                //包含刷新容器和配置表逻辑
                this.RefreshContainer(configs ?? new ServiceConfigTable());
            }
            catch (Exception e)
            {
                this._log.Warn("刷新关联服务节点时发生异常", e);

                if (this._associateTable == null)
                    this._associateTable = new ServiceConfigTable();
            }
            #endregion

            #region 重新将本地服务注册至关联节点
            try
            {
                if (this._localTable.Count > 0)
                {
                    this._remoteHandle.Register(this.AssociateUri, this._localTable.ToArray());
                    this._log.Debug("将本地服务重新注册至关联节点");
                }
            }
            catch (Exception e)
            {
                this._log.Warn("将本地服务重新注册至关联节点时发生异常", e);
            }
            #endregion

            return false;
        }
        /// <summary>
        /// 尝试检查注册到该节点的服务可用情况，返回是否需要刷新服务表
        /// </summary>
        /// <returns></returns> 
        private bool RefreshClientTable()
        {
            if (this._clientTable.Count == 0) 
                return false;

            //TODO:优化为并行超时检查
            var flag = false;
            var table = new List<ServiceConfig>();
            //for 避免意外修改
            for (var i = 0; i < this._clientTable.Count; i++)
            {
                Exception e;
                var config = this._clientTable[i];
                var success = this._remoteHandle.TryConnect(new Uri(config.HostUri), _connectTimeout, out e);
                if (success)
                    this.AddIfNotExist(table, config);
                else
                    flag = true;
                this._log.Debug(string.Format("服务{0},{1}|{2} {3}"
                    , config.Name
                    , config.AssemblyName
                    , config.HostUri
                    , success ? "可用，保留" : "不可用，移除")
                    , e);
            }
            this._clientTable = table;
            return flag;
        }
        //刷新容器
        private void RefreshContainer(ServiceConfigTable associateTable)
        {
            //排除从关联节点获取到的本地服务
            associateTable.Services = (associateTable ?? new ServiceConfigTable()).Services.Where(o =>
                //由于存在多地址情况 只判断Name+AssemblyName是否相同即可，避免本地暴露的服务被指向其他地址
                !this._localTable.Exists(p => p.Name.Equals(o.Name) && p.AssemblyName.Equals(o.AssemblyName))).ToArray();

            //卸载原有而新服务表中不存在的远程服务
            this._container.ClearRemoteServices(this.FilterANotInB(this._associateTable, associateTable));
            
            //替换关联节点服务表 先于容器注册前替换避免此时的调用拿到组件而没有服务信息
            this._associateTable = associateTable;
            //清空配置表
            this.ClearServiceTable();

            //注册原来没有而新服务表中存在的远程服务
            //this._container.RegisterRemoteServices(this.FilterANotInB(associateTable, this._associateTable));
            //重新注册避免外部原因导致的远程组件移除
            this._container.RegisterRemoteServices(associateTable.Services.Select(o => o.Type).ToArray());
        }
        private Type[] FilterANotInB(ServiceConfigTable a, ServiceConfigTable b)
        {
            return a.Services
                .Where(o => b.Services.FirstOrDefault(p => p.Type == o.Type) == null)
                .Select(o => o.Type).ToArray();
        }
        //清空服务表
        private void ClearServiceTable()
        {
            lock (_serviceTableLock)
                this._serviceTable = null;
            this._log.Debug("由于服务发生变更，服务配置表被清空");
        }
        //定时刷新服务
        private void PrepareTimer()
        {
            this._refreshTimer = new System.Timers.Timer(this.Configuration.Interval);
            this._refreshTimer.Elapsed += (s, e) => this.DoRefreshTask();
            this._refreshTimer.Start();
            this._log.InfoFormat("启动定时刷新计划，间隔{0}ms", this.Configuration.Interval);
        }
        //本地服务调用
        private object InvokeLocal(ServiceCall call, out Type returnType)
        {
            if (!this.Resolve<IAuthentication>().Validate(call))
                throw new ServiceException(string.Format("调用者身份认证失败：{0}|{1}"
                    , call.Identity == null ? null : call.Identity.Source
                    , call.Identity == null ? null : call.Identity.AuthKey)
                    , ExceptionCode.NoPermission);

            #region ServiceCall验证
            if (call.Target == null)
                throw new ServiceException("Target不能为空");
            if (call.Target.Type == null)
                throw new ServiceException(string.Format("当前节点不存在服务类型：{0},{1}"
                    , call.Target.Name
                    , call.Target.AssemblyName)
                    , ExceptionCode.ServiceNotFound);
            var target = this._container.Resolve(call.Target.Type);
            if (target == null)
                throw new ServiceException(string.Format("未在容器中找到目标服务{0}"
                    , call.Target.Type.FullName)
                    , ExceptionCode.ServiceNotFound);
            var method = call.Target.Type.GetMethods()
                .FirstOrDefault(o => o.Name.Equals(call.TargetMethod, StringComparison.InvariantCultureIgnoreCase));
            if (method == null)
                throw new ServiceException(string.Format("未在类型{0}中找到名为{1}的公开方法"
                    , call.Target.Type.FullName
                    , call.TargetMethod)
                    , ExceptionCode.MethodNotFound);
            #endregion

            try
            {
                //参数检查
                var args = from p in method.GetParameters() select this.ParseArgument(p, call);
                returnType = method.ReturnType;
                //TODO:Fast Invoker
                var result = method.Invoke(target, args.ToArray());
                this.LogLocalInvoke(call, true);
                return result;
            }
            catch (Exception e)
            {
                this.LogLocalInvoke(call, false);

                if (e is ServiceException) throw e;
                //返回内部异常信息
                throw new ServiceException((e.InnerException ?? e).Message, e.InnerException ?? e);
            }
        }
        //HACK:缺少参数自动用null填充
        private object ParseArgument(System.Reflection.ParameterInfo p, ServiceCall call)
        {
            var arg = call.ArgumentCollection.FirstOrDefault(o =>
                o.Key.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            if (arg == null)
                throw new ServiceException(string.Format("缺少参数{0}", p.Name), ExceptionCode.ArgumentError);

            //HACK:此处针对string的json序列化优化处理，以避免http调用情况下的字符串参数兼容问题和意外不合法反序列化
            if (p.ParameterType == typeof(string))
            {
                if (string.IsNullOrEmpty(arg.Value) && this.InternalDebug("返回空字符串"))
                    return arg.Value;
                if (arg.Value.Equals("null", StringComparison.InvariantCultureIgnoreCase) && this.InternalDebug("遇到null，返回空"))
                    return null;
                if (!arg.Value.StartsWith("\"") && this.InternalDebug("返回非\"开头，返回原始字符串"))
                    return arg.Value;
            }

            try
            {
                return this._serializer.Deserialize(p.ParameterType, arg.Value);
            }
            catch (Exception e)
            {
                //HACK:例外处理 string类型反序列化失败时，直接返回该字符串
                //if (p.ParameterType == typeof(string))
                //    return arg.Value;
                throw new ServiceException(string.Format("参数{0}格式不正确：{1}"
                    , p.Name
                    , e.Message)
                    , ExceptionCode.ArgumentError
                    , e);
            }
        }
        //远程服务调用
        private string InvokeRemote(ServiceCall call)
        {
            return this._remoteHandle.Invoke(call);
        }
        //本地调用记录
        private void LogLocalInvoke(ServiceCall call, bool success)
        {
            this._log.InfoFormat("【本地服务调用{0}】服务：{1}.{2}，身份：{3}|{4}，参数：{5}"
                , success ? "记录" : "失败"
                , call.Target.Name
                , call.TargetMethod
                , call.Identity.Source
                , call.Identity.AuthKey
                , string.Join("$"
                , call.ArgumentCollection.Select(o => string.Format("{0}={1}"
                    , o.Key
                    , o.Value)).ToArray()));
        }
        //打印服务节点参数
        private void RenderParameters()
        {
            this._log.Info("Uri=" + this.Uri);
            this._log.Info("AssociateUri=" + this.AssociateUri);
            this._log.Info("AsyncReceiverUri=" + this.AsyncReceiverUri);
            this._log.Info("Interval=" + this.Configuration.Interval);
            this._log.Info("Wait=" + this.Configuration.Wait);
            this._log.Info("Identity.AuthKey=" + this.Configuration.ID.AuthKey);
            this._log.Info("Identity.Source=" + this.Configuration.ID.Source);
        }
        //判断Uri是否指向本地
        private bool IsLocal(string uri)
        {
            return this.Configuration.Uri != null
                && uri.Equals(this.Configuration.Uri.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
        //准备服务表
        private void PrepareServiceTable(ServiceConfigTable table, List<ServiceConfig> configs)
        {
            //填充services用于负载均衡支持
            configs.ForEach(o =>
            {
                //保留configs属性为了短期兼容原不支持多地址的设计
                table.AddServiceConfig(o);

                var service = table.Services
                    .ToList()
                    .FirstOrDefault(p =>
                        p.Name.Equals(o.Name) && p.AssemblyName.Equals(o.AssemblyName));

                if (service == null)
                {
                    service = new ServiceInfo(o.Name, o.AssemblyName);
                    table.AddServiceInfo(service);
                }

                service.LoadBalancingAlgorithm = o.LoadBalancingAlgorithm;
                service.AddServiceConfig(o);
            });
        }
        private bool AddIfNotExist(List<ServiceConfig> configs, ServiceConfig config)
        {
            if (configs.Exists(o => o.Equals(config))) 
                return false;
            configs.Add(config);
            return true;
        }

        private bool InternalDebug(string msg)
        {
            if (_internalDebug)
                this._log.Debug(msg);
            return true;
        }
    }
}