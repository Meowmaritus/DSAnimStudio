using DarkSoulsScripting.Injection;
using DarkSoulsScripting.Injection.Structures;
using Managed.X86;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting.CodeHookTypes
{
    public abstract class CodeHookBase : IDisposable
    {
        protected IntPtr OriginalLocalCodeStartOffset { get; set; }
        protected int OriginalLocalCodeLength { get; set; }
        protected int CustomLocalCodeAllocSize { get; set; }
        protected bool OriginalCodeAtEnd { get; set; }
        protected IntPtr CustomRemoteCodeReturnDestinationOffset => OriginalLocalCodeStartOffset + OriginalLocalCodeLength;
        protected byte[] OriginalLocalCode { get; set; }
        protected byte[] CustomLocalCode { get; set; }
        protected byte[] CustomRemoteCode { get; set; }
        protected SafeRemoteHandle CustomRemoteCodeHandle { get; private set; }
        protected bool IsPatched { get; private set; } = false;

        public CodeHookBase(IntPtr originalLocalCodeStart, int originalLocalCodeLength, int customLocalCodeAllocSize, bool originalCodeAtEnd = false, byte[] specificOriginalCode = null)
        {
            OriginalLocalCodeStartOffset = originalLocalCodeStart;
            OriginalLocalCodeLength = originalLocalCodeLength;
            CustomLocalCodeAllocSize = customLocalCodeAllocSize;
            OriginalCodeAtEnd = originalCodeAtEnd;

            CustomRemoteCodeHandle = new SafeRemoteHandle(CustomLocalCodeAllocSize);

            if (specificOriginalCode != null)
                OriginalLocalCode = specificOriginalCode;

            _buildAsmArrays();
        }

        private void _buildAsmArrays()
        {
            if (OriginalLocalCode == null)
                OriginalLocalCode = RBytes(OriginalLocalCodeStartOffset.ToInt64(), OriginalLocalCodeLength);

            using (var _customRemoteCodeMemoryStream = new MemoryStream(OriginalLocalCodeLength))
            {
                //Start building wrapped user code at the location immediately after the copy of the original 
                //code ends (unless OriginalCodeAtEnd is defined, in which case, it's at the beginning of CustomRemoteCodeHandle) :
                var customRemoteCodeCodeBuilder = new X86Writer(_customRemoteCodeMemoryStream, (IntPtr)(CustomRemoteCodeHandle.GetHandle().ToInt64()));

                //If OriginalCodeAtEnd is not defined, write the original code at the start of the custom remote code:
                if (!OriginalCodeAtEnd)
                    _customRemoteCodeMemoryStream.Write(OriginalLocalCode, 0, OriginalLocalCodeLength);

                BuildCustomRemoteCode(customRemoteCodeCodeBuilder, _customRemoteCodeMemoryStream);

                //If OriginalCodeAtEnd is defined, write the original code at the end of the custom remote code:
                if (OriginalCodeAtEnd)
                    _customRemoteCodeMemoryStream.Write(OriginalLocalCode, 0, OriginalLocalCodeLength);

                //Jump to return offset at end of wrapped function:
                customRemoteCodeCodeBuilder.Jmp(CustomRemoteCodeReturnDestinationOffset);

                //Copy the bytes of the ASM to our array before exiting the "using" statement and disposing the MemoryStream:
                CustomRemoteCode = _customRemoteCodeMemoryStream.ToArray();
            }

            using (var _customLocalCodeMemoryStream = new MemoryStream(OriginalLocalCodeLength))
            {
                var customLocalCodeCodeBuilder = new X86Writer(_customLocalCodeMemoryStream, OriginalLocalCodeStartOffset);

                // Jump to our wrapped code instead of doing the original code:
                customLocalCodeCodeBuilder.Jmp(CustomRemoteCodeHandle.GetHandle());

                // Fill in the rest of the original instructions with NOP's:
                while (_customLocalCodeMemoryStream.Position < OriginalLocalCodeLength)
                {
                    customLocalCodeCodeBuilder.Nop();
                }

                CustomLocalCode = _customLocalCodeMemoryStream.ToArray();
            }
        }

        public void PatchCode(bool force = false)
        {
            if (IsPatched && !force)
                return;

            //Write the wrapped code to the allocated memory region:
            var wrappedCodeHandle = CustomRemoteCodeHandle.GetHandle();
            WBytes((wrappedCodeHandle, wrappedCodeHandle), CustomRemoteCode);

            //Flush instruction cache for wrapped code:
            Kernel.FlushInstructionCache(DARKSOULS.GetHandle(), CustomRemoteCodeHandle.GetHandle(), (UIntPtr)CustomLocalCodeAllocSize);

            /*
             * 
             * >> The rest of this method: <<
             * 
             * Patch original local code while Dark Souls is suspended so that there will be a 0.0% chance of the 
             * game trying to execute only-partially-overridden code instead of the usual 0.00000001% chance ;)
             * 
             */

            //Suspend execution of Dark Souls process:
            DARKSOULS.Suspend();

            //Overwrite the original local code with the custom local code:
            WBytes((OriginalLocalCodeStartOffset, OriginalLocalCodeStartOffset), CustomLocalCode);

            //Flush the instruction cache over the span of the local code:
            Kernel.FlushInstructionCache(DARKSOULS.GetHandle(), OriginalLocalCodeStartOffset, (UIntPtr)OriginalLocalCodeLength);

            //Resume execution of Dark Souls process:
            DARKSOULS.Resume();

            IsPatched = true;
        }

        public void RestoreCode()
        {
            //Suspend execution of Dark Souls process:
            DARKSOULS.Suspend();

            //Restore the original local code:
            WBytes((OriginalLocalCodeStartOffset, OriginalLocalCodeStartOffset), OriginalLocalCode);

            //Flush the instruction cache over the span of the local code:
            Kernel.FlushInstructionCache(DARKSOULS.GetHandle(), OriginalLocalCodeStartOffset, (UIntPtr)OriginalLocalCodeLength);

            //Resume execution of Dark Souls process:
            DARKSOULS.Resume();

            IsPatched = false;
        }

        protected abstract void BuildCustomRemoteCode(X86Writer w, MemoryStream ms);

        public virtual void Dispose()
        {
            RestoreCode();
            OriginalLocalCode = null;
            CustomLocalCode = null;
            CustomRemoteCode = null;
            CustomRemoteCodeHandle?.Dispose();
        }

        #region LENGTHY_EXPLANATION
        /*
            BEFORE WRAP:

                "DARKSOULS.exe"+BAE780: cvtss2sd xmm0,xmm1
                #### OriginalLocalCode Start: ######################
                ## "DARKSOULS.exe"+BAE784: mov edx,[esi]          ##
                ## "DARKSOULS.exe"+BAE786: mov eax,[edx+000000A0] ##
                ####################################################
                (ReturnOffset points to the following instruction, immediately after the end of the original code)
                "DARKSOULS.exe"+BAE78C: cvtsd2ss xmm0,xmm0

            AFTER WRAP:

                "DARKSOULS.exe"+BAE780: cvtss2sd xmm0,xmm1
                #### CustomLocalCode Start: ########################
                ## "DARKSOULS.exe"+BAE784: jmp WrappedCodeHandle  ##
                ## "DARKSOULS.exe"+BAE789: nop                    ##
                ## "DARKSOULS.exe"+BAE78A: nop                    ##
                ## "DARKSOULS.exe"+BAE78B: nop                    ##
                ####################################################
                (ReturnOffset points to the following instruction, immediately after the end of the original code)
                "DARKSOULS.exe"+BAE78C: cvtsd2ss xmm0,xmm0
                (...)
                #### CustomRemoteCode Start: #######################
                ##                                                ##
                ## --Copy of Original Code--                      ##
                ## mov edx,[esi]                                  ##
                ## mov eax,[edx+000000A0]                         ##
                ##                                                ##
                ## (USER CODE GOES HERE)                          ##
                ##                                                ##
                ## jmp ReturnOffset                               ##
                ####################################################
         */
        #endregion
    }
}
