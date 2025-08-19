using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace DSAnimStudio.TaeEditor
{
    public static class TaeExtensionMethods
    {
        public static Color MultBrightness(this Color c, float brightness)
        {
            return new Color((c.R / 255f) * brightness,
                (c.G / 255f) * brightness,
                (c.B / 255f) * brightness);
        }

        public static string ReadNullPrefixUTF16(this BinaryReaderEx br)
        {
            bool hasValue = br.ReadBoolean();
            if (hasValue)
                return br.ReadUTF16();
            else
                return null;
        }

        public static void WriteNullPrefixUTF16(this BinaryWriterEx bw, string str)
        {
            if (str == null)
            {
                bw.WriteBoolean(false);
            }
            else
            {
                bw.WriteBoolean(true);
                bw.WriteUTF16(str, true);
            }
        }

        public static Color? ReadNullPrefixColor(this BinaryReaderEx br)
        {
            bool hasValue = br.ReadBoolean();
            if (hasValue)
                return new Color(br.ReadUInt32());
            else
                return null;
        }

        public static void WriteNullPrefixColor(this BinaryWriterEx bw, Color? c)
        {
            if (c.HasValue)
            {
                bw.WriteBoolean(true);
                bw.WriteUInt32(c.Value.PackedValue);
            }
            else
            {
                bw.WriteBoolean(false);
            }
        }

        public static int FixAndGetRow(this DSAProj.Action ev, DSAProj.Animation anim)
        {
            // Move to just after last row if disabled.
            if (ev.TrackIndex < 0)
            {
                ev.TrackIndex = anim.SAFE_GetActionTrackCount();
                ev.Mute = true;
            }

            return ev.TrackIndex;
        }

        public static DSAProj.ActionTrack GetTrack(this DSAProj.Action ev, DSAProj.Animation anim)
        {
            if (ev.TrackIndex >= 0 && ev.TrackIndex < anim.SAFE_GetActionTrackCount())
                return anim.SAFE_GetActionTrackByIndex(ev.TrackIndex);
            else 
                return null;
        }

        public static bool HasInternalSimField(this DSAProj.Action ev, string fieldName)
        {
            var template = InternalSimTemplate.Instance;
            if (template.Events.ContainsKey(ev.Type))
            {
                var tEvent = template.Events[ev.Type];

                // Only in games other than current.
                if (tEvent.OnlyInGames.Count != 0 && !tEvent.OnlyInGames.Contains(zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeForTAE))
                    return false;
                // Specifically not in current game.
                if (tEvent.NotInGames.Count != 0 && tEvent.NotInGames.Contains(zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeForTAE))
                    return false;

                if (tEvent.Fields.ContainsKey(fieldName))
                {
                    var tField = tEvent.Fields[fieldName];
                    // Only in games other than current.
                    if (tField.OnlyInGames.Count != 0 && !tField.OnlyInGames.Contains(zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeForTAE))
                        return false;
                    // Specifically not in current game.
                    if (tField.NotInGames.Count != 0 && tField.NotInGames.Contains(zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeForTAE))
                        return false;

                    return true;
                }
            }

            return false;
        }

        public static object ReadField(this DSAProj.Action ev, TAE.Template.ParamTypes type, int fieldOffset)
        {
            //TODO: OPTIMIZE
            var br = ev.GetParamBinReader(zzz_DocumentManager.CurrentDocument.GameRoot.IsBigEndianGame);
            br.Position = fieldOffset;
            switch (type)
            {
                //case TAE.Template.ParamType.aob: return br.ReadBytes(AobLength);
                case TAE.Template.ParamTypes.b: return br.ReadBoolean();
                case TAE.Template.ParamTypes.u8: case TAE.Template.ParamTypes.x8: return br.ReadByte();
                case TAE.Template.ParamTypes.s8: return br.ReadSByte();
                case TAE.Template.ParamTypes.u16: case TAE.Template.ParamTypes.x16: return br.ReadUInt16();
                case TAE.Template.ParamTypes.s16: return br.ReadInt16();
                case TAE.Template.ParamTypes.u32: case TAE.Template.ParamTypes.x32: return br.ReadUInt32();
                case TAE.Template.ParamTypes.s32: return br.ReadInt32();
                case TAE.Template.ParamTypes.u64: case TAE.Template.ParamTypes.x64: return br.ReadUInt64();
                case TAE.Template.ParamTypes.s64: return br.ReadInt64();
                case TAE.Template.ParamTypes.f32: return br.ReadSingle();
                case TAE.Template.ParamTypes.f32grad:
                    var gradStart = br.ReadSingle();
                    var gradEnd = br.ReadSingle();
                    return new System.Numerics.Vector2(gradStart, gradEnd);
                case TAE.Template.ParamTypes.f64: return br.ReadDouble();
                default: throw new Exception($"Invalid ParamType: {type.ToString()}");
            }
        }

        public static string GetInternalSimTypeName(this DSAProj.Action ev)
        {
            var template = InternalSimTemplate.Instance;
            if (template.Events.ContainsKey(ev.Type))
            {
                var tEvent = template.Events[ev.Type];

                // Only in games other than current.
                if (tEvent.OnlyInGames.Count != 0 && !tEvent.OnlyInGames.Contains(zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeForTAE))
                    return "";
                // Specifically not in current game.
                if (tEvent.NotInGames.Count != 0 && tEvent.NotInGames.Contains(zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeForTAE))
                    return "";

                return tEvent.Name;
            }

            return "";
        }

        public static object ReadInternalSimField(this DSAProj.Action act, string fieldName)
        {
            try
            {
                var template = InternalSimTemplate.Instance;
                if (template.Events.ContainsKey(act.Type))
                {
                    var tAct = template.Events[act.Type];

                    // Only in games other than current.
                    if (tAct.OnlyInGames.Count != 0 && !tAct.OnlyInGames.Contains(zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeForTAE))
                        return null;
                    // Specifically not in current game.
                    if (tAct.NotInGames.Count != 0 && tAct.NotInGames.Contains(zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeForTAE))
                        return null;

                    if (tAct.Fields.ContainsKey(fieldName))
                    {
                        var tField = tAct.Fields[fieldName];


                        // Only in games other than current.
                        if (tField.OnlyInGames.Count != 0 && !tField.OnlyInGames.Contains(zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeForTAE))
                            return null;
                        // Specifically not in current game.
                        if (tField.NotInGames.Count != 0 && tField.NotInGames.Contains(zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeForTAE))
                            return null;



                        TAE.Template.ParamTypes fieldType = tField.DefaultValType;
                        int fieldOffset = tField.DefaultValOffset;

                        if (tField.Overrides.ContainsKey(zzz_DocumentManager.CurrentDocument.GameRoot.GameType))
                        {
                            var tOverride = tField.Overrides[zzz_DocumentManager.CurrentDocument.GameRoot.GameType];
                            fieldType = tOverride.OverrideValType ?? fieldType;
                            fieldOffset = tOverride.OverrideValOffset ?? fieldOffset;
                        }

                        return act.ReadField(fieldType, fieldOffset);
                    }
                }

                return null;
            }
            catch (Exception ex) when (Main.EnableErrorHandler.ReadInternalSimField)
            {
                zzz_NotificationManagerIns.PushNotification($"ERROR READING INTERNAL SIM FIELD. ACTION TYPE: {act.Type}, SIM FIELD '{fieldName}'\n\n{ex}");
            }

            return null;
        }

        static Dictionary<TAE.Animation, bool> isModified_Anim = new Dictionary<TAE.Animation, bool>();
        static Dictionary<TAE, bool> isModified_TAE = new Dictionary<TAE, bool>();

        public static Microsoft.Xna.Framework.Vector2 Round(this Microsoft.Xna.Framework.Vector2 v)
        {
            return new Microsoft.Xna.Framework.Vector2((float)Math.Round(v.X), (float)Math.Round(v.Y));
        }

        public static float GetLerpS(this DSAProj.Action ev, float time)
        {
            float lerpDuration = ev.EndTime - ev.StartTime;
            if (lerpDuration > 0)
            {
                float timeSinceFadeStart = time - ev.StartTime;
                return MathHelper.Clamp(timeSinceFadeStart / lerpDuration, 0, 1);
            }
            else
            {
                return time >= ev.StartTime ? 1 : 0;
            }
        }

        public static float ParameterLerp(this DSAProj.Action ev, float time, float a, float b)
        {
            return MathHelper.Lerp(a, b, ev.GetLerpS(time));
        }

        public static void ClearMemes()
        {
            lock (isModified_Anim)
                isModified_Anim.Clear();

            lock (isModified_TAE)
                isModified_TAE.Clear();
        }


        public static bool GetIsModified(this TAE.Animation ev)
        {
            lock (isModified_Anim)
            {
                if (!isModified_Anim.ContainsKey(ev))
                    isModified_Anim.Add(ev, false);

                return isModified_Anim[ev];
            }
         
        }

        public static bool GetIsModified(this TAE tae)
        {
            lock (isModified_TAE)
            {
                if (!isModified_TAE.ContainsKey(tae))
                    isModified_TAE.Add(tae, false);

                if (tae.HasErrorAndNeedsResave)
                    isModified_TAE[tae] = true;

                return isModified_TAE[tae];
            }
           
        }

        public static void ApplyRounding(this TAE.Action ev)
        {
            ev.StartTime = ev.GetStartTimeFr();
            ev.EndTime = ev.GetEndTimeFr();

            if (zzz_DocumentManager.CurrentDocument.EditorScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS30)
                ev.EndTime = (float)Math.Max(ev.EndTime, ev.StartTime + TAE_FRAME_30);
            else if (zzz_DocumentManager.CurrentDocument.EditorScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS60)
                ev.EndTime = (float)Math.Max(ev.EndTime, ev.StartTime + TAE_FRAME_60);
            else
                ev.EndTime = (float)Math.Max(ev.EndTime, ev.StartTime + 0.001f);
        }

        public static void SetIsModified(this TAE.Animation ev, bool v, bool updateGui = true)
        {
            lock (isModified_Anim)
            {
                if (!isModified_Anim.ContainsKey(ev))
                    isModified_Anim.Add(ev, false);

                isModified_Anim[ev] = v;
            }
                
        }

        public static void SetIsModified(this TAE tae, bool v, bool updateGui = true)
        {
            lock (isModified_TAE)
            {
                if (!isModified_TAE.ContainsKey(tae))
                    isModified_TAE.Add(tae, false);

                if (!v)
                    tae.HasErrorAndNeedsResave = false;

                isModified_TAE[tae] = v;
            }
                
        }



        public const double TAE_FRAME_30 = 1.0 / 30.0;
        public const double TAE_FRAME_60 = 1.0 / 60.0;

        // public static double? GetCurrentFrameDuration()
        // {
        //     if (zzz_DocumentManager.CurrentDocument.EditorScreen.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS30)
        //         return TAE_FRAME_30;
        //     else if (zzz_DocumentManager.CurrentDocument.EditorScreen.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS60)
        //         return TAE_FRAME_60;
        //     else
        //         return null;
        // }
        
        public static float RoundTimeToCurrentSnapInterval(float time)
        {
            if (zzz_DocumentManager.CurrentDocument.EditorScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS30)
                return RoundTimeToFrame(time, TAE_FRAME_30);
            else if (zzz_DocumentManager.CurrentDocument.EditorScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS60)
                return RoundTimeToFrame(time, TAE_FRAME_60);
            else
                return time;
        }
        
        public static float GetCurrentSnapInterval()
        {
            if (zzz_DocumentManager.CurrentDocument.EditorScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS30)
                return (float)TAE_FRAME_30;
            else if (zzz_DocumentManager.CurrentDocument.EditorScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS60)
                return (float)TAE_FRAME_60;
            else
                return 1;
        }

        public static int? GetTimeInIntegerFrames(float time)
        {
            if (zzz_DocumentManager.CurrentDocument.EditorScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS30)
                return (int)Math.Round(time / TAE_FRAME_30);
            else if (zzz_DocumentManager.CurrentDocument.EditorScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS60)
                return (int)Math.Round(time / TAE_FRAME_60);
            else
                return null;
        }

        public static float RoundTo30FPS(float time)
        {
            return RoundTimeToFrame(time, TAE_FRAME_60);
        }

        public static float RoundTimeToFrame(float time, double frameDuration)
        {
            return (float)(Math.Round(time / frameDuration) * frameDuration);
        }

        public static float GetStartTimeFr(this TAE.Action ev)
        {
            return RoundTimeToCurrentSnapInterval(ev.StartTime);
        }

        //public static int GetStartFrame(this TAE.Event ev, double frameDuration)
        //{
        //    return (int)Math.Floor(ev.StartTime / frameDuration);
        //}

        //public static int GetEndFrame(this TAE.Event ev, double frameDuration)
        //{
        //    return (int)Math.Floor(ev.EndTime / frameDuration);
        //}

        //public static int GetStartTAEFrame(this TAE.Event ev)
        //{
        //    return (int)Math.Floor(ev.StartTime / TAE_FRAME);
        //}

        //public static int GetEndTAEFrame(this TAE.Event ev)
        //{
        //    return (int)Math.Floor(ev.EndTime / TAE_FRAME);
        //}

        public static float GetEndTimeFr(this TAE.Action ev)
        {
            return RoundTimeToCurrentSnapInterval(ev.EndTime);
        }
    }
}
