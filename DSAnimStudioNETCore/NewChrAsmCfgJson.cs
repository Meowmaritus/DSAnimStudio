using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewChrAsmCfgJson
    {
        public SoulsAssetPipeline.SoulsGames GameType;
        public bool IsFemale;
        public int HeadID = -1;
        public int BodyID = -1;
        public int ArmsID = -1;
        public int LegsID = -1;
        public int RightWeaponID = -1;
        //public int RightWeaponModelIndex = 0;
        public int LeftWeaponID = -1;
        //public int LeftWeaponModelIndex = 0;
        //public bool LeftWeaponFlipBackwards = true;
        //public bool LeftWeaponFlipSideways = true;
        //public bool RightWeaponFlipBackwards = true;
        //public bool RightWeaponFlipSideways = false;
        public NewChrAsm.WeaponStyleType WeaponStyle = NewChrAsm.WeaponStyleType.OneHand;
        public string FaceName_Male = "";
        public string FaceName_Female = "";
        public string FacegenName = "";

        public string HairName_Male = "";
        public string HairName_Female = "";


        private Dictionary<FlverMaterial.ChrCustomizeTypes, Vector4> chrCustomize = new Dictionary<FlverMaterial.ChrCustomizeTypes, Vector4>();
        private object _lock = new object();
        public Dictionary<FlverMaterial.ChrCustomizeTypes, Vector4> ChrCustomize
        {
            get
            {
                Dictionary<FlverMaterial.ChrCustomizeTypes, Vector4> result = null;
                lock (_lock)
                {
                    result = chrCustomize.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                return result;
            }
            set
            {
                lock (_lock)
                {
                    chrCustomize = value.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }
        }

        public void WriteToChrAsm(NewChrAsm chrAsm)
        {
            chrAsm.IsFemale = IsFemale;
            chrAsm.HeadID = HeadID;
            chrAsm.BodyID = BodyID;
            chrAsm.ArmsID = ArmsID;
            chrAsm.LegsID = LegsID;
            chrAsm.RightWeaponID = RightWeaponID;
            //chrAsm.RightWeaponModelIndex = RightWeaponModelIndex;
            chrAsm.LeftWeaponID = LeftWeaponID;
            //chrAsm.LeftWeaponModelIndex = LeftWeaponModelIndex;
            //chrAsm.LeftWeaponFlipBackwards = LeftWeaponFlipBackwards;
            //chrAsm.LeftWeaponFlipSideways = LeftWeaponFlipSideways;
            //chrAsm.RightWeaponFlipBackwards = RightWeaponFlipBackwards;
            //chrAsm.RightWeaponFlipSideways = RightWeaponFlipSideways;
            chrAsm.StartWeaponStyle = WeaponStyle;
            var faces = chrAsm.PossibleFaceModels;
            var facegens = chrAsm.PossibleFacegenModels;
            var hairs = chrAsm.PossibleHairModels;
            chrAsm.FaceIndex_Male = faces.IndexOf(FaceName_Male);
            chrAsm.FaceIndex_Female = faces.IndexOf(FaceName_Female);
            if (chrAsm.FaceIndex < 0)
                chrAsm.FaceIndex = chrAsm.GetDefaultFaceIndexForCurrentGame(chrAsm.IsFemale);
            chrAsm.HairIndex_Male = faces.IndexOf(HairName_Male);
            chrAsm.HairIndex_Female = faces.IndexOf(HairName_Female);
            if (chrAsm.HairIndex < 0)
                chrAsm.HairIndex = chrAsm.GetDefaultHairIndexForCurrentGame(chrAsm.IsFemale);
            chrAsm.FacegenIndex = facegens.IndexOf(FacegenName);
            if (chrAsm.FacegenIndex < 0)
                chrAsm.FacegenIndex = chrAsm.GetDefaultFacegenIndexForCurrentGame();
            //chrAsm.UpdateModels();
            var chrCustomizeDict = ChrCustomize.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var chrCustomizeValues = Enum.GetValues<FlverMaterial.ChrCustomizeTypes>();
            foreach (var e in chrCustomizeValues)
            {
                if (!chrCustomizeDict.ContainsKey(e))
                    chrCustomizeDict.Add(e, FlverMaterial.GetDefaultChrCustomizeColor(e));
            }

            chrAsm.ChrCustomize = chrCustomizeDict;
        }

        public void CopyFromChrAsm(NewChrAsm chrAsm)
        {
            GameType = GameRoot.GameType;
            IsFemale = chrAsm.IsFemale;
            HeadID = chrAsm.HeadID;
            BodyID = chrAsm.BodyID;
            ArmsID = chrAsm.ArmsID;
            LegsID = chrAsm.LegsID;
            RightWeaponID = chrAsm.RightWeaponID;
            //RightWeaponModelIndex = chrAsm.RightWeaponModelIndex;
            LeftWeaponID = chrAsm.LeftWeaponID;
            //LeftWeaponModelIndex = chrAsm.LeftWeaponModelIndex;
            //LeftWeaponFlipBackwards = chrAsm.LeftWeaponFlipBackwards;
            //LeftWeaponFlipSideways = chrAsm.LeftWeaponFlipSideways;
            //RightWeaponFlipBackwards = chrAsm.RightWeaponFlipBackwards;
            //RightWeaponFlipSideways = chrAsm.RightWeaponFlipSideways;
            WeaponStyle = chrAsm.StartWeaponStyle;
            var faces = chrAsm.PossibleFaceModels;
            var facegens = chrAsm.PossibleFacegenModels;
            var hairs = chrAsm.PossibleHairModels;
            int faceIndex_Male = chrAsm.FaceIndex_Male;
            int faceIndex_Female = chrAsm.FaceIndex_Female;
            int hairIndex_Male = chrAsm.HairIndex_Male;
            int hairIndex_Female = chrAsm.HairIndex_Female;
            int facegenIndex = chrAsm.FacegenIndex;
            FaceName_Male = faceIndex_Male >= 0 ? faces[faceIndex_Male] : "";
            FaceName_Female = faceIndex_Female >= 0 ? faces[faceIndex_Female] : "";
            FacegenName = facegenIndex >= 0 ? facegens[facegenIndex] : "";
            HairName_Male = hairIndex_Male >= 0 ? hairs[hairIndex_Male] : "";
            HairName_Female = hairIndex_Female >= 0 ? hairs[hairIndex_Female] : "";
            ChrCustomize = chrAsm.ChrCustomize.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
