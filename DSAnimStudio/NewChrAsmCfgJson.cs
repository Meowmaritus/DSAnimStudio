using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewChrAsmCfgJson
    {
        public GameDataManager.GameTypes GameType;
        public int HeadID = -1;
        public int BodyID = -1;
        public int ArmsID = -1;
        public int LegsID = -1;
        public int RightWeaponID = -1;
        //public int RightWeaponModelIndex = 0;
        public int LeftWeaponID = -1;
        //public int LeftWeaponModelIndex = 0;
        public bool LeftWeaponFlipBackwards = true;
        public bool LeftWeaponFlipSideways = true;
        public bool RightWeaponFlipBackwards = true;
        public bool RightWeaponFlipSideways = false;
        public NewChrAsm.WeaponStyleType WeaponStyle = NewChrAsm.WeaponStyleType.OneHand;

        public void WriteToChrAsm(NewChrAsm chrAsm)
        {
            chrAsm.HeadID = HeadID;
            chrAsm.BodyID = BodyID;
            chrAsm.ArmsID = ArmsID;
            chrAsm.LegsID = LegsID;
            chrAsm.RightWeaponID = RightWeaponID;
            //chrAsm.RightWeaponModelIndex = RightWeaponModelIndex;
            chrAsm.LeftWeaponID = LeftWeaponID;
            //chrAsm.LeftWeaponModelIndex = LeftWeaponModelIndex;
            chrAsm.LeftWeaponFlipBackwards = LeftWeaponFlipBackwards;
            chrAsm.LeftWeaponFlipSideways = LeftWeaponFlipSideways;
            chrAsm.RightWeaponFlipBackwards = RightWeaponFlipBackwards;
            chrAsm.RightWeaponFlipSideways = RightWeaponFlipSideways;
            chrAsm.StartWeaponStyle = WeaponStyle;
            //chrAsm.UpdateModels();
        }

        public void CopyFromChrAsm(NewChrAsm chrAsm)
        {
            GameType = GameDataManager.GameType;
            HeadID = chrAsm.HeadID;
            BodyID = chrAsm.BodyID;
            ArmsID = chrAsm.ArmsID;
            LegsID = chrAsm.LegsID;
            RightWeaponID = chrAsm.RightWeaponID;
            //RightWeaponModelIndex = chrAsm.RightWeaponModelIndex;
            LeftWeaponID = chrAsm.LeftWeaponID;
            //LeftWeaponModelIndex = chrAsm.LeftWeaponModelIndex;
            LeftWeaponFlipBackwards = chrAsm.LeftWeaponFlipBackwards;
            LeftWeaponFlipSideways = chrAsm.LeftWeaponFlipSideways;
            RightWeaponFlipBackwards = chrAsm.RightWeaponFlipBackwards;
            RightWeaponFlipSideways = chrAsm.RightWeaponFlipSideways;
            WeaponStyle = chrAsm.StartWeaponStyle;
        }
    }
}
