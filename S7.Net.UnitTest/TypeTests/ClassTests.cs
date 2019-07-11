using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7.Net.Types;
using S7.Net.UnitTest.Helpers;

namespace S7.Net.UnitTest.TypeTests
{
    [TestClass]
    public class ClassTests
    {
        [TestMethod]
        public void GetClassSizeTest()
        {
            Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(1, 1)), 6);
            Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(2, 15)), 6);
            Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(2, 16)), 6);
            Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(2, 17)), 8);
            Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(3, 15)), 8);
            Assert.AreEqual(Class.GetClassSize(new TestClassUnevenSize(3, 17)), 10);
        }
    }
}
