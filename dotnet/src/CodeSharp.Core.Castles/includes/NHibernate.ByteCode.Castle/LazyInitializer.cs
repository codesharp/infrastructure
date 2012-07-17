﻿using System;
using System.Reflection;
using Castle.Core.Interceptor;
using NHibernate.Proxy;
using NHibernate.Proxy.Poco;
using NHibernate.Type;
using NHibernate.Engine;
using Castle.DynamicProxy;

namespace NHibernate.ByteCode.Castle
{
    /// <summary>
    /// A <see cref="ILazyInitializer"/> for use with the Castle Dynamic Class Generator.
    /// </summary>
    [Serializable]
    [CLSCompliant(false)]
    public class LazyInitializer : BasicLazyInitializer, global::Castle.DynamicProxy.IInterceptor
    {
        private static readonly MethodInfo Exception_InternalPreserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);

        #region Instance

        public bool _constructed;

        /// <summary>
        /// Initializes a new <see cref="LazyInitializer"/> object.
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="persistentClass">The Class to Proxy.</param>
        /// <param name="id">The Id of the Object we are Proxying.</param>
        /// <param name="getIdentifierMethod"></param>
        /// <param name="setIdentifierMethod"></param>
        /// <param name="componentIdType"></param>
        /// <param name="session">The ISession this Proxy is in.</param>
        public LazyInitializer(string entityName, System.Type persistentClass, object id,
                                     MethodInfo getIdentifierMethod, MethodInfo setIdentifierMethod,
                                     IAbstractComponentType componentIdType, ISessionImplementor session)
            : base(entityName, persistentClass, id, getIdentifierMethod, setIdentifierMethod, componentIdType, session)
        {
        }

        /// <summary>
        /// Invoke the actual Property/Method using the Proxy or instantiate the actual
        /// object and use it when the Proxy can't handle the method.
        /// </summary>
        /// <param name="invocation">The <see cref="IInvocation"/> from the generated Castle.DynamicProxy.</param>
        public virtual void Intercept(IInvocation invocation)
        {
            try
            {
                if (_constructed)
                {
                    // let the generic LazyInitializer figure out if this can be handled
                    // with the proxy or if the real class needs to be initialized
                    invocation.ReturnValue = base.Invoke(invocation.Method, invocation.Arguments, invocation.Proxy);

                    // the base LazyInitializer could not handle it so we need to Invoke
                    // the method/property against the real class
                    if (invocation.ReturnValue == InvokeImplementation)
                    {
                        invocation.ReturnValue = invocation.Method.Invoke(GetImplementation(), invocation.Arguments);
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    // TODO: Find out equivalent to CGLIB's 'method.invokeSuper'.
                    return;
                }
            }
            catch (TargetInvocationException tie)
            {
                // Propagate the inner exception so that the proxy throws the same exception as
                // the real object would
                Exception_InternalPreserveStackTrace.Invoke(tie.InnerException, new Object[] { });
                throw tie.InnerException;
            }
        }

        #endregion
    }
}
