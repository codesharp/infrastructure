using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// 服务访问身份
    /// </summary>
    [Serializable]
    [DataContract]
    public class Identity
    {
        /// <summary>
        /// 密钥
        /// </summary>
        [DataMember]
        public string AuthKey { get; set; }
        /// <summary>
        /// 访问来源
        /// </summary>
        [DataMember]
        public string Source { get; set; }
    }
}