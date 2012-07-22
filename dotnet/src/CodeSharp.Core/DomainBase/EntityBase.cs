//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.Core.DomainBase
{
    /// <summary>实体抽象基类
    /// <remarks>
    /// 根据ID判断是否Transient
    /// 非Transient的对象只要ID相等就认为对象相等
    /// </remarks>
    /// </summary>
    /// <typeparam name="T">主键标识类型</typeparam>
    public abstract class EntityBase<T> : IEntity
    {
        /// <summary>获取标识
        /// </summary> 
        public virtual T ID { get; protected set; }

        /// <summary>重载实体相等判断，一律为值类型判断
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var m = obj as EntityBase<T>;
            if (m == null)
                return base.Equals(obj);
            if (this.IsTransient() && m.IsTransient())
                return ReferenceEquals(this, m);
            return this.ID.Equals(m.ID);
        }

        private int? _transientHashCode;
        /// <summary>重载GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (this._transientHashCode.HasValue)
                return _transientHashCode.Value;
            if (this.IsTransient())
            {
                this._transientHashCode = base.GetHashCode();
                return this._transientHashCode.Value;
            }
            return this.ID.GetHashCode();
        }
        /// <summary>判断实体是否是Transient瞬时状态（未持久化）
        /// </summary>
        /// <returns></returns>
        public virtual bool IsTransient()
        {
            return this.ID == null || this.ID.Equals(default(T));
        }
        /// <summary>重载实体相等判断
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator ==(EntityBase<T> first, EntityBase<T> second)
        {
            return Equals(first, second);
        }
        /// <summary>重载实体相等判断
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator !=(EntityBase<T> first, EntityBase<T> second)
        {
            return !Equals(first, second);
        }
    }
    /// <summary>实体抽象基类
    /// <remarks>标识类型为object</remarks>
    /// </summary>
    public abstract class EntityBase : EntityBase<object>
    {

    }
}