using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.ServiceFramework
{
    /// <summary>
    /// 元组
    /// <remarks>4.0后将采用System.Tuple</remarks>
    /// </summary>
    public abstract class Tuple
    {
        private IDictionary<Type, object> _dic;

        private Tuple()
        {
            this._dic = new Dictionary<Type, object>();
        }

        protected Tuple(params Type[] types)
            : this()
        {
            if (types != null)
                foreach (var t in types)
                    this._dic.Add(t, null);
        }
        /// <summary>
        /// 获取或设置
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual object this[int index]
        {
            get
            {
                return this._dic.ElementAt(index).Value;
            }
            set
            {
                for (var i = 0; i < this._dic.Count; i++)
                    if (i == index)
                        this._dic[this._dic.Keys.ElementAt(i)] = value;
            }
        }
    }
    /// <summary>
    /// 元组
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public class Tuple<T1, T2, T3> : Tuple
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public Tuple() : base(typeof(T1), typeof(T2), typeof(T3)) { }

        /// <summary>
        /// 获取或设置首个元素值
        /// </summary>
        /// <returns></returns>
        public T1 V1
        {
            get { return (T1)this[0]; }
            set { this[0] = value; }
        }
        /// <summary>
        /// 获取或设置第二个元素值
        /// </summary>
        /// <returns></returns>
        public T2 V2
        {
            get { return (T2)this[1]; }
            set { this[1] = value; }
        }
        /// <summary>
        /// 获取或设置第三个元素值
        /// </summary>
        /// <returns></returns>
        public T3 V3
        {
            get { return (T3)this[2]; }
            set { this[2] = value; }
        }
    }
}
