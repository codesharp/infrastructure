//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CodeSharp.Core.Services;
using System.Reflection;

namespace CodeSharp.Framework.Castles.Test
{
    /// <summary>对关键兼容点的验证
    /// </summary>
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class MonoCompatibleTest
    {
        //issue:https://github.com/codesharp/infrastructure/issues/15
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Test()
        {
            Assembly sysData = Assembly.Load("System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            var sqlCmdSetType = sysData.GetType("System.Data.SqlClient.SqlCommandSet");
            Assert.IsFalse(sqlCmdSetType != null, "Could not find SqlCommandSet!");
        }
    }
}
