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
		public void AddC8(X86Register8 dest, X86Register8 src) {
			writer.Write(new byte[] { 0x10 });
			reg_emit8(dest, src);
		}
		public void AddC16(X86Register16 dest, X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0x11 });
			reg_emit16(dest, src);
		}
		public void AddC32(X86Register32 dest, X86Register32 src) {
			writer.Write(new byte[] { 0x11 });
			reg_emit32(dest, src);
		}

		public void AddC8(X86Address dest, X86Register8 src) {
			writer.Write(new byte[] { 0x10 });
			dest.Emit(writer, src);
		}
		public void AddC16(X86Address dest, X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0x11 });
			dest.Emit(writer, src);
		}
		public void AddC32(X86Address dest, X86Register32 src) {
			writer.Write(new byte[] { 0x11 });
			dest.Emit(writer, src);
		}

		public void AddC8(X86Register8 dest, X86Address src) {
			writer.Write(new byte[] { 0x12 });
			src.Emit(writer, dest);
		}
		public void AddC16(X86Register16 dest, X86Address src) {
			writer.Write(new byte[] { 0x66, 0x13 });
			src.Emit(writer, dest);
		}
		public void AddC32(X86Register32 dest, X86Address src) {
			writer.Write(new byte[] { 0x13 });
			src.Emit(writer, dest);
		}

		public void AddC8(X86Register8 dest, Byte value) {
			this.writer.Write(new byte[] { 0x80 });
			reg_emit8((X86Register8)0x2, dest);
			writer.Write(value);
		}
		public void AddC16(X86Register16 dest, Int16 value) {
			this.writer.Write(new byte[] { 0x66, 0x81 });
			reg_emit16((X86Register16)0x2, dest);
			writer.Write(value);
		}
		public void AddC32(X86Register32 dest, Int32 value) {
			this.writer.Write(new byte[] { 0x81 });
			reg_emit32((X86Register32)0x2, dest);
			writer.Write(value);
		}

		public void AddC8(X86Address dest, Byte value) {
			this.writer.Write(new byte[] { 0x80 });
			dest.Emit(writer, (X86Register32)0x2);
			writer.Write(value);
		}
		public void AddC16(X86Address dest, Int16 value) {
			this.writer.Write(new byte[] { 0x66, 0x81 });
			dest.Emit(writer, (X86Register32)0x2);
			writer.Write(value);
		}
		public void AddC32(X86Address dest, Int32 value) {
			this.writer.Write(new byte[] { 0x81 });
			dest.Emit(writer, (X86Register32)0x2);
			writer.Write(value);
		}
	}
}
