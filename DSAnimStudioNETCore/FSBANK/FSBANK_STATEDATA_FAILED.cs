namespace FSBANKLOL;

public struct FSBANK_STATEDATA_FAILED
{
	public FSBANK_RESULT errorCode;

	public unsafe fixed char errorString[256];
}
