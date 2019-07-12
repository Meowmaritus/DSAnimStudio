using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections.ObjectModel;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;

namespace DarkSoulsScripting.Injection.Structures
{

    public class SafeDarkSoulsHandle : SafeHandleZeroOrMinusOneIsInvalid
    {

        internal event OnDetachEventHandler OnDetach;
        internal delegate void OnDetachEventHandler();
        internal event OnAttachEventHandler OnAttach;
        internal delegate void OnAttachEventHandler();

        public ReadOnlyDictionary<string, List<uint>> ModuleOffsets { get; private set; }

        public readonly int SafeBaseMemoryOffset = 0x400000;

        public static readonly DarkSoulsVersion[] CompatibleVersions = new DarkSoulsVersion[] 
        {
            DarkSoulsVersion.LatestRelease,
            DarkSoulsVersion.Debug,
        };

        public bool Attached
        {
            get { return (!IsClosed) && (!IsInvalid); }
        }

        public DarkSoulsVersion Version { get; private set;  }
        public string VersionDisplayName
        {
            get
            {
                switch (Version)
                {
                    case DarkSoulsVersion.None:
                        return "None";
                    case DarkSoulsVersion.LatestRelease:
                        return "Latest Steam Release";
                    case DarkSoulsVersion.SteamWorksBeta:
                        return "Steamworks December 2014 Beta (Incompatible)";
                    case DarkSoulsVersion.AncientGFWL:
                        return "Games for Windows Live Release (Incompatible)";
                }
                return Version.ToString();
                //Shut up compiler
            }
        }

        public int ProcessID { get; private set; } = -1;

        public SafeDarkSoulsHandle() : base(true)
        {
        }

        public IntPtr GetHandle()
        {
            return handle;
        }

        private uint GetIngameDllAddress(string moduleName)
        {
            uint[] modules = new uint[255];
            uint cbNeeded = 0;
            PSAPI.EnumProcessModules(Hook.DARKSOULS.GetHandle(), modules, 4 * 1024, ref cbNeeded);

            uint numModules = (uint)(cbNeeded / IntPtr.Size);


            for (int i = 0; i <= numModules - 1; i++)
            {
                var disModule = new IntPtr(modules[i]);
                System.Text.StringBuilder name = new System.Text.StringBuilder();
                PSAPI.GetModuleBaseName(Hook.DARKSOULS.GetHandle(), disModule, name, 255);

                if ((name.ToString().ToUpper().Equals(moduleName.ToUpper())))
                {
                    return modules[i];
                }

            }

            return 0;
        }

