using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StPete139_Calendar_Tests
{
    [TestClass]
    public class SmokeTests
    {
        [TestMethod]
        public void Smoke()
        {
            Console.WriteLine("Smoke Test");
           Assert.AreEqual(2, 1 + 1);
        }
    }
}
