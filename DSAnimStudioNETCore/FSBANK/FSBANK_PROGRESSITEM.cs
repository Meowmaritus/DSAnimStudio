using System;

namespace FSBANKLOL;

public struct FSBANK_PROGRESSITEM
{
	public int subSoundIndex;

	public int threadIndex;

	public STATE state;

	public IntPtr stateData;
}
