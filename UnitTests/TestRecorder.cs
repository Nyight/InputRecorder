using InputRecorder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace UnitTests
{
    [TestClass]
    public class TestRecorder
    {
        private static Random rand;
        private static InputSimulator simulator;
        private static Keys[] acceptedKeys;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            rand = new Random();
            simulator = new InputSimulator();
            acceptedKeys = new[] { Keys.A, Keys.B, Keys.C, Keys.D };
        }

        [TestInitialize]
        public void Setup()
        {

        }

        #region constructors
        [TestMethod]
        public void TestConstructorDefault()
        {
            var recorder = new Recorder();

            Assert.IsTrue(recorder.RecordsMouse);
            Assert.IsFalse(recorder.IsRecording);
            CollectionAssert.AreEqual(new Keys[0], recorder.AcceptedKeys);
            CollectionAssert.AreEqual(new Input[0], recorder.RecordedKeys);
        }

        [TestMethod]
        public void TestConstructorKeys()
        {
            var acceptedKeys = new[] { Keys.A, Keys.B, Keys.C };
            var recorder = new Recorder(acceptedKeys);

            Assert.IsTrue(recorder.RecordsMouse);
            Assert.IsFalse(recorder.IsRecording);
            CollectionAssert.AreEqual(acceptedKeys, recorder.AcceptedKeys);
            CollectionAssert.AreEqual(new Input[0], recorder.RecordedKeys);
        }

        [TestMethod]
        public void TestConstructorNullKeys()
        {
            var recorder = new Recorder(null);

            Assert.IsTrue(recorder.RecordsMouse);
            Assert.IsFalse(recorder.IsRecording);
            CollectionAssert.AreEqual(new Keys[0], recorder.AcceptedKeys);
            CollectionAssert.AreEqual(new Input[0], recorder.RecordedKeys);
        }

        [TestMethod]
        public void TestConstructorMouse()
        {
            var recorder = new Recorder(null, false);

            Assert.IsFalse(recorder.RecordsMouse);
            Assert.IsFalse(recorder.IsRecording);
            CollectionAssert.AreEqual(new Keys[0], recorder.AcceptedKeys);
            CollectionAssert.AreEqual(new Input[0], recorder.RecordedKeys);
        }
        #endregion

        [TestMethod]
        public void TestStartStop()
        {
            var recorder = new Recorder();

            Assert.IsFalse(recorder.IsRecording);
            recorder.Start();
            Assert.IsTrue(recorder.IsRecording);
            recorder.Stop();
            Assert.IsFalse(recorder.IsRecording);
            Assert.AreEqual(0, recorder.RecordedKeys.Count);
        }

        #region record
        [TestMethod]
        public void TestRecordKeys()
        {
            var recorder = new Recorder(acceptedKeys, false);

            CollectionAssert.AreEqual(acceptedKeys, recorder.AcceptedKeys);
            Assert.IsFalse(recorder.IsRecording);
            Assert.IsFalse(recorder.RecordsMouse);

            var pressedKeys = GenerateRandomInput(10, true, false);

            recorder.Start();
            Assert.IsTrue(recorder.IsRecording);

            for (int i = 0; i < 10; i++)
                simulator.Keyboard.KeyPress((VirtualKeyCode)pressedKeys[i].Key).Sleep(10);

            recorder.Stop();
            Assert.IsFalse(recorder.IsRecording);
            Assert.AreEqual(pressedKeys.Count, recorder.RecordedKeys.Count);
            CollectionAssert.AreEqual(pressedKeys, recorder.RecordedKeys);
            foreach (var input in recorder.RecordedKeys)
            {
                Assert.IsTrue(input.IsKey);
                Assert.IsTrue(acceptedKeys.Contains(input.Key));
                Assert.IsTrue(pressedKeys.Contains(input));
                Assert.AreEqual(new Point(), input.ClickLocation);
            }

            recorder.Reset();
            Assert.AreEqual(0, recorder.RecordedKeys.Count);
        }

        [TestMethod]
        public void TestRecordMouse()
        {
            var recorder = new Recorder(null, true);

            CollectionAssert.AreEqual(new Keys[0], recorder.AcceptedKeys);
            Assert.IsFalse(recorder.IsRecording);
            Assert.IsTrue(recorder.RecordsMouse);

            var clickedLocations = GenerateRandomInput(10, false, true);

            recorder.Start();
            Assert.IsTrue(recorder.IsRecording);

            for (int i = 0; i < 10; i++)
            {
                var point = clickedLocations[i];
                simulator.Mouse.MoveMouseTo(point.X, point.Y).LeftButtonClick().Sleep(8);
            }

            recorder.Stop();
            Assert.IsFalse(recorder.IsRecording);
            Assert.AreEqual(clickedLocations.Count, recorder.RecordedKeys.Count);
            CollectionAssert.AreEqual(clickedLocations, recorder.RecordedKeys);
            foreach (var input in recorder.RecordedKeys)
            {
                Assert.IsFalse(input.IsKey);
                Assert.IsTrue(clickedLocations.Contains(input));
                Assert.AreEqual(Keys.None, input.Key);
            }

            recorder.Reset();
            Assert.AreEqual(0, recorder.RecordedKeys.Count);
        }

        [TestMethod]
        public void TestRecordedInput()
        {
            var recorder = new Recorder(acceptedKeys);

            var inputs = GenerateRandomInput(20);
            var receivedInput = new List<Input>();

            recorder.OnInputRecorded += delegate (object sender, InputArgs args)
            {
                receivedInput.Add(args.Input);
            };

            recorder.Start();
            Assert.IsTrue(recorder.IsRecording);

            for (int i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                if (input.IsKey)
                {
                    simulator.Keyboard.KeyPress((VirtualKeyCode)input.Key).Sleep(10);
                }
                else
                {
                    var loc = input.ClickLocation;
                    simulator.Mouse.MoveMouseTo(loc.X, loc.Y).Sleep(2).LeftButtonClick().Sleep(10);
                }
            }

            recorder.Stop();
            Assert.IsFalse(recorder.IsRecording);
            Assert.AreEqual(inputs.Count, recorder.RecordedKeys.Count);
            Assert.AreEqual(inputs.Count, receivedInput.Count);
            CollectionAssert.AreEqual(inputs, recorder.RecordedKeys);
            CollectionAssert.AreEqual(inputs, receivedInput);

            recorder.Reset();
            Assert.AreEqual(0, recorder.RecordedKeys.Count);
        }

        private List<Input> GenerateRandomInput(int count = 10, bool keys = true, bool mouse = true)
        {
            List<Input> inputs = new List<Input>(count);
            for (int i = 0; i < count; i++)
            {
                if (keys && mouse)
                {
                    if (rand.NextDouble() > 0.5f)
                    {
                        var input = new Input(getRandomKey(), 10);
                        inputs.Add(input);
                    }
                    else
                    {
                        var input = new Input(getRandomPoint(), 10);
                        inputs.Add(input);
                    }
                }
                else if (keys)
                {
                    var input = new Input(getRandomKey(), 10);
                    inputs.Add(input);
                }
                else
                {
                    var input = new Input(getRandomPoint(), 10);
                    inputs.Add(input);
                }
            }

            return inputs;
        }

        private Keys getRandomKey() { return acceptedKeys[rand.Next(acceptedKeys.Length)]; }
        private Point getRandomPoint()
        {
            var height = Screen.PrimaryScreen.Bounds.Height;
            var width = Screen.PrimaryScreen.Bounds.Width;
            var x = (int)(width * 0.5f + rand.Next(-100, 100));
            var y = (int)(height - 20 + rand.Next(-10, 10));
            return new Point(x, y);
        }
        #endregion
    }
}