using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DSAnimStudio.NewChrAsm;
using static DSAnimStudio.ParamData;

namespace DSAnimStudio
{
    public abstract class NewEquipSlot : IDisposable
    {
        public NewChrAsm ASM;
        public readonly NewChrAsm.EquipSlotTypes EquipSlotType;
        public string SlotDisplayName;
        public string SlotDisplayNameShort;
        public bool UsesEquipParam;
        public int EquipID;
        public int lastEquipIDLoaded = -1;

        public NewEquipSlot(NewChrAsm asm, NewChrAsm.EquipSlotTypes equipSlot, string slotDisplayName, string slotDisplayNameShort, bool usesEquipParam)
        {
            ASM = asm;
            EquipSlotType = equipSlot;
            SlotDisplayName = slotDisplayName;
            SlotDisplayNameShort = slotDisplayNameShort;
            UsesEquipParam = usesEquipParam;
        }


        public bool CheckIfModelLoaded()
        {
            bool result = false;
            lock (_lock_MODEL)
            {
                result = model != null;
            }
            return result;
        }

        private object _lock_MODEL = new object();
        private Model model = null;

        public abstract void AccessAllModels(Action<Model> doAction);
        
        public abstract void TryToLoadTextures();

        protected abstract void InnerDispose();
        
        public void Dispose()
        {
            InnerDispose();
        }
    }
}
