using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// 默认的服务文档描述形式
    /// </summary>
    public class DefaultServiceDescriptionRender : Interfaces.IServiceDescriptionRender
    {
        #region IServiceDescriptionRender Members
        public virtual string Render(ServiceConfig service)
        {
            return this.BuildDescription(service);
        }
        #endregion

        private string BuildDescription(ServiceConfig service)
        {
            var type = service.Type;
            //排除object继承的方法
            var list = from m in type.GetMethods()
                       where !new string[] { "GetHashCode", "GetType", "Equals", "ToString" }.Contains(m.Name)
                       select string.Format("<div><h3>【方法】{0}</h3><br/>【参数】<br/>{1}<br/>【返回】{2}</div>"
                       , m.Name
                       , this.BuildParameters(m)
                       , m.ReturnType.FullName);
            return "<h2>"
                + service.Name
                + "服务定义</h2>"
                + string.Join("<br/>", list.ToArray());
        }
        private string BuildParameters(MethodInfo method)
        {
            return string.Join("<br/>", method.GetParameters().Select(o =>
                string.Format("类型={0} | 参数名={1}", o.ParameterType.FullName, o.Name)).ToArray());
        }
    }
}