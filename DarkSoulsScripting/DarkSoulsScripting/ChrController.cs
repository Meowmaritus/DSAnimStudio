using System;
using DarkSoulsScripting.Injection;

namespace DarkSoulsScripting
{

    public class ChrController : GameStruct
	{

        protected override void InitSubStructures()
        {
            
        }

        public float MoveX {
			get { return Hook.RFloat(Address + 0x10); }
			set { Hook.WFloat(Address + 0x10, value); }
		}

		public float MoveY {
			get { return Hook.RFloat(Address + 0x18); }
			set { Hook.WFloat(Address + 0x18, value); }
		}

		public float CamRotSpeedH {
			get { return Hook.RFloat(Address + 0x50); }
			set { Hook.WFloat(Address + 0x50, value); }
		}

		public float CamRotSpeedV {
			get { return Hook.RFloat(Address + 0x54); }
			set { Hook.WFloat(Address + 0x54, value); }
		}

		public float CamRotH {
			get { return Hook.RFloat(Address + 0x60); }
			set { Hook.WFloat(Address + 0x60, value); }
		}

		public float CamRotV {
			get { return Hook.RFloat(Address + 0x64); }
			set { Hook.WFloat(Address + 0x64, value); }
		}

		public bool R1Held_Sometimes {
			get { return Hook.RBool(Address + 0x10); }
			set { Hook.WBool(Address + 0x10, value); }
		}

		public bool L2OrR1Held_Sometimes {
			get { return Hook.RBool(Address + 0x85); }
			set { Hook.WBool(Address + 0x85, value); }
		}

		public bool RMouseHeld {
			get { return Hook.RBool(Address + 0x89); }
			set { Hook.WBool(Address + 0x89, value); }
		}

		public bool R1Held {
			get { return Hook.RBool(Address + 0x8b); }
			set { Hook.WBool(Address + 0x8b, value); }
		}

		public bool XHeld {
			get { return Hook.RBool(Address + 0x92); }
			set { Hook.WBool(Address + 0x92, value); }
		}

		public bool L1Held {
			get { return Hook.RBool(Address + 0x97); }
			set { Hook.WBool(Address + 0x97, value); }
		}

		public bool L2OrTabHeld {
			get { return Hook.RBool(Address + 0x98); }
			set { Hook.WBool(Address + 0x98, value); }
		}

		public bool L1Held2 {
			get { return Hook.RBool(Address + 0xb7); }
			set { Hook.WBool(Address + 0xb7, value); }
		}

		public bool R1HeldOrCircleTapped {
			get { return Hook.RBool(Address + 0xb9); }
			set { Hook.WBool(Address + 0xb9, value); }
		}

		public bool RMouseOrR2Held {
			get { return Hook.RBool(Address + 0xbe); }
			set { Hook.WBool(Address + 0xbe, value); }
		}

		public bool R1OrLMouseHeld {
			get { return Hook.RBool(Address + 0xc0); }
			set { Hook.WBool(Address + 0xc0, value); }
		}

		public bool L2Held {
			get { return Hook.RBool(Address + 0xc2); }
			set { Hook.WBool(Address + 0xc2, value); }
		}

		public bool CircleTapped {
			get { return Hook.RBool(Address + 0xc2); }
			set { Hook.WBool(Address + 0xc2, value); }
		}

		public bool XHeld2 {
			get { return Hook.RBool(Address + 0xc7); }
			set { Hook.WBool(Address + 0xc7, value); }
		}

		public bool L1Held3 {
			get { return Hook.RBool(Address + 0xcc); }
			set { Hook.WBool(Address + 0xcc, value); }
		}

		public bool L2Held2 {
			get { return Hook.RBool(Address + 0xcd); }
			set { Hook.WBool(Address + 0xcd, value); }
		}

		public bool L1Held4 {
			get { return Hook.RBool(Address + 0xec); }
			set { Hook.WBool(Address + 0xec, value); }
		}

		public float SecondsR1Held {
			get { return Hook.RFloat(Address + 0xf0); }
			set { Hook.WFloat(Address + 0xf0, value); }
		}

		public float SecondsGuarding {
			get { return Hook.RFloat(Address + 0xf4); }
			set { Hook.WFloat(Address + 0xf4, value); }
		}

		public float SecondsR2Held {
			get { return Hook.RFloat(Address + 0x104); }
			set { Hook.WFloat(Address + 0x104, value); }
		}

		public float SecondsR1Held2 {
			get { return Hook.RFloat(Address + 0x10c); }
			set { Hook.WFloat(Address + 0x10c, value); }
		}

		public float SecondsL1Held {
			get { return Hook.RFloat(Address + 0x13c); }
			set { Hook.WFloat(Address + 0x13c, value); }
		}

		public float SecondsGuarding2 {
			get { return Hook.RFloat(Address + 0x144); }
			set { Hook.WFloat(Address + 0x144, value); }
		}
		
    }

}
