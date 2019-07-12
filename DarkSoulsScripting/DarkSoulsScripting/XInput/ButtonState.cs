using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsScripting.XInput
{
    public struct ButtonState
    {
        public Button Value;

        public ButtonState(Button val) => Value = val;
        public ButtonState(ushort val) => Value = (Button)val;

        public bool this[Button b] 
        { 
            get => (Value & b) == b;
            set => Value = value ? (Value | b) : (Value & ~b);
        }

        public static implicit operator Button(ButtonState b) => b.Value;
        public static implicit operator ushort(ButtonState b) => (ushort)b.Value;
        public static implicit operator ButtonState(Button b) => new ButtonState(b);
        public static implicit operator ButtonState(ushort b) => new ButtonState(b);
    }
}
