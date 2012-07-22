//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using CodeSharp.Core.RepositoryFramework;
using CodeSharp.Core.Castles;
using CodeSharp.Core.DomainBase;
using Castle.Services.Transaction;

namespace CodeSharp.Framework.Castles.Test
{
    public class TestEntity : EntityBase<int>, IAggregateRoot
    {
        //私有setter在NH3.x默认行为改变，增加了更严谨的proxy validator
        //private setter on nh2.x
        //3.x must not lazy or use_proxy_validator=false
        public string Name { get; private set; }
        //public virtual string Name { get; private set; }

        //TODO:完成常用映射验证，避免升级问题

        //由于NH，必须有默认构造器
        protected TestEntity() { }
        public TestEntity(string name) { this.Name = name; }
    }
    public class TestEntityMap : ClassMap<TestEntity>
    {
        public TestEntityMap()
        {
            Table("TestEntity");
            Id(m => m.ID);
            Map(m => m.Name);
        }
    }

    public interface ITestService
    {
        void Create(TestEntity e);
        TestEntity Get(int id);
    }
    [Transactional]
    public class TestService : ITestService
    {
        private static ITestRepository _repository;
        static TestService()
        {
            _repository = RepositoryFactory.GetRepository<ITestRepository, int, TestEntity>();
        }

        #region ITestService Members
        [Transaction(TransactionMode.Requires)]
        void ITestService.Create(TestEntity e)
        {
            _repository.Add(e);
        }
        TestEntity ITestService.Get(int id)
        {
            return _repository.FindBy(id);
        }
        #endregion
    }

    public interface ITestRepository : IRepository<int, TestEntity> { }
    public class TestRepository : NHibernateRepositoryBase<int, TestEntity>, ITestRepository { }
}