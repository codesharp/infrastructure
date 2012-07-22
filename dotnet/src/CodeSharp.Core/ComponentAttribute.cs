//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeSharp.Core
{
    /// <summary>将类声明为组件
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute
    {
        /// <summary>组件生命周期
        /// </summary>
        public LifeStyle LifeStyle { get; set; }
        /// <summary>声明组件生命周期
        /// </summary>
        /// <param name="lifeStyle"></param>
        public ComponentAttribute(LifeStyle lifeStyle)
        {
            this.LifeStyle = lifeStyle;
        }
        /// <summary>声明为组件
        /// <remarks>生命周期默认为瞬态Transient</remarks>
        /// </summary>
        public ComponentAttribute() : this(LifeStyle.Transient) { }
    }
    /// <summary>组件生命周期
    /// </summary>
    public enum LifeStyle
    {
        /// <summary>瞬态
        /// </summary>
        Transient = 0,
        /// <summary>单例
        /// </summary>
        Singleton
    }
}