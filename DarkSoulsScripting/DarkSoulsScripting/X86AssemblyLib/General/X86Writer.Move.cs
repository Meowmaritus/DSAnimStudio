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
		public void Mov8(X86Register8 dest, Byte value) {
			this.writer.Write(new byte[] { 0xc6 });
			reg_emit8(0, dest);
			writer.Write(value);
		}
		public void Mov16(X86Register16 dest, Int16 value) {
			this.writer.Write(new byte[] { 0x66, 0xc7 });
			reg_emit16(0, dest);
			writer.Write(value);
		}
		public void Mov32(X86Register32 dest, Int32 value) {
			this.writer.Write(new byte[] { 0xc7 });
			reg_emit32(0, dest);
			writer.Write(value);
		}

		public void Mov8(X86Address dest, Byte value) {
			this.writer.Write(new byte[] { 0xc6 });
			dest.Emit(writer, X86Register32.None);
			this.writer.Write(value);
		}
		public void Mov16(X86Address dest, Int16 value) {
			this.writer.Write(new byte[] { 0x66, 0xc7 });
			dest.Emit(writer, X86Register32.None);
			this.writer.Write(value);
		}
		public void Mov32(X86Address dest, Int32 value) {
			this.writer.Write(new byte[] { 0xc7 });
			dest.Emit(writer, X86Register32.None);
			this.writer.Write(value);
		}

		public void Mov8(X86Address dest, X86Register8 src) {
			this.writer.Write(new byte[] { 0x88 });
			dest.Emit(writer, src);
		}
		public void Mov16(X86Address dest, X86Register16 src) {
			this.writer.Write(new byte[] { 0x66, 0x89 });
			dest.Emit(writer, src);
		}
		public void Mov32(X86Address dest, X86Register32 src) {
			this.writer.Write(new byte[] { 0x89 });
			dest.Emit(writer, src);
		}

		public void Mov8(X86Register8 dest, X86Address src) {
			this.writer.Write(new byte[] { 0x8a });
			src.Emit(writer, dest);
		}
		public void Mov16(X86Register16 dest, X86Address src) {
			this.writer.Write(new byte[] { 0x66, 0x8b });
			src.Emit(writer, dest);
		}
		public void Mov32(X86Register32 dest, X86Address src) {
			this.writer.Write(new byte[] { 0x8b });
			src.Emit(writer, dest);
		}

		public void Mov8(X86Register8 dest, X86Register8 src) {
			writer.Write(new byte[] { 0x8a });
			reg_emit8(dest, src);
		}
		public void Mov16(X86Register16 dest, X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0x8b });
			reg_emit16(dest, src);
		}
		public void Mov32(X86Register32 dest, X86Register32 src) {
			writer.Write(new byte[] { 0x8b });
			reg_emit32(dest, src);
		}
	}
}
