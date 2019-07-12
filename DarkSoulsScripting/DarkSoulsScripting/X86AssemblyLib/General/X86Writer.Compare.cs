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
		public void Cmp8(X86Register8 dest, X86Register8 src) {
			writer.Write(new byte[] { 0x38 });
			reg_emit8(dest, src);
		}
		public void Cmp16(X86Register16 dest, X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0x39 });
			reg_emit16(dest, src);
		}
		public void Cmp32(X86Register32 dest, X86Register32 src) {
			writer.Write(new byte[] { 0x39 });
			reg_emit32(dest, src);
		}

		public void Cmp8(X86Register8 dest, X86Address src) {
			writer.Write(new byte[] { 0x3A });
			src.Emit(writer, dest);
		}
		public void Cmp16(X86Register16 dest, X86Address src) {
			writer.Write(new byte[] { 0x66, 0x3B });
			src.Emit(writer, dest);
		}
		public void Cmp32(X86Register32 dest, X86Address src) {
			writer.Write(new byte[] { 0x3B });
			src.Emit(writer, dest);
		}

		public void Cmp8(X86Address dest, X86Register8 src) {
			writer.Write(new byte[] { 0x38 });
			dest.Emit(writer, src);
		}
		public void Cmp16(X86Address dest, X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0x39 });
			dest.Emit(writer, src);
		}
		public void Cmp32(X86Address dest, X86Register32 src) {
			writer.Write(new byte[] { 0x38 });
			dest.Emit(writer, src);
		}

		public void Cmp8(X86Register8 dest, Byte src) {
			writer.Write(new byte[] { 0x80 });
			reg_emit8((X86Register8)7, dest);
			imm_emit8(src);
		}
		public void Cmp16(X86Register16 dest, Int16 src) {
			writer.Write(new byte[] { 0x66, 0x81 });
			reg_emit16((X86Register16)7, dest);
			imm_emit16(src);
		}
		public void Cmp32(X86Register32 dest, Int32 src) {
			writer.Write(new byte[] { 0x81 });
			reg_emit32((X86Register32)7, dest);
			imm_emit32(src);
		}

		public void Cmp8(X86Address dest, Byte src) {
			writer.Write(new byte[] { 0x80 });
			dest.Emit(writer, (X86Register8)7);
			imm_emit8(src);
		}
		public void Cmp16(X86Address dest, Int16 src) {
			writer.Write(new byte[] { 0x66, 0x81 });
			dest.Emit(writer, (X86Register8)7);
			imm_emit16(src);
		}
		public void Cmp32(X86Address dest, Int32 src) {
			writer.Write(new byte[] { 0x81 });
			dest.Emit(writer, (X86Register8)7);
			imm_emit32(src);
		}
	}
}
