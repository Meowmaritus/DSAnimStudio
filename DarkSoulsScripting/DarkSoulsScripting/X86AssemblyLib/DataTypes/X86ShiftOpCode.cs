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
    public enum X86ShiftOpCode {
		SHLD = 0, // TODO: Is this value correct?
		SHLR = 1, // TODO: Is this value correct?
		ROL = 0,
		ROR = 1,
		RCL = 2,
		RCR = 3,
		SHL = 4,
		SHR = 5,
		SAR = 7,
	}
}
