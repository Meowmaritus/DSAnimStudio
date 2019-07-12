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
		/// <summary>
		/// out imm
		/// 
		/// Writes data from the specified I/O port, with the value from the AL register
		/// </summary>
		public void Out8(Byte port) {
			writer.Write(new byte[] { 0xE6 });
			writer.Write(port);
		}
		/// <summary>
		/// out imm
		/// 
		/// Writes data from the specified I/O port, with the value from the AX register
		/// </summary>
		public void Out16(Byte port) {
			writer.Write(new byte[] { 0x66, 0xE7 });
			writer.Write(port);
		}
		/// <summary>
		/// out imm
		/// 
		/// Writes data from the specified I/O port, with the value from the EAX register
		/// </summary>
		public void Out32(Byte port) {
			writer.Write(new byte[] { 0xE7 });
			writer.Write(port);
		}

		/// <summary>
		/// out
		/// 
		/// Writes data from an I/O port specified in DX, with the value from the AL register
		/// </summary>
		public void Out8() {
			writer.Write(new byte[] { 0xEE });
		}
		/// <summary>
		/// out
		/// 
		/// Writes data from an I/O port specified in DX, with the value from the AX register
		/// </summary>
		public void Out16() {
			writer.Write(new byte[] { 0x66, 0xEF });
		}
		/// <summary>
		/// out
		/// 
		/// Writes data from an I/O port specified in DX, with the value from the EAX register
		/// </summary>
		public void Out32() {
			writer.Write(new byte[] { 0xEF });
		}
	}
}
