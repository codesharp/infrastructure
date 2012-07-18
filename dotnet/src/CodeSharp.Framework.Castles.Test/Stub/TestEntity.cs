using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace CodeSharp.Framework.Castles.Test.Stub
{
    public class TestEntity : CodeSharp.Core.DomainBase.EntityBase<int>
    {

    }

    public class TestEntityMap : ClassMap<TestEntity>
    {
        public TestEntityMap()
        {
            Table("TestEntity");
            Id(m => m.ID);
        }
    }
}
