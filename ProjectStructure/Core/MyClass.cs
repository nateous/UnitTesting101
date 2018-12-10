using System;

namespace Core
{
    public class MyClass
    {
        public int MyMethod(int a, int b)
        {
            return a + b;
        }

        public string FormatName(string first, string middle, string last)
        {
            return $"{first} {middle} {last}";
        }
    }
}
