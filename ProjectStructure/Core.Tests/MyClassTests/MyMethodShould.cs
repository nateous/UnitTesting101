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
            Assert.AreEqual(50, result, "So 2 plus 3 is supposed to be 5, because that is how teh maths work.");
        }

        [TestCase(0, 0, 0)]
        [TestCase(1, 0, 2, "1 plus 0 really aught to be 1 you know.")]
        [TestCase(0, 1, 1)]
        [TestCase(1, 1, 2)]
        [TestCase(-5, 5, 1)]
        [TestCase(-4, -3, -7)]
        public void ReturnCorrectValueGivenTwoInputs(int a, int b, int expected, string message = "Clearly your math skills need work.")
        {
            var myClass = new MyClass();
            var result = myClass.MyMethod(a, b);
            Assert.AreEqual(expected, result, message);
        }

        //show test method snippet

        //show test cases snippet
    }
}
