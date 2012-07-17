namespace NHibernate.Caches.SysCache
{
    using System;
    using System.Collections.Generic;

    public class CacheConfig
    {
        private readonly Dictionary<string, string> properties;
        private readonly string regionName;

        public CacheConfig(string region, string expiration, string priority)
        {
            this.regionName = region;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("expiration", expiration);
            dictionary.Add("priority", priority);
            this.properties = dictionary;
        }

        public IDictionary<string, string> Properties
        {
            get
            {
                return this.properties;
            }
        }

        public string Region
        {
            get
            {
                return this.regionName;
            }
        }
    }
}

