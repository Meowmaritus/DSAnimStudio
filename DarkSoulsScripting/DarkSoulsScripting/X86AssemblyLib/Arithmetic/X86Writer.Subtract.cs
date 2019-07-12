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
		public void Sub8(X86Register8 dest, X86Register8 src) {
			writer.Write(new byte[] { 0x2A });
			reg_emit8(dest, src);
		}
		public void Sub16(X86Register16 dest, X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0x2B });
			reg_emit16(dest, src);
		}
		public void Sub32(X86Register32 dest, X86Register32 src) {
			writer.Write(new byte[] { 0x2B });
			reg_emit32(dest, src);
		}

		public void Sub8(X86Address dest, X86Register8 src) {
			writer.Write(new byte[] { 0x28 });
			dest.Emit(writer, src);
		}
		public void Sub16(X86Address dest, X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0x29 });
			dest.Emit(writer, src);
		}
		public void Sub32(X86Address dest, X86Register32 src) {
			writer.Write(new byte[] { 0x29 });
			dest.Emit(writer, src);
		}

		public void Sub8(X86Register8 dest, X86Address src) {
			writer.Write(new byte[] { 0x2A });
			src.Emit(writer, dest);
		}
		public void Sub16(X86Register16 dest, X86Address src) {
			writer.Write(new byte[] { 0x66, 0x2B });
			src.Emit(writer, dest);
		}
		public void Sub32(X86Register32 dest, X86Address src) {
			writer.Write(new byte[] { 0x2B });
			src.Emit(writer, dest);
		}

		public void Sub8(X86Register8 dest, Byte value) {
			this.writer.Write(new byte[] { 0x80 });
			reg_emit8((X86Register8)5, dest);
			writer.Write(value);
		}
		public void Sub16(X86Register16 dest, Int16 value) {
			this.writer.Write(new byte[] { 0x66, 0x81 });
			reg_emit16((X86Register16)5, dest);
			writer.Write(value);
		}
		public void Sub32(X86Register32 dest, Int32 value) {
			this.writer.Write(new byte[] { 0x81 });
			reg_emit32((X86Register32)5, dest);
			writer.Write(value);
		}

		public void Sub8(X86Address dest, Byte value) {
			this.writer.Write(new byte[] { 0x80 });
			dest.Emit(writer, (X86Register32)5);
			writer.Write(value);
		}
		public void Sub16(X86Address dest, Int16 value) {
			this.writer.Write(new byte[] { 0x66, 0x81 });
			dest.Emit(writer, (X86Register32)5);
			writer.Write(value);
		}
		public void Sub32(X86Address dest, Int32 value) {
			this.writer.Write(new byte[] { 0x81 });
			dest.Emit(writer, (X86Register32)5);
			writer.Write(value);
		}

		public void Sbb8(X86Register8 dest, X86Register8 src) {
			writer.Write(new byte[] { 0x18 });
			reg_emit8(dest, src);
		}
		public void Sbb16(X86Register16 dest, X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0x19 });
			reg_emit16(dest, src);
		}
		public void Sbb32(X86Register32 dest, X86Register32 src) {
			writer.Write(new byte[] { 0x19 });
			reg_emit32(dest, src);
		}

		public void Sbb8(X86Address dest, X86Register8 src) {
			writer.Write(new byte[] { 0x18 });
			dest.Emit(writer, src);
		}
		public void Sbb16(X86Address dest, X86Register16 src) {
			writer.Write(new byte[] { 0x66, 0x19 });
			dest.Emit(writer, src);
		}
		public void Sbb32(X86Address dest, X86Register32 src) {
			writer.Write(new byte[] { 0x19 });
			dest.Emit(writer, src);
		}

		[Obsolete("The use of Sbb8(ref, mem) does not function properly.", true)]
		public void Sbb8(X86Register8 dest, X86Address src) {
			writer.Write(new byte[] { 0x1A });
			src.Emit(writer, dest);
		}
		public void Sbb16(X86Register16 dest, X86Address src) {
			writer.Write(new byte[] { 0x66, 0x1B });
			src.Emit(writer, dest);
		}
		public void Sbb32(X86Register32 dest, X86Address src) {
			writer.Write(new byte[] { 0x1B });
			src.Emit(writer, dest);
		}

		public void Sbb8(X86Register8 dest, Byte value) {
			this.writer.Write(new byte[] { 0x80 });
			reg_emit8((X86Register8)3, dest);
			writer.Write(value);
		}
		public void Sbb16(X86Register16 dest, Int16 value) {
			this.writer.Write(new byte[] { 0x66, 0x81 });
			reg_emit16((X86Register16)3, dest);
			writer.Write(value);
		}
		public void Sbb32(X86Register32 dest, Int32 value) {
			this.writer.Write(new byte[] { 0x81 });
			reg_emit32((X86Register32)3, dest);
			writer.Write(value);
		}

		public void Sbb8(X86Address dest, Byte value) {
			this.writer.Write(new byte[] { 0x80 });
			dest.Emit(writer, (X86Register32)3);
			writer.Write(value);
		}
		public void Sbb16(X86Address dest, Int16 value) {
			this.writer.Write(new byte[] { 0x66, 0x81 });
			dest.Emit(writer, (X86Register32)3);
			writer.Write(value);
		}
		public void Sbb32(X86Address dest, Int32 value) {
			this.writer.Write(new byte[] { 0x81 });
			dest.Emit(writer, (X86Register32)3);
			writer.Write(value);
		}
	}
}
