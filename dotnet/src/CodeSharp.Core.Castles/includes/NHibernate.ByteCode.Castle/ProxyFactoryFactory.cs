using NHibernate.Bytecode;
using NHibernate.Proxy;

namespace NHibernate.ByteCode.Castle
{
    public class ProxyFactoryFactory : IProxyFactoryFactory
    {
        #region IProxyFactoryFactory Members

        public IProxyFactory BuildProxyFactory()
        {
            return new ProxyFactory();
        }

        public IProxyValidator ProxyValidator
        {
            get { return new DynProxyTypeValidator(); }
        }

        #endregion


        //NHibernate3新增成员
        //http://mausch.github.com/nhibernate-3.2.0GA/html/d92f91a7-5663-f481-1bd7-3625741962c2.htm
        #region IProxyFactoryFactory Members

        public bool IsInstrumented(System.Type entityClass)
        {
            return true;
        }

        public bool IsProxy(object entity)
        {
            return entity is INHibernateProxy;
        }

        #endregion
    }
}