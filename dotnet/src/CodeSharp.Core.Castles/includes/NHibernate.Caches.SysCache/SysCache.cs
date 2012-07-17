namespace NHibernate.Caches.SysCache
{
    using log4net;
    using NHibernate.Cache;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Caching;

    public class SysCache : ICache
    {
        private readonly Cache cache;
        private const string CacheKeyPrefix = "NHibernate-Cache:";
        private static readonly string DefauktRegionPrefix = string.Empty;
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromSeconds(300.0);
        private TimeSpan expiration;
        private static readonly ILog log = LogManager.GetLogger(typeof(NHibernate.Caches.SysCache.SysCache));
        private CacheItemPriority priority;
        private readonly string region;
        private string regionPrefix;
        private readonly string rootCacheKey;
        private bool rootCacheKeyStored;

        public SysCache() : this("nhibernate", null)
        {
        }

        public SysCache(string region) : this(region, null)
        {
        }

        public SysCache(string region, IDictionary<string, string> properties)
        {
            this.region = region;
            this.cache = HttpRuntime.Cache;
            this.Configure(properties);
            this.rootCacheKey = this.GenerateRootCacheKey();
            this.StoreRootCacheKey();
        }

        public void Clear()
        {
            this.RemoveRootCacheKey();
            this.StoreRootCacheKey();
        }

        private void Configure(IDictionary<string, string> props)
        {
            if (props == null)
            {
                if (log.IsWarnEnabled)
                {
                    log.Warn("configuring cache with default values");
                }
                this.expiration = DefaultExpiration;
                this.priority = CacheItemPriority.Normal;
                this.regionPrefix = DefauktRegionPrefix;
            }
            else
            {
                this.priority = GetPriority(props);
                this.expiration = GetExpiration(props);
                this.regionPrefix = GetRegionPrefix(props);
            }
        }

        private static CacheItemPriority ConvertCacheItemPriorityFromXmlString(string priorityString)
        {
            if (string.IsNullOrEmpty(priorityString))
            {
                return CacheItemPriority.Normal;
            }
            string s = priorityString.Trim().ToLowerInvariant();
            if ((s.Length == 1) && char.IsDigit(priorityString, 0))
            {
                int num = int.Parse(s);
                if ((num >= 1) && (num <= 6))
                {
                    return (CacheItemPriority) num;
                }
            }
            else
            {
                switch (s)
                {
                    case "abovenormal":
                        return CacheItemPriority.AboveNormal;

                    case "belownormal":
                        return CacheItemPriority.BelowNormal;

                    case "default":
                        return CacheItemPriority.Normal;

                    case "high":
                        return CacheItemPriority.High;

                    case "low":
                        return CacheItemPriority.Low;

                    case "normal":
                        return CacheItemPriority.Normal;

                    case "notremovable":
                        return CacheItemPriority.NotRemovable;
                }
            }
            log.Error("priority value out of range: " + priorityString);
            throw new IndexOutOfRangeException("Priority must be a valid System.Web.Caching.CacheItemPriority; was: " + priorityString);
        }

        public void Destroy()
        {
            this.Clear();
        }

        private string GenerateRootCacheKey()
        {
            return this.GetCacheKey(Guid.NewGuid());
        }

        public object Get(object key)
        {
            if (key != null)
            {
                string cacheKey = this.GetCacheKey(key);
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("Fetching object '{0}' from the cache.", cacheKey));
                }
                object obj2 = this.cache.Get(cacheKey);
                if (obj2 == null)
                {
                    return null;
                }
                DictionaryEntry entry = (DictionaryEntry) obj2;
                if (key.Equals(entry.Key))
                {
                    return entry.Value;
                }
            }
            return null;
        }

        private string GetCacheKey(object key)
        {
            return string.Concat(new object[] { "NHibernate-Cache:", this.regionPrefix, this.region, ":", key.ToString(), "@", key.GetHashCode() });
        }

        private static TimeSpan GetExpiration(IDictionary<string, string> props)
        {
            string str;
            TimeSpan defaultExpiration = DefaultExpiration;
            if (!props.TryGetValue("expiration", out str))
            {
                props.TryGetValue("cache.default_expiration", out str);
            }
            if (str != null)
            {
                try
                {
                    int num = Convert.ToInt32(str);
                    defaultExpiration = TimeSpan.FromSeconds((double) num);
                    log.Debug("new expiration value: " + num);
                }
                catch (Exception exception)
                {
                    log.Error("error parsing expiration value");
                    throw new ArgumentException("could not parse 'expiration' as a number of seconds", exception);
                }
                return defaultExpiration;
            }
            if (log.IsDebugEnabled)
            {
                log.Debug("no expiration value given, using defaults");
            }
            return defaultExpiration;
        }

        private static CacheItemPriority GetPriority(IDictionary<string, string> props)
        {
            string str;
            CacheItemPriority normal = CacheItemPriority.Normal;
            if (props.TryGetValue("priority", out str))
            {
                normal = ConvertCacheItemPriorityFromXmlString(str);
                if (log.IsDebugEnabled)
                {
                    log.Debug("new priority: " + normal);
                }
            }
            return normal;
        }

        private static string GetRegionPrefix(IDictionary<string, string> props)
        {
            string defauktRegionPrefix;
            if (props.TryGetValue("regionPrefix", out defauktRegionPrefix))
            {
                log.DebugFormat("new regionPrefix :{0}", defauktRegionPrefix);
                return defauktRegionPrefix;
            }
            defauktRegionPrefix = DefauktRegionPrefix;
            log.Debug("no regionPrefix value given, using defaults");
            return defauktRegionPrefix;
        }

        public void Lock(object key)
        {
        }

        public long NextTimestamp()
        {
            return Timestamper.Next();
        }

        public void Put(object key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", "null key not allowed");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value", "null value not allowed");
            }
            string cacheKey = this.GetCacheKey(key);
            if (this.cache[cacheKey] != null)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("updating value of key '{0}' to '{1}'.", cacheKey, value));
                }
                this.cache.Remove(cacheKey);
            }
            else if (log.IsDebugEnabled)
            {
                log.Debug(string.Format("adding new data: key={0}&value={1}", cacheKey, value));
            }
            if (!this.rootCacheKeyStored)
            {
                this.StoreRootCacheKey();
            }
            this.cache.Add(cacheKey, new DictionaryEntry(key, value), new CacheDependency(null, new string[] { this.rootCacheKey }), DateTime.Now.Add(this.expiration), Cache.NoSlidingExpiration, this.priority, null);
        }

        public void Remove(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            string cacheKey = this.GetCacheKey(key);
            if (log.IsDebugEnabled)
            {
                log.Debug("removing item with key: " + cacheKey);
            }
            this.cache.Remove(cacheKey);
        }

        private void RemoveRootCacheKey()
        {
            this.cache.Remove(this.rootCacheKey);
        }

        private void RootCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            this.rootCacheKeyStored = false;
        }

        private void StoreRootCacheKey()
        {
            this.rootCacheKeyStored = true;
            this.cache.Add(this.rootCacheKey, this.rootCacheKey, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, new CacheItemRemovedCallback(this.RootCacheItemRemoved));
        }

        public void Unlock(object key)
        {
        }

        public TimeSpan Expiration
        {
            get
            {
                return this.expiration;
            }
        }

        public CacheItemPriority Priority
        {
            get
            {
                return this.priority;
            }
        }

        public string Region
        {
            get
            {
                return this.region;
            }
        }

        public string RegionName
        {
            get
            {
                return this.region;
            }
        }

        public int Timeout
        {
            get
            {
                return 0xea60000;
            }
        }
    }
}

