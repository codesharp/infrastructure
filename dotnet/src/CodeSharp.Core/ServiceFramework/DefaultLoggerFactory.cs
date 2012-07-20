using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeSharp.ServiceFramework.Interfaces;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// Common.Logging LoggerFactory
    /// </summary>
    public class DefaultLoggerFactory : Interfaces.ILoggerFactory
    {
        public DefaultLoggerFactory()
        {
            var properties = new System.Collections.Specialized.NameValueCollection();
            properties["showDateTime"] = "true";
            Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter(properties);  
        }

        #region ILoggerFactory Members

        public ILog Create(string name)
        {
            return new DefaultLogger(Common.Logging.LogManager.GetLogger(name));
        }

        public ILog Create(Type type)
        {
            return new DefaultLogger(Common.Logging.LogManager.GetLogger(type));
        }

        #endregion
    }
}
