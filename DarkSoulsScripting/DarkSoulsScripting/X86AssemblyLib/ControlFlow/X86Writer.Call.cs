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
		public void Call(Int32 displacement) {
			writer.Write(new byte[] { 0xE8 });
			writer.Write(displacement - 5);
		}
		public void Call(IntPtr target) {
			// Just make the displacement.
			Call(target.ToInt32() - this.Position.ToInt32());
		}
		public void Call(X86Label label) {
			if (label.IsMarked) {
				Call(label.Position);
			} else {
				label.AddPatchRequired();
				writer.Write(new byte[] { 0xE8 });
				writer.Write(0xDEADBEEF);
			}
		}
		public void Call(X86Register32 register) {
			this.writer.Write(new byte[] { 0xFF });
			reg_emit32((X86Register32)0x2, register);
			//this.writer.Write(direct.ToInt32());
		}
		public void Call(X86Address address) {
			this.writer.Write(new byte[] { 0xFF });
			address.Emit(writer, (X86Register32)0x2);
		}
		public void Call_Far(X86Address address) {
			this.writer.Write(new byte[] { 0xFF });
			address.Emit(writer, (X86Register32)0x3);
		}
	}
}
