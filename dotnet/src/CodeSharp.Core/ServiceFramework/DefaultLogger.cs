using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.ServiceFramework.Interfaces;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// Common.Logging提供的日志记录器
    /// </summary>
    public class DefaultLogger : ILog
    {
        private Common.Logging.ILog _log;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        public DefaultLogger(Common.Logging.ILog log)
        {
            this._log = log;
        }

        #region ILoger Members

        public bool IsDebugEnabled
        {
            get { return this._log.IsDebugEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return this._log.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return this._log.IsFatalEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return this._log.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return this._log.IsWarnEnabled; }
        }

        public void Info(object message)
        {
            this._log.Info(message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            this._log.InfoFormat(format, args);
        }

        public void Info(object message, Exception exception)
        {
            this._log.Info(message, exception);
        }

        public void Error(object message)
        {
            this._log.Error(message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            this._log.ErrorFormat(format, args);
        }

        public void Error(object message, Exception exception)
        {
            this._log.Error(message, exception);
        }

        public void Warn(object message)
        {
            this._log.Warn(message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            this._log.WarnFormat(format, args);
        }

        public void Warn(object message, Exception exception)
        {
            this._log.Warn(message, exception);
        }

        public void Debug(object message)
        {
            this._log.Debug(message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            this._log.DebugFormat(format, args);
        }

        public void Debug(object message, Exception exception)
        {
            this._log.Debug(message, exception);
        }

        public void Fatal(object message)
        {
            this._log.Fatal(message);
        }

        public void FatalFormat(string format, params object[] args)
        {
            this._log.FatalFormat(format, args);
        }

        public void Fatal(object message, Exception exception)
        {
            this._log.Fatal(message, exception);
        }

        #endregion
    }
}