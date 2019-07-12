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
		/// in imm
		/// 
		/// Reads data from the specified I/O port, and sets the AL register
		/// </summary>
		public void In8(Byte port) {
			writer.Write(new byte[] { 0xE4 });
			writer.Write(port);
		}
		/// <summary>
		/// in imm
		/// 
		/// Reads data from the specified I/O port, and sets the AX register
		/// </summary>
		public void In16(Byte port) {
			writer.Write(new byte[] { 0x66, 0xE5 });
			writer.Write(port);
		}
		/// <summary>
		/// in imm
		/// 
		/// Reads data from the specified I/O port, and sets the EAX register
		/// </summary>
		public void In32(Byte port) {
			writer.Write(new byte[] { 0xE5 });
			writer.Write(port);
		}

		/// <summary>
		/// in
		/// 
		/// Reads data from an I/O port specified in DX, and sets the AL register
		/// </summary>
		public void In8() {
			writer.Write(new byte[] { 0xEC });
		}
		/// <summary>
		/// in
		/// 
		/// Reads data from an I/O port specified in DX, and sets the AX register
		/// </summary>
		public void In16() {
			writer.Write(new byte[] { 0x66, 0xED });
		}
		/// <summary>
		/// in
		/// 
		/// Reads data from an I/O port specified in DX, and sets the EAX register
		/// </summary>
		public void In32() {
			writer.Write(new byte[] { 0xED });
		}
	}
}
