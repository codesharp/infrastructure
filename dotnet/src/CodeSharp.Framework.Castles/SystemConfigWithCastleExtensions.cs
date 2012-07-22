//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.MicroKernel.Registration;
using CodeSharp.Core.Castles;

namespace CodeSharp.Framework.Castles
{
    /// <summary>用于描述面向常规业务系统的扩展
    /// </summary>
    public static class SystemConfigWithCastleExtensions
    {
        /// <summary>基于Castle的常规应用初始化
        /// <remarks>
        /// 较粗粒度的初始化，
        /// 包含Configure()调用，Castle容器初始化，系统配置表，基础库注册，程序集注册，web上下文服务
        /// 若熟悉配置框架或有更轻量需求可按需进行定制，使用SystemConfig.Configure
        /// </remarks>
        /// </summary>
        /// <param name="app">应用名，对应configfiles下的目录</param>
        /// <param name="versionFlag">指定环境版本</param>
        /// <param name="func">自定义初始化</param>
        /// <param name="assemblies">设置要被注册的程序集</param>
        public static SystemConfigWithCastle Initialize(this SystemConfig config, Action<WindsorResolver> func, params Assembly[] assemblies)
        {
            return config.Castle()
                .ReadCommonProperties()
                .BusinessDependency(assemblies)
                .Resolve(func);
        }
    }
}