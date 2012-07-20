using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.ServiceFramework.Interfaces
{
    /// <summary>
    /// 用于提供身份/权限验证支持
    /// </summary>
    public interface IAuthentication
    {
        /// <summary>
        /// 检查身份/权限是否有效
        /// </summary>
        /// <param name="call"></param>
        /// <returns></returns>
        bool Validate(ServiceCall call);
    }
}