using InputRecorder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class TestInputArgs
    {

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {

        }

        [TestInitialize]
        public void Setup()
        {

        }

        [TestMethod]
        public void TestConstructorInput()
        {
            var input = new Input();
            var args = new InputArgs(input);

            Assert.AreEqual(input, args.Input);
        }

        [TestMethod]
        public void TestCreate()
        {
            var input = new Input();
            var args = new InputArgs(input);
            var cargs = InputArgs.Create(input);


            Assert.AreEqual(input, cargs.Input);
            Assert.AreEqual(args, cargs);
        }
    }
}
