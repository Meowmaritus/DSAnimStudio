using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FSBANKLOL;

public struct StringWrapper
{
	private IntPtr nativeUtf8Ptr;

	public static implicit operator string(StringWrapper fstring)
	{
		if (fstring.nativeUtf8Ptr == IntPtr.Zero)
		{
			return "";
		}
		int i;
		for (i = 0; Marshal.ReadByte(fstring.nativeUtf8Ptr, i) != 0; i++)
		{
		}
		if (i > 0)
		{
			byte[] array = new byte[i];
			Marshal.Copy(fstring.nativeUtf8Ptr, array, 0, i);
			return Encoding.UTF8.GetString(array, 0, i);
		}
		return "";
	}
}
