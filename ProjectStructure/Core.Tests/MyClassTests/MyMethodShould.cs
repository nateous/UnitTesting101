using System;
using NUnit.Framework;

namespace Core.Tests.MyClassTests
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

        [TestCase(0, 0, 0)]
        [TestCase(1, 0, 1)]
        [TestCase(0, 1, 1)]
        [TestCase(1, 1, 2)]
        [TestCase(-5, 5, 0)]
        [TestCase(-4, -3, -7)]
        public void ReturnCorrectValueGivenTwoInputs(int a, int b, int expected)
        {
            var myClass = new MyClass();
            var result = myClass.MyMethod(a, b);
            Assert.AreEqual(expected, result);
        }
    }
}
