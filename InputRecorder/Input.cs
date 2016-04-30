using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace InputRecorder
{
    public class Input
    {
        [JsonProperty("Key")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Keys Key { get; private set; }

        [JsonProperty("ClickLocation")]
        public Point ClickLocation { get; private set; }

        [JsonProperty("delayInMilliseconds")]
        public int DelayInMilliseconds { get; set; }

        [JsonIgnore]
        public int X { get { return ClickLocation.X; } }
        [JsonIgnore]
        public int Y { get { return ClickLocation.Y; } }
        [JsonIgnore]
        public bool IsKey { get { return Key != Keys.None; } }

        [JsonConstructor]
        public Input() { }

        public Input(Keys k, int delayInMilliseconds)
        {
            Key = k;
            DelayInMilliseconds = delayInMilliseconds;
        }

        public Input(int x, int y, int delayInMilliseconds) : this(new Point(x, y), delayInMilliseconds) { }
        public Input(Point clickLocation, int delayInMilliseconds)
        {
            ClickLocation = clickLocation;
            DelayInMilliseconds = delayInMilliseconds;
        }

        public Input(Input input)
        {
            Key = input.Key;
            ClickLocation = new Point(input.ClickLocation.X, input.ClickLocation.Y);
            DelayInMilliseconds = input.DelayInMilliseconds;
        }

        public Input Clone() { return (IsKey) ? new Input(Key, DelayInMilliseconds) : new Input(ClickLocation, DelayInMilliseconds); }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Input other = (Input)obj;

            if (Key != other.Key)
                return false;

            if (ClickLocation != other.ClickLocation)
                return false;

            //if (Math.Abs(DelayInMilliseconds - other.DelayInMilliseconds) > 2)
            //    return false;

            return true;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            unchecked
            {
                hash = hash * 23 + Key.GetHashCode();
                hash = hash * 23 + ClickLocation.GetHashCode();
                //hash = hash * 23 + DelayInMilliseconds.GetHashCode();
            }
            return hash;
        }

        public static bool operator ==(Input a, Input b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Input a, Input b)
        {
            return !(a == b);
        }
    }
}