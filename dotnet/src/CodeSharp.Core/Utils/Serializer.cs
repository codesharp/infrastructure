using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace CodeSharp.Core.Utils
{
    /// <summary>序列化器
    /// <remarks>
    /// 支持JSON，XML序列化，自定义转换器
    /// 使用老式的JavaScriptSerializer实现，与最新DataContract有区别
    /// </remarks>
    /// </summary>
    public sealed class Serializer
    {
        private JavaScriptSerializer _javaScriptSerializer= new JavaScriptSerializer();
        /// <summary>
        /// 配置JSON序列化器
        /// </summary>
        /// <param name="converters">注册自定义转换器</param>
        /// <returns></returns>
        public Serializer JavaScriptSerializer(params JavaScriptConverter[] converters)
        {
            _javaScriptSerializer = new JavaScriptSerializer();
            if (converters != null)
                this._javaScriptSerializer.RegisterConverters(converters);
            return this;
        }
        /// <summary>
        /// JSON序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string JsonSerialize(object obj)
        {
            return this._javaScriptSerializer.Serialize(obj);
        }
        /// <summary>
        /// JSON反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public T JsonDeserialize<T>(string input)
        {
            return this._javaScriptSerializer.Deserialize<T>(input);
        }
        /// <summary>
        /// JSON反序列化
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public object JsonDeserialize(string input)
        {
            return this._javaScriptSerializer.DeserializeObject(input);
        }
        /// <summary>
        /// XML序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string XmlSerialize(object obj)
        {
            throw new NotImplementedException();
        }
    }
}