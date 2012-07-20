using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;

namespace CodeSharp.ServiceFramework
{
    //TODO:支持匿名对象

    /// <summary>
    /// 内置的默认序列化器
    /// <remarks>
    /// 基于DataContractJsonSerializer
    /// 支持JSON，XML序列化，自定义转换器，多态
    /// </remarks>
    /// </summary>
    public class DefaultJSONSerializer : Interfaces.ISerializer
    { 
        /// <summary>
        /// 按参数的类型协定进行JSON序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string Serialize(object obj)
        {
            return obj == null ? "null" : this.Serialize(obj, obj.GetType());
        }
        /// <summary>
        /// JSON序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type">期望（协定）类型，该类型必须是参数obj的已知类型，即KnownType属性的协定</param>
        /// <returns>根据期望类型和实际类型返回协定类型</returns>
        public string Serialize(object obj, Type type)
        {
            if (type == null || obj == null) return "null";
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            {
                new DataContractJsonSerializer(type).WriteObject(stream, obj);
                stream.Position = 0;
                return reader.ReadToEnd();
            }
        }
        /// <summary>
        /// JSON反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public T Deserialize<T>(string input)
        {
            return (T)this.Deserialize(typeof(T), input);
        }
        /// <summary>
        /// JSON反序列化
        /// <remarks>多态支持</remarks>
        /// </summary>
        /// <param name="target">期望类型</param>
        /// <param name="input">json字符串</param>
        /// <returns></returns>
        public object Deserialize(Type target, string input)
        {
            //if (target.Equals(typeof(string)))
            //    return input;
            if (input == null)
                return null;
            try
            {
                //HACK:Unicode会导致部分中文字符允许被非法反序列化，如：“李”=>“Ng”，而合法的情况应该是“"李"”->“李”
                //using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(input)))
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
                    return new DataContractJsonSerializer(target).ReadObject(stream);
            }
            catch (Exception e)
            {
                throw new Exceptions.ServiceException(string.Format("反序列化为类型{0}时失败：{1}，原始字符串为{2}"
                    , target.FullName
                    , e.Message
                    , input)
                    , e);
            }
        }
    }
}