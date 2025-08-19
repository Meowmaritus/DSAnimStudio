namespace FSBANKLOL;

public struct FSBANK_STATEDATA_WARNING
{
	public FSBANK_RESULT warnCode;

	public unsafe fixed char warningString[256];
}
