using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CodeSharp.Core.Services;
using System.Reflection;

namespace CodeSharp.Framework.Castles.Test
{
    /// <summary>初始化测试
    /// </summary>
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class SystemConfigTest
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void CompileSymbol()
        {
            SystemConfig.CompileSymbol = "release";
            Assert.AreEqual("Debug", System.Configuration.ConfigurationManager.AppSettings["EnvironmentVersionFlag"]);
            Assert.AreEqual(EnvironmentVersionFlag.Release, Util.GetEnvironmentVersionFlag());
            Assert.Throws(typeof(NotSupportedException), () => SystemConfig.CompileSymbol = "debug");
        }
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Castle()
        {
            SystemConfig.ConfigFilesAssemblyName = "CodeSharp.Framework.Castles.Test";
            SystemConfig.Configure("ConfigFiles")
                .Castle()
                .BusinessDependency(Assembly.GetExecutingAssembly());

            Assert.AreEqual("abc", SystemConfig.Settings["key1"]);

            Assert.DoesNotThrow(() => DependencyResolver.Resolve<ILoggerFactory>().Create(this.GetType()).Info("hi"));

            Assert.DoesNotThrow(() => DependencyResolver.Resolve<ITestService>());

            Assert.DoesNotThrow(() => DependencyResolver.Resolve<NHibernate.ISessionFactory>());

            //NH Tests
            var e = new TestEntity("abc");
            var s = DependencyResolver.Resolve<ITestService>(); 
            s.Create(e);
            DependencyResolver.Resolve<Castle.Facilities.NHibernateIntegration.ISessionManager>().OpenSession().Evict(e);
            var e2= s.Get(e.ID);
            //private setter?
            Assert.AreEqual(e.Name, e2.Name);
            Assert.AreEqual("abc", e2.Name);

            Assert.DoesNotThrow(() => SystemConfig.Cleanup());
        }
    }
}
