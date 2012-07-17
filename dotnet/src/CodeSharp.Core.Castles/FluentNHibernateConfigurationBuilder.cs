//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

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
    /// ����֧��fluentNH��������
    /// <remarks>����״̬�º��Բ����ڵ�dll</remarks>
    /// </summary>
    public class FluentNHibernateConfigurationBuilder : IConfigurationBuilder
    {
        /// <summary>
        /// ����nh����
        /// </summary>
        /// <param name="facilityConfiguration"></param>
        /// <returns></returns>
        public NHibernate.Cfg.Configuration GetConfiguration(IConfiguration facilityConfiguration)
        {
            var configuration = new DefaultConfigurationBuilder().GetConfiguration(facilityConfiguration);

            //����ӳ��
            var assemblies = facilityConfiguration.Children["assemblies"];
            assemblies.Children.ForEach(o => configuration.AddMappingsFromAssembly(Assembly.Load(o.Value)));
            //�����������
            var fluent = FluentNHibernate.Cfg.Fluently.Configure(configuration);
            var classes = facilityConfiguration.Children["classes"];

            if (classes == null) return configuration;

            classes.Children.ForEach(o =>
            {
                var m = new PersistenceModel();
                var type = Type.GetType(o.Value, false);
                if (type == null)
                    throw new Exception("�Ҳ�������" + o.Value + "����ȷ���Ƿ����ø��������ڵĳ���");

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