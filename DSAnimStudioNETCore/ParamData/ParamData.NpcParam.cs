using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;
using static DSAnimStudio.ParamData.AtkParam;
using static DSAnimStudio.ParamData.WepAbsorpPosParam;

namespace DSAnimStudio
{
    public abstract partial class ParamData
    {
        public class NpcParam : ParamData
        {
            public int BehaviorVariationID;

            public float TurnVelocity;

            public bool[] DrawMask;

            public short NormalChangeResourceID = -1;
            public short NormalChangeTexChrID = -1;
            public short NormalChangeModelID = -1;
            public short NormalChangeAnimChrID = -1;
            public short ERRetargetReferenceChrID = -1;
            public short ERSoundBankID = -1;
            public short ERSoundBankAddID = -1;
            public int ERLoadAssetID = -1;
            public int[] ERResidentMaterialExParamIDs = new int[0];

            public float HitHeight;
            public float HitRadius;
            public float HitYOffset;

            public int AC6NpcEquipPartsParamID = -1;

            public Vector3 GetCameraDefaultOffsetFromHitCapsule()
            {
                float heightOrWidth = MathF.Max(HitHeight, HitRadius * 2);
                float minDistToFitInView = heightOrWidth / (2f * MathF.Tan(MathHelper.ToRadians(43) / 2f));
                float y = (HitHeight / 2) + HitYOffset;
                float z = minDistToFitInView + HitRadius;
                return new Vector3(0, y, -z);
            }

            public void ApplyToNpcModel(zzz_DocumentIns doc, Model mdl)
            {
                for (int i = 0; i < Math.Min(Model.DRAW_MASK_LENGTH, DrawMask.Length); i++)
                {
                    mdl.DrawMask[i] = DrawMask[i];
                    mdl.DefaultDrawMask[i] = DrawMask[i];
                }
                mdl.BaseTrackingSpeed = TurnVelocity;

                if (NormalChangeTexChrID >= 0)
                {
                    doc.TexturePool.AddTexturesOfChr(NormalChangeTexChrID);
                    doc.Scene.RequestTextureLoad();
                }

                if (doc.GameRoot.GameType is SoulsGames.AC6 && !mdl.IS_PLAYER)
                {
                    var newNpcParts = new AC6NpcPartsEquipper(doc, mdl.ModelIdx, mdl);
                    newNpcParts.EquipID = AC6NpcEquipPartsParamID;
                    newNpcParts.UpdateModels();
                    var oldNpcPartsEquip = mdl.AC6NpcParts;
                    mdl.AC6NpcParts = newNpcParts;
                    oldNpcPartsEquip?.Dispose();
                }
                else
                {
                    mdl.AC6NpcParts?.Dispose();
                    mdl.AC6NpcParts = null;
                }

                //if (mdl == Scene.MainModel)
                //{
                //    //var offset = GetCameraDefaultOffsetFromHitCapsule();
                //    //GFX.CurrentWorldView.SetOrbitCamStartOffsetNew(Math.Abs(offset.Z), offset.Y);
                //    if (resetCamera)
                //        GFX.CurrentWorldView.SetStartPositionForCharacterModel(HitHeight, HitRadius * 2, HitYOffset);
                //}
            }

