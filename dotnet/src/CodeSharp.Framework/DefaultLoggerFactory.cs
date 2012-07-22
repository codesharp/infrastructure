//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.Framework
{
    /// <summary>
    /// 用于提供在未启用容器且没有使用log4net时的默认日志工厂
    /// <remarks>外部框架的各种ILoggerFactory均可挂接在此类下实现</remarks>
    /// </summary>
    public class DefaultLoggerFactory : CodeSharp.Core.DefaultLoggerFactory//ServiceFramework.Interfaces.ILoggerFactory
    {

    }
    /// <summary>
    /// 用于提供在未启用容器且没有使用log4net时的默认日志工厂
    /// <remarks>外部框架的各种ILog均可挂接在此类下实现</remarks>
    /// </summary>
    public class DefaultLogger : CodeSharp.Core.LogWrapper//ServiceFramework.Interfaces.ILog
    {
        public DefaultLogger(CodeSharp.Core.ILog log) : base(log) { }
    }
}