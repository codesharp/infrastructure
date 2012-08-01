using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace CodeSharp.Framework.Web
{
    /// <summary>提供与http上下文相关的辅助
    /// </summary>
    public static class HttpUtil
    {
        /// <summary>获取Http上下文文本信息
        /// </summary>
        /// <returns></returns>
        public static string GetHttpContextInfo()
        {
            var context = HttpContext.Current;
            return string.Format("\nUrl:{0};\nUserName:{1};\nBrowserInfo:{2};\nData:{3};\nMachine:{4};\nHttpMethod:{5}\n"
                , context.Request.Url
                , context.User != null ? context.User.Identity.Name : ""
                , HttpUtility.HtmlEncode(context.Request.ServerVariables["ALL_HTTP"])
                , context.Request.ContentEncoding.GetString(context.Request.BinaryRead(context.Request.TotalBytes))
                , context.Server.MachineName
                , context.Request.HttpMethod);
        }
        /// <summary>获取当前Http请求的客户端Url Scheme
        /// <remarks>
        /// X-Forwarded-Proto
        /// 由于可能会使用client到反向代理的ssl通道，所以请使用该方法来获取当前请求是来自http还是https
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        public static string GetClientUrlScheme()
        {
            //HACK:此处根据反向代理的header设置而来
            var proto = HttpContext.Current.Request.Headers["X-Forwarded-Proto"];
            return string.IsNullOrEmpty(proto)
                ? HttpContext.Current.Request.Url.Scheme
                : proto;
        }
        /// <summary>获取当前Http请求的客户端IP串
        /// <remarks>由于反向代理，IP将取自X-Forwarded-For否则直接返回Request.UserHostAddress的值</remarks>
        /// </summary>
        /// <returns></returns>
        public static string GetClientIP()
        {
            var forwarded = HttpContext.Current.Request.Headers["X-Forwarded-For"];
            return string.IsNullOrWhiteSpace(forwarded)
                ? HttpContext.Current.Request.UserHostAddress
                : forwarded;
        }
    }
}