//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

using CodeSharp.Core;
using CodeSharp.Core.Services;
using CodeSharp.Core.Utils;

namespace CodeSharp.Framework
{
    /// <summary>业务应用配置框架
    /// <remarks>
    /// 20120314重构版本
    /// 关注固化核心约束：
    /// 配置文件集中化，系统配置集中管理，可选的容器化架构
    /// 统一异常体系，上下文服务
    /// </remarks>
    /// </summary>
    public partial class SystemConfig
    {
        //@"c:\application_config";
        private static readonly string _folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "application_config");
        private static readonly string _commonServiceConfigKey = "CommonServiceConfig";
        //配置服务地址
        private static readonly string _sysConfigServiceUriKey = "sysConfigServiceUri";
        //文件模板中的变量
        private static readonly string _flag_site = "#{site}";
        private static readonly string _flag_version = "#{version}";
        //默认的配置文件程序集名称
        private static readonly string _configFilesAssemblyName = "ConfigFiles";

        //应用配置静态实例
        private static SystemConfig _settings;
        /// <summary>获取应用框架配置实例
        /// </summary>
        public static SystemConfig Settings
        {
            get
            {
                if (_settings == null)
                    throw new InvalidOperationException("请先调用Initialize或Confiure执行应用配置框架TBF初始化");
                return _settings;
            }
        }

        //启用全局缓存管理的自动刷新功能
        private bool _enableRefresh;
        private bool _isCommonConfigReaded;
        private bool _justTestIdentity;
        //寄存一些类型信息，类似于对象容器
        private IDictionary<Type, object> _dict;
        private IExceptionSystem _exceptionSystem;
        private NonWeakReferenceCacheManager _cacheManager;

        /// <summary>获取当前应用的名称
        /// </summary>
        public string AppName { get; private set; }
        /// <summary>获取当前配置/环境版本
        /// </summary>
        public string VersionFlag { get; private set; }
        /// <summary>获取当前是否启用了容器化架构/依赖注入
        /// </summary>
        public virtual bool IsDependencyResolverEnable { get; set; }
        /// <summary>获取基础框架配置实例
        /// </summary>
        public CodeSharp.Core.Configuration FoundationConfig { get; private set; }

        protected SystemConfig(string app, string versionFlag)
        {
            this.AppName = app;
            this.VersionFlag = versionFlag;
            this._dict = new Dictionary<Type, object>();
            this._enableRefresh = true;//总是开启
        }
        protected SystemConfig(SystemConfig config)
            : this(config.AppName
            , config.VersionFlag)
        {
            this._enableRefresh = config._enableRefresh;
            this._isCommonConfigReaded = config._isCommonConfigReaded;
            this._justTestIdentity = config._justTestIdentity;

            this._dict = config._dict;
            this._exceptionSystem = config._exceptionSystem;
            this._cacheManager = config._cacheManager;

            this.FoundationConfig = config.FoundationConfig;
        }

