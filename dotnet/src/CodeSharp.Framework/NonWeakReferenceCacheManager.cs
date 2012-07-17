//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CodeSharp.Core;
using CodeSharp.Core.Services;

namespace CodeSharp.Framework
{
    /// <summary>非弱引用缓存管理
    /// </summary>
    public class NonWeakReferenceCacheManager
    {
        private ILog _log; 
        private IDictionary<string, ICache> _dict;

        private ILog log
        {
            get
            {
                return this._log == null 
                    ? SystemConfig.Settings.GetLoggerFactory().Create(typeof(NonWeakReferenceCacheManager)) 
                    : this._log;
            }
        }
        public NonWeakReferenceCacheManager()
        {
            this._dict = new Dictionary<string, ICache>();
        }
        public NonWeakReferenceCacheManager(ILoggerFactory factory)
            : this()
        {
            this._log = factory.Create(typeof(NonWeakReferenceCacheManager));
        }

        public void Add(string key, ICache cache)
        {
            if (this._dict.Count >= 5)
                throw new InvalidOperationException("该管理器不适合存储大量不同类型粒度过细的全局缓存，请考虑缓存粒度");
            this._dict.Add(key, cache);
        }
        /// <summary>获取缓存，不存在则返回Null
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ICache Get(string key)
        {
            return this.Get(key, true);
        }
        /// <summary>获取缓存，不存在则返回Null
        /// </summary>
        /// <param name="key"></param>
        /// <param name="force">是否强制自动刷新</param>
        /// <returns></returns>
        public ICache Get(string key, bool force)
        {
            if (!this._dict.ContainsKey(key))
                return null;

            var c = this._dict[key];

            if (force && c is IRefreshCache)
                this.DoRefresh(key, c as IRefreshCache);

            return c;
        }

