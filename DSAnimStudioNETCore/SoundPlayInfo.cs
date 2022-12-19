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
        public int SoundType;
        public int SoundID;
        public string SoundEventName => GameRoot.GameTypeUsesWwise ?
            Wwise.GetSoundName(SoundType, SoundID) : FmodManager.GetSoundName(SoundType, SoundID);
        public bool UpdatingPosition = true;
        public float NightfallLifetime;
        public Model SourceModel;
        public int DummyPolyID;
        public bool PlayRepeatedlyWhileEventActive = false;
        public bool KillSoundOnEventEnd = false;

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
                        return (SourceModel.GetLockonPoint() ?? SourceModel.CurrentTransformPosition) - GFX.CurrentWorldView.RootMotionOffsetFromWrappedCenter;
                    }

                    if (SourceModel.DummyPolyMan.DummyPolyByRefID.ContainsKey(DummyPolyID))
                    {
                        return Vector3.Transform(Vector3.Zero,
                            SourceModel.DummyPolyMan.DummyPolyByRefID[DummyPolyID][0].CurrentMatrix
                            * SourceModel.CurrentTransform.WorldMatrix) - GFX.CurrentWorldView.RootMotionOffsetFromWrappedCenter;

                    }

                    return (SourceModel.GetLockonPoint() ?? SourceModel.CurrentTransformPosition) - GFX.CurrentWorldView.RootMotionOffsetFromWrappedCenter;
                };

            }
            else
            {
                getPosFunc = () => (SourceModel.GetLockonPoint() ?? SourceModel.CurrentTransformPosition) - GFX.CurrentWorldView.RootMotionOffsetFromWrappedCenter;
            }

            

            return getPosFunc;
        }
    }
}