        #region 基本初始化
        /// <summary>初始化
        /// <remarks>
        /// 最小粒度应用初始化api，包含应用必须条件：
        /// 应用名称，环境版本，配置文件约定和生成、环境配置、异常体系、上下文服务
        /// </remarks>
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static SystemConfig Configure(string app)
        {
            return SystemConfig.Configure(app, Util.GetEnvironmentVersionFlag().ToString());
        }
        /// <summary>初始化
        /// <remarks>
        /// 最小粒度应用初始化api，包含应用必须条件：
        /// 应用名称，环境版本，配置文件约定和生成、环境配置、异常体系、上下文服务
        /// </remarks>
        /// </summary>
        /// <param name="app"></param>
        /// <param name="versionFlag">
        /// 指定环境版本，默认从appsetting的键EnvironmentVersionFlag以及编译参数获取
        /// Release编译时总是使用Release版本
        /// </param>
        /// <returns></returns>
        public static SystemConfig Configure(string app, string versionFlag)
        {
            if (_settings != null)
                throw new InvalidOperationException("不能重复初始化配置框架（TBF）");

            var flag = Util.GetEnvironmentVersionFlag(versionFlag).ToString();
            //读取专用配置文件程序集
            var configsAssembly = Assembly.Load(_configFilesAssemblyName);
            //实例化
            _settings = new SystemConfig(app, flag);
            //配置文件变量
            var fileParameters = SystemConfig.GenerateFileParameters(_settings);
            //基础框架配置实例初始化
            _settings.FoundationConfig = CodeSharp.Core.Configuration
                .ConfigWithEmbeddedXml(null
                , _folder
                , configsAssembly
                , _configFilesAssemblyName + "." + app
                , fileParameters);
            //配置项初始化
            SystemConfig.RenderProperties(_settings, configsAssembly, _configFilesAssemblyName);
            //默认配置文件初始化
            SystemConfig.RenderFile(_settings, configsAssembly, _configFilesAssemblyName, "log4net.config", fileParameters);
            SystemConfig.RenderFile(_settings, configsAssembly, _configFilesAssemblyName, "MemCache.config", fileParameters);
            //设置运行时变量
            _settings.FoundationConfig.RuntimeProperties(Util.GetHowToGenerateRuntimeProperties(_settings));
            //启用全局缓存管理
            _settings.NonWeakReferenceCache();

            //以下编写涉及基本对象注入的无容器时的初始化
            //默认异常体系
            _settings.ExceptionSystem(new DefaultExceptionSystem());

            return _settings;
        }
        /// <summary>从现有配置实例初始化
        /// </summary>
        /// <param name="existingConfig"></param>
        /// <returns></returns>
        public static SystemConfig Configure(SystemConfig existingConfig)
        {
            return _settings = existingConfig;
        }
        private static void RenderFile(SystemConfig config
            , Assembly configsAssembly
            , string configsAssemblyName
            , string fileName
            , IDictionary<string, string> parameters)
        {
            var file = string.Format("{0}.{1}", configsAssemblyName, fileName);
            var file_v = string.Format("{0}.{1}.{2}", configsAssemblyName, config.VersionFlag, fileName);
            var file_app = string.Format("{0}.{1}.{2}", configsAssemblyName, config.AppName, fileName);
            var file_app_v = string.Format("{0}.{1}.{2}.{3}", configsAssemblyName, config.AppName, config.VersionFlag, fileName);

            //配置文件生成规则：根据环境版本，带版本的优先，应用优先，默认使用全局配置文件
            //app.version.fileName
            if (Exist(configsAssembly, file_app_v))
                _settings.FoundationConfig.File(fileName, configsAssembly, file_app_v, parameters);
            //app.fileName
            else if (Exist(configsAssembly, file_app))
                _settings.FoundationConfig.File(fileName, configsAssembly, file_app, parameters);
            //version.fileName
            else if (Exist(configsAssembly, file_v))
                _settings.FoundationConfig.File(fileName, configsAssembly, file_v, parameters);
            //fileName
            else
                _settings.FoundationConfig.File(fileName, configsAssembly, file, parameters);
        }
        private static bool Exist(Assembly assembly, string fullName)
        {
            return assembly.GetManifestResourceInfo(fullName) != null;
        }
        private static void RenderProperties(SystemConfig config, Assembly configsAssembly, string configsAssemblyName)
        {
            var fileName = "properties.config";
            var pro = string.Format("{0}.{1}", configsAssemblyName, fileName);
            var pro_v = string.Format("{0}.{1}.{2}", configsAssemblyName, config.VersionFlag, fileName);
            var pro_app = string.Format("{0}.{1}.{2}", configsAssemblyName, config.AppName, fileName);
            var pro_app_v = string.Format("{0}.{1}.{2}.{3}", configsAssemblyName, config.AppName, config.VersionFlag, fileName);
            /*
             * 配置项文件读取顺序：
             * 全局配置项文件->带版本的全局配置项文件->应用配置项文件->带版本的应用配置项文件->环境变量->输出
             * 后读配置覆盖更早读取的
             */
            //fileName
            if (Exist(configsAssembly, pro))
                _settings.FoundationConfig.ReadProperties(configsAssembly, pro);
            //version.fileName
            if (Exist(configsAssembly, pro_v))
                _settings.FoundationConfig.ReadProperties(configsAssembly, pro_v);
            //app.fileName
            if (Exist(configsAssembly, pro_app))
                _settings.FoundationConfig.ReadProperties(configsAssembly, pro_app);
            //app.version.fileName
            if (Exist(configsAssembly, pro_app_v))
                _settings.FoundationConfig.ReadProperties(configsAssembly, pro_app_v);
            //环境变量，最后读取防止被意外覆盖
            _settings.FoundationConfig.ReadProperties(_settings.GetPropertiesOfEnvironment(_settings));
            //输出为properties.config
            _settings.FoundationConfig.RenderProperties();
        }
        private static IDictionary<string, string> GenerateFileParameters(SystemConfig config)
        {
            return new Dictionary<string, string>() { 
                { _flag_site, Util.GetSiteName(config.AppName) }, 
                { _flag_version, config.VersionFlag } };
        }
        #endregion

        /// <summary>清理/卸载当前应用框架配置实例
        /// </summary>
        public static void Cleanup()
        {
            if (_settings == null) return;

            _settings.InternalCleanup();
            _settings = null;
        }

