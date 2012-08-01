//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace CodeSharp.Framework
{
    /// <summary>仅用于声明少量系统内部全局静态变量
    /// <remarks>
    /// 无法通过配置服务后期绑定的全局变量
    /// 非上述情况不可使用
    /// </remarks>
    /// </summary>
    public sealed class Static
    {
        /// <summary>
        /// 环境变量配置键名称
        /// </summary>
        public static readonly string EnvironmentVersionFlag = "EnvironmentVersionFlag";
        /// <summary>
        /// 自动刷新配置表的配置键名称
        /// </summary>
        public static readonly string AutoRefreshSettingsFlag = "AutoRefreshSettingsFlag";
    }
    /// <summary>仅用于提供少量的辅助方法
    /// <remarks>
    /// 主要用于解决框架因素产生的工具方法需求
    /// 仅用于UI层或宿主程序
    /// </remarks>
    /// </summary>
    public sealed class Util
    {
        private static EnvironmentVersionFlag? _environmentVersionFlag = null;
        private static bool? _autoRefreshSettingsFlag = null;
        /// <summary>获取当前环境变量
        /// <remarks>Release编译下总是使用Release</remarks>
        /// </summary>
        /// <returns></returns>
        public static EnvironmentVersionFlag GetEnvironmentVersionFlag()
        {
            return GetEnvironmentVersionFlag(ConfigurationManager.AppSettings[Static.EnvironmentVersionFlag]);
        }
        /// <summary>获取当前环境变量
        /// <remarks>Release编译总是使用Release</remarks>
        /// </summary>
        /// <param name="flag">指定环境变量</param>
        /// <returns></returns>
        public static EnvironmentVersionFlag GetEnvironmentVersionFlag(string flag)
        {
            //HACK:根据CompileSymbol来处理EnvironmentVersionFlag获取方式，Release编译总是使用Release
            if (!(SystemConfig.CompileSymbol ?? string.Empty).ToLower().Split('|').Contains("release"))
            {
                try
                {
                    if (!_environmentVersionFlag.HasValue)
                        _environmentVersionFlag = (EnvironmentVersionFlag)Enum.Parse(typeof(EnvironmentVersionFlag), flag ?? "Debug");
                    return _environmentVersionFlag.Value;
                }
                catch
                {
                    return EnvironmentVersionFlag.Debug;
                }
            }
            return EnvironmentVersionFlag.Release;
        }
        /// <summary>获取是否允许自动刷新配置等静态缓存
        /// </summary>
        /// <returns></returns>
        public static bool GetAutoRefreshSettingsFlag()
        {
            if (!_autoRefreshSettingsFlag.HasValue)
            {
                _autoRefreshSettingsFlag = Util.GetEnvironmentVersionFlag() != EnvironmentVersionFlag.Debug
                    || Convert.ToBoolean(ConfigurationManager.AppSettings[Static.AutoRefreshSettingsFlag]);
            }
            return _autoRefreshSettingsFlag.Value;
        }
        /// <summary>获取生成运行时变量的函数
        /// </summary>
        /// <returns></returns>
        internal static Func<string, IDictionary<string, object>> GetHowToGenerateRuntimeProperties(SystemConfig config)
        {
            return flag =>
            {
                //HACK:以下的key的可以在log4net中使用 %property{key}
                var dict = new Dictionary<string, object>();
                dict.Add("appName", config.AppName);
                dict.Add("versionFlag", config.VersionFlag);
                dict.Add("host", Environment.MachineName);

                //仅当错误级别的log才提供的信息
                //if (flag == CodeSharp.Core.Configuration.RuntimeProperties_Environment_Error)
                //    dict.Add("httpDump", new HttpDump());
                //else
                //    dict.Add("httpDump", "");

                return dict;
            };
        }
    }
    /// <summary>环境变量
    /// </summary>
    public enum EnvironmentVersionFlag
    {
        Debug = 0,
        Test = 1,
        Exp = 2,
        Release = 3
    }
}