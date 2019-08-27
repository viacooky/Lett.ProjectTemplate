using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dotNetCore.Lib.MSTest
{
    [TestClass]
    public class UnitTest1
    {
        private HelloWorld _helloWorld = new HelloWorld();
        [TestMethod]
        public void Say_Test()
        {
            _helloWorld.SayHello();
        }
    }
}
