/*
 * (c) 2008 The Managed.X86 Project
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Alex Lyman (<mailto:mail.alex.lyman@gmail.com>)
 */

using System;

namespace Managed.X86
{
    partial class X86Writer {
		/// <summary>
		/// Patches the instruction at the current position to go to the specified address.
		/// 
		/// Used with Jump and Call instructions
		/// </summary>
		/// <param name="target">The target address</param>

		public void Patch(IntPtr target) {
			int disp, size = 0;
			int peek = stream.ReadByte();
			int pos = this.Position.ToInt32();
			switch (peek) {
				#region call, jump32
				case 0xe8:
				case 0xe9: ++size; break; /* call, jump32 */
				#endregion
				#region prefix for 32-bit disp
				case 0x0f: {
						int peek2 = stream.ReadByte();
						if (!(peek2 >= 0x70 && peek2 <= 0x8f)) throw new NotSupportedException();
						++size; ++pos; break; /* prefix for 32-bit disp */
					}
				#endregion
				#region loop
				case 0xe0:
				case 0xe1:
				case 0xe2: /* loop */
				#endregion
				case 0xeb: /* jump8 */
				default:
					if (peek >= 0x70 && peek <= 0x7f) {
						break; /* conditional jump opcodes */
					} else {
						throw new NotSupportedException();
					}
			}
			disp = (target.ToInt32()) - pos;
			if (size != 0) imm_emit32(disp - 4);
			else if (is_imm8(disp - 1)) imm_emit8(disp - 1);
			else throw new NotSupportedException();
		}
	}
}
