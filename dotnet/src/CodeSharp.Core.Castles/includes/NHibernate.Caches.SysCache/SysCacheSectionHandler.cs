namespace NHibernate.Caches.SysCache
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Xml;

    public class SysCacheSectionHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            List<CacheConfig> list = new List<CacheConfig>();
            XmlNodeList list2 = section.SelectNodes("cache");
            foreach (XmlNode node in list2)
            {
                string region = null;
                string expiration = null;
                string priority = "3";
                XmlAttribute attribute = node.Attributes["region"];
                XmlAttribute attribute2 = node.Attributes["expiration"];
                XmlAttribute attribute3 = node.Attributes["priority"];
                if (attribute != null)
                {
                    region = attribute.Value;
                }
                if (attribute2 != null)
                {
                    expiration = attribute2.Value;
                }
                if (attribute3 != null)
                {
                    priority = attribute3.Value;
                }
                if ((region != null) && (expiration != null))
                {
                    list.Add(new CacheConfig(region, expiration, priority));
                }
            }
            return list.ToArray();
        }
    }
}

