using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.ServiceFramework.Interfaces
{
    /// <summary>
    /// 提供序列化支持
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// 按参数的类型协定进行序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string Serialize(object obj);
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type">期望（协定）类型</param>
        /// <returns>根据期望类型和实际类型返回协定类型</returns>
        string Serialize(object obj, Type type);
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        T Deserialize<T>(string input);
        /// <summary>
        /// 反序列化
        /// <remarks>多态支持</remarks>
        /// </summary>
        /// <param name="target">期望类型</param>
        /// <param name="input">序列化字符串</param>
        /// <returns></returns>
        object Deserialize(Type target, string input);
    }
}