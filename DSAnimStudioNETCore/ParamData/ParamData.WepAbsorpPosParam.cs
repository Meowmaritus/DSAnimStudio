using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSAnimStudio.ParamData.AtkParam;
using static DSAnimStudio.ParamData.WepAbsorpPosParam;

namespace DSAnimStudio
{
    public abstract partial class ParamData
    {
        public class WepAbsorpPosParam : ParamData
        {
            public enum HangPosTypes
            {
                Back = 0,
                LeftHip = 1,
                RightHip = 2,
                LeftShoulder = 3,
                RightShoulder = 4,
            }

            public enum DispPosTypes : byte
            {
                RightHand = 0,
                LeftHand = 1,
                Other = 3,
            }

            public enum WepInvisibleTypes : sbyte
            {
                Undefined = -1,
                None = 0,
                Sheath = 1,
                DualSwordLeftHand = 2,
            }

            public enum NewWepAbsorpPosType
            {
                Right,
                Left,
                RightBoth,
                LeftBoth,
                RightHang,
                LeftHang,
            }

            public struct AbsorpPosCluster
            {
                public short Model0DummyPoly;
                public short Model1DummyPoly;
                public short Model2DummyPoly;
                public short Model3DummyPoly;
                public DispPosTypes Model0DispType;
                public DispPosTypes Model1DispType;
                public DispPosTypes Model2DispType;
                public DispPosTypes Model3DispType;



                public static readonly AbsorpPosCluster Default = new AbsorpPosCluster()
                {
                    Model0DummyPoly = -1,
                    Model1DummyPoly = -1,
                    Model2DummyPoly = -1,
                    Model3DummyPoly = -1,
                    Model0DispType = DispPosTypes.RightHand,
                    Model1DispType = DispPosTypes.RightHand,
                    Model2DispType = DispPosTypes.RightHand,
                    Model3DispType = DispPosTypes.RightHand,
                };
            }

            public enum WepAbsorpPosType
            {
                Model0_Right,
                Model0_Left,
                Model1_Right,
                Model1_Left,
                Model2_Right,
                Model2_Left,
                Model3_Right,
                Model3_Left,
                Model0_RightBoth,
                Model1_RightBoth,
                Model2_RightBoth,
                Model3_RightBoth,
                Model0_LeftBoth,
                Model1_LeftBoth,
                Model2_LeftBoth,
                Model3_LeftBoth,
                Model0_LeftHang,
                Model0_RightHang,
                Model1_LeftHang,
                Model1_RightHang,
                Model2_LeftHang,
                Model2_RightHang,
                Model3_LeftHang,
                Model3_RightHang,
            }

            public HangPosTypes HangPosType;
            public bool IsSkeletonBind;

            public sbyte ERHideMaskRH1 = -1;
            public sbyte ERHideMaskRH2 = -1;
            public sbyte ERHideMaskLH1 = -1;
            public sbyte ERHideMaskLH2 = -1;

            public Dictionary<NewWepAbsorpPosType, AbsorpPosCluster> NewAbsorpPos = new Dictionary<NewWepAbsorpPosType, AbsorpPosCluster>();

            //public Dictionary<WepAbsorpPosType, short> AbsorpPos = new Dictionary<WepAbsorpPosType, short>();
            //public Dictionary<WepAbsorpPosType, DispPosTypes> WepDispPosTypes = new Dictionary<WepAbsorpPosType, DispPosTypes>();

            public WepInvisibleTypes WepInvisibleType_Model0;
            public WepInvisibleTypes WepInvisibleType_Model1;
            public WepInvisibleTypes WepInvisibleType_Model2;
            public WepInvisibleTypes WepInvisibleType_Model3;

            public override void Read(BinaryReaderEx br)
            {
                NewAbsorpPos = new Dictionary<NewWepAbsorpPosType, AbsorpPosCluster>();

                var start = br.Position;

                // Empty bytes added at start
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsAssetPipeline.SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.AC6)
                    br.Position += 4;

                //u8 SheathTime;
                //u8 pad[3]
                HangPosType = (HangPosTypes)br.ReadByte();
                IsSkeletonBind = br.ReadByte() != 0;

                br.Position += 2; //pad

                var right = new AbsorpPosCluster();
                var left = new AbsorpPosCluster();
                var rightBoth = new AbsorpPosCluster();
                var leftBoth = new AbsorpPosCluster();
                var rightHang = new AbsorpPosCluster();
                var leftHang = new AbsorpPosCluster();

