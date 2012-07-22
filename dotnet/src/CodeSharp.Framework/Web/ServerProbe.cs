//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Web;

namespace CodeSharp.Framework.Web
{
    /// <summary>服务器探针
    /// <remarks>
    /// 仅用于放置输出服务端相关参数信息的方法
    /// 统一维护不可新增，不保证稳定
    /// </remarks>
    /// </summary>
    public sealed class ServerProbe
    {
        internal static readonly string _requestTimeFlag = "____RequestTime";
        internal static readonly string _machineNameFlag = "____MachineName";
        internal static readonly int _maxCostLimit = 1500;

        /// <summary>准备参数，需要在请求起始位置执行
        /// </summary>
        public static void Prepare()
        {
            HttpContext.Current.Items[_requestTimeFlag] = DateTime.Now;
        }
        /// <summary>输出服务器相关可公开信息
        /// </summary>
        /// <returns></returns>
        public static string RenderPublicInfo()
        {
            var context = HttpContext.Current;
            var request = HttpContext.Current.Request;
            var response = HttpContext.Current.Response;
            var url = request.Url.ToString().ToLower();

            if (!context.Items.Contains(_requestTimeFlag))
                return string.Empty;

            var time = System.DateTime.Now - (DateTime)context.Items[_requestTimeFlag];
            return string.Format(
                "<div style=\"color:#fff\">机器名：{0}，服务端加载时间：{1}，页面渲染时间：<span id=\"TimeShow\"></span></div>"
                , context.Server.MachineName
                , time.TotalMilliseconds.ToString() + "ms");
        }
        /// <summary>输出服务器相关可公开信息
        /// <remarks>输出到Header</remarks>
        /// </summary>
        /// <returns></returns>
        public static void RenderInfo()
        {
            var context = HttpContext.Current;
            var request = HttpContext.Current.Request;
            var response = HttpContext.Current.Response;
            var url = request.Url.ToString().ToLower();

            if (!context.Items.Contains(_requestTimeFlag))
                return;

            var time = (System.DateTime.Now - (DateTime)context.Items[_requestTimeFlag]).TotalMilliseconds;

            if (time > _maxCostLimit)
                SystemConfig.Settings.GetLoggerFactory().Create("RequestPerformance").WarnFormat(
                    "以下请求耗时过长，请近期内查明并修正：url={0}|time={1}|user={2}|method={3}"
                    , url
                    , time
                    , context.User.Identity.Name
                    , context.Request.RequestType);

            try
            {
                //HACK:若在此前调用了Response.End()或文件流下载
                //有可能造成此处异常，但不影响请求完成
                response.AppendHeader(_requestTimeFlag, time + "ms");
                response.AppendHeader(_machineNameFlag, context.Server.MachineName);
            }
            catch { }
        }
        /// <summary>返回服务器、进程等信息
        /// </summary>
        /// <returns></returns>
        public static string RenderServerInfo()
        {
            return string.Format(@"
# Server Information

## TimeZone

- Now = {0}
- UtcNow = {1}
- CurrentTimeZone = {2}

## CultureInfo

- Name = {3}
- DateTimeFormat.ShortDatePattern = {4}
- DateTimeFormat.LongDatePattern = {5}
- NumberFormat = {6}

## Environment

- CommandLine = {7}
- CurrentDirectory = {8}
- Is64BitOperatingSystem = {9}
- Is64BitProcess = {10}
- OSVersion = {11}
- ProcessorCount = {12}
- SystemDirectory = {13}
- SystemPageSize = {14}
- Version = {15}

## AppDomain

- BaseDirectory = {16}
- RelativeSearchPath = {17}
- ShadowCopyFiles = {18}

"
                , System.DateTime.Now
                , System.DateTime.UtcNow
                , System.TimeZone.CurrentTimeZone.StandardName

                , System.Globalization.CultureInfo.CurrentCulture.Name
                , System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern
                , System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern
                , System.Globalization.CultureInfo.CurrentCulture.NumberFormat

                , System.Environment.CommandLine
                , System.Environment.CurrentDirectory
                , System.Environment.Is64BitOperatingSystem
                , System.Environment.Is64BitProcess
                , System.Environment.MachineName
                , System.Environment.OSVersion
                , System.Environment.ProcessorCount
                , System.Environment.SystemDirectory
                , System.Environment.SystemPageSize
                , System.Environment.Version

                , System.AppDomain.CurrentDomain.BaseDirectory
                , System.AppDomain.CurrentDomain.RelativeSearchPath
                , System.AppDomain.CurrentDomain.ShadowCopyFiles);
        }
    }
}