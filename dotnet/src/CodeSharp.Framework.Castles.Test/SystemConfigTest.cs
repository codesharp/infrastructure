using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CodeSharp.Framework.Castles.Test
{
    /// <summary>初始化测试
    /// </summary>
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class SystemConfigTest
    {
        [Test]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void Castle()
        {
            SystemConfig.ConfigFilesAssemblyName = "CodeSharp.Framework.Castles.Test";
            SystemConfig.Configure("ConfigFiles").Castle();
        }
    }
}
