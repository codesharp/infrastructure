//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Runtime.Serialization;

namespace CodeSharp.Framework
{
    /// <summary>定义基本异常系统
    /// 所有已知的Web(或外观层)异常均需要对用户进行友好反馈
    /// 包含：参数检查，违反业务规则的操作等
    /// 除了已知异常外，其余异常都作为Error抛出，并发送邮件
    /// PS:强类型异常限制较多，异常传播时会给远程应用带来依赖问题，建议直接使用Exception
    /// </summary>
    public interface IExceptionSystem
    {
        /// <summary>是否是已知异常
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        bool IsKnown(Exception e);
    }
    /// <summary>默认的异常体系声明
    /// </summary>
    public class DefaultExceptionSystem : IExceptionSystem
    {
        #region IExceptionSystem Members

        public virtual bool IsKnown(Exception e)
        {
            return e.Message.StartsWith("已知异常");
        }

        #endregion
    }
}