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
    public enum X86ConditionCode {
		Overflow = 0x0,

		NoOverflow = 0x1,
		
		Below = 0x2,
		NotAboveOrEqual = 0x2,
		
		NotBelow = 0x3,
		AboveOrEqual = 0x3,
		
		Equal = 0x4,
		Zero = 0x4,
		
		NotEqual = 0x5,
		NotZero = 0x5,

		BelowOrEqual = 0x6,
		NotAbove = 0x6,

		NotBelowOrEqual = 0x7,
		Above = 0x7,

		Sign = 0x8,
		
		NotSign = 0x9,

		Parity = 0xA,
		ParityEven = 0xA,

		NotParity = 0xB,
		ParityOdd = 0xB,

		LessThan = 0xC,
		NotGreaterThanEqualTo = 0xC,

		GreaterThan = 0xD,
		NotLessThanEqualTo = 0xD,

		LessThanEqualTo = 0xE,
		NotGreaterThan = 0xE,

		GreaterThanEqualTo = 0xF,
		NotLessThan = 0xF,
	}
}