        private void DoRefresh(string key, IRefreshCache rc)
        {
            if (rc.IsRefreshing) return;
            if (DateTime.Now - rc.LastRefreshTime < rc.Interval) return;

            lock (rc.Lock)
            {
                if (rc.IsRefreshing) return;
                rc.IsRefreshing = true;
            }

            //单独刷新
            //若项太多则浪费线程
            //占用默认线程池
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    if (!rc.CheckValid())
                    {
                        rc.Refresh();
                        this.log.DebugFormat("刷新缓存“{0}”[{1}]", key, rc);
                    }
                }
                catch (Exception e)
                {
                    this.log.Warn(string.Format("缓存“{0}”[{1}]刷新时发生异常", key, rc), e);
                }
                finally
                {
                    rc.LastRefreshTime = DateTime.Now;
                    rc.IsRefreshing = false;
                }
            });
        }

        /// <summary>缓存的项目
        /// </summary>
        public interface ICache
        {

        }
        /// <summary>可刷新的缓存项目
        /// </summary>
        public interface IRefreshCache : ICache
        {
            object Lock { get; }
            TimeSpan Interval { get; }
            bool IsRefreshing { get; set; }
            DateTime LastRefreshTime { get; set; }
            bool CheckValid();
            void Refresh();
        }
    }

    #region 系统配置表缓存
    public class SysConfigTable
    {
        private IDictionary<string, SysConfigItem> _items;
        public string Version { get; private set; }
        public SysConfigTable(string xml)
        {
            /*
<configuration version='{0}'>
    <properties>
        <key cn=""></key>
    </properties> 
</configuration>
             */
            this._items = new Dictionary<string, SysConfigItem>();

            var el = XElement.Parse(xml);
            this.Version = el.Attribute(XName.Get("version")).Value;

            el.Element(XName.Get("properties")).Elements().ToList().ForEach(o =>
            {
                var key = o.Name.LocalName;
                var cn = o.Attribute(XName.Get("cn")).Value;
                var val = o.Value;

                if (!this._items.ContainsKey(key))
                    this._items.Add(key, null);
                this._items[key] = new SysConfigItem(key, cn, val);
            });
        }

        public SysConfigItem this[string key]
        {
            get { return this._items[key]; }
        }

        public void Add(string key, string cn, string value)
        {

        }
    }
    public class SysConfigItem
    {
        public string Key { get; private set; }
        public string CN { get; private set; }
        public string Value { get; private set; }

        public SysConfigItem(string key, string cn, string value)
        {
            this.Key = key;
            this.CN = cn;
            this.Value = value;
        }
    }
    public class SysConfigTablesCache : NonWeakReferenceCacheManager.IRefreshCache
    {
        private Random _rd = new Random();
        private ILog _log; 
        private ILog log
        {
            get
            {
                return this._log == null
                    ? SystemConfig.Settings.GetLoggerFactory().Create(typeof(SysConfigTablesCache))
                    : this._log;
            }
        }
        private IDictionary<string, Properties> _configTables;

        private string _versionFlag;
        private string _uri;

        public SysConfigTablesCache( string versionFlag, string uri, TimeSpan interval)
        {
            this._versionFlag = versionFlag;
            this._uri = uri;
            this.Interval = interval;

            this.Lock = new object();
            this._configTables = new Dictionary<string, Properties>();

            this.PrepareCall();
        }
        public SysConfigTablesCache(ILoggerFactory factory, string versionFlag, string uri, TimeSpan interval)
            : this(versionFlag, uri, interval)
        {
            this._log = factory.Create(typeof(SysConfigTablesCache));
        }
        /// <summary>
        /// 读取配置表并填充至基础配置实例中，若已经存在则忽略
        /// </summary>
        /// <param name="tableKey"></param>
        /// <param name="isIndependent">是否读取为独立配置表，独立配置表不会被填充到基础配置实例中</param>
        public void ReadProperties(string tableKey, bool isIndependent)
        {
            if (this._configTables.ContainsKey(tableKey))
                return;
            this._configTables.Add(tableKey, this.Read(tableKey, this._versionFlag, isIndependent));
            this.LastRefreshTime = DateTime.Now;
        }
        /// <summary>
        /// 获取配置表
        /// </summary>
        /// <param name="tableKey"></param>
        /// <returns></returns>
        public SysConfigTable GetTable(string tableKey)
        {
            return this._configTables[tableKey].Table;
        }

        #region IRefreshCache Members
        public bool CheckValid()
        {
            return false;
        }
        public void Refresh()
        {
            var keys = this._configTables.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++)
            {
                var k = keys[i];
                var c = this._configTables[k];
                try
                {
                    if (!this.CheckValid(c.TableKey, c.Version))
                        this._configTables[k] = this.Read(c.TableKey, c.VersionFlag, c.IsIndependent);
                }
                catch (Exception e)
                {
                    this.log.Error("配置刷新时发生异常", e);
                }
            }
        }
        public TimeSpan Interval { get; private set; }
        public object Lock { get; private set; }
        public bool IsRefreshing { get; set; }
        public DateTime LastRefreshTime { get; set; }
        #endregion

        private bool CheckValid(string tableKey, string version)
        {
            return false;
        }
        private Properties Read(string tableKey, string versionFlag, bool isIndependent)
        {
            return null;
        }
        //HACK:此处实现直接对NSF进行深度使用，不可参考，配置服务相关设施完善后会迁移到更解耦方式
        private void PrepareCall()
        {
            
        }

        class Properties
        {
            public string TableKey { get; private set; }
            public SysConfigTable Table { get; private set; }
            public string Version { get; private set; }
            public string VersionFlag { get; private set; }
            public bool IsIndependent { get; private set; }

            public Properties(string key, string versionFlag, string xml, bool isIndependent)
            {
                this.TableKey = key;
                this.VersionFlag = versionFlag;
                this.Table = new SysConfigTable(xml);
                this.Version = this.Table.Version;
                this.IsIndependent = isIndependent;
            }
        }
    }
    #endregion
}