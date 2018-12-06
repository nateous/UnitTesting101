using System;
using NUnit.Framework;

namespace Core.Tests
{
    public class MyMethodShould
    {
        [Test]
        public void Return5Given2And3()
        {
            var myClass = new MyClass();
            var result = myClass.MyMethod(2, 3);
            Assert.AreEqual(5, result);
        }
    }
}
