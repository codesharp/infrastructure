//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.RepositoryFramework;
using CodeSharp.Core.Services;

namespace CodeSharp.Core.Castles
{
    /// <summary>
    /// 工具方法
    /// </summary>
    internal sealed class Util
    {
        /// <summary>
        /// 判断是否是MVC Controller
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsController(Type type)
        {
            return type != null
                   && type.Name.EndsWith("Controller", StringComparison.InvariantCultureIgnoreCase)
                   && !type.IsAbstract;
            //&& typeof(IController).IsAssignableFrom(type);
        }
        /// <summary>
        /// 判断是否是Repository
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsRepository(Type type)
        {
            return type != null
                 && type.Name.EndsWith("Repository", StringComparison.InvariantCultureIgnoreCase)
                 && !type.IsAbstract
                 && !type.IsInterface;
        }
        /// <summary>
        /// 判断是否是Service
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsService(Type type)
        {
            return type != null
                 && type.Name.EndsWith("Service", StringComparison.InvariantCultureIgnoreCase)
                 && !type.IsAbstract
                 && !type.IsInterface;
        }
        /// <summary>
        /// 判断是否有ComponentAttribute属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsComponent(Type type)
        {
            return type != null
                 && type.GetCustomAttributes(typeof(ComponentAttribute), false).Count() > 0
                 && !type.IsAbstract
                 && !type.IsInterface;
        }
        /// <summary>
        /// 用于框架内部的log
        /// </summary>
        /// <returns></returns>
        public static ILog GetLogger()
        {
            return DependencyResolver.Resolve<ILoggerFactory>().Create("CodeSharp.Core.Castles");
        }
    }
}