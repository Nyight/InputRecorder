using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace InputRecorder
{
    public class Playback
    {
        private enum MouseButton { Left, Right, LeftDouble, RightDouble };
        private static readonly InputSimulator INPUT = new InputSimulator();
        private static readonly object LOCK = new object();

        public event EventHandler<InputArgs> OnInputSent = delegate { };
        public event EventHandler OnPlaybackDone = delegate { };

        public int CurrentPosition { get; private set; }
        public List<Input> Score { get; private set; }
        public bool IsPlaying { get { return _playbackEngine.Enabled; } }

        private System.Timers.Timer _playbackEngine;
        private bool _exact;

        public Playback(Recorder recorder = null) : this(recorder == null ? null : recorder.RecordedKeys) { }
        public Playback(List<Input> recordedKeys)
        {
            Score = recordedKeys == null ? null : new List<Input>(recordedKeys);
            _playbackEngine = new System.Timers.Timer();
            _playbackEngine.Elapsed += playbackEngine_Tick;
        }

        public void PlayExact(int delayBetweenInputMilliseconds = 1500) { PlayExact(Score, delayBetweenInputMilliseconds); }
        public void PlayExact(Recorder recorder, int delayBetweenInputMilliseconds = 1500) { PlayExact(recorder.RecordedKeys, delayBetweenInputMilliseconds); }
        public void PlayExact(List<Input> recordedKeys, int delayBetweenInputMilliseconds = 1500) { play(recordedKeys, delayBetweenInputMilliseconds, true); }

        public void Play() { Play(Score); }
        public void Play(Recorder recorder) { Play(recorder.RecordedKeys); }
        public void Play(List<Input> recordedKeys) { play(recordedKeys, 1500, false); }

        private void play(List<Input> recordedKeys, int delayBetweenInputMilliseconds, bool exact)
        {
            lock (LOCK)
            {
                if (IsPlaying) return;

                _exact = exact;
                Score = new List<Input>(recordedKeys);
                Reset();

                _playbackEngine.Interval = exact ? delayBetweenInputMilliseconds : recordedKeys[0].DelayInMilliseconds;
                _playbackEngine.Start();
            }
        }

        public void Pause(int forInMilliseconds = 0)
        {
            lock (LOCK)
            {
                Stop();
                if (forInMilliseconds > 0)
                    Task.Delay(forInMilliseconds).ContinueWith(t => Resume());
            }
        }

        public void Resume() { _playbackEngine.Start(); }
        public void Stop() { _playbackEngine.Stop(); }
        public void Reset() { lock (LOCK) { Stop(); CurrentPosition = 0; } }
        public void Restart() { lock (LOCK) { Reset(); _playbackEngine.Start(); } }

        private void playbackEngine_Tick(object sender, EventArgs e)
        {
            lock (LOCK)
            {
                var input = Score[CurrentPosition];

                if (input.IsKey)
                    keyPress(input.Key);
                else
                    mouseClick(MouseButton.Left, input.ClickLocation);

                OnInputSent(this, InputArgs.Create(input));

                if (++CurrentPosition >= Score.Count)
                {
                    _playbackEngine.Stop();
                    OnPlaybackDone(this, EventArgs.Empty);
                    return;
                }

                if (!_exact)
                {
                    _playbackEngine.Stop();
                    _playbackEngine.Interval = Score[CurrentPosition].DelayInMilliseconds;
                    _playbackEngine.Start();
                }
            }
        }

        private void mouseClick(MouseButton button, Point location, int times = 1)
        {
            mouseMove(location.X, location.Y);
            Func<IMouseSimulator> action = null;
            switch (button)
            {
                case MouseButton.Left: action = INPUT.Mouse.LeftButtonClick; break;
                case MouseButton.LeftDouble: action = INPUT.Mouse.LeftButtonDoubleClick; break;
                case MouseButton.Right: action = INPUT.Mouse.RightButtonClick; break;
                case MouseButton.RightDouble: action = INPUT.Mouse.RightButtonDoubleClick; break;
            }

            for (int i = 0; i < times; i++)
                action();
        }

        private void mouseMove(int x, int y)
        {
            INPUT.Mouse.MoveMouseTo(x, y);
        }

        private void keyPress(Keys key, int milliseconds = 10)
        {
            keyPress((VirtualKeyCode)key, milliseconds);
        }

        private async void keyPress(VirtualKeyCode k, int milliseconds)
        {
            INPUT.Keyboard.KeyDown(k);
            await Task.Delay(milliseconds);
            INPUT.Keyboard.KeyUp(k);
        }
    }
}