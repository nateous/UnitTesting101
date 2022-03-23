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

    #region hidden
    public class MyFatController
    {
        //[Route("/{id})]
        //public IActionResult Get(int id)
        public object GetStuff(int id)
        {

            return null;
        }
    }

    public class MyOverlyComplexRepository
    {
        public object GetItem(int id)
        {
            return new object();
        }
    }
    #endregion

    public class MyAnemicService
    {
        private readonly MyOverlyComplexRepository _repo = new MyOverlyComplexRepository();

        public object GetItem(int id)
        {
            return _repo.GetItem(id);
        }
    }
}
