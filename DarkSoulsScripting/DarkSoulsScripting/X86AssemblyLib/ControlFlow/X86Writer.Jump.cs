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
		public void Jmp(Int32 displacement) {
			writer.Write(new byte[] { 0xE9 });
			writer.Write(displacement - 5);
		}
		public void Jmp(IntPtr target) {
			// Just make the displacement.
			Jmp(target.ToInt32() - this.Position.ToInt32());
		}
		public void Jmp(X86Label label) {
			if (label.IsMarked) {
				Jmp(label.Position);
			} else {
				label.AddPatchRequired();
				writer.Write(new byte[] { 0xE9 });
				writer.Write(0xDEADBEEF);
			}
		}
		public void Jmp(X86Register32 register) {
			this.writer.Write(new byte[] { 0xFF });
			reg_emit32((X86Register32)0x4, register);
			//this.writer.Write(direct.ToInt32());
		}
		public void Jmp(X86Address address) {
			this.writer.Write(new byte[] { 0xFF });
			address.Emit(writer, (X86Register32)0x4);
		}
		public void JmpFar(X86Address address) {
			this.writer.Write(new byte[] { 0xFF });
			address.Emit(writer, (X86Register32)0x5);
		}

		public void Jmp(X86ConditionCode cond, X86Label label) {
			if (label.IsMarked) {
				Jmp(cond, label.Position.ToInt32() - this.Position.ToInt32());
			} else {
				label.AddPatchRequired();
				writer.Write(new byte[] { 0x0F, (byte)(0x80 | (byte)cond) });
				writer.Write(0xDEADBEEF);
			}
		}

		public void Jmp(X86ConditionCode cond, Int32 displacement) {
			if (is_imm8(displacement - 2)) {
				writer.Write(new byte[] { (byte)(0x70 | (byte)cond) });
				imm_emit8(displacement - 2);
			} else {
				writer.Write(new byte[] { 0x0F, (byte)(0x80 | (byte)cond) });
				imm_emit32(displacement - 6);
			}
		}
	}
}
