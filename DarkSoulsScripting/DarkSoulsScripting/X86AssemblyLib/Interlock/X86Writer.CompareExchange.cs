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
		public void CmpXChg8(X86Register8 dest, X86Register8 src) {
			writer.Write(new byte[] { 0x0F, 0xB0 });
			reg_emit8(src, dest);
		}
		public void CmpXChg16(X86Register16 dest, X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0x0F, 0xB1 });
			reg_emit16(src, dest);
		}
		public void CmpXChg32(X86Register32 dest, X86Register32 src) {
			writer.Write(new byte[] { 0x0F, 0xB1 });
			reg_emit32(src, dest);
		}

		public void CmpXChg8(X86Address dest, X86Register8 src) {
			writer.Write(new byte[] { 0x0F, 0xB0 });
			dest.Emit(writer, src);
		}
		public void CmpXChg16(X86Address dest, X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0x0F, 0xB1 });
			dest.Emit(writer, src);
		}
		public void CmpXChg32(X86Address dest, X86Register32 src) {
			writer.Write(new byte[] { 0x0F, 0xB1 });
			dest.Emit(writer, src);
		}
	}
}
