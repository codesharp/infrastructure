//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

using CodeSharp.Core.Interfaces;
using CodeSharp.Core.Services;
using CodeSharp.Core.Utils;

namespace CodeSharp.Core
{
    /// <summary>为应用的提供的基础框架配置
    /// </summary>
    public class Configuration
    {
        //全局静态字段 仅用于配置框架内部以及和外部约定
        /// <summary>运行时变量flag
        /// </summary>
        public static readonly string RuntimeProperties_Environment = "environment";
        /// <summary>运行时变量flag，当发生错误时使用
        /// </summary>
        public static readonly string RuntimeProperties_Environment_Error = "environment_at_error";

        //默认配置框架实例
        private static Configuration _instance;

        private IDictionary<string, ConfigItem> _properties;//配置项集合
        private string _root;//配置路径
        private string _propertiesFileName = "properties.config";//properties文件名
        private string _messages = string.Empty;//内部调试信息，在外部log未可用前使用
        private Func<string, IDictionary<string, object>> _runtimeProperties;
        //总是返回可用log
        private ILog _log
        {
            get
            {
                try { return DependencyResolver.Resolve<ILoggerFactory>().Create(typeof(Configuration)); }
                catch { return new InnerLog(this); }
            }
        }
        private Configuration()
        {
            this._properties = new Dictionary<string, ConfigItem>();
        }

        /// <summary>获取初始化过程中的部分调试信息
        /// </summary>
        public string Messages { get { return this._messages; } }
        /// <summary>读出初始化过程中的部分调试信息后清除
        /// </summary>
        /// <returns></returns>
        public string PopMessages()
        {
            var m = this.Messages;
            this._messages = null;
            return m;
        }

        #region Config/Setting
        /// <summary>获取配置项的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">未找到键值为{0}的配置</exception>
        public string GetSetting(string key)
        {
            return this.GetSetting(key, true);
        }
        /// <summary>获取配置项的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="throwOnError">获取失败时是否抛出异常，否则返回Null</param>
        /// <returns></returns>
        public string GetSetting(string key, bool throwOnError)
        {
            try
            {
                return this.GetConfig(key).Value;
            }
            catch (KeyNotFoundException e)
            {
                if (throwOnError)
                    throw e;
                return null;
            }
        }
        /// <summary>获取配置项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">未找到键值为{0}的配置</exception>
        public ConfigItem GetConfig(string key)
        {
            if (!this._properties.ContainsKey(key))
                throw new KeyNotFoundException(string.Format("未找到键值为{0}的配置", key));
            return this._properties[key];
        }
        #endregion

        /// <summary>设置解释器,即IOC容器
        /// <remarks>在进行其他配置前请先设置此方法</remarks>
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns> 
        public Configuration Resolver(IDependencyResolver resolver)
        {
            DependencyResolver.Resolver = resolver;
            return this;
        }

