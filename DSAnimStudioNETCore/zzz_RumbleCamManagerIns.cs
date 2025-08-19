using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation.SIBCAM;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsFormats;

namespace DSAnimStudio
{
    public class zzz_RumbleCamManagerIns
    {
        public zzz_DocumentIns ParentDocument;
        public zzz_RumbleCamManagerIns(zzz_DocumentIns parentDocument)
        {
            ParentDocument = parentDocument;
        }







        public void ClearAll()
        {
            lock (_lock)
            {
                rumbleCams.Clear();
                rumbleCamCache_Sibcam.Clear();
                rumbleCamCache_HKX.Clear();
            }
        }

        public void ClearActive()
        {
            lock (_lock)
            {
                rumbleCams.Clear();
            }
        }

        public abstract class RumbleCam
        {
            public Vector3? Location = null;
            public float DistFalloffStart = -1;
            public float DistFallofEnd = -1;

            public enum States
            {
                Playing,
                ReturningToCenter,
                Finished,
            }

            public SibcamPlayer.View GetCurrentView() => CurrentView;

            public States State = States.Playing;
            private SibcamPlayer.View CurrentView = SibcamPlayer.View.Default;
            public void UpdatePlayback(float deltaTime)
            {
                if (State == States.Playing)
                {
                    UpdatePlayback_Inner(deltaTime);
                    CurrentView = GetCurrentView_Inner();
                    if (IsFinished_Inner())
                    {
                        State = States.ReturningToCenter;
                    }
                }
                else if (State == States.ReturningToCenter)
                {
                    if (Main.Config.EnableRumbleCamSmoothing)
                    {

                        CurrentView.MoveMatrix = NewBlendableTransform.Lerp(CurrentView.MoveMatrix,
                            NewBlendableTransform.Identity, 6 * deltaTime);

                        float fovDist = (CurrentView.Fov - 1);
                        Vector3 offsetTestZero =
                            Vector3.Transform(Vector3.Zero, CurrentView.MoveMatrix.GetMatrixFull());
                        Vector3 offsetTestForward =
                            Vector3.Transform(Vector3.Forward, CurrentView.MoveMatrix.GetMatrixFull());
                        float testZeroDist = offsetTestZero.Length();
                        float testForwardDist = (offsetTestForward - Vector3.Forward).Length();

                        if (fovDist < 0.001f && testZeroDist < 0.001f && testForwardDist < 0.001f)
                        {
                            State = States.Finished;
                        }
                    }
                    else
                    {
                        State = States.Finished;
                    }
                }
            }

            protected abstract SibcamPlayer.View GetCurrentView_Inner();
            protected abstract void UpdatePlayback_Inner(float deltaTime);
            protected abstract bool IsFinished_Inner();
            public bool IsFinished() => State == States.Finished;


            public class RumbleCamHkx : RumbleCam
            {
                HavokAnimationData Data;
                float time;
                SibcamPlayer.View currentView = SibcamPlayer.View.Default;
                bool isFinished = false;
                public RumbleCamHkx(HavokAnimationData data, int fileSize)
                {
                    Data = data;
                    currentView = new SibcamPlayer.View()
                    {
                        MoveMatrix = Data.GetTransformOnFrame(0, 0, false),
                        Fov = 1,
                    };
                }
                protected override SibcamPlayer.View GetCurrentView_Inner()
                {
                    return currentView;
                }

                protected override void UpdatePlayback_Inner(float deltaTime)
                {
                    time += deltaTime;
                    if (time >= Data.Duration)
                    {
                        isFinished = true;
                        time = Data.Duration;
                    }
                    currentView = new SibcamPlayer.View()
                    {
                        MoveMatrix = Data.GetTransformOnFrame(0, time / Data.FrameDuration, false) * NewBlendableTransform.Invert(Data.GetTransformOnFrame(0, 0, false)),
                        Fov = 1,
                    };
                }

                protected override bool IsFinished_Inner()
                {
                    return isFinished;
                }
            }

            public class RumbleCamSib : RumbleCam
            {
                SibcamPlayer player;
                public RumbleCamSib(SIBCAM2 sibcam)
                {
                    player = new SibcamPlayer(sibcam);
                }

                protected override SibcamPlayer.View GetCurrentView_Inner()
                {
                    return player.CurrentView;
                }

                protected override void UpdatePlayback_Inner(float deltaTime)
                {
                    player.UpdatePlayback(deltaTime);
                }

                protected override bool IsFinished_Inner()
                {
                    return player.IsFinish;
                }
            }
        }