            public override void Read(BinaryReaderEx br)
            {
                var game = zzz_DocumentManager.CurrentDocument.GameRoot.GameType;
                var trueStart = br.Position;
                // Random empty bytes added at start...
                if (game is SoulsAssetPipeline.SoulsGames.ER or SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.AC6)
                    br.Position += 4;
                //else if (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                //    br.Position += 0x10;




                var start = br.Position;

                BehaviorVariationID = br.ReadInt32();

                if (game is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    BehaviorVariationID = (int)(ID / 1_0000);

                    AC6NpcEquipPartsParamID = br.GetInt32(trueStart + 0x398);
                }

                //aiThinkId
                //nameId
                br.Position += 0x8;
                if (game is SoulsGames.DES)
                    br.Position += 4;
                TurnVelocity = br.ReadSingle();
                if (game is SoulsGames.DES)
                    br.Position += 4;
                HitHeight = br.ReadSingle();
                HitRadius = br.ReadSingle();
                //weight
                br.Position += 4;
                HitYOffset = br.ReadSingle();

                if (game == SoulsGames.DES)
                {
                    br.Position = start + 0xD4;
                    DrawMask = br.ReadBooleans(8);
                }
                else
                {
                    if (game == SoulsAssetPipeline.SoulsGames.DS3 ||
                    game == SoulsAssetPipeline.SoulsGames.BB ||
                    game == SoulsAssetPipeline.SoulsGames.SDT ||
                    game == SoulsAssetPipeline.SoulsGames.ER ||
                    game == SoulsAssetPipeline.SoulsGames.ERNR ||
                    game == SoulsAssetPipeline.SoulsGames.AC6)
                    {
                        DrawMask = new bool[32];
                    }
                    else
                    {
                        DrawMask = new bool[16];
                    }

                    if (game == SoulsAssetPipeline.SoulsGames.SDT)
                    {
                        br.Position = start + 0x14E;
                    }
                    else
                    {
                        br.Position = start + 0x146;
                    }

                    byte mask1 = br.ReadByte();
                    byte mask2 = br.ReadByte();
                    for (int i = 0; i < 8; i++)
                        DrawMask[i] = ((mask1 & (1 << i)) != 0);
                    for (int i = 0; i < 8; i++)
                        DrawMask[8 + i] = ((mask2 & (1 << i)) != 0);

                    if (game == SoulsAssetPipeline.SoulsGames.DS3 ||
                        game == SoulsAssetPipeline.SoulsGames.BB ||
                        game == SoulsAssetPipeline.SoulsGames.SDT ||
                        game == SoulsAssetPipeline.SoulsGames.ER ||
                        game == SoulsAssetPipeline.SoulsGames.ERNR ||
                        game == SoulsAssetPipeline.SoulsGames.AC6)
                    {
                        if (game == SoulsAssetPipeline.SoulsGames.SDT)
                        {
                            br.Position = start + 0x152;
                        }
                        else
                        {
                            br.Position = start + 0x14A;
                        }

                        byte mask3 = br.ReadByte();
                        byte mask4 = br.ReadByte();
                        for (int i = 0; i < 8; i++)
                            DrawMask[16 + i] = ((mask3 & (1 << i)) != 0);
                        for (int i = 0; i < 8; i++)
                            DrawMask[24 + i] = ((mask4 & (1 << i)) != 0);
                    }

                }




                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DES)
                {
                    NormalChangeResourceID = br.GetInt16(start + 0x00EE);
                }
                else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS1 or SoulsAssetPipeline.SoulsGames.DS1R)
                {
                    NormalChangeResourceID = br.GetInt16(start + 0x010E);
                    NormalChangeTexChrID = br.GetInt16(start + 0x011A);
                }
                else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.BB)
                {
                    NormalChangeResourceID = br.GetInt16(start + 0x010E);
                    NormalChangeTexChrID = br.GetInt16(start + 0x011A);
                    NormalChangeModelID = br.GetInt16(start + 0x0150);
                    NormalChangeAnimChrID = br.GetInt16(start + 0x0162);
                }
                else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS3)
                {
                    NormalChangeResourceID = br.GetInt16(start + 0x010E);
                    NormalChangeTexChrID = br.GetInt16(start + 0x011A);
                    NormalChangeModelID = br.GetInt16(start + 0x01C2);
                    NormalChangeAnimChrID = br.GetInt16(start + 0x01C4);
                }
                else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.SDT)
                {
                    NormalChangeResourceID = br.GetInt16(start + 0x010E);
                    NormalChangeTexChrID = br.GetInt16(start + 0x011A);
                    NormalChangeModelID = br.GetInt16(start + 0x01CA);
                    NormalChangeAnimChrID = br.GetInt16(start + 0x01CC);
                }
                else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.AC6)
                {
                    NormalChangeResourceID = br.GetInt16(trueStart + 0x0112);
                    NormalChangeTexChrID = br.GetInt16(trueStart + 0x011E);
                    NormalChangeModelID = br.GetInt16(trueStart + 0x01CA);
                    NormalChangeAnimChrID = br.GetInt16(trueStart + 0x01CC);

                    ERSoundBankID = br.GetInt16(trueStart + 0x0248);
                    ERSoundBankAddID = br.GetInt16(trueStart + 0x02A4);

                    ERLoadAssetID = br.GetInt32(trueStart + 0x0278);

                    ERResidentMaterialExParamIDs = br.GetInt32s(trueStart + 0x280, 5);
                }
            }
        }

    }
}
