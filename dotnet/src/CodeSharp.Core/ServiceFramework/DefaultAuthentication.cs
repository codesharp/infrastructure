using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// 提供简单身份验证
    /// <remarks>32bit MD5</remarks>
    /// </summary>
    public class DefaultAuthentication : Interfaces.IAuthentication
    {
        #region IAuthentication Members

        public virtual bool Validate(ServiceCall call)
        {
            var id = call.Identity;

            if (id == null)
                throw new Exceptions.ServiceException("Identity不能为空");
            return !string.IsNullOrEmpty(id.AuthKey)
                && !string.IsNullOrEmpty(id.Source)
                && System.Web.Security.FormsAuthentication
                .HashPasswordForStoringInConfigFile(id.Source, "MD5")
                .Equals(id.AuthKey, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}
