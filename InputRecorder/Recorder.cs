using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InputRecorder
{
    public class Recorder : IDisposable
    {
        private static readonly object LOCK = new object();

        public EventHandler<InputArgs> OnInputRecorded = delegate { };

        public List<Keys> AcceptedKeys { get; set; }
        public List<Input> RecordedKeys { get; private set; }
        public bool IsRecording { get; private set; }
        public bool RecordsMouse { get; private set; }

        private IKeyboardMouseEvents _hooks;
        private DateTime _currentTime;

        public Recorder(IEnumerable<Keys> acceptedKeys = null, bool mouse = true)
        {
            AcceptedKeys = new List<Keys>(acceptedKeys ?? new Keys[0]);

            RecordsMouse = mouse;
            IsRecording = false;
            RecordedKeys = new List<Input>();

            _hooks = Hook.GlobalEvents();
        }

        private void hooks_MouseClick(object sender, MouseEventArgs e)
        {
            lock (LOCK)
            {
                var delay = DateTime.Now - _currentTime;
                addInput(new Input(e.Location, (int)Math.Round(delay.TotalMilliseconds)));
                _currentTime = DateTime.Now;
            }
        }

        private void hooks_KeyUp(object sender, KeyEventArgs e)
        {
            lock (LOCK)
            {
                var delay = DateTime.Now - _currentTime;
                addInput(new Input(e.KeyCode, (int)Math.Round(delay.TotalMilliseconds)));
                _currentTime = DateTime.Now;
            }
        }

        private void addInput(Input input)
        {
            RecordedKeys.Add(input);
            OnInputRecorded(this, InputArgs.Create(input));
        }

        public void Start()
        {
            lock (LOCK)
            {
                IsRecording = true;
                RecordedKeys.Clear();
                hookin();
                _currentTime = DateTime.Now;
            }
        }

        private void hookin()
        {
            _hooks.KeyUp += hooks_KeyUp;

            if (RecordsMouse)
                _hooks.MouseClick += hooks_MouseClick;
        }

        public void Stop()
        {
            lock (LOCK)
            {
                IsRecording = false;
                unhook();
            }
        }

        private void unhook()
        {
            _hooks.KeyUp -= hooks_KeyUp;

            if (RecordsMouse)
                _hooks.MouseClick -= hooks_MouseClick;
        }

        public void Reset()
        {
            lock (LOCK)
            {
                IsRecording = false;
                RecordedKeys.Clear();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //_keyboard.UnHookKeyboard();
                    unhook();
                    _hooks.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Recorder() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}