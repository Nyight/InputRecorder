using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InputRecorder;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;

namespace UnitTests
{
    [TestClass]
    public class TestInput
    {
        private static Array keyOptions;
        private static Random rand;

        private Keys _key;
        private int _delay;
        private Point _point;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            keyOptions = Enum.GetValues(typeof(Keys));
            rand = new Random();
        }

        [TestInitialize]
        public void Setup()
        {
            _key = (Keys)keyOptions.GetValue(rand.Next(keyOptions.Length));
            _delay = rand.Next(1000);
            _point = new Point(rand.Next(100, 100));
        }

        #region constructors
        [TestMethod]
        public void TestConstructorKey()
        {
            var input = new Input(_key, _delay);

            Assert.IsTrue(input.IsKey);
            Assert.AreEqual(_key, input.Key);
            Assert.AreEqual(_delay, input.DelayInMilliseconds);
        }

        [TestMethod]
        public void TestConstructorPoint()
        {
            var input = new Input(_point, _delay);

            Assert.IsFalse(input.IsKey);
            Assert.AreEqual(_point, input.ClickLocation);
            Assert.AreEqual(_delay, input.DelayInMilliseconds);
        }

        [TestMethod]
        public void TestConstructorXY()
        {
            var input = new Input(_point.X, _point.Y, _delay);

            Assert.IsFalse(input.IsKey);
            Assert.AreEqual(_point, input.ClickLocation);
            Assert.AreEqual(_delay, input.DelayInMilliseconds);
        }

        [TestMethod]
        public void TestConstructorDefault()
        {
            var input = new Input();

            Assert.IsFalse(input.IsKey);
            Assert.AreEqual(Keys.None, input.Key);
            Assert.AreEqual(Point.Empty, input.ClickLocation);
            Assert.AreEqual(0, input.DelayInMilliseconds);
        }
        #endregion

        #region clone
        [TestMethod]
        public void TestCloneKey()
        {
            var input = new Input(_key, _delay);
            var clone = input.Clone();

            Assert.AreEqual(input, clone);
        }

        [TestMethod]
        public void TestClonePoint()
        {
            var input = new Input(_point, _delay);
            var clone = input.Clone();

            Assert.AreEqual(input, clone);
        }

        [TestMethod]
        public void TestCloneXY()
        {
            var input = new Input(_point.X, _point.Y, _delay);
            var clone = input.Clone();

            Assert.AreEqual(input, clone);
        }

        [TestMethod]
        public void TestCloneDefault()
        {
            var input = new Input();
            var clone = input.Clone();

            Assert.AreEqual(input, clone);
        }
        #endregion

        #region json
        [TestMethod]
        public void TestJsonKey()
        {
            var input = new Input(_key, _delay);
            assertJson(input);
        }

        [TestMethod]
        public void TestJsonPoint()
        {
            var input = new Input(_point, _delay);
            assertJson(input);
        }

        [TestMethod]
        public void TestJsonXY()
        {
            var input = new Input(_point.X, _point.Y, _delay);
            assertJson(input);
        }

        private void assertJson(Input input)
        {
            var json = JsonConvert.SerializeObject(input);
            var testjson = json.ToLower();

            Assert.IsFalse(testjson.Contains("\"x\":"));
            Assert.IsFalse(testjson.Contains("\"y\":"));
            Assert.IsFalse(testjson.Contains("\"iskey\":"));

            Assert.IsTrue(testjson.Contains("\"key\":"));
            Assert.IsTrue(testjson.Contains("\"clicklocation\":"));
            Assert.IsTrue(testjson.Contains("\"delayinmilliseconds\":"));

            var result = JsonConvert.DeserializeObject<Input>(json);
            Assert.AreEqual(input, result);
        }
        #endregion
    }
}