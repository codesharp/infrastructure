//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Windsor;

namespace CodeSharp.Core.Web
{
	/// <summary>为Controller提供基于Windsor的依赖注入支持
	/// </summary>
	public class WindsorControllerFactory : DefaultControllerFactory
	{
		private IWindsorContainer _container;

		public WindsorControllerFactory(IWindsorContainer container)
		{
			if (container == null)
				throw new ArgumentNullException("container");
			_container = container;
		}

        protected override IController GetControllerInstance(RequestContext context, Type controllerType)
        {
            if (controllerType == null)
                throw new HttpException(404
                    , string.Format("The controller for path '{0}' could not be found or it does not implement IController.", context.HttpContext.Request.Path));
            
            return _container.Kernel.HasComponent(controllerType)
                ? (IController)_container.Resolve(controllerType)
                : base.GetControllerInstance(context, controllerType);
        }

        public override void ReleaseController(IController controller)
        {
            var disposable = controller as IDisposable;

            if (disposable != null)
                disposable.Dispose();

            _container.Release(controller);
        }
	}
}
