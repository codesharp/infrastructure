using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.ServiceFramework.Interfaces
{
    /// <summary>
    /// 提供服务描述文档的生成
    /// </summary>
    public interface IServiceDescriptionRender
    {
        /// <summary>
        /// 生成服务描述文本
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        string Render(ServiceConfig service);
    } 
}