        public bool TryAttachToDarkSouls(out string errorMsg)
        {
            errorMsg = null;
            Process selectedProcess = null;
            Process[] _allProcesses = Process.GetProcesses();
            try
            {
                var potentialProcesses = new List<Process>();
                foreach (Process proc in _allProcesses)
                {
                    if (proc.MainWindowTitle.ToUpper().Equals("DARK SOULS"))
                    {
                        potentialProcesses.Add(proc);
                    }
                }

                if (potentialProcesses.Count == 0)
                {
                    errorMsg = "Unable to find any process likely to be Dark Souls (i.e. has \"DARK SOULS\" in the title bar).";
                    return false;
                }
                else if (potentialProcesses.Count == 1)
                {
                    selectedProcess = potentialProcesses[0];
                }
                else if (potentialProcesses.Count >= 2)
                {
                    var mostObviousChoice = potentialProcesses.FirstOrDefault(x => x.ProcessName.ToUpper() == "DARKSOULS");
                    if (mostObviousChoice != default(Process))
                    {
                        selectedProcess = mostObviousChoice;
                        potentialProcesses.Clear();
                        potentialProcesses.Add(selectedProcess);
                        Console.WriteLine("Note: Multiple candidates found for Dark Souls process, but the one named \"DARKSOULS\" was chosen automatically.");
                    }
                    else
                    {
                        var purgedList = new List<Process>();
                        foreach (var p in potentialProcesses)
                        {
                            //Set this process as the active process for the Kernel32 read/write functions in Hook to use
                            SetHandle((IntPtr)Kernel.OpenProcess(Kernel.PROCESS_ALL_ACCESS, false, p.Id));
                            //Run the hook check.
                            CheckHook();
                            //If CheckHook() fails, the process will be detached afterward. If it's still attached, add it to the list of still-valid processes.
                            if (Attached)
                                purgedList.Add(p);
                            else
                                ReleaseHandle();
                        }
                        potentialProcesses = purgedList;
                    }

                    if (potentialProcesses.Count == 0)
                    {
                        errorMsg = "Found one or more processes likely to be Dark Souls (i.e. had \"DARK SOULS\" in the title bar)\n" +
                            "but none of them passed the byte matching check and as such none were confirmed as valid.\n" +
                            "Obviously, if you're running a valid Dark Souls process currently and get this message, there is a bug that needs to be fixed.";
                        return false;
                    }
                    else if (potentialProcesses.Count == 1)
                    {
                        selectedProcess = potentialProcesses[0];
                    }
                    else if (potentialProcesses.Count >= 2)
                    {
                        errorMsg = $"Found {potentialProcesses.Count} valid Dark Souls processes running. Impossible to know which one you want to hook to\n" +
                            "(and impossible to ask you which one you want to hook to, since they all have the same name).\nPlease close all but 1 and try again." +
                            "\n\nNote: If you need to have multiple instances of the game open for \"educational purposes\", then you can make the executable you\n" +
                            "want to hook to named \"DARKSOULS.exe\" and it will be chosen instead of showing this error message.";
                        return false;
                    }
                }

                if (selectedProcess != null)
                {
                    ProcessID = selectedProcess.Id;
                    SetHandle((IntPtr)Kernel.OpenProcess(Kernel.PROCESS_ALL_ACCESS, false, selectedProcess.Id));
                    CheckHook();
                    Dictionary<string, List<uint>> modulesInputDict = new Dictionary<string, List<uint>>();

                    if (Attached)
                    {
                        foreach (ProcessModule dll in selectedProcess.Modules)
                        {
                            string indexName = dll.ModuleName.ToUpper();
                            if (modulesInputDict.ContainsKey(indexName))
                            {
                                modulesInputDict[indexName].Add((uint)dll.BaseAddress);
                            }
                            else
                            {
                                modulesInputDict.Add(indexName, new uint[] { (uint)dll.BaseAddress }.ToList());
                            }
                        }
                    }

                    ModuleOffsets = new ReadOnlyDictionary<string, List<uint>>(modulesInputDict);
                }
                else
                {
                    errorMsg = "Could not find any valid Dark Souls process.";
                    ReleaseHandle();
                    return false;
                }

                if (!Attached)
                {
                    errorMsg = "Found Dark Souls process but failed to attach to it.\nTry explicitly running your program (or scripting environment) as an administrator.";
                    ReleaseHandle();
                    return false;
                }
                else
                {
                    OnAttach?.Invoke();
                    return true;
                }
            }
            catch (Exception e)
            {
                errorMsg = "Encountered the following exception while trying to attach to Dark Souls process:\n\n" + e.Message;
                return false;
            }
            finally
            {
                foreach (var p in _allProcesses)
                {
                    p?.Dispose();
                }
            }
        }

        private void CheckHook()
        {
            Version = DarkSoulsVersion.LatestRelease;
            var versionFlagThing = Hook.RUInt32((0x400080, 0x400080));
            if (versionFlagThing == 0xFC293654u)
            {
                Version = DarkSoulsVersion.LatestRelease;

                if (!Kernel.VirtualProtectEx(handle, (IntPtr)(0x10CC000), (UIntPtr)(0x1DE000), Kernel.PAGE_EXECUTE_READWRITE, out _))
                {
                    throw new Exception("VirtualProtectEx Returned False");
                }

                if (!Kernel.FlushInstructionCache(handle, (IntPtr)0x10CC000, (UIntPtr)0x1DE000))
                {
                    throw new Exception("FlushInstructionCache Returned False");
                }

                Kernel.WriteProcessMemory_SAFE(handle, 0xBE73FE, new byte[] { 0x20 }, 1, 0);
                Kernel.WriteProcessMemory_SAFE(handle, 0xBE719F, new byte[] { 0x20 }, 1, 0);
                Kernel.WriteProcessMemory_SAFE(handle, 0xBE722B, new byte[] { 0x20 }, 1, 0);

                WorldState.Autosave = false;
            }
            else if (versionFlagThing == 0xCE9634B4u)
            {
                Version = DarkSoulsVersion.Debug;

                if (!Kernel.VirtualProtectEx(handle, (IntPtr)(0x10D1000), (UIntPtr)(0x1DD000), Kernel.PAGE_EXECUTE_READWRITE, out _))
                {
                    throw new Exception("VirtualProtectEx Returned False");
                }

                if (!Kernel.FlushInstructionCache(handle, (IntPtr)0x10D1000, (UIntPtr)0x1DD000))
                {
                    throw new Exception("FlushInstructionCache Returned False");
                }
            }
            else if (versionFlagThing == 0xE91B11E2u)
            {
                Version = DarkSoulsVersion.SteamWorksBeta;
            }
            /*
            else if (versionFlagThing == 0x????????u)
            {
                Version = DarkSoulsVersion.AncientGFWL;
            }
            */
            else
            {
                Version = DarkSoulsVersion.None;
            }

            if (!CompatibleVersions.Contains(Version))
            {
                Close();
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            OnDetach?.Invoke();
            return Kernel.CloseHandle(handle);
        }

        public void Suspend()
        {
            Kernel.SuspendProcess(ProcessID);
        }

        public void Resume()
        {
            Kernel.ResumeProcess(ProcessID);
        }

    }

}
