//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

using Castle.Core.Configuration;
using Castle.Facilities.NHibernateIntegration.Internal;
using Castle.Facilities.NHibernateIntegration;
using Castle.Facilities.NHibernateIntegration.Builders;
using NHibernate.Cfg;
using FluentNHibernate;

namespace CodeSharp.Core.Castles
{
    /// <summary>
    /// 用以支持fluentNH配置生成
    /// <remarks>调试状态下忽略不存在的dll</remarks>
    /// </summary>
    public class FluentNHibernateConfigurationBuilder : IConfigurationBuilder
    {
        /// <summary>
        /// 生成nh配置
        /// </summary>
        /// <param name="facilityConfiguration"></param>
        /// <returns></returns>
        public NHibernate.Cfg.Configuration GetConfiguration(IConfiguration facilityConfiguration)
        {
            var configuration = new DefaultConfigurationBuilder().GetConfiguration(facilityConfiguration);

            //程序集映射
            var assemblies = facilityConfiguration.Children["assemblies"];
            assemblies.Children.ForEach(o => configuration.AddMappingsFromAssembly(Assembly.Load(o.Value)));
            //逐个类型声明
            var fluent = FluentNHibernate.Cfg.Fluently.Configure(configuration);
            var classes = facilityConfiguration.Children["classes"];

            if (classes == null) return configuration;

            classes.Children.ForEach(o =>
            {
                var m = new PersistenceModel();
                var type = Type.GetType(o.Value, false);
                if (type == null)
                    throw new Exception("找不到类型" + o.Value + "，请确认是否引用该类型所在的程序集");

                m.Add(type);
                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream))
                {
                    m.WriteMappingsTo(writer);
                    writer.Flush();
                    stream.Seek(0, 0);
                    configuration.AddInputStream(stream);
                }
            });

            return configuration;
        }
    }
}