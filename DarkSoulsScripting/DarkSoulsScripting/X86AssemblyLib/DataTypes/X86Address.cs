/*
 * (c) 2008 The Managed.X86 Project
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Alex Lyman (<mailto:mail.alex.lyman@gmail.com>)
 */

using System;
using System.IO;

namespace Managed.X86
{
    public struct X86Address {
		public readonly X86Register32 BaseRegister;
		public readonly Int32 Offset;
		public readonly X86Register32 IndexRegister;
		public readonly Byte IndexShift;

		public X86Address(X86Register32 baseRegister, Int32 offset) {
			this.BaseRegister = baseRegister;
			this.Offset = offset;
			this.IndexRegister = X86Register32.None;
			this.IndexShift = 0;
		}

		public X86Address(X86Register32 baseRegister, Int32 offset, X86Register32 indexRegister, byte indexShift) {
			this.BaseRegister = baseRegister ;
			this.Offset = offset;
			this.IndexRegister = indexRegister;
			this.IndexShift = indexShift;
		}

		internal void Emit(BinaryWriter writer, X86Register8 otherReg) {
			this.Emit(writer, (byte)otherReg);
		}
		internal void Emit(BinaryWriter writer, X86Register16 otherReg) {
			this.Emit(writer, (byte)otherReg);
		}
		internal void Emit(BinaryWriter writer, X86Register32 otherReg) {
			this.Emit(writer, (byte)otherReg);
		}
		private void Emit(BinaryWriter writer, byte otherReg) {
			if (otherReg == 0xFF) { otherReg = 0; }
			if (IndexRegister == X86Register32.None) {
				if (this.BaseRegister == X86Register32.None) {
					mem_emit(
						writer,
						otherReg,
						this.Offset
					);
				} else {
					membase_emit(
						writer,
						otherReg,
						this.BaseRegister,
						this.Offset
					);
				}
			} else {
				memindex_emit(
					writer, 
					otherReg, 
					this.BaseRegister, 
					this.Offset, 
					this.IndexRegister, 
					this.IndexShift
				);
			}
		}

		private void address_byte(BinaryWriter writer, Byte m, Byte o, Byte r) {
			writer.Write(
				(byte)
					(((m & 0x03) << 6)
					| ((o & 0x07) << 3)
					| (r & 0x07)
					)
			);
		}
		private void mem_emit(BinaryWriter writer, Byte r, Int32 dest) {
			address_byte(writer, 0, (byte)r, 5);
			writer.Write(dest);
		}
		private void membase_emit(BinaryWriter writer, Byte r, X86Register32 basereg, Int32 disp) {
			do {
				if ((basereg) == X86Register32.ESP) {
					if ((disp) == 0) {
						address_byte(writer, 0, (byte)r, (byte)X86Register32.ESP);
						address_byte(writer, 0, (byte)X86Register32.ESP, (byte)X86Register32.ESP);
					} else if (X86Writer.is_imm8(disp)) {
						address_byte(writer, 1, (byte)r, (byte)X86Register32.ESP);
						address_byte(writer, 0, (byte)X86Register32.ESP, (byte)X86Register32.ESP);
						writer.Write((byte)(disp));
					} else {
						address_byte(writer, 2, (byte)r, (byte)X86Register32.ESP);
						address_byte(writer, 0, (byte)X86Register32.ESP, (byte)X86Register32.ESP);
						writer.Write(disp);
					}
					break;
				} else if (disp == 0 && basereg != X86Register32.EBP) {
					address_byte(writer, 0, (byte)r, (byte)basereg);
				} else if (X86Writer.is_imm8(disp)) {
					address_byte(writer, 1, (byte)r, (byte)basereg);
					writer.Write((byte)(disp));
				} else {
					address_byte(writer, 2, (byte)r, (byte)basereg);
					writer.Write(disp);
				}
			} while (false);
		}
		private void memindex_emit(BinaryWriter writer, Byte r, X86Register32 basereg, Int32 disp, X86Register32 indexreg, Byte shift) {
			if (basereg == X86Register32.None) {
				address_byte(writer, 0, (byte)r, 4);
				address_byte(writer, (byte)shift, (byte)indexreg, 5);
				writer.Write((Int32)disp);
			} else if (disp == 0 && basereg == X86Register32.EBP) {
				address_byte(writer, 0, (byte)r, 4);
				address_byte(writer, (byte)shift, (byte)indexreg, (byte)basereg);
			} else if (X86Writer.is_imm8(disp)) {
				address_byte(writer, 1, (byte)r, 4);
				address_byte(writer, (byte)shift, (byte)indexreg, (byte)basereg);
				writer.Write((Byte)disp);
			} else {
				address_byte(writer, 2, (byte)r, 4);
				address_byte(writer, (byte)shift, (byte)indexreg, 5);
				writer.Write((Int32)disp);
			}
		}

		public override string ToString() {
			if (IndexRegister == X86Register32.None) {
				if (this.BaseRegister == X86Register32.None) {
					return String.Format("[{0:X}]", this.Offset);
				} else {
					return String.Format(
						"[{0}+{1}]",
						this.BaseRegister,
						this.Offset
					).Replace("+-", "-");
				}
			} else {
				return String.Format(
					"[{0}+{1}+{2}*{3}]",
					this.BaseRegister,
					this.Offset,
					this.IndexRegister,
					1 << this.IndexShift
				).Replace("+-", "-");
			}
		}
	}
}