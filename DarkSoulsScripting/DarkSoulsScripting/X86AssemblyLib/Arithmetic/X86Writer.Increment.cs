/*
 * (c) 2008 The Managed.X86 Project
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Alex Lyman (<mailto:mail.alex.lyman@gmail.com>)
 */


namespace Managed.X86
{
    partial class X86Writer {
		public void Inc8(X86Register8 reg) {
			writer.Write(new byte[] { 0xFE });
			reg_emit8((X86Register8)0, reg);
		}
		public void Inc16(X86Register16 reg) {
			writer.Write(new byte[] { 0x66, 0xFF });
			reg_emit16((X86Register16)0, reg);
		}
		public void Inc32(X86Register32 reg) {
			writer.Write(new byte[] { 0xFF });
			reg_emit32((X86Register32)0, reg);
		}

		public void Inc8(X86Address mem) {
			writer.Write(new byte[] { 0xFE });
			mem.Emit(writer, (X86Register8)0);
		}
		public void Inc16(X86Address mem) {
			writer.Write(new byte[] { 0x66, 0xFF });
			mem.Emit(writer, (X86Register8)0);
		}
		public void Inc32(X86Address mem) {
			writer.Write(new byte[] { 0xFF });
			mem.Emit(writer, (X86Register8)0);
		}
	}
}
