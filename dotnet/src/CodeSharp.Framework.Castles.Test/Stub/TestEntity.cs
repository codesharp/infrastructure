using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using CodeSharp.Core.RepositoryFramework;
using CodeSharp.Core.Castles;
using CodeSharp.Core.DomainBase;

namespace CodeSharp.Framework.Castles.Test
{
    public class TestEntity : EntityBase<int>, IAggregateRoot { }
    public class TestEntityMap : ClassMap<TestEntity>
    {
        public TestEntityMap()
        {
            Table("TestEntity");
            Id(m => m.ID);
        }
    }

    public interface ITestService { }
    public class TestService : ITestService
    {
        private static ITestRepository _repository;
        static TestService()
        {
            _repository = RepositoryFactory.GetRepository<ITestRepository, int, TestEntity>();
        }
    }

    public interface ITestRepository : IRepository<int, TestEntity> { }
    public class TestRepository : NHibernateRepositoryBase<int, TestEntity>, ITestRepository { }
}