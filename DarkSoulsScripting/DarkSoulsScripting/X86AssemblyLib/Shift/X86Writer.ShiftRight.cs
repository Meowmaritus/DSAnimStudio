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
		public void Shr8(X86Register8 reg, byte count) {
			if (count == 1) {
				writer.Write(new byte[] { 0xD0 });
				reg_emit8((X86Register8)5, reg);
			} else {
				writer.Write(new byte[] { 0xC0 });
				reg_emit8((X86Register8)5, reg);
				imm_emit8(count);
			}
		}
		public void Shr16(X86Register16 reg, byte count) {
			if (count == 1) {
				writer.Write(new byte[] { 0x66, 0xD1 });
				reg_emit16((X86Register16)5, reg);
			} else {
				writer.Write(new byte[] { 0x66, 0xC1 });
				reg_emit16((X86Register16)5, reg);
				imm_emit8(count);
			}
		}
		public void Shr32(X86Register32 reg, byte count) {
			if (count == 1) {
				writer.Write(new byte[] { 0xD1 });
				reg_emit32((X86Register32)5, reg);
			} else {
				writer.Write(new byte[] { 0xC1 });
				reg_emit32((X86Register32)5, reg);
				imm_emit8(count);
			}
		}

		public void Shr8(X86Address mem, byte count) {
			if (count == 1) {
				writer.Write(new byte[] { 0xD0 });
				mem.Emit(writer, (X86Register8)4);
			} else {
				writer.Write(new byte[] { 0xC0 });
				mem.Emit(writer, (X86Register8)4);
				imm_emit8(count);
			}
		}
		public void Shr16(X86Address mem, byte count) {
			if (count == 1) {
				writer.Write(new byte[] { 0x66, 0xD1 });
				mem.Emit(writer, (X86Register8)5);
			} else {
				writer.Write(new byte[] { 0x66, 0xC1 });
				mem.Emit(writer, (X86Register8)5);
				imm_emit8(count);
			}
		}
		public void Shr32(X86Address mem, byte count) {
			if (count == 1) {
				writer.Write(new byte[] { 0xD1 });
				mem.Emit(writer, (X86Register8)5);
			} else {
				writer.Write(new byte[] { 0xC1 });
				mem.Emit(writer, (X86Register8)5);
				imm_emit8(count);
			}
		}

		public void Shr8(X86Register8 reg) {
			writer.Write(new byte[] { 0xD2 });
			reg_emit8((X86Register8)5, reg);
		}
		public void Shr16(X86Register16 reg) {
			writer.Write(new byte[] { 0x66, 0xD3 });
			reg_emit16((X86Register16)5, reg);
		}
		public void Shr32(X86Register32 reg) {
			writer.Write(new byte[] { 0xD3 });
			reg_emit32((X86Register32)5, reg);
		}

		public void Shr8(X86Address mem) {
			writer.Write(new byte[] { 0xD2 });
			mem.Emit(writer, (X86Register8)5);
		}
		public void Shr16(X86Address mem) {
			writer.Write(new byte[] { 0x66, 0xD3 });
			mem.Emit(writer, (X86Register8)5);
		}
		public void Shr32(X86Address mem) {
			writer.Write(new byte[] { 0xD3 });
			mem.Emit(writer, (X86Register8)5);
		}

		public void Shr64(X86Register32 left, X86Register32 right, byte count) {
			writer.Write(new byte[] { 0x0F, 0xAC });
			reg_emit32(right, left);
			imm_emit8(count);
		}
		public void Shr64(X86Address left, X86Register32 right, byte count) {
			writer.Write(new byte[] { 0x0F, 0xAC });
			left.Emit(writer, right);
			imm_emit8(count);
		}

		public void Shr64(X86Register32 left, X86Register32 right) {
			writer.Write(new byte[] { 0x0F, 0xAD });
			reg_emit32(right, left);
		}
		public void Shr64(X86Address left, X86Register32 right) {
			writer.Write(new byte[] { 0x0F, 0xAD });
			left.Emit(writer, right);
		}
	}
}