        #region 配置追加 ReadProperties 耦合于properties.config
        /// <summary>
        /// 从指定程序集中读取配置
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="manifestResourceName">嵌入资源的完整名称</param>
        /// <returns></returns>
        public Configuration ReadProperties(Assembly assembly, string manifestResourceName)
        {
            using (var reader = new StreamReader(assembly.GetManifestResourceStream(manifestResourceName), Encoding.Default))
                return ReadProperties(reader.ReadToEnd());
        }
        /// <summary>
        /// 从xml文本中读取配置
        /// </summary>
        /// <param name="propertiesXml">配置xml文本，格式参见properties.config</param>
        /// <returns></returns>
        public Configuration ReadProperties(string propertiesXml)
        {
            IList<ConfigItem> readed;
            return this.ReadProperties(propertiesXml, out readed);
        }
        /// <summary>
        /// 从xml文本中读取配置
        /// <remarks>若出现重复的配置将会覆盖</remarks>
        /// </summary>
        /// <param name="propertiesXml">配置xml文本，格式参见properties.config</param>
        /// <param name="readed">本次读取的列表</param>
        /// <returns></returns>
        public Configuration ReadProperties(string propertiesXml, out IList<ConfigItem> readed)
        {
            var body = propertiesXml;
            var configs = new List<ConfigItem>();

            #region 填充配置集合
            XElement.Parse(body)
                .Element("properties")
                .Descendants()
                .ToList()
                .ForEach(p =>
                {
                    var item = new ConfigItem()
                    {
                        Key = p.Name.LocalName,
                        Value = p.Value
                    };
                    configs.Add(item);
                    if (this._properties.ContainsKey(item.Key))
                        this._properties[item.Key] = item;
                    else
                        this._properties.Add(p.Name.LocalName, item);

                    if (this._log.IsDebugEnabled)
                        this._log.DebugFormat("读取并替换了键{0}={1}", item.Key, item.Value);
                });
            #endregion

            readed = configs;
            return this;
        }
        /// <summary>
        /// 将所有配置项写入文件
        /// </summary>
        /// <returns></returns>
        public Configuration RenderProperties()
        {
            #region 写入文件
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                var xml = XElement.Parse(@"<?xml version='1.0' encoding='utf-8' ?>
                    <configuration>
                    <properties></properties> 
                    </configuration>");
                this._properties.ToList().ForEach(o =>
                    xml.Element("properties").Add(new XElement(XName.Get(o.Key), o.Value.Value)));
                writer.Write(xml.ToString());
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                FileHelper.WriteTo(stream
                       , Configuration._instance._root
                       , Configuration._instance._propertiesFileName
                       , FileMode.Create);
            }
            #endregion
            return this;
        }
        #endregion

        #region 追加配置文件 File()
        /// <summary>
        /// 追加配置文件
        /// </summary>
        /// <param name="name">生成的文件名</param>
        /// <param name="assembly">资源所在的程序集</param>
        /// <param name="resourceName">资源全名</param>
        /// <param name="parameters">自定义模板参数，用于替换XML文件中的标记部分</param>
        /// <returns></returns>
        public Configuration File(string name, Assembly assembly, string resourceName, IDictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Keys.Count == 0)
                return this.File(name, assembly.GetManifestResourceStream(resourceName));
            using (var reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
                return this.File(name, reader.ReadToEnd(), parameters);
        }
        /// <summary>
        /// 追加配置文件
        /// </summary>
        /// <param name="name">生成的文件名</param>
        /// <param name="content">文件内容</param>
        /// <param name="parameters">自定义模板参数，用于替换XML文件中的标记部分</param>
        /// <returns></returns>
        public Configuration File(string name, string content, IDictionary<string, string> parameters)
        {
            if (parameters != null)
                parameters.ToList().ForEach(p => content = content.Replace(p.Key, p.Value));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                return this.File(name, stream);
        }
        /// <summary>
        /// 追加配置文件
        /// </summary>
        /// <param name="name">生成的文件名</param>
        /// <param name="content"></param>
        /// <returns></returns>
        public Configuration File(string name, Stream content)
        {
            FileHelper.WriteTo(content, Configuration._instance._root, name, FileMode.Create);
            return this;
        }
        #endregion

