using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using SoulsAssetPipeline.Animation.SIBCAM;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class RumbleCamManager
    {
        public static void ClearAll()
        {
            lock (_lock)
            {
                rumbleCams.Clear();
                rumbleCamCache_Sibcam.Clear();
                rumbleCamCache_HKX.Clear();
            }
        }

        public static void ClearActive()
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

            public abstract SibcamPlayer.View GetCurrentView();
            public abstract void UpdatePlayback(float deltaTime);
            public abstract bool IsFinished();

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
                public override SibcamPlayer.View GetCurrentView()
                {
                    return currentView;
                }

                public override void UpdatePlayback(float deltaTime)
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

                public override bool IsFinished()
                {
                    return isFinished;
                }
            }

            public class RumbleCamSib : RumbleCam
            {
                SibcamPlayer player;
                public RumbleCamSib(SIBCAM sibcam)
                {
                    player = new SibcamPlayer(sibcam);
                }

                public override SibcamPlayer.View GetCurrentView()
                {
                    return player.CurrentView;
                }

                public override void UpdatePlayback(float deltaTime)
                {
                    player.UpdatePlayback(deltaTime);
                }

                public override bool IsFinished()
                {
                    return player.IsFinish;
                }
            }
        }

        private static List<RumbleCam> rumbleCams = new List<RumbleCam>();

        private static Dictionary<int, SIBCAM> rumbleCamCache_Sibcam = new Dictionary<int, SIBCAM>();
        private static Dictionary<int, HavokAnimationData> rumbleCamCache_HKX = new Dictionary<int, HavokAnimationData>();

        public static bool GameTypeUsesHkxRumblecam => (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.SDT or SoulsAssetPipeline.SoulsGames.ER);

        private static object _lock = new object();


        private static byte[] RetrieveRumbleCamFromBnd(int id)
        {
            int binderID = id + (GameTypeUsesHkxRumblecam ? 100000 : 0);
            IBinder binder = null;
            var rumblebndFile = GameData.ReadFile((GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS1 
                or SoulsAssetPipeline.SoulsGames.DS1R 
                or SoulsAssetPipeline.SoulsGames.DES) 
                ? "/other/default.rumblebnd" : "/other/default.rumblebnd.dcx");
            if (BND3.IsRead(rumblebndFile, out BND3 asBND3))
                binder = asBND3;
            else if (BND4.IsRead(rumblebndFile, out BND4 asBND4))
                binder = asBND4;
            return binder.Files.FirstOrDefault(f => f.ID == binderID)?.Bytes;
        }


        public static void AddRumbleCam(int id, Vector3? pos, float falloffStart, float falloffEnd)
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
                        rumbleCamCache_HKX.Add(id, NewHavokAnimation.ReadAnimationDataFromHkx(rumbleCamData, $"camera_{id:D3}"));
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
                        rumbleCamCache_Sibcam.Add(id, SIBCAM.Read(rumbleCamData));
                    }
                    cam = new RumbleCam.RumbleCamSib(rumbleCamCache_Sibcam[id]);
                }

                cam.Location = pos;
                cam.DistFalloffStart = falloffStart;
                cam.DistFallofEnd = falloffEnd;

                rumbleCams.Add(cam);
            }

        }

        public static void UpdateAll(float deltaTime, Vector3 cameraLocation)
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


            }


            GFX.CurrentWorldView.RumbleCam = view;
        }
    }
}