        #region GetSetting/Property
        /// <summary>获取配置 不存在的配置返回空
        /// <remarks>
        /// 从properties.config以及配置框架中读取的配置
        /// HACK:不再兼容原使用ConfigurationManager.AppSettings的语法，不允许使用该方式进行配置，语法AppSettings会被纳入代码审查规范-不合理使用标准
        /// </remarks>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                try
                {
                    return GetSetting(key) ?? string.Empty;
                }
                catch (KeyNotFoundException) { return string.Empty; }
            }
        }
        /// <summary>获取配置，等同于this[""]
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            return this[key];
        }
        /// <summary>获取配置
        /// <remarks>来自基础配置实例的配置项</remarks>
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">未找到键值为{0}的配置</exception>
        public static string GetSetting(string key)
        {
            return _settings.GetPropertyValue(key);
        }
        //从基础配置实例中获取属性值
        private string GetPropertyValue(string key)
        {
            this.TryAutoRefreshProperties();
            return this.FoundationConfig.GetConfig(key).Value;
        }
        #endregion

        internal SystemConfig SetObject<T>(T t) where T : class
        {
            var k = typeof(T);
            if (!this._dict.ContainsKey(k))
                this._dict.Add(k, null);
            this._dict[k] = t;
            return this;
        }
        internal T GetObject<T>() where T : class
        {
            return this._dict[typeof(T)] as T;
        }
        /// <summary>获取可用的日志工厂实例
        /// </summary>
        public ILoggerFactory GetLoggerFactory()
        {
            return this.IsDependencyResolverEnable
                ? DependencyResolver.Resolve<ILoggerFactory>()
                //应用框架默认的日志工厂
                : new CodeSharp.Framework.DefaultLoggerFactory();
        }
        /// <summary>获取可用的异常体系实例
        /// </summary>
        /// <returns></returns>
        public IExceptionSystem GetExceptionSystem()
        {
            return this.IsDependencyResolverEnable
                ? DependencyResolver.Resolve<IExceptionSystem>()
                : this._exceptionSystem;
        }
        /// <summary>从配置程序集中按规则读取并在应用配置文件目录下生成额外的配置文件
        /// 使用默认的文件变量
        /// </summary>
        /// <param name="fileName">文件名称，如：log4net.config</param>
        /// <returns></returns>
        public SystemConfig File(string fileName)
        {
            return this.File(fileName, SystemConfig.GenerateFileParameters(this));
        }
        /// <summary>从配置程序集中按规则读取并在应用配置文件目录下生成额外的配置文件
        /// </summary>
        /// <param name="fileName">文件名称，如：log4net.config</param>
        /// <param name="fileParameters">定义要在文本中替换的文件变量</param>
        /// <returns></returns>
        public SystemConfig File(string fileName, IDictionary<string, string> fileParameters)
        {
            SystemConfig.RenderFile(this, Assembly.Load(_configFilesAssemblyName), _configFilesAssemblyName, fileName, fileParameters);
            return this;
        }

        #region NonWeakReferenceCache通用全局缓存以及自动刷新机制
        /// <summary>启用全局缓存管理功能
        /// </summary>
        /// <returns></returns>
        public SystemConfig NonWeakReferenceCache()
        {
            if (this._cacheManager == null)
                //CacheManager没有外部消费者，无注入需求
                this._cacheManager = new NonWeakReferenceCacheManager();
            return this;
        }
        /// <summary>添加全局缓存
        /// <remarks>添加后将对其进行简易管理，非WeakReference</remarks>
        /// </summary>
        /// <param name="key">缓存的键</param>
        /// <param name="cache">缓存内容</param>
        /// <returns></returns>
        public SystemConfig NonWeakReferenceCache(string key, NonWeakReferenceCacheManager.ICache cache)
        {
            this.NonWeakReferenceCache();
            this._cacheManager.Add(key, cache);
            return this;
        }
        /// <summary>获取指定的全局缓存内容，不存在则返回Null
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public NonWeakReferenceCacheManager.ICache NonWeakReferenceCache(string key)
        {
            return this.NonWeakReferenceCache(key
                //为避免在开发环境多线程刷新需要多重判断是否允许自动刷新
                , this._enableRefresh && Util.GetAutoRefreshSettingsFlag());
        }
        /// <summary>获取指定的全局缓存内容，不存在则返回Null
        /// </summary>
        /// <param name="key">缓存键名</param>
        /// <param name="refresh">是否在获取时激活自动刷新，默认为true</param>
        /// <returns></returns>
        public NonWeakReferenceCacheManager.ICache NonWeakReferenceCache(string key, bool refresh)
        {
            return this._cacheManager.Get(key, refresh);
        }
        #endregion

        #region 系统配置表 SysConfigTable、ReadProperties
        private static readonly string SysConfigCachekey = "SysConfig";
        public SysConfigTable SysConfig(string key)
        {
            return (this.NonWeakReferenceCache(SysConfigCachekey) as SysConfigTablesCache).GetTable(key);
        }
        /// <summary>读取配置表并填充至基础配置实例中
        /// <remarks>填充进配置实例的配置项可被用于支持依赖注入</remarks>
        /// </summary>
        /// <param name="key">配置表的表名</param>
        /// <returns></returns>
        public SystemConfig ReadProperties(string key)
        {
            return this.ReadProperties(key, false);
        }
        /// <summary>读取配置表并填充至基础配置实例中
        /// <remarks>填充进配置实例的配置项可被用于支持依赖注入</remarks>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isIndependent">是否读取为独立配置表，独立配置表不会被填充到基础配置实例中</param>
        /// <returns></returns>
        public SystemConfig ReadProperties(string key, bool isIndependent)
        {
            this.EnableSysConfig(TimeSpan.FromSeconds(60));
            (this.NonWeakReferenceCache(SysConfigCachekey, false) as SysConfigTablesCache).ReadProperties(key, isIndependent);
            return this;
        }
        /// <summary>启用系统配置表功能
        /// <remarks>除非要显示指定刷新间隔，一般不需要单独调用此声明</remarks>
        /// </summary>
        /// <param name="interval">与上一次刷新时间对比，刷新时间最小间隔，默认60s</param>
        /// <returns></returns>
        public SystemConfig EnableSysConfig(TimeSpan interval)
        {
            if (this.NonWeakReferenceCache(SysConfigCachekey, false) == null)
                this.NonWeakReferenceCache(SysConfigCachekey
                    , new SysConfigTablesCache(this.VersionFlag
                        , this[_sysConfigServiceUriKey]
                        , interval));
            return this;
        }
        private void TryAutoRefreshProperties()
        {
            this.NonWeakReferenceCache(SysConfigCachekey, true);
        }
        #endregion

        /// <summary>读取基础配置表数据
        /// <remarks>该表包含了大部分程序运行的基本配置信息</remarks>
        /// </summary>
        /// <returns></returns>
        public SystemConfig ReadCommonProperties()
        {
            if (!this._isCommonConfigReaded)
                this.ReadProperties(_commonServiceConfigKey);
            this._isCommonConfigReaded = true;
            return this;
        }
        /// <summary>设置默认zh-CN的时区/culture
        /// </summary>
        /// <returns></returns>
        public SystemConfig Globalization()
        {
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("zh-CN");
            culture.DateTimeFormat.DateSeparator = "-";
            culture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            culture.DateTimeFormat.LongDatePattern = "yyyy-MM-dd HH:mm:ss";
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            return this;
        }
        /// <summary>自定义异常体系，若需要定义系统的已知未知异常等请调用此方法进行定义
        /// </summary>
        /// <param name="exceptionSystem">异常体系的实例</param>
        /// <returns></returns>
        public SystemConfig ExceptionSystem(IExceptionSystem exceptionSystem)
        {
            this._exceptionSystem = exceptionSystem;
            return this;
        }

        protected ILog _log { get { return this.GetLoggerFactory().Create(this.GetType()); } }
        protected string ParseProperties(List<SysConfigItem> configs, string configsVersion)
        {
            var xml = XElement.Parse(string.Format(@"
<?xml version='1.0' encoding='utf-8' ?>
<configuration version='{0}'>
    <properties></properties> 
</configuration>".Trim()
                , configsVersion));
            configs.ForEach(o =>
            {
                if (string.IsNullOrEmpty(o.Key)) return;
                var el = new XElement(XName.Get(o.Key.Trim()), o.Value);
                el.SetAttributeValue(XName.Get("cn"), o.CN);
                xml.Element("properties").Add(el);
            });
            return xml.ToString();
        }

        //提供环境相关变量，如：环境版本，服务名，IP等
        private string GetPropertiesOfEnvironment(SystemConfig settings)
        {
            var configs = new List<SysConfigItem>();
            //环境版本，如：Debug,Test,Exp,Release等
            configs.Add(new SysConfigItem("sysConfig_versionFlag", "", settings.VersionFlag));
            //宿主环境的编译标识 Debug|Release
#if DEBUG
            configs.Add(new SysConfigItem("sysConfig_compileSymbol", "", "debug"));
#else
            configs.Add(new SysConfigItem("sysConfig_compileSymbol", "", "release"));
#endif
            //应用名
            configs.Add(new SysConfigItem("sysConfig_appName", "", settings.AppName));
            return this.ParseProperties(configs, DateTime.Now.ToString("yyyyMMdd"));
        }
        //清理/卸载
        private void InternalCleanup()
        {
            //卸载基础配置实例
            CodeSharp.Core.Configuration.Cleanup();
            //卸载NSF节点实例
            //TODO:对该实例的关联对象进行卸载，主要处理静态对象
        }
    }
}