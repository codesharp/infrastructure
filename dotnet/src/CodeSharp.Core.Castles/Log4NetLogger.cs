//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.Core.Castles
{
    /// <summary>
    /// Log4Net提供的日志记录器
    /// 支持注入上下文以便于跟踪问题
    /// <remarks>若需要扩展可继承此类</remarks>
    /// </summary>
    public class Log4NetLogger : CodeSharp.Core.ILog
    {
        protected log4net.ILog _log;
        protected Func<IDictionary<string, object>> _properties;
        protected Func<IDictionary<string, object>> _propertiesWhenError;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="log"></param>
        public Log4NetLogger(log4net.ILog log) : this(log, null, null) { }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="log"></param>
        /// <param name="properties">
        /// 可用于传递系统或环境变量等，可以作为property输出在日志内容中，layout格式如：%property{name}
        /// Tip：利用ToString()重载可以实现Active Property，但需要注意此特性对性能的消耗，建议合理配合使用IsDebugEnabled等属性
        /// http://logging.apache.org/log4net/release/manual/contexts.html
        /// 不需要则留空
        /// </param>
        /// <param name="propertiesWhenError">在Error以及Fatal级别时使用的变量集合，不需要则留空</param>
        public Log4NetLogger(log4net.ILog log
            , Func<IDictionary<string, object>> properties
            , Func<IDictionary<string, object>> propertiesWhenError)
        {
            this._log = log;
            this._properties = properties;
            this._propertiesWhenError = propertiesWhenError;
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
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._properties))
                this._log.Debug(message);
        }
        public virtual void DebugFormat(string format, params object[] args)
        {
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._properties))
                this._log.DebugFormat(format, args);
        }
        public virtual void Debug(object message, Exception exception)
        {
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._properties))
                this._log.Debug(message, exception);
        }

        public virtual void Info(object message)
        {
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._properties))
                this._log.Info(message);
        }
        public virtual void InfoFormat(string format, params object[] args)
        {
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._properties))
                this._log.InfoFormat(format, args);
        }
        public virtual void Info(object message, Exception exception)
        {
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._properties))
                this._log.Info(message, exception);
        }

        public virtual void Warn(object message)
        {
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._properties))
                this._log.Warn(message);
        }
        public virtual void WarnFormat(string format, params object[] args)
        {
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._properties))
                this._log.WarnFormat(format, args);
        }
        public virtual void Warn(object message, Exception exception)
        {
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._properties))
                this._log.Warn(message, exception);
        }

        public virtual void Error(object message)
        {
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._propertiesWhenError))
                this._log.Error(message);
        }
        public virtual void ErrorFormat(string format, params object[] args)
        {
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._propertiesWhenError))
                this._log.ErrorFormat(format, args);
        }
        public virtual void Error(object message, Exception exception)
        {
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._propertiesWhenError))
                this._log.Error(message, exception);
        }

        public virtual void Fatal(object message)
        {
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._propertiesWhenError))
                this._log.Fatal(message);
        }
        public virtual void FatalFormat(string format, params object[] args)
        {
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._propertiesWhenError))
                this._log.FatalFormat(format, args);
        }
        public virtual void Fatal(object message, Exception exception)
        {
            using (new AutoPopFrame(log4net.LogicalThreadContext.Stacks, this._propertiesWhenError))
                this._log.Fatal(message, exception);
        }
        #endregion

        //TODO:由于是基础组件，应为AutoPopFrame使用Pool设计避免大量对象创建
        //注入和释放上下文变量
        class AutoPopFrame : IDisposable
        {
            private log4net.Util.ThreadContextStacks _stacks;
            private IDictionary<string, object> _properties;
            public AutoPopFrame(log4net.Util.ThreadContextStacks stacks, Func<IDictionary<string, object>> properties)
            {
                this._stacks = stacks;
                this._properties = properties != null ? properties() : null;

                if (this._properties == null) return;

                //push
                try
                {
                    foreach (var i in this._properties)
                        this._stacks[i.Key].Push(i.Value.ToString());
                }
                catch { }//TODO:将内部异常加入到内部log以便跟踪
            }

            #region IDisposable Members

            public void Dispose()
            {
                if (this._properties == null) return;

                //pop
                try
                {
                    foreach (var i in this._properties)
                        this._stacks[i.Key].Pop();
                }
                catch { }//TODO:将内部异常加入到内部log以便跟踪
            }

            #endregion
        }
    }
}