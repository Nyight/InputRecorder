using InputRecorder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnitTests
{
    [TestClass]
    public class TestPlayback
    {
        private const int TIME_TOLERANCE = 500;
        private static readonly Random rand = new Random();
        private static readonly List<Keys> keys = new List<Keys>();

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            foreach (var k in new[] { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "a", "s", "d", "f", "g", "h", "j", "k", "l" })
                keys.Add((Keys)Enum.Parse(typeof(Keys), k, true));
        }

        [TestInitialize]
        public void Setup()
        {

        }

        #region constructors
        [TestMethod]
        public void TestConstructorRecorder()
        {
            var recorder = new Recorder();
            var playback = new Playback(recorder);

            CollectionAssert.AreEqual(recorder.RecordedKeys, playback.Score);
        }

        [TestMethod]
        public void TestConstructorInputList()
        {
            var list = getRandomList(20);
            var playback = new Playback(list);

            CollectionAssert.AreEqual(list, playback.Score);
        }
        #endregion

        #region playback
        [TestMethod]
        public async Task TestPlayBackExact()
        {
            var list = getRandomList(20);
            var playback = new Playback(list);
            var received = new List<Input>();
            var sw = new Stopwatch();
            var totalTime = list.Count * 1500;

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            playback.OnInputSent += (sender, e) => { received.Add(e.Input); };
            playback.OnPlaybackDone += delegate { tcs.SetResult(true); };

            Assert.IsFalse(playback.IsPlaying);
            sw.Start();
            playback.PlayExact();
            Assert.IsTrue(playback.IsPlaying);

            await tcs.Task;
            sw.Stop();
            
            Assert.IsTrue(Math.Abs(totalTime - sw.ElapsedMilliseconds) < TIME_TOLERANCE);
            CollectionAssert.AreEqual(list, received);
        }

        [TestMethod]
        public async Task TestPlayBackAsRecorded()
        {
            var list = getRandomList(20);
            var playback = new Playback(list);
            var received = new List<Input>();
            var sw = new Stopwatch();
            var totalTime = list.Sum((i) => { return i.DelayInMilliseconds; });

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            playback.OnInputSent += (sender, e) => { received.Add(e.Input); };
            playback.OnPlaybackDone += delegate { tcs.SetResult(true); };

            Assert.IsFalse(playback.IsPlaying);
            sw.Start();
            playback.Play();
            Assert.IsTrue(playback.IsPlaying);

            await tcs.Task;
            sw.Stop();
            Assert.IsTrue(Math.Abs(totalTime - sw.ElapsedMilliseconds) < TIME_TOLERANCE);
            CollectionAssert.AreEqual(list, received);
        }

        [TestMethod]
        public async Task TestPlayBackPause()
        {
            var list = getRandomList(20);
            var playback = new Playback(list);
            var received = new List<Input>();
            var sw = new Stopwatch();
            var delay = rand.Next(100, 2000);
            var totalTime = list.Sum((i) => { return i.DelayInMilliseconds; }) + delay;

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            playback.OnInputSent += (sender, e) => { received.Add(e.Input); };
            playback.OnPlaybackDone += delegate { tcs.SetResult(true); };

            Assert.IsFalse(playback.IsPlaying);
            sw.Start();
            playback.Play();
            Assert.IsTrue(playback.IsPlaying);

            playback.Pause(delay);
            await tcs.Task;
            sw.Stop();
            Assert.IsTrue(Math.Abs(totalTime - sw.ElapsedMilliseconds) < TIME_TOLERANCE);
            CollectionAssert.AreEqual(list, received);
        }

        [TestMethod]
        public async Task TestPlayBackFunctionality()
        {
            var list = getRandomList(10);
            var playback = new Playback(list);
            var received = new List<Input>();
            var time = list.GetRange(0, 3).Sum((i) => { return i.DelayInMilliseconds; });

            playback.OnInputSent += (sender, e) => { received.Add(e.Input); };

            Assert.IsFalse(playback.IsPlaying);
            playback.Play();
            Assert.IsTrue(playback.IsPlaying);

            await Task.Delay(time);
            playback.Stop();
            Assert.IsFalse(playback.IsPlaying);

            time = list.GetRange(3, 3).Sum((i) => { return i.DelayInMilliseconds; });
            received.Clear();
            playback.Resume();
            Assert.IsTrue(playback.IsPlaying);

            await Task.Delay(time);
            playback.Stop();
            Assert.IsFalse(playback.IsPlaying);

            playback.Reset();
            Assert.AreEqual(0, playback.CurrentPosition);

            time = list.Sum((i) => { return i.DelayInMilliseconds; });
            received.Clear();

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            playback.OnPlaybackDone += delegate { tcs.SetResult(true); };
            playback.Play();
            await tcs.Task;

            CollectionAssert.AreEqual(list, received);
            received.Clear();

            tcs = new TaskCompletionSource<bool>();
            playback.OnPlaybackDone += delegate { tcs.SetResult(true); };
            playback.Restart();
            Assert.IsTrue(playback.IsPlaying);
            Assert.AreEqual(0, playback.CurrentPosition);
            await tcs.Task;
            CollectionAssert.AreEqual(list, received);
        }
        #endregion

        private List<Input> getRandomList(int count)
        {
            var inputs = new List<Input>();
            for (int i = 0; i < count; i++)
            {
                if (rand.NextDouble() > 0.5f)
                    inputs.Add(new Input(randomKey, rand.Next(50, 200)));
                else
                    inputs.Add(new Input(randomPoint, rand.Next(50, 200)));
            }

            return inputs;
        }

        private Keys randomKey { get { return keys[rand.Next(keys.Count)]; } }
        private Point randomPoint
        {
            get
            {
                var height = Screen.PrimaryScreen.Bounds.Height;
                var width = Screen.PrimaryScreen.Bounds.Width;
                var x = (int)(width * 0.5f + rand.Next(-100, 100));
                var y = (int)(height - 20 + rand.Next(-10, 10));
                return new Point(x, y);
            }
        }
    }
}