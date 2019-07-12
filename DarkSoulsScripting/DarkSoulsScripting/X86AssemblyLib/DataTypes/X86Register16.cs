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
    public enum X86Register16 : byte {
		None = 0xff,

		AX = 0,
		CX = 1,
		DX = 2,
		BX = 3,
		SP = 4,
		BP = 5,
		SI = 6,
		DI = 7,
	}
}
