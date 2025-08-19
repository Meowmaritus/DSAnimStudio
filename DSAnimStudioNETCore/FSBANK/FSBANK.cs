using System;
using System.Runtime.InteropServices;

namespace FSBANKLOL;

public class FSBANK
{
	[DllImport("fsbank.dll")]
	public static extern FSBANK_RESULT FSBank_MemoryInit(FSBANK_MEMORY_ALLOC_CALLBACK userAlloc, FSBANK_MEMORY_REALLOC_CALLBACK userRealloc, FSBANK_MEMORY_FREE_CALLBACK userFree);

	[DllImport("fsbank.dll")]
	public static extern FSBANK_RESULT FSBank_Init(FSBVERSION version, INITFLAGS flags, uint numSimultaneousJobs, IntPtr cacheDirectory);

	[DllImport("fsbank.dll")]
	public static extern FSBANK_RESULT FSBank_Release();

	[DllImport("fsbank.dll")]
	public static extern FSBANK_RESULT FSBank_Build(IntPtr subSounds, uint numSubSounds, FORMAT encodeFormat, BUILDFLAGS buildFlags, uint quality, IntPtr encryptKey, IntPtr outputFileName);

	[DllImport("fsbank.dll")]
	public static extern FSBANK_RESULT FSBank_FetchFSBMemory(out IntPtr data, out uint length);

	[DllImport("fsbank.dll")]
	public static extern FSBANK_RESULT FSBank_BuildCancel();

	[DllImport("fsbank.dll")]
	public static extern FSBANK_RESULT FSBank_FetchNextProgressItem(out IntPtr progressItem);

	[DllImport("fsbank.dll")]
	public static extern FSBANK_RESULT FSBank_ReleaseProgressItem(IntPtr progressItem);

	[DllImport("fsbank.dll")]
	public static extern FSBANK_RESULT FSBank_MemoryGetStats(out uint currentAllocated, out uint maximumAllocated);
}
