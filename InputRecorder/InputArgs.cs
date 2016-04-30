using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputRecorder
{
    public class InputArgs : EventArgs, IEquatable<InputArgs>
    {
        public Input Input { get; }

        public InputArgs(Input input)
        {
            Input = input;
        }

        public static InputArgs Create(Input input)
        {
            return new InputArgs(input);
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals(obj as InputArgs);
        }

        public bool Equals(InputArgs other)
        {
            if (other == null) return false;
            if (Input != other.Input) return false;

            return true;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            int hash = 19;
            unchecked
            {
                hash = hash * 23 + Input.GetHashCode();
            }
            return hash;
        }

        public static bool operator ==(InputArgs a, InputArgs b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(InputArgs a, InputArgs b)
        {
            return !(a == b);
        }
    }
}