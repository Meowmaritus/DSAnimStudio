using System;

namespace FSBANKLOL;

public delegate IntPtr FSBANK_MEMORY_REALLOC_CALLBACK(IntPtr ptr, uint size, StringWrapper sourceStr);
