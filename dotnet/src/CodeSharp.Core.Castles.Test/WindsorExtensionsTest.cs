//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.DynamicProxy;
using Castle.MicroKernel;
using System.Diagnostics;
using Castle.MicroKernel.ComponentActivator;
using Castle.Core;
using Castle.Core.Internal;
using CodeSharp.ServiceFramework.Castles;
using System.Reflection;

namespace CodeSharp.Core.Castles.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class WindsorExtensionsTest
    {
        private CodeSharp.Core.Configuration _config;
        private IWindsorContainer _windsor;
        [TestFixtureSetUp]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitialize]
        public void Init()
        {
            _config = CodeSharp.Core.Configuration.Config().Castle(o => _windsor = o.Container);
        }
        [TestFixtureTearDown]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanup]
        public void Cleanup()
        {
            CodeSharp.Core.Configuration.Cleanup();
        }

        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void RegisterAnything()
        {
            _windsor.RegisterAnything<TestClass, TestSubClass>();
            //不满足依赖
            Assert.Throws(typeof(Castle.MicroKernel.Handlers.HandlerException), () => _windsor.Resolve<TestClass>());
            //注入配置
            var a = Assembly.GetExecutingAssembly();
            _config.ReadProperties(a, a.GetName().Name + ".properties.config");
            Assert.IsInstanceOf<TestSubClass>(_windsor.Resolve<TestClass>());
        }

        public class TestClass { }
        public class TestSubClass : TestClass
        {
            public TestSubClass(string name) { }
        }
    }
}
