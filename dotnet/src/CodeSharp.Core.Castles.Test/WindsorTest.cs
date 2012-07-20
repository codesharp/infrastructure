using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.DynamicProxy;
using Castle.MicroKernel;
using System.Diagnostics;

namespace CodeSharp.Core.Castles.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class WindsorTest
    {
        [Test(Description = "3.x New Feature")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void IsDefault()
        {
            var windsor = new WindsorContainer();

            windsor.Register(Component.For<ITest>().ImplementedBy<Test1>());
            Assert.IsInstanceOf<Test1>(windsor.Resolve<ITest>());

            windsor.Register(Component.For<ITest>().ImplementedBy<Test2>().IsDefault());
            Assert.IsInstanceOf<Test2>(windsor.Resolve<ITest>());

            //不支持注册Null
            Assert.Throws(typeof(ArgumentNullException), () => windsor.Register(Component.For<ITest>().Instance(null).IsDefault()));

            //覆盖默认组件为“没有设置proxy”
            //非法的注册总是导致失败
            windsor.Register(Component.For<ITest>().IsDefault());
            Assert.Throws(typeof(Castle.MicroKernel.ComponentRegistrationException), () => windsor.Resolve<ITest>());
        }

        [Test(Description = "构造器注入，根据依赖切换")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void ConstructorDependency()
        {
            var windsor = new WindsorContainer();

            windsor.Register(Component.For<TestMain>().LifeStyle.Transient);
            Assert.IsFalse(windsor.Resolve<TestMain>().Resolved);

            windsor.Register(Component.For<ITest>().ImplementedBy<Test1>());
            Assert.IsTrue(windsor.Resolve<TestMain>().Resolved);
        }

        [Test(Description = "验证将组件依赖从有到无的切换，这是ServiceFramework组件切换的基础")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void HandlerSelector()
        {
            var windsor = new WindsorContainer();

            var h = new TestHandlerSelector();
            //自定义选择器
            windsor.Kernel.AddHandlerSelector(h);
            windsor.Register(Component.For<ITest>().Named(TestHandlerSelector.PREFIX + "Test").ImplementedBy<Test1>().IsDefault());
            windsor.Register(Component.For<TestMain>().LifeStyle.Transient);
            

            h.Enable = true;
            Assert.IsTrue(windsor.Resolve<TestMain>().Resolved);

            //利用IHandlerSelector将依赖置为不满足
            h.Enable = false;
            Assert.IsFalse(windsor.Resolve<TestMain>().Resolved);
        }

        public interface ITest { }
        public class Test1 : ITest { }
        public class Test2 : ITest { }
        public class TestMain
        {
            private ITest _dependency;
            public TestMain() { }
            public TestMain(ITest dependency) { this._dependency = dependency; }

            public virtual bool Resolved { get { return this._dependency != null; } }
        }
        public class TestInterceptor : IInterceptor
        {
            public TestInterceptor(string anything) { }

            public void Intercept(IInvocation invocation)
            {
                throw new NotImplementedException();
            }
        }

        public class TestHandlerSelector : IHandlerSelector
        {
            public static string PREFIX = "remote_";
            public bool Enable { get; set; }

            public bool HasOpinionAbout(string key, Type service)
            {
                Trace.WriteLine(key, "HasOpinionAbout Key");
                Trace.WriteLine(service, "HasOpinionAbout service");
                return (key ?? "").StartsWith(PREFIX) || service == typeof(ITest);
            }

            public IHandler SelectHandler(string key, Type service, IHandler[] handlers)
            {
                Trace.WriteLine(key, "SelectHandler Key");
                Trace.WriteLine(service, "SelectHandler service");
                Trace.WriteLine(this.Enable);
                return this.Enable ? handlers[0] : null;
            }
        }
    }
}
