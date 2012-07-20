using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.ServiceFramework.Interfaces
{
    /// <summary>
    /// 服务容器
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// 获取指定类型的实例，不存在则返回Null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Resolve<T>() where T : class;
        /// <summary>
        /// 获取指定类型的实例，不存在则返回Null
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object Resolve(Type type);
        /// <summary>
        /// 向容器中注册指定类型
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="classType"></param>
        void Register(Type serviceType, Type classType);
        /// <summary>
        /// 向容器中注册指定对象实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="instance"></param>
        void Register(Type serviceType, object instance);
        /// <summary>
        /// 注册远程服务
        /// </summary>
        /// <param name="serviceTypes"></param>
        void RegisterRemoteServices(params Type[] serviceTypes);
        /// <summary>
        /// 清除远程服务
        /// </summary>
        /// <param name="serviceTypes"></param>
        void ClearRemoteServices(params Type[] serviceTypes);
    }
}
