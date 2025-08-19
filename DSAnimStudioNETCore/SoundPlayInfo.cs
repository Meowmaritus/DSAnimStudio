using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class SoundPlayInfo
    {
        public readonly zzz_DocumentIns Document;
        public SoundPlayInfo(zzz_DocumentIns doc)
        {
            Document = doc;
        }

        public int SoundType;
        public int SoundID;
        public string GetSoundEventName()
        {
            var engineType = Document.SoundManager.EngineType;
            if (engineType is zzz_SoundManagerIns.EngineTypes.Wwise)
            {
                return Document.SoundManager.WwiseManager.GetSoundName(SoundType, SoundID);
            }
            else if (engineType is zzz_SoundManagerIns.EngineTypes.FMOD)
            {
                return Document.Fmod.GetSoundName(SoundType, SoundID);
            }
            else if (engineType is zzz_SoundManagerIns.EngineTypes.MagicOrchestra)
            {
                return Document.Fmod.GetSoundName(SoundType, SoundID);
            }
            return null;
        }

        public bool UpdatingPosition = true;
        public float NightfallLifetime;
        public int NightfallCooldownID = -1;
        public Model SourceModel;
        public int DummyPolyID;
        public bool PlayRepeatedlyWhileEventActive = false;
        public bool KillSoundOnActionEnd = false;
        public bool DoNotKillOneShotUntilComplete = false;

        public bool Matches(SoundPlayInfo other)
        {
            if (other == null)
                return false;
            return SoundType == other.SoundType && SoundID == other.SoundID;
        }


        public Func<Vector3> GetGetPosFunc()
        {
            Func<Vector3> getPosFunc = () => Vector3.Zero;
            if (DummyPolyID >= 0)
            {
                getPosFunc = () =>
                {
                    if (DummyPolyID == -1)
                    {
                        return (SourceModel.GetLockonPoint() ?? SourceModel.CurrentTransformPosition);// - GFX.CurrentWorldView.RootMotionOffsetFromWrappedCenter;
                    }

                    if (SourceModel.DummyPolyMan.NewCheckDummyPolyExists(DummyPolyID))
                    {
                        return Vector3.Transform(Vector3.Zero,
                            SourceModel.DummyPolyMan.NewGetDummyPolyByRefID(DummyPolyID)[0].CurrentMatrix
                            * SourceModel.CurrentTransform.WorldMatrix);// - GFX.CurrentWorldView.RootMotionOffsetFromWrappedCenter;

                    }

                    return (SourceModel.GetLockonPoint() ?? SourceModel.CurrentTransformPosition);// - GFX.CurrentWorldView.RootMotionOffsetFromWrappedCenter;
                };

            }
            else
            {
                getPosFunc = () => ((SourceModel.GetLockonPoint() ?? SourceModel.CurrentTransformPosition));// - GFX.CurrentWorldView.RootMotionOffsetFromWrappedCenter;
            }

            

            return getPosFunc;
        }
    }
}
