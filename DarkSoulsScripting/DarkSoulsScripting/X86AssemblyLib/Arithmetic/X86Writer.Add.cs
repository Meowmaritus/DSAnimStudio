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
		public void Add8 (X86Register8 dest, X86Register8 src) {
			writer.Write(new byte[] { 0x00 });
			reg_emit8(dest, src);
		}
		public void Add16(X86Register16 dest, X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0x01 });
			reg_emit16(dest, src);
		}
		public void Add32(X86Register32 dest, X86Register32 src) {
			writer.Write(new byte[] { 0x01 });
			reg_emit32(dest, src);
		}

		public void Add8(X86Address dest, X86Register8 src) {
			writer.Write(new byte[] { 0x00 });
			dest.Emit(writer, src);
		}
		public void Add16(X86Address dest, X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0x01 });
			dest.Emit(writer, src);
		}
		public void Add32(X86Address dest, X86Register32 src) {
			writer.Write(new byte[] { 0x01 });
			dest.Emit(writer, src);
		}

		public void Add8(X86Register8 dest, X86Address src) {
			writer.Write(new byte[] { 0x02 });
			src.Emit(writer, dest);
		}
		public void Add16(X86Register16 dest, X86Address src) {
			writer.Write(new byte[] { 0x66, 0x03 });
			src.Emit(writer, dest);
		}
		public void Add32(X86Register32 dest, X86Address src) {
			writer.Write(new byte[] { 0x03 });
			src.Emit(writer, dest);
		}

		public void Add8(X86Register8 dest, Byte value) {
			this.writer.Write(new byte[] { 0x80 });
			reg_emit8(0, dest);
			writer.Write(value);
		}
		public void Add16(X86Register16 dest, Int16 value) {
			this.writer.Write(new byte[] { 0x66, 0x81 });
			reg_emit16(0, dest);
			writer.Write(value);
		}
		public void Add32(X86Register32 dest, Int32 value) {
			this.writer.Write(new byte[] { 0x81 });
			reg_emit32(0, dest);
			writer.Write(value);
		}

		public void Add8(X86Address dest, Byte value) {
			this.writer.Write(new byte[] { 0x80 });
			dest.Emit(writer, X86Register32.None);
			writer.Write(value);
		}
		public void Add16(X86Address dest, Int16 value) {
			this.writer.Write(new byte[] { 0x66, 0x81 });
			dest.Emit(writer, X86Register32.None);
			writer.Write(value);
		}
		public void Add32(X86Address dest, Int32 value) {
			this.writer.Write(new byte[] { 0x81 });
			dest.Emit(writer, X86Register32.None);
			writer.Write(value);
		}
	}
}
