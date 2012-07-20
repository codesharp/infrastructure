using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.ServiceFramework.Exceptions
{
    //TODO:异常代码
    /// <summary>
    /// 公开传播的异常代码
    /// </summary>
    public enum ExceptionCode
    {
        /// <summary>
        /// 未知异常
        /// </summary>
        Unknown = 400,
        /// <summary>
        /// 服务未找到 
        /// </summary>
        ServiceNotFound = 404,
        /// <summary>
        /// 方法未找到
        /// </summary>
        MethodNotFound = 404,
        /// <summary>
        /// 没有权限
        /// </summary>
        NoPermission = 401,
        /// <summary>
        /// 参数错误
        /// </summary>
        ArgumentError = 400
    }
}
