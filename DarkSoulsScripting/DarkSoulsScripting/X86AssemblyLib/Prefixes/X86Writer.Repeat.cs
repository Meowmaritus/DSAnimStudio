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
		public void Repeat() { writer.Write(new byte[] { 0xF3 }); }
		public void RepeatZero() { writer.Write(new byte[] { 0xF3 }); }
		public void RepeatEqual() { writer.Write(new byte[] { 0xF3 }); }

		public void RepeatNotZero() { writer.Write(new byte[] { 0xF2 }); }
		public void RepeatNotEqual() { writer.Write(new byte[] { 0xF2 }); }
	}
}
