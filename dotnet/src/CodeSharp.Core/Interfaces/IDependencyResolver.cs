//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.Core.Interfaces
{
    /// <summary>依赖解释器
    /// </summary>
    public interface IDependencyResolver
    {
        /// <summary>解释指定类型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        T Resolve<T>();
        /// <summary>解释指定类型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="type">实现类型</param>
        /// <returns></returns>
        T Resolve<T>(Type type);
        /// <summary>解释指定类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        object Resolve(Type type);
        /// <summary>解释指定类型
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        object Resolve(string key, Type type);
        /// <summary>释放指定实例
        /// </summary>
        /// <param name="instance"></param>
        void Release(object instance);
    }
}