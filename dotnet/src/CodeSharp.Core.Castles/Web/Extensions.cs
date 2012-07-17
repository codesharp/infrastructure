//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;
using System.Reflection;
using CodeSharp.Core.Castles;

namespace CodeSharp.Core.Web
{
    public static class Extensions
    {
        /// <summary>将MVC ControllerFactory设置为WindsorControllerFactory
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns> 
        public static IWindsorContainer ControllerFactory(this IWindsorContainer container)
        {
            System.Web.Mvc.ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(container));
            return container;
        }

        #region 注册Controller
        /// <summary>注册Controller
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterController<T>(this IWindsorContainer container)
        {
            container.RegisterControllers(typeof(T));
            return container;
        }
        /// <summary>注册Controller
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterControllers(this IWindsorContainer container, params Assembly[] assemblies)
        {
            container.RegisterFromInterface(Util.IsController, assemblies);
            return container;
            //if (assemblies != null)
            //    assemblies.ToList().ForEach(assembly => container.RegisterControllers(assembly.GetExportedTypes()));
            //return container;
        }
        /// <summary>注册Controller
        /// </summary>
        /// <param name="container"></param>
        /// <param name="controllerTypes"></param>
        /// <returns></returns>
        public static IWindsorContainer RegisterControllers(this IWindsorContainer container, params Type[] controllerTypes)
        {
            container.RegisterFromInterface(Util.IsController, controllerTypes);
            return container;
            //if (controllerTypes != null)
            //{
            //    controllerTypes.ToList().ForEach(type =>
            //    {
            //        if (Util.IsController(type))
            //            container.Register(Component
            //                .For(type)
            //                .Named(type.FullName.ToLower())
            //                .Parameters(GenerateParameters(type))
            //                .LifeStyle.Transient);
            //    });
            //}
            //return container;
        }
        #endregion
    }
}