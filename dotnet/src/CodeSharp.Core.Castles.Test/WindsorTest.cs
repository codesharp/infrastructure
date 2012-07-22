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
            var windsor = new Castle.Windsor.WindsorContainer();

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
            var windsor = new Castle.Windsor.WindsorContainer();

            windsor.Register(Component.For<TestMain1>().LifeStyle.Transient);
            Assert.IsFalse(windsor.Resolve<TestMain1>().Resolved);

            windsor.Register(Component.For<ITest>().ImplementedBy<Test1>());
            Assert.IsTrue(windsor.Resolve<TestMain1>().Resolved);
        }

        [Test(Description = "验证将组件依赖从有到无的切换，这是ServiceFramework组件切换的基础")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void HandlerSelector()
        {
            var windsor = new Castle.Windsor.WindsorContainer();
            ServiceFramework.Configuration.Configure().Castle(windsor).Endpoint();
            var c = (ServiceFramework.Configuration.Instance() as ConfigurationWithCastle).WindsorContainer;
            windsor.Resolve<RemoteServiceInterceptor>();
            Assert.Throws(typeof(Castle.MicroKernel.Handlers.HandlerException)
                , () => windsor.Resolve<CodeSharp.ServiceFramework.Castles.WindsorContainer.RemovedInterceptor>());

            //首次依赖不满足
            windsor.Register(Component.For<TestMain2>().LifeStyle.Transient);
            Assert.IsFalse(windsor.Resolve<TestMain2>().Resolved);
            //注册远程组件
            c.RegisterRemoteServices(typeof(ITest));
            Assert.IsTrue(windsor.Resolve<TestMain2>().Resolved);
            //卸载远程组件
            c.ClearRemoteServices(typeof(ITest));
            Assert.IsFalse(windsor.Resolve<TestMain2>().Resolved);
            //本地实现注册
            windsor.Register(Component.For<ITest>().ImplementedBy<Test1>().IsDefault());
            Assert.IsTrue(windsor.Resolve<TestMain2>().Resolved);
            Assert.IsInstanceOf<Test1>(windsor.Resolve<TestMain2>().Dependency);
            //重新注册远程组件
            c.RegisterRemoteServices(typeof(ITest));
            Assert.IsTrue(windsor.Resolve<TestMain2>().Resolved);
            Assert.IsNotInstanceOf<Test1>(windsor.Resolve<TestMain2>().Dependency);
        }
        [Test(Description = "验证将组件依赖切换，这是ServiceFramework组件切换的基础")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void HandlerSelector_HasDefault()
        {
            var windsor = new Castle.Windsor.WindsorContainer();

            windsor.Register(Component.For<TestMain2>().LifeStyle.Transient);
            Assert.IsFalse(windsor.Resolve<TestMain2>().Resolved);

            var h = new TestHandlerSelector_HasDefault();
            windsor.Kernel.AddHandlerSelector(h);
            //原始默认组件
            windsor.Register(Component.For<ITest>().ImplementedBy<Test1>());
            //新组件
            windsor.Register(Component.For<ITest>().ImplementedBy<Test2>());

            //通过HandlerSelector启用新组件为默认组件
            h.Enable = true;
            Assert.IsTrue(windsor.Resolve<TestMain2>().Resolved);
            Assert.IsInstanceOf<Test2>(windsor.Resolve<TestMain2>().Dependency);

            //通过HandlerSelector取消新组件
            h.Enable = false;
            Assert.IsTrue(windsor.Resolve<TestMain2>().Resolved);
            //切换回原默认组件
            Assert.IsInstanceOf<Test1>(windsor.Resolve<TestMain2>().Dependency);
        }
        
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void DefaultComponentForServiceFilter()
        {
            var windsor = new Castle.Windsor.WindsorContainer();

            windsor.Register(Component.For<ITest>().ImplementedBy<Test1>().LifeStyle.Transient);
            Assert.AreEqual(typeof(Test1), windsor.Kernel.GetHandler(typeof(ITest)).ComponentModel.Implementation);

            windsor.Register(Component.For<ITest>().ImplementedBy<Test2>().LifeStyle.Transient.IsDefault());
            Assert.AreEqual(typeof(Test2), windsor.Kernel.GetHandler(typeof(ITest)).ComponentModel.Implementation);

            //移除默认组件标记
            var handler = windsor.Kernel.GetHandler(typeof(ITest));
            handler.ComponentModel.ExtendedProperties.Remove(Constants.DefaultComponentForServiceFilter);
            //handler.Init(windsor.Kernel as IKernelInternal);
            //(windsor.Kernel as IKernelInternal).RegisterHandler(handler.ComponentModel.Name + Guid.NewGuid(), handler, false);
            var naming = windsor.Kernel.GetSubSystem(SubSystemConstants.NamingKey) as Castle.MicroKernel.SubSystems.Naming.DefaultNamingSubSystem;
          
            //由于默认组件在注册时已经确定，无法后期修改
            //https://github.com/castleproject/Castle.Windsor-READONLY/blob/master/src/Castle.Windsor/MicroKernel/SubSystems/Naming/DefaultNamingSubSystem.cs

            Assert.IsFalse(windsor.Kernel.GetHandler(typeof(ITest)).ComponentModel.ExtendedProperties.Contains(Constants.DefaultComponentForServiceFilter));
            return;
            Assert.AreEqual(typeof(Test1), windsor.Kernel.GetHandler(typeof(ITest)).ComponentModel.Implementation);
        }

        public interface ITest { }
        public class Test1 : ITest { }
        public class Test2 : ITest { }
        public class TestMain1
        {
            private ITest _dependency;

            public TestMain1() { }
            public TestMain1(ITest dependency) { this._dependency = dependency; }

            public virtual bool Resolved { get { return this._dependency != null; } }
        }
        public class TestMain2
        {
            //可选依赖
            public ITest Dependency { get; set; }
            public TestMain2() { }
            //component一旦注册无法删除，依赖关系已经建立，可选依赖应通过setter注入
            //public TestMain2(ITest dependency) { this.Dependency = dependency; }
            public virtual bool Resolved { get { return this.Dependency != null; } }
        }

        public class TestInterceptor : IInterceptor
        {
            public TestInterceptor() { Console.WriteLine("TestInterceptor1"); }
            public void Intercept(IInvocation invocation)
            {
                throw new NotImplementedException();
            }
        }
        public class TestInterceptor2 : TestInterceptor
        {
            public TestInterceptor2(object anything)
            {
                Console.WriteLine("TestInterceptor2");
            }
            public void Intercept(IInvocation invocation)
            {
                throw new NotImplementedException();
            }
        }
        public class TestHandlerSelector : IHandlerSelector
        {
            public bool Enable { get; set; }

            public bool HasOpinionAbout(string key, Type service)
            {
                return service == typeof(ITest);
            }

            public IHandler SelectHandler(string key, Type service, IHandler[] handlers)
            {
                Trace.WriteLine(key, "HasOpinionAbout Key");
                Trace.WriteLine(service, "HasOpinionAbout service");
                Trace.WriteLine(this.Enable || handlers.Length == 1, "HasOpinionAbout Enable");
                //由于组件无法移除，只能总是返回一个“依赖不会满足”的组件
                return this.Enable || handlers.Length == 1 ? handlers[0] : null;
            }
        }
        public class TestHandlerSelector_HasDefault : IHandlerSelector
        {
            public bool Enable { get; set; }

            public bool HasOpinionAbout(string key, Type service)
            {
                Trace.WriteLine(key, "HasOpinionAbout Key");
                Trace.WriteLine(service, "HasOpinionAbout service");
                return service == typeof(ITest);
            }

            public IHandler SelectHandler(string key, Type service, IHandler[] handlers)
            {
                Trace.WriteLine(key, "SelectHandler Key");
                Trace.WriteLine(service, "SelectHandler service");
                Trace.WriteLine(this.Enable);
                return this.Enable ? handlers[1] : null;
            }
        }
        public class TestComponentActivator : DefaultComponentActivator
        {
            public TestComponentActivator(ComponentModel model, IKernel kernel,
                                       ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
                : base(model, kernel, onCreation, onDestruction)
            {
            }

            public static bool Enable { get; set; }
            public override object Create(Castle.MicroKernel.Context.CreationContext context, Burden burden)
            {
                Console.WriteLine(burden);

                //return null;
                if (Enable)
                    //burden.SetRootInstance(f);
                    return base.Create(context, burden);
                else
                    return null;
            }
        }
    }
}
