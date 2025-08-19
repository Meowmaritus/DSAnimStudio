using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using Vector2 = System.Numerics.Vector2;
using System.Reflection;
using SoulsFormats;
using System.Windows.Forms;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class Debug : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveOnlyInDebug;

            public override string NewImguiWindowTitle => "Debug";
            
            protected override void Init()
            {
                
            }

            protected override void PreUpdate()
            {
                if (!Main.IsDebugBuild)
                    IsOpen = false;
            }

            private NewAnimSlot.Request DebugSlotRequest = NewAnimSlot.Request.Empty;
            private NewAnimSlot.SlotTypes DebugSlotType;

            private int highlightCategory = 0;
            private int highlightAnim = 0;
            private int highlightTrackIndex = 0;
            
            void doMatrix(string name, ref Matrix m)
            {
                var extraInfo = "";

                if (m == Matrix.Identity)
                    extraInfo += "<IDENTITY>";
                else if (m == new Matrix())
                    extraInfo += "<ALL ZERO>";
                
                if (m.HasAnyNaN())
                    extraInfo += "<Contains NaN>";
                
                if (m.HasAnyInfinity())
                    extraInfo += "<Contains Infinities>";
                
                
                if (ImGui.TreeNode($"{nameof(Matrix)} {name}{extraInfo}##Matrix_{name}"))
                {
                    ImGui.PushID($"Matrix_{name}");
                    ImGui.PushItemWidth(OSD.DefaultItemWidth / 2);

                    ImGui.InputFloat("##matrix_M11", ref m.M11, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M12", ref m.M12, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M13", ref m.M13, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M14", ref m.M14, 0, 0);
                    
                    
                    ImGui.InputFloat("##matrix_M21", ref m.M21, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M22", ref m.M22, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M23", ref m.M23, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M24", ref m.M24, 0, 0);
                    
                    
                    ImGui.InputFloat("##matrix_M31", ref m.M31, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M32", ref m.M32, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M33", ref m.M33, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M34", ref m.M34, 0, 0);
                    
                    
                    ImGui.InputFloat("##matrix_M41", ref m.M41, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M42", ref m.M42, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M43", ref m.M43, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M44", ref m.M44, 0, 0);
                    
                    ImGui.PopItemWidth();
                    ImGui.PopID();
                    ImGui.TreePop();
                }
            }

            void doVector3(string name, ref Vector3 v)
            {
                if (ImGui.TreeNode($"{nameof(Vector3)} {name}<{v.X:0.0000}, {v.Y:0.0000}, {v.Z:0.0000}>##Vector3_{name}"))
                {
                    ImGui.PushID($"Vector3_{name}");

                    ImGui.InputFloat($"X", ref v.X, 0, 0);
                    ImGui.InputFloat($"Y", ref v.Y, 0, 0);
                    ImGui.InputFloat($"Z", ref v.Z, 0, 0);

                    ImGui.PopID();
                    ImGui.TreePop();
                }
            }

            void doVector4(string name, ref Vector4 v)
            {
                if (ImGui.TreeNode($"{nameof(Vector4)} {name}<{v.X:0.0000}, {v.Y:0.0000}, {v.Z:0.0000}, {v.W:0.0000}>##Vector4_{name}"))
                {
                    ImGui.PushID($"Vector4_{name}");

                    ImGui.InputFloat($"X", ref v.X, 0, 0);
                    ImGui.InputFloat($"Y", ref v.Y, 0, 0);
                    ImGui.InputFloat($"Z", ref v.Z, 0, 0);
                    ImGui.InputFloat($"W", ref v.W, 0, 0);

                    ImGui.PopID();
                    ImGui.TreePop();
                }
            }

            void doQuaternion(string name, ref Quaternion v)
            {
                if (ImGui.TreeNode($"{nameof(Quaternion)} {name}<{v.X:0.0000}, {v.Y:0.0000}, {v.Z:0.0000}, {v.W:0.0000}>##Quaternion_{name}"))
                {
                    ImGui.PushID($"Quaternion_{name}");

                    ImGui.InputFloat($"X", ref v.X, 0, 0);
                    ImGui.InputFloat($"Y", ref v.Y, 0, 0);
                    ImGui.InputFloat($"Z", ref v.Z, 0, 0);
                    ImGui.InputFloat($"W", ref v.W, 0, 0);

                    ImGui.PopID();
                    ImGui.TreePop();
                }
            }
            
            void doMatrix_CS(string name, ref System.Numerics.Matrix4x4 m)
            {
                if (ImGui.TreeNode($"[CS]{nameof(System.Numerics.Matrix4x4)} {name}##Matrix_CS_{name}"))
                {
                    ImGui.PushID($"Matrix_CS_{name}");
                    ImGui.PushItemWidth(OSD.DefaultItemWidth / 2);

                    ImGui.InputFloat("##matrix_M11", ref m.M11, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M12", ref m.M12, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M13", ref m.M13, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M14", ref m.M14, 0, 0);
                    
                    
                    ImGui.InputFloat("##matrix_M21", ref m.M21, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M22", ref m.M22, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M23", ref m.M23, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M24", ref m.M24, 0, 0);
                    
                    
                    ImGui.InputFloat("##matrix_M31", ref m.M31, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M32", ref m.M32, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M33", ref m.M33, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M34", ref m.M34, 0, 0);
                    
                    
                    ImGui.InputFloat("##matrix_M41", ref m.M41, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M42", ref m.M42, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M43", ref m.M43, 0, 0);
                    ImGui.SameLine();
                    ImGui.InputFloat("##matrix_M44", ref m.M44, 0, 0);
                    
                    ImGui.PopItemWidth();
                    ImGui.PopID();
                    ImGui.TreePop();
                }
            }

            void doVector3_CS(string name, ref System.Numerics.Vector3 v)
            {
                if (ImGui.TreeNode($"[CS]{nameof(System.Numerics.Vector3)} {name}<{v.X:0.0000}, {v.Y:0.0000}, {v.Z:0.0000}>##Vector3_CS_{name}"))
                {
                    ImGui.PushID($"Vector3_CS_{name}");

                    ImGui.InputFloat($"X", ref v.X, 0, 0);
                    ImGui.InputFloat($"Y", ref v.Y, 0, 0);
                    ImGui.InputFloat($"Z", ref v.Z, 0, 0);

                    ImGui.PopID();
                    ImGui.TreePop();
                }
            }

            void doVector4_CS(string name, ref System.Numerics.Vector4 v)
            {
                if (ImGui.TreeNode($"[CS]{nameof(System.Numerics.Vector4)} {name}<{v.X:0.0000}, {v.Y:0.0000}, {v.Z:0.0000}, {v.W:0.0000}>##Vector4_CS_{name}"))
                {
                    ImGui.PushID($"Vector4_CS_{name}");

                    ImGui.InputFloat($"X", ref v.X, 0, 0);
                    ImGui.InputFloat($"Y", ref v.Y, 0, 0);
                    ImGui.InputFloat($"Z", ref v.Z, 0, 0);
                    ImGui.InputFloat($"W", ref v.W, 0, 0);

                    ImGui.PopID();
                    ImGui.TreePop();
                }
            }

            void doQuaternion_CS(string name, ref System.Numerics.Quaternion v)
            {
                if (ImGui.TreeNode($"[CS]{nameof(System.Numerics.Quaternion)} {name}<{v.X:0.0000}, {v.Y:0.0000}, {v.Z:0.0000}, {v.W:0.0000}>##Quaternion_CS_{name}"))
                {
                    ImGui.PushID($"Quaternion_CS_{name}");

                    ImGui.InputFloat($"X", ref v.X, 0, 0);
                    ImGui.InputFloat($"Y", ref v.Y, 0, 0);
                    ImGui.InputFloat($"Z", ref v.Z, 0, 0);
                    ImGui.InputFloat($"W", ref v.W, 0, 0);

                    ImGui.PopID();
                    ImGui.TreePop();
                }
            }
            
            void doTransform(string name, ref Transform t)
            {
                if (ImGui.TreeNode($"{nameof(Transform)} {name}##Transform_{name}"))
                {
                    ImGui.PushID($"Transform_{name}");
                    
                    bool worldOverride = t.OverrideMatrixWorld != Matrix.Identity;
                    
                    ImGui.Text($"Currently using {nameof(t.OverrideMatrixWorld)}: {(worldOverride)}");
                    
                    doVector3($"{nameof(t.Position)}", ref t.Position);
                    doVector3($"{nameof(t.Scale)}", ref t.Scale);
                    doQuaternion($"{nameof(t.Rotation)}", ref t.Rotation);
                    doVector3($"{nameof(t.Position)}", ref t.Position);
                    
                    doMatrix($"{nameof(t.OverrideMatrixWorld)}", ref t.OverrideMatrixWorld);
                    
                    ImGui.PushItemWidth(OSD.DefaultItemWidth / 2);
                    ImGui.PopItemWidth();
                    
                    ImGui.PopID();
                    ImGui.TreePop();
                }
            }

            byte nonref_doByte(string name, byte a)
            {
                int val = a;
                ImGui.InputInt(name, ref val);
                if (val > byte.MaxValue)
                    val = byte.MaxValue;
                if (val < byte.MinValue)
                    val = byte.MinValue;
                return (byte)val;
            }

            short nonref_doShort(string name, short a)
            {
                int val = a;
                ImGui.InputInt(name, ref val);
                if (val > short.MaxValue)
                    val = short.MaxValue;
                if (val < short.MinValue)
                    val = short.MinValue;
                return (short)val;
            }

            void doShort(string name, ref short a)
            {
                a = nonref_doShort(name, a);
            }

            ushort nonref_doUShort(string name, ushort a)
            {
                int val = a;
                ImGui.InputInt(name, ref val);
                if (val > ushort.MaxValue)
                    val = ushort.MaxValue;
                if (val < ushort.MinValue)
                    val = ushort.MinValue;
                return (ushort)val;
            }

            void doUShort(string name, ref ushort a)
            {
                a = nonref_doUShort(name, a);
            }

            bool nonref_doBool(string name, bool b)
            {
                bool val = b;
                ImGui.Checkbox(name, ref val);
                return val;
            }

            System.Drawing.Color nonref_doSysColor(string name, System.Drawing.Color c)
            {
                byte r = nonref_doByte($"{name}###R", c.R);
                ImGui.SameLine();
                byte g = nonref_doByte($"###G", c.G);
                ImGui.SameLine();
                byte b = nonref_doByte($"###B", c.B);
                ImGui.SameLine();
                byte a = nonref_doByte($"###A", c.A);
                ImGui.PopID();
                
                return System.Drawing.Color.FromArgb(a, r, g, b);
            }

            Vector3 nonref_doVector3(string name, Vector3 a)
            {
                Vector3 val = a;
                doVector3(name, ref val);
                return val;
            }

            System.Numerics.Vector3 nonref_doVector3_CS(string name, System.Numerics.Vector3 a)
            {
                System.Numerics.Vector3 val = a;
                doVector3_CS(name, ref val);
                return val;
            }

            int nonref_doInt(string name, int a)
            {
                int val = a;
                ImGui.InputInt(name, ref val);
                return val;
            }

            void doDummy_Contents(FLVER.Dummy dmy)
            {
                dmy.Position = nonref_doVector3_CS(nameof(dmy.Position), dmy.Position);
                dmy.Color = nonref_doSysColor($"{nameof(dmy.Color)}", dmy.Color);
                dmy.Forward = nonref_doVector3_CS(nameof(dmy.Forward), dmy.Forward);
                dmy.ReferenceID = nonref_doShort(nameof(dmy.ReferenceID), dmy.ReferenceID);
                dmy.ParentBoneIndex = nonref_doShort(nameof(dmy.ParentBoneIndex), dmy.ParentBoneIndex);
                dmy.Upward = nonref_doVector3_CS(nameof(dmy.Upward), dmy.Upward);
                dmy.AttachBoneIndex = nonref_doShort($"{nameof(dmy.AttachBoneIndex)}", dmy.AttachBoneIndex);
                dmy.Flag1 = nonref_doBool($"{nameof(dmy.Flag1)}", dmy.Flag1);
                dmy.UseUpwardVector = nonref_doBool($"{nameof(dmy.UseUpwardVector)}", dmy.UseUpwardVector);
                dmy.Unk30 = nonref_doInt(nameof(dmy.Unk30), dmy.Unk30);
                dmy.Unk34 = nonref_doInt(nameof(dmy.Unk34), dmy.Unk34);


            }

            
            // Rename to doHit_Contents
            void doHit(ParamData.AtkParam.Hit hit)
            {
                ImGui.InputFloat(nameof(hit.Radius), ref hit.Radius);
                doShort(nameof(hit.DmyPoly1), ref hit.DmyPoly1);
                doShort(nameof(hit.DmyPoly2), ref hit.DmyPoly2);
                Tools.EnumPicker(nameof(hit.HitType), ref hit.HitType);
                Tools.EnumPicker(nameof(hit.DummyPolySourceSpawnedOn), ref hit.DummyPolySourceSpawnedOn);
                ImGui.Checkbox(nameof(hit.DmyPoly1FallbackToBody), ref hit.DmyPoly1FallbackToBody);
                ImGui.Checkbox(nameof(hit.DmyPoly2FallbackToBody), ref hit.DmyPoly2FallbackToBody);
                ImGui.Separator();
                ImGui.Checkbox(nameof(hit.AC6_IsFan), ref hit.AC6_IsFan);
                doUShort(nameof(hit.AC6Fan_Reach), ref hit.AC6Fan_Reach);
                doShort(nameof(hit.AC6Fan_RotationX), ref hit.AC6Fan_RotationX);
                doShort(nameof(hit.AC6Fan_RotationY), ref hit.AC6Fan_RotationY);
                doShort(nameof(hit.AC6Fan_RotationZ), ref hit.AC6Fan_RotationZ);
                ImGui.InputFloat(nameof(hit.AC6Fan_ThicknessTop), ref hit.AC6Fan_ThicknessTop);
                ImGui.InputFloat(nameof(hit.AC6Fan_ThicknessBottom), ref hit.AC6Fan_ThicknessBottom);
                ImGui.Separator();
                ImGui.Checkbox(nameof(hit.AC6_IsExtend), ref hit.AC6_IsExtend);
                ImGui.InputFloat(nameof(hit.AC6_Extend_LengthStart), ref hit.AC6_Extend_LengthStart);
                ImGui.InputFloat(nameof(hit.AC6_Extend_LengthEnd), ref hit.AC6_Extend_LengthEnd);
                ImGui.InputFloat(nameof(hit.AC6_Extend_LengthSpreadTime), ref hit.AC6_Extend_LengthSpreadTime);
                ImGui.InputFloat(nameof(hit.AC6_Extend_RadiusEnd), ref hit.AC6_Extend_RadiusEnd);
                ImGui.InputFloat(nameof(hit.AC6_Extend_RadiusSpreadTime), ref hit.AC6_Extend_RadiusSpreadTime);
                ImGui.InputFloat(nameof(hit.AC6_Extend_SpreadDelay), ref hit.AC6_Extend_SpreadDelay);

            }

            void doAtkParam(ParamData.AtkParam atkParam)
            {
                ImGui.PushID("AtkParam");
                if (ImGui.TreeNode("[AtkParam]"))
                {
                    ImGui.InputText(nameof(atkParam.Name), ref atkParam.Name, 1024);
                    ImGui.InputInt(nameof(atkParam.ID), ref atkParam.ID);
                    Tools.EnumPicker(nameof(atkParam.HitSourceType), ref atkParam.HitSourceType);
                    Tools.EnumPicker(nameof(atkParam.AC6HitboxType), ref atkParam.AC6HitboxType);
                    ImGui.InputFloat(nameof(atkParam.AC6_Hit0Extend_LengthEnd), ref atkParam.AC6_Hit0Extend_LengthEnd);
                    ImGui.InputFloat(nameof(atkParam.AC6_Hit0Extend_LengthSpreadTime), ref atkParam.AC6_Hit0Extend_LengthSpreadTime);
                    ImGui.InputFloat(nameof(atkParam.AC6_Hit0Extend_LengthStart), ref atkParam.AC6_Hit0Extend_LengthStart);
                    ImGui.InputFloat(nameof(atkParam.AC6_Hit0Extend_RadiusEnd), ref atkParam.AC6_Hit0Extend_RadiusEnd);
                    ImGui.InputFloat(nameof(atkParam.AC6_Hit0Extend_RadiusSpreadTime), ref atkParam.AC6_Hit0Extend_RadiusSpreadTime);
                    ImGui.InputFloat(nameof(atkParam.AC6_Hit0Extend_SpreadDelay), ref atkParam.AC6_Hit0Extend_SpreadDelay);


                    if (ImGui.TreeNode("Hits"))
                    {
                        for (int h = 0; h < atkParam.Hits.Length; h++)
                        {
                            if (ImGui.TreeNode($"Hit[{h}]"))
                            {
                                ImGui.PushID($"Hit[{h}]");
                                doHit(atkParam.Hits[h]);
                                ImGui.PopID();
                                ImGui.TreePop();
                            }
                        }
                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("Damage Stats"))
                    {
                        atkParam.AtkDark = nonref_doShort(nameof(atkParam.AtkDark), atkParam.AtkDark);
                        atkParam.AtkDarkCorrection = nonref_doShort(nameof(atkParam.AtkDarkCorrection), atkParam.AtkDarkCorrection);
                        atkParam.AtkFire = nonref_doShort(nameof(atkParam.AtkFire), atkParam.AtkFire);
                        atkParam.AtkFireCorrection = nonref_doShort(nameof(atkParam.AtkFireCorrection), atkParam.AtkFireCorrection);
                        atkParam.AtkMag = nonref_doShort(nameof(atkParam.AtkMag), atkParam.AtkMag);
                        atkParam.AtkMagCorrection = nonref_doShort(nameof(atkParam.AtkMagCorrection), atkParam.AtkMagCorrection);
                        atkParam.AtkObj = nonref_doShort(nameof(atkParam.AtkObj), atkParam.AtkObj);
                        atkParam.AtkPhys = nonref_doShort(nameof(atkParam.AtkPhys), atkParam.AtkPhys);
                        atkParam.AtkPhysCorrection = nonref_doShort(nameof(atkParam.AtkPhysCorrection), atkParam.AtkPhysCorrection);
                        atkParam.AtkStam = nonref_doShort(nameof(atkParam.AtkStam), atkParam.AtkStam);
                        atkParam.AtkStamCorrection = nonref_doShort(nameof(atkParam.AtkStamCorrection), atkParam.AtkStamCorrection);
                        atkParam.AtkSuperArmor = nonref_doShort(nameof(atkParam.AtkSuperArmor), atkParam.AtkSuperArmor);
                        atkParam.AtkSuperArmorCorrection = nonref_doShort(nameof(atkParam.AtkSuperArmorCorrection), atkParam.AtkSuperArmorCorrection);
                        atkParam.AtkThrowEscape = nonref_doShort(nameof(atkParam.AtkThrowEscape), atkParam.AtkThrowEscape);
                        atkParam.AtkThrowEscapeCorrection = nonref_doShort(nameof(atkParam.AtkThrowEscapeCorrection), atkParam.AtkThrowEscapeCorrection);
                        atkParam.AtkThun = nonref_doShort(nameof(atkParam.AtkThun), atkParam.AtkThun);
                        atkParam.AtkThunCorrection = nonref_doShort(nameof(atkParam.AtkThunCorrection), atkParam.AtkThunCorrection);
                        atkParam.GuardAtkRate = nonref_doShort(nameof(atkParam.GuardAtkRate), atkParam.GuardAtkRate);
                        atkParam.GuardAtkRateCorrection = nonref_doShort(nameof(atkParam.GuardAtkRateCorrection), atkParam.GuardAtkRateCorrection);
                        atkParam.GuardBreakCorrection = nonref_doShort(nameof(atkParam.GuardBreakCorrection), atkParam.GuardBreakCorrection);
                        atkParam.GuardBreakRate = nonref_doShort(nameof(atkParam.GuardBreakRate), atkParam.GuardBreakRate);
                        atkParam.GuardRate = nonref_doShort(nameof(atkParam.GuardRate), atkParam.GuardRate);
                        atkParam.GuardStaminaCutRate = nonref_doShort(nameof(atkParam.GuardStaminaCutRate), atkParam.GuardStaminaCutRate);
                        atkParam.ThrowTypeID = nonref_doShort(nameof(atkParam.ThrowTypeID), atkParam.ThrowTypeID);
                        ImGui.TreePop();
                    }
                    ImGui.TreePop();
                }
                ImGui.PopID();
            }

            void doNewDummyPolyManager_Contents(NewDummyPolyManager dmy)
            {
                ImGui.InputInt(nameof(dmy.OverrideUnmappedDummyPolyRefBoneID), ref dmy.OverrideUnmappedDummyPolyRefBoneID);
                ImGui.Checkbox(nameof(dmy.OverrideDummyPolyFollowFlag_EnableOverride), ref dmy.OverrideDummyPolyFollowFlag_EnableOverride);
                ImGui.Checkbox(nameof(dmy.OverrideDummyPolyFollowFlag_Value), ref dmy.OverrideDummyPolyFollowFlag_Value);

                if (ImGui.TreeNode("DummyPoly List"))
                {

                    foreach (var kvp in dmy.DummyPolyByRefID)
                    {
                        if (ImGui.TreeNode($"DummyPoly {kvp.Key}"))
                        {
                            for (int i = 0; i < kvp.Value.Count; i++)
                            {
                                if (ImGui.TreeNode($"DummyPoly {kvp.Key}[{i}]"))
                                {
                                    var d = kvp.Value[i];
                                    ImGui.PushID(d.GUID);
                                    ImGui.PushID($"{kvp.Key}_{i}_{d.GUID}.dummy");
                                    if (ImGui.TreeNode($"{nameof(d.dummy)}"))
                                    {

                                        doDummy_Contents(d.dummy);

                                        ImGui.TreePop();
                                    }
                                    ImGui.PopID();

                                    doMatrix($"{nameof(d.AttachMatrix)}", ref d.AttachMatrix);
                                    doMatrix($"{nameof(d.ReferenceMatrix)}", ref d.ReferenceMatrix);
                                    var mat = d.CurrentMatrix;
                                    doMatrix($"{nameof(d.CurrentMatrix)}", ref mat);
                                    ImGui.Checkbox($"{nameof(d.DebugSelected)}", ref d.DebugSelected);
                                    ImGui.Checkbox($"{nameof(d.DisableTextDraw)}", ref d.DisableTextDraw);
                                    ImGui.InputInt($"{nameof(d.Draw_CalculatedOrder)}", ref d.Draw_CalculatedOrder);

                                    ImGui.Text($"{nameof(d.DummyFollowFlag)} = {d.DummyFollowFlag}");

                                    ImGui.Text($"{nameof(d.FollowBoneIndex)} = {d.FollowBoneIndex}");



                                    ImGui.PopID();
                                    ImGui.TreePop();
                                }
                            }

                            ImGui.TreePop();
                        }
                    }

                    ImGui.TreePop();
                }

                int hitSlotIndex = 0;
                foreach (var hitSlot in dmy.HitboxSlots)
                {
                    if (ImGui.TreeNode($"HitSlot[{hitSlotIndex}]"))
                    {
                        ImGui.PushID($"HitSlot[{hitSlotIndex}]");
                        if (hitSlot.Attack != null)
                            doAtkParam(hitSlot.Attack);
                        ImGui.Text($"{nameof(hitSlot.ElapsedTime)} = {hitSlot.ElapsedTime}");
                        ImGui.InputInt(nameof(hitSlot.SlotID), ref hitSlot.SlotID);
                        Tools.EnumPicker(nameof(hitSlot.HitboxSpawnType), ref hitSlot.HitboxSpawnType);
                        int hitPrimIndex = 0;
                        foreach (var kvp in hitSlot.HitPrims)
                        {
                            if (ImGui.TreeNode($"HitPrim[{hitPrimIndex}]"))
                            {
                                ImGui.PushID($"HitPrim[{hitPrimIndex}]");
                                if (ImGui.TreeNode("Hit"))
                                {
                                    doHit(kvp.Key);
                                    ImGui.TreePop();
                                }

                                ImGui.Text($"Prims count = {kvp.Value.Count}");
                                
                                ImGui.PopID();
                                ImGui.TreePop();
                            }

                            hitPrimIndex++;
                        }
                        ImGui.PopID();
                        ImGui.TreePop();
                    }
                    hitSlotIndex++;
                }
            }

            void doNewAnimSkeleton_Contents(NewAnimSkeleton skel)
            {
                ImGui.Text($"{nameof(skel.DebugName)} = '{skel.DebugName}'");
                

                ImGui.Checkbox($"{nameof(skel.ForceDrawBoneBoxes)}", ref skel.ForceDrawBoneBoxes);
                ImGui.Checkbox($"{nameof(skel.ForceDrawBoneLines)}", ref skel.ForceDrawBoneLines);
                ImGui.Checkbox($"{nameof(skel.ForceDrawBoneText)}", ref skel.ForceDrawBoneText);
                ImGui.Checkbox($"{nameof(skel.ForceDrawBoneTransforms)}", ref skel.ForceDrawBoneTransforms);

                if (skel is NewAnimSkeleton_FLVER asFlverSkel)
                {
                    ImGui.Checkbox($"[{nameof(NewAnimSkeleton_FLVER)}]{nameof(asFlverSkel.EnableRefPoseMatrices)}", ref asFlverSkel.EnableRefPoseMatrices);
                }

                if (skel is NewAnimSkeleton_HKX asHkxSkel)
                {
                    //ImGui.Checkbox($"[{nameof(NewAnimSkeleton_HKX)}]{nameof(asHkxSkel.SkeletonPackfile)}", ref asHkxSkel.SkeletonPackfile);
                }

                void doBone(int i)
                {
                    ImGui.PushID($"{skel.GUID}->Bone{i}");
                    
                    var bone = skel.Bones[i];

                    if (ImGui.TreeNode(bone.Name))
                    {
                        ImGui.InputInt($"{nameof(bone.MapToOtherBoneIndex)}", ref bone.MapToOtherBoneIndex);
                        if (skel.OtherSkeletonThisIsMappedTo != null)
                        {
                            if (bone.MapToOtherBoneIndex >= 0)
                            {
                                if (bone.MapToOtherBoneIndex < skel.OtherSkeletonThisIsMappedTo.Bones.Count)
                                {
                                    ImGui.Text($"Mapped To Bone: '{skel.OtherSkeletonThisIsMappedTo.DebugName}'->'{(skel.OtherSkeletonThisIsMappedTo.Bones[bone.MapToOtherBoneIndex].Name)}'");
                                }
                                else
                                {
                                    ImGui.Text($"Mapped To Bone: <OUT OF RANGE>");
                                }
                            }
                            else
                            {
                                ImGui.Text($"Mapped To Bone: <NONE>");
                            }
                        }
                        else
                        {
                            ImGui.Text($"Mapped To Bone: <NOT MAPPED TO OTHER SKELETON>");
                        }

                        if (ImGui.TreeNode($"{nameof(bone.Masks)}"))
                        {
                            var boneMasks = (NewBone.BoneMasks[])Enum.GetValues(typeof(NewBone.BoneMasks));
                            foreach (var boneMaskVal in boneMasks)
                            {
                                bool hasMask = (bone.Masks & boneMaskVal) != 0;
                                ImGui.Checkbox($"{boneMaskVal}", ref hasMask);
                                if (hasMask)
                                    bone.Masks |= boneMaskVal;
                                else
                                    bone.Masks &= (~boneMaskVal);
                            }
                            ImGui.TreePop();
                        }

                        ImGui.Checkbox($"{nameof(bone.IsNub)}", ref bone.IsNub);
                        ImGui.Checkbox($"{nameof(bone.EnablePrimDraw)}", ref bone.EnablePrimDraw);
                        ImGui.Checkbox($"{nameof(bone.IsDebugDrawBoxes)}", ref bone.IsDebugDrawBoxes);
                        ImGui.Checkbox($"{nameof(bone.IsDebugDrawLines)}", ref bone.IsDebugDrawLines);
                        ImGui.Checkbox($"{nameof(bone.IsDebugDrawText)}", ref bone.IsDebugDrawText);
                        ImGui.Checkbox($"{nameof(bone.IsDebugDrawTransforms)}", ref bone.IsDebugDrawTransforms);
                        ImGui.Checkbox($"{nameof(bone.IsBoneGluerDebugView)}", ref bone.IsBoneGluerDebugView);

                        ImGui.SliderFloat($"{nameof(bone.SetWeight)}", ref bone.SetWeight, 0, 1);
                        ImGui.Checkbox($"{nameof(bone.DisableWeight)}", ref bone.DisableWeight);
                        
                        ImGui.Separator();
                        
                        foreach (var ci in bone.ChildIndices)
                        {
                            doBone(ci);
                        }
                        
                        ImGui.TreePop();
                    }
                    
                    
                    
                    ImGui.PopID();
                }

                if (ImGui.TreeNode("[Bones]"))
                {
                    foreach (var idx in skel.TopLevelBoneIndices)
                    {
                        doBone(idx);
                    }
                    ImGui.TreePop();
                }
            }

            void doRootMotionData(string name, RootMotionData data)
            {
                ImGui.Text("[Under Construction]");
            }
            
            void doHavokAnimationData_Contents(HavokAnimationData data)
            {
                if (data is NewHavokAnimation_Dummy)
                    ImGui.Text("Data Type: Dummy");
                else if (data is NewHavokAnimation_SplineCompressed)
                    ImGui.Text("Data Type: SplineCompressed");
                if (data is NewHavokAnimation_InterleavedUncompressed)
                    ImGui.Text("Data Type: InterleavedUncompressed");
                
                doRootMotionData($"{nameof(data.RootMotion)}", data.RootMotion);
                
                ImGui.Text("[Under Construction]");
            }
            
            void doNewHavokAnimation_Contents(NewHavokAnimation anim)
            {
                if (anim == null)
                {
                    ImGui.Text("<NULL>");
                    return;
                }
                
                if (ImGui.TreeNode($"{nameof(anim.Data)}"))
                {
                    ImGui.PushID($"{nameof(anim.Data)}");
                    doHavokAnimationData_Contents(anim.Data);
                    ImGui.PopID();
                    ImGui.TreePop();
                }

                ImGui.Checkbox($"{nameof(anim.EnableLooping)}", ref anim.EnableLooping);

                if (anim.TaeAnimation != null)
                {
                    ImGui.Text($"{nameof(anim.TaeAnimation)}.{nameof(anim.TaeAnimation.SplitID)} = {anim.TaeAnimation.SplitID.GetFormattedIDString(MainProj)}");
                }
                else
                {
                    ImGui.Text($"{nameof(anim.TaeAnimation)} is NULL");
                }
                
                ImGui.Text($"{nameof(anim.Name)} = {anim.Name}");
                ImGui.Text($"{nameof(anim.GetID)}() => {anim.GetID(MainTaeScreen.Proj)}");
                ImGui.Text($"{nameof(anim.Duration)} = {anim.Duration}");
                ImGui.Text($"{nameof(anim.LoopCount)} = {anim.LoopCount}");
                ImGui.Text($"{nameof(anim.FrameCount)} = {anim.FrameCount}");
                ImGui.Text($"{nameof(anim.FrameDuration)} = {anim.FrameDuration}");
                ImGui.Text($"{nameof(anim.BlendHint)} = {anim.BlendHint}");
                ImGui.Text($"{nameof(anim.Weight)} = {anim.Weight}");
                ImGui.Text($"{nameof(anim.CurrentFrame)} = {anim.CurrentFrame}");
                ImGui.Text($"{nameof(anim.CurrentTime)} = {anim.CurrentTime}");
                ImGui.Text($"{nameof(anim.CurrentTimeUnlooped)} = {anim.CurrentTimeUnlooped}");
                ImGui.Text($"{nameof(anim.IsAdditiveBlend)} = {anim.IsAdditiveBlend}");
                ImGui.Text($"{nameof(anim.IsUpperBody)} = {anim.IsUpperBody}");
                ImGui.Text($"{nameof(anim.RootMotionTransformDelta)} = {anim.RootMotionTransformDelta}");
                ImGui.Text($"{nameof(anim.FileSize)} = {anim.FileSize}");
                
            }
            
            void doAnimContainer_Contents(NewAnimationContainer animContainer)
            {
                if (ImGui.TreeNode($"{nameof(animContainer.Skeleton)}"))
                {
                    ImGui.PushID(animContainer.Skeleton.GUID);
                    doNewAnimSkeleton_Contents(animContainer.Skeleton);
                    ImGui.PopID();
                    ImGui.TreePop();
                }
                 
                ImGui.Checkbox($"{nameof(animContainer.EnableLooping)}", ref animContainer.EnableLooping);
                ImGui.Checkbox($"{nameof(animContainer.ForcePlayAnim)}", ref animContainer.ForcePlayAnim);
                ImGui.Checkbox($"{nameof(animContainer.ForceDisableAnimLayerSystem)}", ref animContainer.ForceDisableAnimLayerSystem);
                ImGui.Text($"{nameof(animContainer.ModelName_ForDebug)} = {animContainer.ModelName_ForDebug}");
                ImGui.InputFloat($"{nameof(animContainer.DebugAnimWeight)}", ref animContainer.DebugAnimWeight, 0, 1);
                
                string animName = "<NULL>";
                
                if (animContainer.CurrentAnimation != null)
                    animName = $"<{animContainer.CurrentAnimation.GetID(MainTaeScreen.Proj).GetFormattedIDString(MainTaeScreen.Proj)}|{animContainer.CurrentAnimation.Name}>";
                
                if (ImGui.TreeNode($"{nameof(animContainer.CurrentAnimation)}{animName}"))
                {
                    ImGui.PushID(nameof(animContainer.CurrentAnimation));
                    doNewHavokAnimation_Contents(animContainer.CurrentAnimation);
                    ImGui.PopID();
                    ImGui.TreePop();
                }
            }

            void doBoneGluer(string name, NewBoneGluer gluer)
            {
                if (ImGui.TreeNode($"{name}##BoneGluer_{name}"))
                {
                    ImGui.PushID(gluer.GUID);
                    
                    Tools.EnumPicker($"{nameof(gluer.Mode)}", ref gluer.Mode);
                    Tools.EnumPicker($"{nameof(gluer.Method)}", ref gluer.Method);
                    ImGui.Checkbox($"{nameof(gluer.Enabled)}", ref gluer.Enabled);
                    ImGui.Checkbox($"{nameof(gluer.EnableDebugDraw)}", ref gluer.EnableDebugDraw);
                    
                    if (gluer.LeaderModel != null)
                        ImGui.Text($"{nameof(gluer.LeaderModel)} = '{gluer.LeaderModel.Name}'");
                    else
                        ImGui.Text($"{nameof(gluer.LeaderModel)} = NULL");
                    
                    if (gluer.FollowerModel != null)
                        ImGui.Text($"{nameof(gluer.FollowerModel)} = '{gluer.FollowerModel.Name}'");
                    else
                        ImGui.Text($"{nameof(gluer.FollowerModel)} = NULL");

                    lock (gluer._lock_boneGlueEntries)
                    {
                        if (ImGui.TreeNode($"{nameof(gluer.BoneGlueEntries)}[{gluer.BoneGlueEntries.Count}]##{nameof(gluer.BoneGlueEntries)}"))
                        {
                            foreach (var entry in gluer.BoneGlueEntries)
                            {
                                ImGui.PushID(entry.GUID);
                                if (ImGui.TreeNode($"'{entry.FollowerBoneName}'-->'{entry.LeaderBoneName}'"))
                                {
                                    ImGui.Checkbox($"{nameof(entry.Enabled)}", ref entry.Enabled);
                                    ImGui.Checkbox($"{nameof(entry.EnableDebugDraw)}", ref entry.EnableDebugDraw);
                                    ImGui.TreePop();
                                }
                                
                                ImGui.PopID();
                            }
                            
                            ImGui.TreePop();
                        }
                    }
                    
                    ImGui.PopID();
                    ImGui.TreePop();
                }
            }

            void doSkeletonMapper(string name, NewSkeletonMapper mapper)
            {
                if (ImGui.TreeNode($"{name}##SkeletonMapper_{name}"))
                {
                    ImGui.PushID(mapper.GUID);
                    
                    Tools.EnumPicker($"{nameof(mapper.Mode)}", ref mapper.Mode);
                    ImGui.Checkbox($"{nameof(mapper.Enabled)}", ref mapper.Enabled);
                    ImGui.Checkbox($"{nameof(mapper.NotImplemented_IsDebugDisp)}", ref mapper.NotImplemented_IsDebugDisp);
                    
                    ImGui.PopID();
                    ImGui.TreePop();
                }
            }
            
            //void doSubmesh(FlverSubmeshRenderer submesh, Model parentModel, int submeshIndex)
            //{
            //    ImGui.PushID($"Debug->SceneDebug->Model{parentModel.GUID}-->Submeshes[{submeshIndex}]");

            //    if (ImGui.TreeNode($"Submesh {submeshIndex} [{submesh.FullMaterialName}]"))
            //    {

            //        ImGui.TreePop();
            //    }
            //    ImGui.PopID();
            //}
            
            void doModel(Model model)
            {
                ImGui.PushID($"Debug->SceneDebug->Model[{model.GUID}]");

                if (ImGui.TreeNode(model.Name))
                {
                    if (model.MainMesh != null)
                    {
                        if (ImGui.TreeNode($"{nameof(model.MainMesh)} [{(model.MainMesh.Name ?? "")}]"))
                        {
                            var m = model.MainMesh;
                            ImGui.PushID($"Model-{model.GUID}-MainMesh");


                            ImGui.Checkbox($"{nameof(m.TextureReloadQueued)}", ref m.TextureReloadQueued);
                            ImGui.Checkbox($"{nameof(m.ExceedsBoneCount)}", ref m.ExceedsBoneCount);



                            for (int smi = 0; smi < m.Submeshes.Count; smi++)
                            {
                                var sm = m.Submeshes[smi];
                                if (ImGui.TreeNode($"Submesh {smi} [{(sm.FullMaterialName ?? "")}]"))
                                {
                                    bool usesRefPose = sm.UsesRefPose;
                                    ImGui.Checkbox($"{nameof(sm.UsesRefPose)}", ref usesRefPose);
                                    sm.UsesRefPose = usesRefPose;

                                    ImGui.Text($"{nameof(sm.VertexCount)} = {sm.VertexCount}");
                                    ImGui.Text($"{nameof(sm.PrettyMaterialName)} = {sm.PrettyMaterialName}");

                                    ImGui.TreePop();
                                }
                            }



                            ImGui.PopID();
                            ImGui.TreePop();
                        }
                    }

                    ImGui.Text($"{nameof(model.USE_GLOBAL_BONE_MATRIX)} = {model.USE_GLOBAL_BONE_MATRIX}");
                    
                    doTransform($"{nameof(model.StartTransform)}", ref model.StartTransform);
                    doTransform($"{nameof(model.CurrentTransform)}", ref model.CurrentTransform);


                    ImGui.Checkbox($"{nameof(model.EnableSkinning)}", ref model.EnableSkinning);
                    ImGui.Checkbox($"{nameof(model.ApplyBindPose)}", ref model.ApplyBindPose);
                    ImGui.Checkbox($"{nameof(model.EnableBoneGluers)}", ref model.EnableBoneGluers);
                    ImGui.Checkbox($"{nameof(model.EnableSkeletonMappers)}", ref model.EnableSkeletonMappers);

                    ImGui.Checkbox($"{nameof(model.DebugDispBoneGluers)}", ref model.DebugDispBoneGluers);
                    ImGui.Checkbox($"{nameof(model.DebugDispDummyPolyText)}", ref model.DebugDispDummyPolyText);
                    ImGui.Checkbox($"{nameof(model.DebugDispDummyPolyTransforms)}", ref model.DebugDispDummyPolyTransforms);

                    ImGui.Checkbox($"{nameof(model.IsVisibleByUser)}", ref model.IsVisibleByUser);
                    ImGui.Checkbox($"{nameof(model.IsHiddenByTae)}", ref model.IsHiddenByTae);
                    ImGui.Checkbox($"{nameof(model.IsHiddenByAbsorpPos)}", ref model.IsHiddenByAbsorpPos);
                    ImGui.Checkbox($"{nameof(model.Debug_ForceShowNoMatterWhat)}", ref model.Debug_ForceShowNoMatterWhat);
                    ImGui.Text($"{nameof(model.EffectiveVisibility)} = {model.EffectiveVisibility}");
                    ImGui.SliderFloat($"{nameof(model.Opacity)}", ref model.Opacity, 0, 1);
                    ImGui.SliderFloat($"{nameof(model.DebugOpacity)}", ref model.DebugOpacity, 0, 1);
                    
                    ImGui.Checkbox($"{nameof(model.IS_REMO_DUMMY)}", ref model.IS_REMO_DUMMY);
                    ImGui.Checkbox($"{nameof(model.IsRemoModel)}", ref model.IsRemoModel);
                    //ImGui.Checkbox($"{nameof(model.ModelType)}", ref model.ModelType);
                    ImguiOSD.Tools.EnumPicker($"{nameof(model.ModelType)}", ref model.ModelType);
                    ImGui.Checkbox($"{nameof(model.IS_REMO_NOTSKINNED)}", ref model.IS_REMO_NOTSKINNED);
                    ImGui.Text($"{nameof(model.IS_PLAYER)} = {model.IS_PLAYER}");

                    if (ImGui.TreeNode($"{nameof(model.DrawMask)}"))
                    {
                        for (int m = 0; m < model.DrawMask.Length; m++)
                        {
                            bool visi = model.DrawMask[m];
                            ImGui.Checkbox($"{nameof(model.DrawMask)}[{m}]", ref visi);
                            //model.GetMaterialNamesPerMask()
                            model.DrawMask[m] = visi;
                        }
                    }

                    if (ImGui.TreeNode($"{nameof(model.DefaultDrawMask)}"))
                    {
                        for (int m = 0; m < model.DefaultDrawMask.Length; m++)
                        {
                            bool visi = model.DefaultDrawMask[m];
                            ImGui.Checkbox($"{nameof(model.DefaultDrawMask)}[{m}]", ref visi);
                            //model.GetMaterialNamesPerMask()
                            model.DefaultDrawMask[m] = visi;
                        }
                    }
                    
                    
                    ImGui.SliderFloat($"{nameof(model.DebugAnimWeight_Deprecated)}", ref model.DebugAnimWeight_Deprecated, 0, 1);

                    ImGui.Text($"{nameof(model.ModelIdx)} = {model.ModelIdx}");
                    
                    
                    if (model.SkeletonFlver != null)
                    {
                        if (ImGui.TreeNode($"{nameof(model.SkeletonFlver)}"))
                        {
                            ImGui.PushID(model.SkeletonFlver.GUID);
                            doNewAnimSkeleton_Contents(model.SkeletonFlver);
                            ImGui.PopID();
                            ImGui.TreePop();
                        }
                    }
                    
                    if (model.AnimContainer != null)
                    {
                        if (ImGui.TreeNode($"{nameof(model.AnimContainer)}"))
                        {
                            ImGui.PushID(model.AnimContainer.GUID);
                            doAnimContainer_Contents(model.AnimContainer);
                            ImGui.PopID();
                            ImGui.TreePop();
                        }
                    }

                    if (model.DummyPolyMan != null)
                    {
                        if (ImGui.TreeNode($"{nameof(model.DummyPolyMan)}"))
                        {
                            ImGui.PushID(model.DummyPolyMan.GUID);
                            doNewDummyPolyManager_Contents(model.DummyPolyMan);
                            ImGui.PopID();
                            ImGui.TreePop();
                        }
                    }

                    if (model.SkeletonRemapper != null)
                    {
                        doSkeletonMapper($"{nameof(model.SkeletonRemapper)}", model.SkeletonRemapper);
                    }

                    if (model.BoneGluer != null)
                    {
                        doBoneGluer($"{nameof(model.BoneGluer)}", model.BoneGluer);
                    }

                    if (model.ChrAsm != null)
                    {
                        ImGui.Separator();
                        ImGui.Text("[Armor Slots]");
                        model.ChrAsm?.ForAllArmorModels(doModel);
                        ImGui.Separator();
                        ImGui.Text("[Weapon Slots]");
                        model.ChrAsm?.ForAllWeaponModels(doModel);
                    }

                    if (model.AC6NpcParts != null)
                    {
                        ImGui.Separator();
                        ImGui.Text("[AC6 NPC Parts]");
                        model.AC6NpcParts?.AccessModelsOfAllParts((i, p, m) =>
                        {
                            if (m != null)
                            {
                                doModel(m);
                            }
                        });
                    }

                    ImGui.TreePop();
                }

                

                ImGui.Separator();
                ImGui.PopID();
            }

            void doModel_MapperAndGluerOnly(Model model)
            {
                ImGui.PushID($"doModel_MapperAndGluerOnly_{model.GUID}");
                ImGui.PushStyleColor(ImGuiCol.Border, Color.White.ToNVector4()); 
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(2, 2));
                ImGui.TreeNodeEx(model.Name, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.FramePadding);
                //ImGui.Text(model.Name);
                ImGui.PopStyleVar();
                ImGui.PopStyleVar();
                ImGui.PopStyleColor();
                
                if (model.BoneGluer != null)
                    doBoneGluer(nameof(model.BoneGluer), model.BoneGluer);
                if (model.SkeletonRemapper != null)
                    doSkeletonMapper(nameof(model.SkeletonRemapper), model.SkeletonRemapper);
                ImGui.PopID();
            }
            
            void doShaderParameter(string compositeStr, Microsoft.Xna.Framework.Graphics.EffectParameter p)
            {
                if (p.ParameterType == Microsoft.Xna.Framework.Graphics.EffectParameterType.Void)
                {
                    ImGui.Text($"{compositeStr}");
                }
                else if (p.ParameterType == Microsoft.Xna.Framework.Graphics.EffectParameterType.Bool)
                {
                    var v = p.GetValueBoolean();
                    var prev = v;
                    ImGui.Checkbox(compositeStr, ref v);
                    if (v != prev)
                        p.SetValue(v);
                }
                else if (p.ParameterType == Microsoft.Xna.Framework.Graphics.EffectParameterType.Int32)
                {
                    var v = p.GetValueInt32();
                    var prev = v;
                    ImGui.InputInt(compositeStr, ref v);
                    if (v != prev)
                        p.SetValue(v);
                }
                else if (p.ParameterType == Microsoft.Xna.Framework.Graphics.EffectParameterType.Single)
                {
                    if (p.ParameterClass == Microsoft.Xna.Framework.Graphics.EffectParameterClass.Matrix)
                    {
                        if (p.RowCount == 4 && p.ColumnCount == 4)
                        {
                            var v = p.GetValueMatrix();
                            var prev = v;
                            doMatrix(compositeStr, ref v);
                            if (v != prev)
                                p.SetValue(v);
                        }
                    }
                    else if (p.ParameterClass == Microsoft.Xna.Framework.Graphics.EffectParameterClass.Scalar)
                    {
                        var v = p.GetValueSingle();
                        var prev = v;
                        ImGui.InputFloat(compositeStr, ref v);
                        if (v != prev)
                            p.SetValue(v);
                    }
                    else if (p.ParameterClass == Microsoft.Xna.Framework.Graphics.EffectParameterClass.Vector)
                    {
                        ImGui.PushItemWidth(OSD.DefaultItemWidth * 2);
                        if (p.ColumnCount == 2)
                        {
                            var v = p.GetValueVector2().ToCS();
                            var prev = v;
                            ImGui.InputFloat2(compositeStr, ref v);
                            if (v != prev)
                                p.SetValue(v.ToXna());
                        }
                        else if (p.ColumnCount == 3)
                        {
                            var v = p.GetValueVector3().ToCS();
                            var prev = v;
                            ImGui.InputFloat3(compositeStr, ref v);
                            if (v != prev)
                                p.SetValue(v.ToXna());
                        }
                        else if (p.ColumnCount == 4)
                        {
                            var v = p.GetValueVector4().ToCS();
                            var prev = v;
                            ImGui.InputFloat4(compositeStr, ref v);
                            if (v != prev)
                                p.SetValue(v.ToXna());
                        }
                        ImGui.PopItemWidth();
                    }

                }
                else if (p.ParameterType == Microsoft.Xna.Framework.Graphics.EffectParameterType.String)
                {
                    var v = p.GetValueString();
                    ImGui.Text($"{compositeStr} = '{v}'");
                }
                else if (p.ParameterType == Microsoft.Xna.Framework.Graphics.EffectParameterType.Texture2D)
                {
                    var v = p.GetValueTexture2D();
                    
                    ImGui.Text(compositeStr);



                    if (v == null)
                    {

                    }
                    else
                    {
                        if (!v.IsDisposed)
                        {
                            ImGui.Image(Main.SetDynamicBindTexture(v), new Vector2((int)Math.Round((float)256 * ((float)v.Width / (float)v.Height)), 256));
                        }
                        else
                        {
                            ImGui.Text($"<Unloaded>");
                        }
                        
                        //ImGui.Image(v.GetNativePointer(), new Vector2(256, 256));
                    }
                    
                    
                }
                else
                {
                    ImGui.Text($"{compositeStr} <Value type not implemented.>");
                }
            }

            void doShader(string shaderKind, Microsoft.Xna.Framework.Graphics.Effect effect)
            {
                if (ImGui.TreeNode($"{shaderKind}<{(effect.Name ?? "No Name")}>##Shader__{shaderKind}"))
                {
                    ImGui.PushID($"Shader__{shaderKind}");
                    if (ImGui.TreeNode("Parameters"))
                    {
                        for (int i = 0; i < effect.Parameters.Count; i++)
                        {
                            var p = effect.Parameters[i];

                            var compositeStr = $"{p.ParameterClass} {p.ParameterType} {p.Name} : {p.Semantic} {p.RowCount}";

                            if (p.Elements.Count > 0)
                            {
                                if (ImGui.TreeNode(compositeStr))
                                {
                                    for (int j = 0; j < p.Elements.Count; j++)
                                    {
                                        doShaderParameter($"Element[{j}]", p.Elements[j]);
                                        
                                    }
                                }
                                
                            }
                            else

                            {
                                doShaderParameter(compositeStr, p);
                            }

                           
                            
                            
                        }
                        ImGui.TreePop();
                    }

                    ImGui.PopID();
                    ImGui.TreePop();
                }
            }

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                if (OSD.EnableDebug_QuickDebug)
                {
                    if (ImGui.TreeNodeEx("[QUICK DEBUG]", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        _QuickDebug.BuildDebugMenu(MainTaeScreen);
                        ImGui.TreePop();
                    }
                    
                    ImGui.Separator();
                }

                ImGui.Text($"Error check thread running: {((MainProj?.IsAsyncErrorCheckRunning() == true) ? "YES" : "NO")}");

                ImGui.Separator();

                bool forcedSSSEnabled = GFX.FlverShader.Effect.ForcedSSS_Enable;
                ImGui.Checkbox("Forced SSS - Enabled", ref forcedSSSEnabled);
                GFX.FlverShader.Effect.ForcedSSS_Enable = forcedSSSEnabled;

                float forcedSSSIntensity = GFX.FlverShader.Effect.ForcedSSS_Intensity;
                ImGui.SliderFloat("Force SSS - Intensity", ref forcedSSSIntensity, 0, 1);
                GFX.FlverShader.Effect.ForcedSSS_Intensity = forcedSSSIntensity;

                ImGui.Separator();

                if (MainTaeScreen != null)
                {
                    if (ImGui.TreeNode("Anim View History BACKWARD"))
                    {
                        MainTaeScreen.ImguiDebugAddAnimViewBackwardStackItems();
                        ImGui.TreePop();
                    }
                }

                ImGui.Separator();

                if (ImGui.TreeNode("Shaders"))
                {
                    doShader($"{nameof(GFX.FlverShader)}", GFX.FlverShader.Effect);
                    doShader($"{nameof(GFX.DbgPrimSolidShader)}", GFX.DbgPrimSolidShader.Effect);
                    doShader($"{nameof(GFX.DbgPrimWireShader)}", GFX.DbgPrimWireShader.Effect);
                    doShader($"{nameof(GFX.NewGrid3DShader)}", GFX.NewGrid3DShader.Effect);
                    doShader($"{nameof(GFX.NewSimpleGridShader)}", GFX.NewSimpleGridShader.Effect);
                    doShader($"{nameof(GFX.SkyboxShader)}", GFX.SkyboxShader.Effect);
                    doShader($"{nameof(Main.MainFlverTonemapShader)}", Main.MainFlverTonemapShader.Effect);
                }

                Tools.EnumPicker("Display RenderTarget", ref Main.ViewRenderTarget);

                //ImGui.DragFloat($"{nameof(Main.RenderTargetBlurAnisoPower)}", ref Main.RenderTargetBlurAnisoPower, 0.01f, 0, 0, "%.6f");
                //ImGui.DragFloat($"{nameof(Main.RenderTargetBlurAnisoMin)}", ref Main.RenderTargetBlurAnisoMin, 0.01f, 0, 0, "%.6f");
                //ImGui.DragFloat($"{nameof(Main.RenderTargetBlurAnisoMax)}", ref Main.RenderTargetBlurAnisoMax, 0.01f, 0, 0, "%.6f");

                //ImGui.DragFloat($"{nameof(Main.RenderTargetBlurDirections)}", ref Main.RenderTargetBlurDirections, 0.01f, 0, 0, "%.6f");
                //ImGui.DragFloat($"{nameof(Main.RenderTargetBlurQuality)}", ref Main.RenderTargetBlurQuality, 0.01f, 0, 0, "%.6f");
                //if (Main.RenderTargetBlurQuality < 0.1f)
                //    Main.RenderTargetBlurQuality = 0.1f;
                //ImGui.DragFloat($"{nameof(Main.RenderTargetBlurSize)}", ref Main.RenderTargetBlurSize, 0.01f, 0, 0, "%.6f");

                //ImGui.Checkbox($"{(nameof(Main.RenderTargetDebugBlurDisp))}", ref Main.RenderTargetDebugBlurDisp);
                //ImGui.Checkbox($"{(nameof(Main.RenderTargetDebugBlurMaskDisp))}", ref Main.RenderTargetDebugBlurMaskDisp);
                //ImGui.Checkbox($"{(nameof(Main.RenderTargetDebugDisableBlur))}", ref Main.RenderTargetDebugDisableBlur);

                ImGui.Separator();

                ImGui.Checkbox("Global Bone Glue Enable", ref NewBoneGluer.GlobalEnable);
                ImGui.Checkbox("Global Skeleton Mapper Enable", ref NewSkeletonMapper.GlobalEnable);
                
                var mainModel = MainTaeScreen?.Graph?.ViewportInteractor?.CurrentModel;
                if (mainModel != null)
                {
                    doModel_MapperAndGluerOnly(mainModel);
                    mainModel.ChrAsm?.ForAllArmorModels(m => doModel_MapperAndGluerOnly(m));
                }
                
                ImGui.Separator();
                
                ImGui.Separator();
                
                if (ImGui.TreeNode("[Scene Debug]"))
                {
                    
                    
                    zzz_DocumentManager.CurrentDocument.Scene.AccessAllModels(doModel);
                    ImGui.TreePop();
                }
                
                ImGui.Separator();
                
                ImGui.InputInt("highlightCategory", ref highlightCategory, 0, 0);
                ImGui.InputInt("highlightAnim", ref highlightAnim, 0, 0);
                ImGui.InputInt("highlightTrackIndex", ref highlightTrackIndex, 0, 0);
                
                //if (Tools.SimpleClickButton("Debug Test - Highlight"))
                //{
                //    var proj = MainTaeScreen.Proj;
                //    if (proj != null)
                //    {
                //        if (proj.AllAnimCategoriesDict.ContainsKey(highlightCategory))
                //            MainTaeScreen.RequestHighlightAnimCategory(proj.AllAnimCategoriesDict[highlightCategory]);

                //        var anim = proj.GetFirstAnimationFromFullID(SplitAnimID.FromFullID(proj, highlightAnim));
                //        if (anim != null)
                //        {
                //            MainTaeScreen.RequestHighlightAnimation(anim);
                //            if (highlightTrackIndex >= 0 && highlightTrackIndex < anim.ActionTracks.Count)
                //            {
                //                var track = anim.ActionTracks[highlightTrackIndex];
                //                MainTaeScreen.RequestHighlightTrack(track);
                //                var action = track.GetActions(anim).FirstOrDefault();
                //                if (action != null)
                //                    MainTaeScreen.RequestHighlightAction(action);
                //            }
                //        }


                //    }
                //}
                
                
                var animContainer = zzz_DocumentManager.CurrentDocument.Scene.MainModel?.AnimContainer;
                if (animContainer != null)
                {
                    DebugSlotRequest =
                        NewAnimSlot.ShowImguiWidgetForRequest(ref DebugSlotType, DebugSlotRequest, animContainer);

                    ImGui.Separator();
                    ImGui.PushID("Debug.AnimSlotWeights");
                    animContainer.AccessAnimSlots(slots =>
                    {
                        foreach (var kvp in slots)
                        {
                            ImGui.SliderFloat(kvp.Key.ToString(), ref kvp.Value.SlotWeight, 0, 1);
                        }
                    });
                    ImGui.PopID();
                    ImGui.Separator();
                }
                

                ImGui.Text($"AnyFieldFocused: {OSD.AnyFieldFocused}");
                ImGui.Text($"Focused window: {(OSD.FocusedWindow?.ImguiTag ?? "None")}");
                
                //DBG.DbgPrim_Grid.OverrideColor = HandleColor("Grid Color", DBG.DbgPrim_Grid.OverrideColor.Value);

                if (Tools.SimpleClickButton("Set All StaticWindows to 300x300"))
                {
                    OSD.ForAllStaticWindows(window =>
                    {
                        window.RequestWindowSize = new Vector2(300, 300);
                    });
                }
                
                ImGui.Separator();
                
                Main.BuildDebugToggleImgui();
                
                if (OSD.RequestExpandAllTreeNodes || OSD.IsInit)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("[Texture Pool]"))
                {
                    var texDbg = zzz_DocumentManager.CurrentDocument.TexturePool.GetAllFetchesDbgInfos();
                    int index = 0;
                    foreach (var tex in texDbg)
                    {
                        if (ImGui.TreeNode($"{(tex.Value.IsTextureLoaded ? "■ " : "□ ")}{tex.Key}###TexDbg_{index}_{tex.Key}"))
                        {
                            ImGui.Text($"TexName: {tex.Value.TexName}");
                            ImGui.Text($"RequestType: {tex.Value.RequestType}");
                            ImGui.Text($"IsTextureLoaded: {tex.Value.IsTextureLoaded}");
                            if (tex.Value.Extra != null)
                            {
                                if (ImGui.TreeNode("[TexInfo]"))
                                {
                                    ImGui.Text($"Platform: {tex.Value.Extra.Platform}");
                                    if (ImGui.TreeNode("[TPF.Texture]"))
                                    {
                                        ImGui.Text($"Name: {tex.Value.Extra.TpfEntryStub.Name}");
                                        ImGui.Text($"Format: {tex.Value.Extra.TpfEntryStub.Format}");
                                        ImGui.Text($"Type: {tex.Value.Extra.TpfEntryStub.Type}");
                                        ImGui.Text($"Mipmaps: {tex.Value.Extra.TpfEntryStub.Mipmaps}");
                                        ImGui.Text($"Flags1: {tex.Value.Extra.TpfEntryStub.Flags1}");
                                        

                                        if (tex.Value.Extra.TpfEntryStub.Header != null)
                                        {
                                            if (ImGui.TreeNode("[Header]"))
                                            {
                                                ImGui.Text($"Width: {tex.Value.Extra.TpfEntryStub.Header.Width}");
                                                ImGui.Text($"Height: {tex.Value.Extra.TpfEntryStub.Header.Height}");
                                                ImGui.Text($"TextureCount: {tex.Value.Extra.TpfEntryStub.Header.TextureCount}");
                                                ImGui.Text($"Unk1: {tex.Value.Extra.TpfEntryStub.Header.Unk1}");
                                                ImGui.Text($"Unk2: {tex.Value.Extra.TpfEntryStub.Header.Unk2}");
                                                ImGui.Text($"DXGIFormat: {tex.Value.Extra.TpfEntryStub.Header.DXGIFormat}");
                                                ImGui.TreePop();
                                            }
                                        }

                                        if (tex.Value.Extra.TpfEntryStub.FloatStruct != null)
                                        {
                                            if (ImGui.TreeNode("[FloatStruct]"))
                                            {
                                                ImGui.Text($"Unk00: {tex.Value.Extra.TpfEntryStub.FloatStruct.Unk00}");

                                                if (tex.Value.Extra.TpfEntryStub.FloatStruct.Values != null)
                                                {
                                                    if (ImGui.TreeNode("[Values]"))
                                                    {
                                                        foreach (var v in tex.Value.Extra.TpfEntryStub.FloatStruct.Values)
                                                            ImGui.Text($"{v:0.000000}");
                                                        ImGui.TreePop();
                                                    }
                                                }

                                                ImGui.TreePop();
                                            }
                                        }

                                        ImGui.TreePop();
                                    }
                                    ImGui.TreePop();
                                }
                            }

                            ImGui.TreePop();
                        }
                        index++;
                    }

                    ImGui.TreePop();
                }

                
                ImGui.Button("Hot Reload FlverShader.xnb\nFrom '..\\..\\..\\..\\Content\\Shaders\\' Folder");
                if (ImGui.IsItemClicked())
                    GFX.ReloadFlverShader();

                ImGui.Button("Hot Reload FlverTonemapShader.xnb\nFrom '..\\..\\..\\..\\Content\\Shaders\\' Folder");
                if (ImGui.IsItemClicked())
                    GFX.ReloadTonemapShader();

                ImGui.Button("Hot Reload CubemapSkyboxShader.xnb\nFrom '..\\..\\..\\..\\Content\\Shaders\\' Folder");
                if (ImGui.IsItemClicked())
                    GFX.ReloadCubemapSkyboxShader();

                ImGui.Button("Hot Reload NewGrid3D.xnb\nFrom '..\\..\\..\\..\\Content\\Shaders\\' Folder");
                if (ImGui.IsItemClicked())
                    GFX.ReloadNewGrid3DShader();

                
            }
        }
    }
}
