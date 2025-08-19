using System;
using System.Runtime.InteropServices;

namespace FSBANKLOL;

public struct FSBANK_SUBSOUND
{
	public IntPtr fileNames;

	public IntPtr fileData;

	public IntPtr fileDataLengths;

	public uint numFiles;

	public BUILDFLAGS overrideFlags;

	public uint overrideQuality;

	public float desiredSampleRate;

	public float percentOptimizedRate;

	public void filenames_external(string name)
	{
		fileNames = Marshal.StringToBSTR(name);
	}
}
