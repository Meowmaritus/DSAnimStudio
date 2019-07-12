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
		public void Push32(X86Register32 reg) {
			writer.Write(new byte[] { 0xFF });
			reg_emit32((X86Register32)6, reg);
		}
		public void Push32(X86Address addr) {
			writer.Write(new byte[] { 0xFF });
			addr.Emit(writer, (X86Register8)6);
		}
		public void Push32(Int32 imm) {
			if (is_imm8(imm)) {
				writer.Write(new byte[] { 0x6A });
				imm_emit8(imm);
			} else {
				writer.Write(new byte[] { 0x68 });
				imm_emit32(imm);
			}
		}
	}
}
