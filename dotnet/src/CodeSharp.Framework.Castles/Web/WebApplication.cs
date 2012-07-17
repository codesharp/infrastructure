//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using Castle.Windsor;
using CodeSharp.Core.Castles;
using CodeSharp.Core.Services;

namespace CodeSharp.Framework.Castles.Web
{
    /// <summary>HttpApplication
    /// <remarks>
    /// 继承自CodeSharp.Framework.Web.WebApplication并额外实现了Castle.Windsor.IContainerAccessor接口
    /// </remarks>
    /// </summary>
    public class WebApplication : CodeSharp.Framework.Web.WebApplication, IContainerAccessor
    {
        #region IContainerAccessor Members
        IWindsorContainer IContainerAccessor.Container
        {
            get { return (DependencyResolver.Resolver as WindsorResolver).Container; }
        }
        #endregion
    }
}