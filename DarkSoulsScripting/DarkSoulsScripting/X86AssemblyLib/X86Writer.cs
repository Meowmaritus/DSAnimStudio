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
    public partial class X86Writer {
		readonly Stream stream;
		readonly BinaryWriter writer;
		readonly IntPtr baseAddress;

		public X86Writer(Stream stream, IntPtr baseAddress) {
			this.stream = stream;
			this.writer = new BinaryWriter(stream);
			this.baseAddress = baseAddress;
		}

        public void RawAsmBytes(byte[] b)
        {
            writer.Write(b);
        }

		public IntPtr Position {
			get {
				return new IntPtr(baseAddress.ToInt32() + stream.Position);
			}
			set {
				stream.Seek(
					value.ToInt64() - baseAddress.ToInt64(),
					SeekOrigin.Begin
				);
			}
		}

		#region Utilities

		private void address_byte(Byte m, Byte o, Byte r) {
			this.writer.Write(
				(byte)
					(((m & 0x03) << 6)
					| ((o & 0x07) << 3)
					| (r & 0x07)
					)
			);
		}
		private void reg_emit32(X86Register32 dest, X86Register32 src) {
			address_byte(3, (byte)(dest), (byte)(src));
		}
		private void reg_emit16(X86Register16 dest, X86Register16 src) {
			address_byte(3, (byte)(dest), (byte)(src));
		}
		private void reg_emit8(X86Register8 dest, X86Register8 src) {
			address_byte(3, (byte)(dest), (byte)(src));
		}

		private void imm_emit8(Int32 value) { writer.Write((Byte)value); }
		private void imm_emit16(Int32 value) { writer.Write((Int16)value); }
		private void imm_emit32(Int32 value) { writer.Write((Int32)value); }

		internal static bool is_imm8(Int32 imm) { return (((int)(imm) >= -128 && (int)(imm) <= 127)); }
		internal static bool is_imm16(Int32 imm) { return (((int)(imm) >= -(1 << 16) && (int)(imm) <= ((1 << 16) - 1))); }

		#endregion

		public X86Label CreateLabel() { return new X86Label(this); }
		public X86Label CreateLabel(IntPtr position) { return new X86Label(this, position); }
		public X86Label CreateLabel(int offset) { return new X86Label(this, new IntPtr(this.Position.ToInt32() + offset)); }

		public void Nop() { writer.Write(new byte[] { 0x90 }); }

		public void CpuId() { writer.Write(new byte[] { 0x0F, 0xA2 }); }

		public void ClearCarryFlag() { writer.Write(new byte[] { 0xF8 }); }
		public void ClearDirectionFlag() { writer.Write(new byte[] { 0xFC }); }
		public void ClearInteruptFlag() { writer.Write(new byte[] { 0xFA }); }
		public void ComplementCarryFlag() { writer.Write(new byte[] { 0xF5 }); }

		public void ByteSwap(X86Register32 reg) {
			writer.Write(
				new byte[] {
					0x0F,
					(byte)(0xC8 | (byte)reg)
				}
			);
		}

		public void Halt() { writer.Write(new byte[] { 0xF4 }); }

		public void Int(byte type) {
			writer.Write(new byte[] { 0xCD });
			writer.Write(type);
		}

		public void Return() { writer.Write(new byte[] { 0xCB }); }
		public void Return(Int16 stackDisp) {
			writer.Write(new byte[] { 0xCA });
			writer.Write(stackDisp);
		}

		public void Retn() {
			writer.Write(new byte[] { 0xC3 });
		}

	}

}