        #region AssemblyResolve
        /// <summary>
        /// 指定当前AppDomain下程序集加载失败时的额外加载方式
        /// <remarks>默认重试2次，阈值20</remarks>
        /// </summary>
        /// <param name="howToLoad">额外的加载方式</param>
        /// <returns></returns>
        public Configuration AssemblyResolve(Func<string, Assembly> howToLoad)
        {
            return this.AssemblyResolve(howToLoad, 2, 20);
        }
        /// <summary>
        /// 指定当前AppDomain下程序集加载失败时的额外加载方式
        /// </summary>
        /// <param name="howToLoad">额外的加载方式</param>
        /// <param name="retry">重试次数</param>
        /// <param name="threshold">最大重试次数</param>
        /// <returns></returns>
        public Configuration AssemblyResolve(Func<string, Assembly> howToLoad, int retry, int threshold)
        {
            //计数器 防止死循环
            var counter = new Dictionary<string, int>();
            var total = 0;
            //订阅程序集加载失败事件
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var name = args.Name;
                if (!counter.ContainsKey(name))
                    counter.Add(name, 0);
                ++total;
                ++counter[name];
                var info = string.Format("程序集{1}加载失败，第{0}次重新尝试加载，总共第{2}次\n", counter[name], name, total);

                if (this._log.IsDebugEnabled)
                    this._log.Debug(info);

                if (counter[name] >= retry)
                {
                    var error = string.Format("找不到程序集{0}，加载{1}次后停止重试\n", name, retry);
                    this._log.Fatal(error);
                    //HACK:置0无效，加载失败不会再次执行加载失败逻辑
                    counter[name] = 0;
                    throw new Exception(error);
                }
                if (total >= threshold)
                {
                    var error = string.Format("对程序集{0}的加载修正达到最大重试次数{1}，不再进行重试\n", name, threshold);
                    this._log.Fatal(error);
                    throw new Exception(error);
                }
                return howToLoad(name);
            };
            return this;
        }
        #endregion

        #region RuntimeProperties 可用于应用层于基础层的反向影响/倒置依赖
        //可用场景：
        //Log打印宿主环境信息或额外信息，
        /// <summary>
        /// 获取运行时属性集合
        /// </summary>
        /// <param name="flag">用于指示希望获得哪种类型的信息，此设计为了便于属性分类</param>
        /// <returns></returns>
        public IDictionary<string, object> RuntimeProperties(string flag)
        {
            return this._runtimeProperties != null ? this._runtimeProperties(flag) : new Dictionary<string, object>();
        }
        /// <summary>
        /// 设置运行时属性集合
        /// <remarks>
        /// 将被部分基础设施使用，
        /// 如：log将使用flag=environment的属性集合
        /// </remarks>
        /// </summary>
        /// <param name="howToGet">获取信息的回调，注意该回调执行的时机和宿主程序的依赖注入完成情况</param>
        /// <returns></returns>
        public Configuration RuntimeProperties(Func<string, IDictionary<string, object>> howToGet)
        {
            this._runtimeProperties = howToGet;
            return this;
        }
        #endregion

        //卸载清理当前实例
        private void InternalCleanup()
        {
            //其他关联对象的清理逻辑可在此处完成
        }

        //Static