                right.Model0DummyPoly = br.ReadInt16();
                left.Model0DummyPoly = br.ReadInt16();
                rightBoth.Model0DummyPoly = br.ReadInt16();
                leftHang.Model0DummyPoly = br.ReadInt16();
                rightHang.Model0DummyPoly = br.ReadInt16();

                right.Model1DummyPoly = br.ReadInt16();
                left.Model1DummyPoly = br.ReadInt16();
                rightBoth.Model1DummyPoly = br.ReadInt16();
                leftHang.Model1DummyPoly = br.ReadInt16();
                rightHang.Model1DummyPoly = br.ReadInt16();

                right.Model2DummyPoly = br.ReadInt16();
                left.Model2DummyPoly = br.ReadInt16();
                rightBoth.Model2DummyPoly = br.ReadInt16();
                leftHang.Model2DummyPoly = br.ReadInt16();
                rightHang.Model2DummyPoly = br.ReadInt16();

                right.Model3DummyPoly = br.ReadInt16();
                left.Model3DummyPoly = br.ReadInt16();
                rightBoth.Model3DummyPoly = br.ReadInt16();
                leftHang.Model3DummyPoly = br.ReadInt16();
                rightHang.Model3DummyPoly = br.ReadInt16();

                WepInvisibleType_Model0 = (WepInvisibleTypes)br.ReadByte();
                WepInvisibleType_Model1 = (WepInvisibleTypes)br.ReadByte();
                WepInvisibleType_Model2 = (WepInvisibleTypes)br.ReadByte();
                WepInvisibleType_Model3 = (WepInvisibleTypes)br.ReadByte();

                leftBoth.Model0DummyPoly = br.ReadInt16();
                leftBoth.Model1DummyPoly = br.ReadInt16();
                leftBoth.Model2DummyPoly = br.ReadInt16();
                leftBoth.Model3DummyPoly = br.ReadInt16();

                right.Model0DispType = (DispPosTypes)br.ReadByte();
                left.Model0DispType = (DispPosTypes)br.ReadByte();
                rightBoth.Model0DispType = (DispPosTypes)br.ReadByte();
                leftBoth.Model0DispType = (DispPosTypes)br.ReadByte();
                rightHang.Model0DispType = (DispPosTypes)br.ReadByte();
                leftHang.Model0DispType = (DispPosTypes)br.ReadByte();

                right.Model1DispType = (DispPosTypes)br.ReadByte();
                left.Model1DispType = (DispPosTypes)br.ReadByte();
                rightBoth.Model1DispType = (DispPosTypes)br.ReadByte();
                leftBoth.Model1DispType = (DispPosTypes)br.ReadByte();
                rightHang.Model1DispType = (DispPosTypes)br.ReadByte();
                leftHang.Model1DispType = (DispPosTypes)br.ReadByte();

                right.Model2DispType = (DispPosTypes)br.ReadByte();
                left.Model2DispType = (DispPosTypes)br.ReadByte();
                rightBoth.Model2DispType = (DispPosTypes)br.ReadByte();
                leftBoth.Model2DispType = (DispPosTypes)br.ReadByte();
                rightHang.Model2DispType = (DispPosTypes)br.ReadByte();

                right.Model3DispType = (DispPosTypes)br.ReadByte();
                left.Model3DispType = (DispPosTypes)br.ReadByte();
                rightBoth.Model3DispType = (DispPosTypes)br.ReadByte();
                leftBoth.Model3DispType = (DispPosTypes)br.ReadByte();
                rightHang.Model3DispType = (DispPosTypes)br.ReadByte();
                leftHang.Model3DispType = (DispPosTypes)br.ReadByte();

                br.Position = start + 0x54;

                ERHideMaskRH1 = br.ReadSByte();
                ERHideMaskRH2 = br.ReadSByte();
                ERHideMaskLH1 = br.ReadSByte();
                ERHideMaskLH2 = br.ReadSByte();

                NewAbsorpPos.Add(NewWepAbsorpPosType.Right, right);
                NewAbsorpPos.Add(NewWepAbsorpPosType.Left, left);
                NewAbsorpPos.Add(NewWepAbsorpPosType.RightBoth, rightBoth);
                NewAbsorpPos.Add(NewWepAbsorpPosType.LeftBoth, leftBoth);
                NewAbsorpPos.Add(NewWepAbsorpPosType.RightHang, rightHang);
                NewAbsorpPos.Add(NewWepAbsorpPosType.LeftHang, leftHang);
            }
        }

    }
}