        private List<RumbleCam> rumbleCams = new List<RumbleCam>();

        private Dictionary<int, SIBCAM2> rumbleCamCache_Sibcam = new Dictionary<int, SIBCAM2>();
        private Dictionary<int, HavokAnimationData> rumbleCamCache_HKX = new Dictionary<int, HavokAnimationData>();

        public int RumbleCamBindIDOffset => (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.SDT or SoulsAssetPipeline.SoulsGames.ER or SoulsAssetPipeline.SoulsGames.ERNR) ? 100000 : 0;
        public bool GameTypeUsesHkxRumblecam => (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.SDT or SoulsAssetPipeline.SoulsGames.ER or SoulsAssetPipeline.SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.AC6);

        private object _lock = new object();


        private byte[] RetrieveRumbleCamFromBnd(int id)
        {
            int binderID = id + RumbleCamBindIDOffset;

            IBinder binder = null;

            var rumblebndName = "/other/default.rumblebnd.dcx";

            if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS1 or SoulsAssetPipeline.SoulsGames.DS1R or SoulsAssetPipeline.SoulsGames.DES)
                rumblebndName = "/other/default.rumblebnd";

            if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                rumblebndName = "/other/rumblehk.rumblehkbnd.dcx";

            var rumblebndFile = ParentDocument.GameData.ReadFile(rumblebndName);
            if (BND3.IsRead(rumblebndFile, out BND3 asBND3))
                binder = asBND3;
            else if (BND4.IsRead(rumblebndFile, out BND4 asBND4))
                binder = asBND4;
            return binder.Files.FirstOrDefault(f => f.ID == binderID)?.Bytes;
        }


        public void AddRumbleCam(zzz_DocumentIns doc, int id, Vector3? pos, float falloffStart, float falloffEnd)
        {
            lock (_lock)
            {
                RumbleCam cam = null;
                if (GameTypeUsesHkxRumblecam)
                {
                    if (!rumbleCamCache_HKX.ContainsKey(id))
                    {
                        var rumbleCamData = RetrieveRumbleCamFromBnd(id);
                        if (rumbleCamData == null)
                            return;
                        rumbleCamCache_HKX.Add(id, NewHavokAnimation.ReadAnimationDataFromHkx(doc, rumbleCamData, SplitAnimID.FromFullID(doc.GameRoot, id), $"camera_{id:D3}"));
                    }
                    cam = new RumbleCam.RumbleCamHkx(rumbleCamCache_HKX[id], 1);
                }
                else
                {
                    if (!rumbleCamCache_Sibcam.ContainsKey(id))
                    {
                        var rumbleCamData = RetrieveRumbleCamFromBnd(id);
                        if (rumbleCamData == null)
                            return;
                        rumbleCamCache_Sibcam.Add(id, SIBCAM2.Read(rumbleCamData));
                    }
                    cam = new RumbleCam.RumbleCamSib(rumbleCamCache_Sibcam[id]);
                }

                cam.Location = pos;
                cam.DistFalloffStart = falloffStart;
                cam.DistFallofEnd = falloffEnd;

                rumbleCams.Add(cam);
            }

        }

        public void UpdateAll(float deltaTime, Vector3 cameraLocation)
        {
            var view = SibcamPlayer.View.Default;

            lock (_lock)
            {


                var finishedRumblecams = new List<RumbleCam>();
                foreach (var r in rumbleCams)
                {
                    r.UpdatePlayback(deltaTime);

                    var thisView = r.GetCurrentView();

                    if (r.Location != null)
                    {
                        float weight = 1;
                        var dist = (cameraLocation - r.Location.Value).Length();
                        if (dist >= r.DistFalloffStart)
                        {
                            if (dist <= r.DistFallofEnd)
                            {
                                weight = 1 - ((dist - r.DistFalloffStart) / (r.DistFallofEnd - r.DistFalloffStart));
                            }
                            else
                            {
                                weight = 0;
                            }
                        }
                        thisView.MoveMatrix = NewBlendableTransform.Lerp(NewBlendableTransform.Identity, thisView.MoveMatrix, weight);
                        thisView.Fov = MathHelper.Lerp(1, thisView.Fov, weight);
                    }

                    view.MoveMatrix *= thisView.MoveMatrix;
                    view.Fov *= thisView.Fov;

                    if (r.IsFinished())
                        finishedRumblecams.Add(r);
                }

                foreach (var f in finishedRumblecams)
                    rumbleCams.Remove(f);
            }




            GFX.CurrentWorldView.RumbleCam = view;
        }
    }
}
