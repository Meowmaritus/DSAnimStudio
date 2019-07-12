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
		public void Div8(X86Register8 src) {
			writer.Write(new byte[] { 0xF6 });
			reg_emit8((X86Register8)6, src);
		}
		public void Div16(X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0xF7 });
			reg_emit16((X86Register16)6, src);
		}
		public void Div32(X86Register32 src) {
			writer.Write(new byte[] { 0xF7 });
			reg_emit32((X86Register32)6, src);
		}

		public void Div8(X86Address src) {
			writer.Write(new byte[] { 0xF6 });
			src.Emit(writer, (X86Register8)6);
		}
		public void Div16(X86Address src) {
			writer.Write(new byte[] { 0x66, 0xF7 });
			src.Emit(writer, (X86Register8)6);
		}
		public void Div32(X86Address src) {
			writer.Write(new byte[] { 0xF7 });
			src.Emit(writer, (X86Register8)6);
		}

		public void IDiv8(X86Register8 src) {
			writer.Write(new byte[] { 0xF6 });
			reg_emit8((X86Register8)7, src);
		}
		public void IDiv16(X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0xF7 });
			reg_emit16((X86Register16)7, src);
		}
		public void IDiv32(X86Register32 src) {
			writer.Write(new byte[] { 0xF7 });
			reg_emit32((X86Register32)7, src);
		}

		public void IDiv8(X86Address src) {
			writer.Write(new byte[] { 0xF6 });
			src.Emit(writer, (X86Register8)7);
		}
		public void IDiv16(X86Address src) {
			writer.Write(new byte[] { 0x66, 0xF7 });
			src.Emit(writer, (X86Register8)7);
		}
		public void IDiv32(X86Address src) {
			writer.Write(new byte[] { 0xF7 });
			src.Emit(writer, (X86Register8)7);
		}
	}
}
