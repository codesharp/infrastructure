﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.ServiceFramework.Interfaces
{
    /// <summary> 
    /// 提供日志记录器的创建
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// 创建Log
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ILog Create(string name);
        /// <summary>
        /// 创建Log
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        ILog Create(Type type);
    }
}
