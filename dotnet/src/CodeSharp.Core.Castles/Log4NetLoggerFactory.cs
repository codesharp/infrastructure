//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.Core.Castles
{
    /// <summary>
    /// Log4NetLoggerFactory
    /// </summary>
    public class Log4NetLoggerFactory : CodeSharp.Core.Services.ILoggerFactory
    {
        private Func<IDictionary<string, object>> _properties;
        private Func<IDictionary<string, object>> _propertiesWhenError;

        /// <summary>
        /// 初始化
        /// </summary>
        public Log4NetLoggerFactory() { }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="properties">用于设置log4net context</param>
        /// <param name="propertiesWhenError">用于设置发生错误时的log4net context，面向Error以及以上级别</param>
        public Log4NetLoggerFactory(Func<IDictionary<string, object>> properties, Func<IDictionary<string, object>> propertiesWhenError)
        {
            this._properties = properties;
            this._propertiesWhenError = propertiesWhenError;
        }

        #region ILoggerFactory Members
        /// <summary>
        /// 创建指定名称的logger实例
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ILog Create(string name)
        {
            return new Log4NetLogger(log4net.LogManager.GetLogger(name), this._properties, this._propertiesWhenError);
        }
        /// <summary>
        /// 依据类型名称创建logger实例
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ILog Create(Type type)
        {
            return new Log4NetLogger(log4net.LogManager.GetLogger(type), this._properties, this._propertiesWhenError);
        }
        #endregion
    }
}