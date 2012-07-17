//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.Core
{
    /// <summary>对ILog的decorator，可用于简化常见ILog整合进统一log的快速实现
    /// 如：class ZooKeeperLogger:Logger,ZooKeeperNet.ILog{}
    /// <remarks>可继承此类扩展，用于简化ILog抽象</remarks>
    /// </summary>
    public class LogWrapper : ILog
    {
        private ILog _log;
        public LogWrapper(ILog log)
        {
            this._log = log;
        }

        #region ILog Members

        public virtual bool IsDebugEnabled
        {
            get { return this._log.IsDebugEnabled; }
        }

        public virtual bool IsErrorEnabled
        {
            get { return this._log.IsErrorEnabled; }
        }

        public virtual bool IsFatalEnabled
        {
            get { return this._log.IsFatalEnabled; }
        }

        public virtual bool IsInfoEnabled
        {
            get { return this._log.IsInfoEnabled; }
        }

        public virtual bool IsWarnEnabled
        {
            get { return this._log.IsWarnEnabled; }
        }

        public virtual void Debug(object message)
        {
            this._log.Debug(message);
        }

        public virtual void DebugFormat(string format, params object[] args)
        {
            this._log.DebugFormat(format, args);
        }

        public virtual void Debug(object message, Exception exception)
        {
            this._log.Debug(message, exception);
        }

        public virtual void Info(object message)
        {
            this._log.Info(message);
        }

        public virtual void InfoFormat(string format, params object[] args)
        {
            this._log.InfoFormat(format, args);
        }

        public virtual void Info(object message, Exception exception)
        {
            this._log.Info(message, exception);
        }

        public virtual void Warn(object message)
        {
            this._log.Warn(message);
        }

        public virtual void WarnFormat(string format, params object[] args)
        {
            this._log.WarnFormat(format, args);
        }

        public virtual void Warn(object message, Exception exception)
        {
            this._log.Warn(message, exception);
        }

        public virtual void Error(object message)
        {
            this._log.Error(message);
        }

        public virtual void ErrorFormat(string format, params object[] args)
        {
            this._log.ErrorFormat(format, args);
        }

        public virtual void Error(object message, Exception exception)
        {
            this._log.Error(message, exception);
        }

        public virtual void Fatal(object message)
        {
            this._log.Fatal(message);
        }

        public virtual void FatalFormat(string format, params object[] args)
        {
            this._log.FatalFormat(format, args);
        }

        public virtual void Fatal(object message, Exception exception)
        {
            this._log.Fatal(message, exception);
        }

        #endregion
    }
}
