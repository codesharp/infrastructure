//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Diagnostics;
using Castle.Core.Interceptor;
using System.Collections;
using Castle.DynamicProxy;
using CodeSharp.Core.Services;

namespace CodeSharp.Framework.Castles
{
    /// <summary>����������
    /// <remarks>
    /// ������
    /// ��ʱͳ��
    /// ���ܼ���
    /// ������־
    /// �쳣ϵͳ
    /// </remarks>
    /// </summary>
    public class ServiceInterceptor : IInterceptor
    {
        private IExceptionSystem _exceptionSystem;
        private ILoggerFactory _loggerFactory;

        //HACK:��������ֹ�������ӹ�����������Ӱ������㣬���ܻ��������������ע����Ϊ�Ĳ���
        public ServiceInterceptor(ILoggerFactory fatory, IExceptionSystem es)
        {
            this._loggerFactory = fatory;
            this._exceptionSystem = es;
        }

        #region IInterceptor ��Ա
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
                    .WarnFormat("��ʱ������class={0}|method={1}|time={2}ms|arguments={3}|���ڽ��ڲ���������"
                    , invocation.Method.DeclaringType.FullName
                    , invocation.Method.Name
                    , time.TotalMilliseconds, arguments);
            }
        }
    }
}