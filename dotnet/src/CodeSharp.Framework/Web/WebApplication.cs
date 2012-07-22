//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using CodeSharp.Core;
using CodeSharp.Core.Services;

namespace CodeSharp.Framework.Web
{
    /// <summary>HttpApplication 包含：统一module，加载耗时统计，默认全局异常处理
    /// </summary>
    public class WebApplication : HttpApplication
    {
        private static List<IHttpModule> _empty = new List<IHttpModule>();
        protected static readonly string _fileExtensions = ".js|.css|.jpg|.gif|.png|.bmp";
        protected ILog _log { get { return SystemConfig.Settings.GetLoggerFactory().Create(this.GetType()); } }
        //异常体系声明
        protected IExceptionSystem _exceptionSystem
        {
            get { return SystemConfig.Settings.GetExceptionSystem(); }
        }
        //动态注册httpmodule
        public override void Init()
        {
            base.Init();
            //动态注册
            this.DeclareHttpModules().ToList().ForEach(o => o.Init(this));
        }
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //准备探针参数
            Web.ServerProbe.Prepare();
        }
        protected void Application_EndRequest(object sender, EventArgs e)
        {
            //写出探针信息
            Web.ServerProbe.RenderInfo();
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            var context = (sender as HttpApplication).Context;
            var exception = Server.GetLastError().GetBaseException();
            var known = this.IsKnownException(exception);
            //已知异常非正式环境也同样记录
            if (known && SystemConfig.Settings.VersionFlag != "Release")
                _log.Warn(exception);
            else if (!known)
                _log.Error(exception);

            //ajax请求处理
            var header = context.Request.Headers["X-Requested-With"];
            if (!string.IsNullOrEmpty(header) && header.ToLower().IndexOf("xmlhttp") >= 0)
            {
                Server.ClearError();
                Response.StatusCode = 400;
                Response.Clear();
                Response.Write(exception.Message);
                Response.End();
                return;
            }
            //自定义错误处理
            this.OnError(exception);
        }

        protected virtual IEnumerable<IHttpModule> DeclareHttpModules()
        {
            return _empty;
        }
        protected virtual bool IsKnownException(Exception e)
        {
            return this._exceptionSystem.IsKnown(e) || this.IsKnownHttpException(e);
        }
        protected virtual void OnError(Exception e)
        {
            Server.ClearError();

            //为避免依赖才写在此处
            var error = @"
<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <title>发生错误，请您稍后再试</title>
</head>
<body>
    <div style='height:100px'>&nbsp;</div>
    <div style='background-color:#F2DEDE; border: 1px #EED3D7 solid; padding: 20px;
        margin: auto; width: 700px;'>
        <h2 style='font-size:13px; color: #B94A48'>
            抱歉，您访问的网页发生了错误，请您稍后再试 ^_^</h2>
        <div style='font-size:12px;line-height:20px;'>
            {0}
        </div>
    </div>
</body>
</html>";
            Response.Write(SystemConfig.Settings.VersionFlag != "Release"
                ? string.Format(error, this.ParseError(e))//详细堆栈信息
                : (this.IsKnownException(e)
                ? string.Format(error, e.Message)//已知异常信息
                : string.Format(error, string.Empty)));//未知异常
            Response.End();
        }
        private string ParseError(Exception e)
        {
            return string.Format(
                "【Message】{0}<br/>【Source】{1}<br/>【StackTrace】<br/>{2}<br/>【HelpLink】{3}<br/>"
                , (e.Message ?? "").Replace("\n", "<br/>")
                , e.Source
                , (e.StackTrace ?? "").Replace("\n", "<br/>")
                , e.HelpLink);
        }
        //声明部分http异常为已知异常
        private bool IsKnownHttpException(Exception e)
        {
            //主要为MVC的404异常
            return e is HttpException
                && (e.Message.IndexOf("could not be found or it does not implement IController") >= 0
                || e.Message.IndexOf("was not found or does not implement IController") >= 0
                || e.Message.IndexOf("was not found on controller") >= 0);
        }
    }
}