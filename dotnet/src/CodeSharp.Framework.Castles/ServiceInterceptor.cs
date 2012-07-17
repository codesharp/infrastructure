//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Diagnostics;
using Castle.Core.Interceptor;
using System.Collections;
using Castle.DynamicProxy;
using CodeSharp.Core.Services;

namespace CodeSharp.Framework.Castles
{
    /// <summary>服务拦截器
    /// <remarks>
    /// 包含：
    /// 耗时统计
    /// 性能计数
    /// 错误日志
    /// 异常系统
    /// </remarks>
    /// </summary>
    public class ServiceInterceptor : IInterceptor
    {
        private IExceptionSystem _exceptionSystem;
        private ILoggerFactory _loggerFactory;

        //HACK:拦截器禁止随意增加构造器参数，影响面过广，可能会造成其他层次组件注入行为的差异
        public ServiceInterceptor(ILoggerFactory fatory, IExceptionSystem es)
        {
            this._loggerFactory = fatory;
            this._exceptionSystem = es;
        }

        #region IInterceptor 成员
        public void Intercept(IInvocation invocation)
        {
            var watch = new Stopwatch();
            watch.Start();
            try { invocation.Proceed(); }
            catch (Exception e)
            {
                this.LogError(e, invocation);
                throw e;
            }
            finally
            {
                watch.Stop();
                this.MeasurePerformance(watch.Elapsed, invocation);
            }
        }
        #endregion

        private void LogError(Exception e, IInvocation invocation)
        {
            var log = this._loggerFactory.Create(invocation.Method.DeclaringType);

            var format = "Method'{0}' in class'{1}' happened an error.";
            if (this._exceptionSystem.IsKnown(e))
                log.Info(string.Format(format
                    , invocation.Method.Name
                    , invocation.Method.DeclaringType)
                    , e);
            else
                log.Error(string.Format(format
                    , invocation.Method.Name
                    , invocation.Method.DeclaringType)
                    , e);
        }
        private void MeasurePerformance(TimeSpan time, IInvocation invocation)
        {
            if (time >= new TimeSpan(0, 0, 0, 0, 500))
            {
                var arguments = string.Empty;
                foreach (object argument in invocation.Arguments)
                    arguments += argument ?? "Null" + "%____%";
                this._loggerFactory.Create("MethodPerformance")
                    .WarnFormat("耗时方法：class={0}|method={1}|time={2}ms|arguments={3}|请于近期查明并修正"
                    , invocation.Method.DeclaringType.FullName
                    , invocation.Method.Name
                    , time.TotalMilliseconds, arguments);
            }
        }
    }
}