        /// <summary>获取框架配置实例
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">未初始化框架配置</exception>
        public static Configuration Instance()
        {
            if (_instance == null)
                throw new NullReferenceException("未初始化框架配置");
            return _instance;
        }
        /// <summary>从嵌入的xml文件中初始化框架配置
        /// <remarks>
        /// 配置项文件默认为properties.config
        /// </remarks>
        /// </summary>
        /// <param name="versionFlag">配置版本 如：Debug Release </param>
        /// <param name="folder">指定生成配置文件的目录 如：c://application_config</param>
        /// <param name="assembly">指定配置文件所在的程序集</param>
        /// <param name="nameSpace">指定配置文件的命名空间 如：Taobao.BusinessFramework.ConfigFiles</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">不可重复初始化配置</exception>
        public static Configuration ConfigWithEmbeddedXml(string versionFlag
            , string folder
            , Assembly assembly
            , string nameSpace)
        {
            return Configuration.ConfigWithEmbeddedXml(versionFlag
                , folder
                , assembly
                , nameSpace
                , null);
        }
        /// <summary>从嵌入的xml文件中初始化框架配置
        /// <remarks>
        /// 配置项文件默认为properties.config
        /// </remarks>
        /// </summary>
        /// <param name="versionFlag">配置版本 如：Debug Release </param>
        /// <param name="folder">指定生成配置文件的目录 如：c://application_config</param>
        /// <param name="assembly">指定配置文件所在的程序集</param>
        /// <param name="nameSpace">指定配置文件的命名空间 如：Taobao.BusinessFramework.ConfigFiles</param>
        /// <param name="parameters">自定义模板参数，用于替换XML文件中的标记部分</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">不可重复初始化配置</exception>
        public static Configuration ConfigWithEmbeddedXml(string versionFlag
            , string folder
            , Assembly assembly
            , string nameSpace
            , IDictionary<string, string> parameters)
        {
            //初始化配置
            Configuration.Config();
            //配置文件生成路径
            Configuration._instance._root = folder;

            #region 生成配置文件
            assembly.GetManifestResourceNames().ToList().ForEach(o =>
            {
                //可以不使用版本versionFlag
                var prefix = string.IsNullOrEmpty(versionFlag)
                    ? string.Format("{0}.", nameSpace)
                    : string.Format("{0}.{1}.", nameSpace, versionFlag);

                //排除不符合命名空间的文件
                if (o.IndexOf(prefix) < 0)
                    return;

                //properties文件
                if (o.ToLower().IndexOf("properties") >= 0)
                {
                    Configuration._instance.ReadProperties(assembly, o);
                    return;
                }

                using (var reader = new StreamReader(assembly.GetManifestResourceStream(o)))
                {
                    var content = reader.ReadToEnd();
                    //替换自定义参数
                    if (parameters != null)
                        parameters.ToList().ForEach(p => content = content.Replace(p.Key, p.Value));
                    //写入文件
                    FileHelper.WriteTo(content
                        , Configuration._instance._root
                        , o.Replace(prefix, "")
                        , FileMode.Create);
                }
            });
            #endregion

            return Configuration._instance;
        }
        /// <summary>初始化框架配置
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">不可重复初始化配置</exception>
        public static Configuration Config()
        {
            if (Configuration._instance != null)
                throw new InvalidOperationException("不可重复初始化配置");
            Configuration._instance = new Configuration();
            return Configuration._instance;
        }
        /// <summary>清理、卸载配置
        /// </summary>
        public static void Cleanup()
        {
            if (Configuration._instance == null) return;
            Configuration._instance.InternalCleanup();
            Configuration._instance = null;
        }

        /// <summary>配置项
        /// </summary>
        public class ConfigItem
        {
            /// <summary>键
            /// </summary>
            public string Key { get; set; }
            /// <summary>值
            /// </summary>
            public string Value { get; set; }
        }
        /// <summary>配置框架内部log
        /// </summary>
        private class InnerLog : ILog
        {
            private Configuration _config;
            public InnerLog(Configuration config)
            {
                this._config = config;
            }

            #region ILog Members

            public bool IsDebugEnabled
            {
                get { return true; }
            }

            public bool IsErrorEnabled
            {
                get { return true; }
            }

            public bool IsFatalEnabled
            {
                get { return true; }
            }

            public bool IsInfoEnabled
            {
                get { return true; }
            }

            public bool IsWarnEnabled
            {
                get { return true; }
            }

            public void Info(object message)
            {
                this._config._messages += (message + "\n");
            }

            public void InfoFormat(string format, params object[] args)
            {
                this._config._messages += (string.Format(format, args) + "\n");
            }

            public void Info(object message, Exception exception)
            {

            }

            public void Error(object message)
            {
                this._config._messages += (message + "\n");
            }

            public void ErrorFormat(string format, params object[] args)
            {
                this._config._messages += (string.Format(format, args) + "\n");
            }

            public void Error(object message, Exception exception)
            {

            }

            public void Warn(object message)
            {
                this._config._messages += (message + "\n");
            }

            public void WarnFormat(string format, params object[] args)
            {
                this._config._messages += (string.Format(format, args) + "\n");
            }

            public void Warn(object message, Exception exception)
            {

            }

            public void Debug(object message)
            {
                this._config._messages += (message + "\n");
            }

            public void DebugFormat(string format, params object[] args)
            {
                this._config._messages += (string.Format(format, args) + "\n");
            }

            public void Debug(object message, Exception exception)
            {

            }

            public void Fatal(object message)
            {
                this._config._messages += (message + "\n");
            }

            public void FatalFormat(string format, params object[] args)
            {
                this._config._messages += (string.Format(format, args) + "\n");
            }

            public void Fatal(object message, Exception exception)
            {

            }

            #endregion

        }
    }
}