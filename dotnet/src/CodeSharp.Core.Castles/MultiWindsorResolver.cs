//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;

namespace CodeSharp.Core.Castles
{
    /// <summary>
    /// Multi-level Castle Windsor IOC
    /// </summary>
    public class MultiWindsorResolver : WindsorResolver
    {
        private IWindsorContainer _global;
        private IWindsorContainer _local;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="container">父容器</param>
        public MultiWindsorResolver(IWindsorContainer container)
            : base(container)
        {
            //全局
            this._global = container;
            //第二级 本地应用容器
            this._local = new WindsorContainer() { Parent = this._global };
            //第三级 子容器
            this._container = new WindsorContainer() { Parent = this._local };
        }
        /// <summary>
        /// 获取全局（父）容器
        /// </summary>
        public IWindsorContainer Global { get { return this._global; } }
        /// <summary>
        /// 获取本地容器
        /// </summary>
        public IWindsorContainer Local { get { return this._local; } }
        /// <summary>
        /// 获取子容器（该容器具备所有容器的服务）
        /// </summary>
        public override IWindsorContainer Container
        {
            get
            {
                return base.Container;
            }
        }
    }
}