namespace NHibernate.Caches.SysCache
{
    using log4net;
    using NHibernate.Cache;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Text;

    public class SysCacheProvider : ICacheProvider
    {
        private static readonly Dictionary<string, ICache> caches = new Dictionary<string, ICache>();
        private static readonly ILog log = LogManager.GetLogger(typeof(SysCacheProvider));

        static SysCacheProvider()
        {
            CacheConfig[] section = ConfigurationManager.GetSection("syscache") as CacheConfig[];
            if (section != null)
            {
                foreach (CacheConfig config in section)
                {
                    caches.Add(config.Region, new NHibernate.Caches.SysCache.SysCache(config.Region, config.Properties));
                }
            }
        }

        public ICache BuildCache(string regionName, IDictionary<string, string> properties)
        {
            ICache cache;
            if (regionName == null)
            {
                regionName = string.Empty;
            }
            if (caches.TryGetValue(regionName, out cache))
            {
                return cache;
            }
            if (properties == null)
            {
                properties = new Dictionary<string, string>(1);
            }
            if (log.IsDebugEnabled)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("building cache with region: ").Append(regionName).Append(", properties: ");
                foreach (KeyValuePair<string, string> pair in properties)
                {
                    builder.Append("name=");
                    builder.Append(pair.Key);
                    builder.Append("&value=");
                    builder.Append(pair.Value);
                    builder.Append(";");
                }
                log.Debug(builder.ToString());
            }
            return new NHibernate.Caches.SysCache.SysCache(regionName, properties);
        }

        public long NextTimestamp()
        {
            return Timestamper.Next();
        }

        public void Start(IDictionary<string, string> properties)
        {
        }

        public void Stop()
        {
        }
    }
}

