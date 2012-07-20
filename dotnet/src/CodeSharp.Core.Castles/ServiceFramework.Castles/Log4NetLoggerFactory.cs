using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.ServiceFramework.Interfaces;

namespace CodeSharp.ServiceFramework.Castles
{
    /// <summary>
    /// Log4NetLoggerFactory
    /// </summary>
    public class Log4NetLoggerFactory : Interfaces.ILoggerFactory
    {
        #region ILoggerFactory Members

        public ILog Create(string name)
        {
            return new Log4NetLogger(log4net.LogManager.GetLogger(name));
        }

        public ILog Create(Type type)
        {
            return new Log4NetLogger(log4net.LogManager.GetLogger(type));
        }

        #endregion
    }
}
