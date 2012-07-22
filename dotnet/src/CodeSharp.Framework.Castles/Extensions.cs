//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Reflection;

namespace CodeSharp.Framework.Castles
{
    /// <summary>为SystemConfig提供基于Castle容器的相关功能使用的配置扩展
    /// </summary>
    public static class Extensions
    {
        /// <summary>启用castle作为容器化架构的默认容器
        /// <remarks>
        /// 若您决定启用容器，
        /// 强烈建议SystemConfig.Configure(app)之后立刻声明Castle()再进行其他特性的配置，
        /// 以确保您启用的特性能顺利享受到容器带来的变化
        /// 如：SystemConfig.Configure().Castle().DefaultAppAgent();
        /// </remarks>
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static SystemConfigWithCastle Castle(this SystemConfig config)
        {
            return SystemConfig.Configure(new SystemConfigWithCastle(config)) as SystemConfigWithCastle;
        }
    }
}