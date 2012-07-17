//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.Core.Interfaces;

namespace CodeSharp.Core.Services
{
    /// <summary>支持Resolve Everything
    /// </summary>
    public static class DependencyResolver
    {
        private static ILog _log { get { return DependencyResolver.Resolve<ILoggerFactory>().Create(typeof(DependencyResolver)); } }
        /// <summary>解释器
        /// </summary>
        public static IDependencyResolver Resolver { get; internal set; }
        /// <summary>解释指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">无法创建类型'{0}'的实例</exception>
        public static T Resolve<T>()
        {
            return (T)DependencyResolver.Resolve(typeof(T));
        }
        /// <summary>解释指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">无法创建类型'{0}'的实例</exception>
        public static T Resolve<T>(Type type)
        {
            return (T)DependencyResolver.Resolve(type);
        }
        /// <summary>解释指定类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">无法创建类型'{0}'的实例</exception>
        public static object Resolve(Type type)
        {
            if (DependencyResolver.Resolver != null)
            {
                var instance = DependencyResolver.Resolver.Resolve(type);
                if (instance != null)
                    return instance;
            }

            try
            {
                var obj = Activator.CreateInstance(type);
                _log.DebugFormat("以CreateInstance方式创建了类型'{0}'实例，请检查该类型是否被正确注册", type.FullName);
                return obj;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(string.Format("无法创建类型'{0}'的实例", type.Name), e);
            }
        }
        /// <summary>释放实例
        /// </summary>
        /// <param name="instance"></param>
        public static void Release(object instance)
        {
            if (DependencyResolver.Resolver != null)
                DependencyResolver.Resolver.Release(instance);
        }
    }
}