using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaternion = System.Numerics.Quaternion;

namespace DSAnimStudio
{
    public class NewBoneGluer : IDisposable
    {
        public static bool GlobalEnable = true;
        
        public readonly string GUID = Guid.NewGuid().ToString();
        public bool Enabled = true;
        public Model LeaderModel;
        public Model FollowerModel;
        // If exposed publicly, ensure _lock_boneGlueEntries is used.
        public List<BoneGlueEntry> BoneGlueEntries;
        
        public object _lock_boneGlueEntries = new object();

        public bool EnableDebugDraw = false;

        public enum GlueModes
        {
            ShiftEntireSkeleton,
            ShiftChildBones
        }

        public enum GlueMethods
        {
            MatchPosition,
            MatchEverything,
        }
        
        public GlueModes Mode = GlueModes.ShiftChildBones;
        public GlueMethods Method = GlueMethods.MatchPosition;
        
        public NewBoneGluer(Model leader, Model follower, List<BoneGlueEntry> boneGlueEntries, GlueModes mode, GlueMethods method)
        {
            LeaderModel = leader;
            FollowerModel = follower;
            Mode = mode;
            Method = method;
            lock (_lock_boneGlueEntries)
            {
                BoneGlueEntries = boneGlueEntries;
            }
        }

        public void AddGlueEntry(string leaderBone, string followerBone)
        {
            lock (_lock_boneGlueEntries)
            {
                if (!BoneGlueEntries.Any(bge => bge.LeaderBoneName == leaderBone && bge.FollowerBoneName == followerBone))
                    BoneGlueEntries.Add(new BoneGlueEntry(leaderBone, followerBone));
            }
        }

        public void Update()
        {
            if (!(Enabled && GlobalEnable))
                return;
            
            bool globalIsDebugDraw = EnableDebugDraw || Main.HelperDraw.EnableBoneGlue;
            var followerBoneIndices = new List<int>();
            lock (_lock_boneGlueEntries)
            {
                foreach (var entry in BoneGlueEntries)
                {
                    
                    bool isDebugDraw = globalIsDebugDraw || entry.EnableDebugDraw;
                    
                    int followerBoneIndex = FollowerModel.SkeletonFlver.GetBoneIndexByName(entry.FollowerBoneName);
                    int leaderBoneIndex = LeaderModel.SkeletonFlver.GetBoneIndexByName(entry.LeaderBoneName);
                    
                    if (followerBoneIndex >= 0)
                    {
                        FollowerModel.SkeletonFlver.Bones[followerBoneIndex].IsBoneGluerDebugView = isDebugDraw || FollowerModel.DebugDispBoneGluers;
                    }

                    if (leaderBoneIndex >= 0)
                    {
                        LeaderModel.SkeletonFlver.Bones[leaderBoneIndex].IsBoneGluerDebugView = isDebugDraw || LeaderModel.DebugDispBoneGluers;
                    }

                    if (!entry.Enabled)
                        continue;


                    if (followerBoneIndex >= 0)
                    {
                        followerBoneIndices.Add(followerBoneIndex);
                    }
                }
            }

            if (!FollowerModel.EnableBoneGluers || !LeaderModel.EnableBoneGluers)
                return;
            foreach (var i in followerBoneIndices)
            {
                GlueFollowerBone(i);
            }
            
        }
        
        private void GlueFollowerBone(int followerBoneIndex)
        {
            try
            {
                if (LeaderModel != null && FollowerModel != null && LeaderModel.HasValidFlverSkeleton() && FollowerModel.HasValidFlverSkeleton())
                {
                    NewAnimSkeleton leaderSkel = LeaderModel.SkeletonFlver;

                    // if (leaderSkel.OtherSkeletonThisIsMappedTo != null)
                    //     leaderSkel = leaderSkel.OtherSkeletonThisIsMappedTo;
                    
                    var followerSkel = FollowerModel.SkeletonFlver;
                    lock (_lock_boneGlueEntries)
                    {
                        var bone = followerSkel.Bones[followerBoneIndex];
                        var matchingGlueEntries = BoneGlueEntries.Where(bge => bge.FollowerBoneName == bone.Name);
                        foreach (var entry in matchingGlueEntries)
                        {
                            int leaderBoneIndex = leaderSkel.GetBoneIndexByName(entry.LeaderBoneName);
                            var followerBone = followerSkel.Bones[followerBoneIndex];
                            var leaderBone = leaderSkel.Bones[leaderBoneIndex];
                            //var leaderFK = leaderSkel.GetBoneTransformFK(entry.LeaderBoneName);


                            var oldFK = followerBone.FKMatrix;
                            
                            
                            var fk = leaderBone.FKMatrix;// * Utils.GetDebugMatrixFuzz(0.1f);
                            
                            Matrix fkShift = Matrix.Identity;

                            if (Method == GlueMethods.MatchPosition)
                            {
                                var oldPos = Vector3.Transform(Vector3.Zero, oldFK);
                                var newPos = Vector3.Transform(Vector3.Zero, fk);
                                fkShift = Matrix.CreateTranslation(newPos - oldPos);
                            }
                            else if (Method == GlueMethods.MatchEverything)
                            {
                                fkShift = Matrix.Invert(oldFK) * fk;
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                            
                            

                            if (Mode == GlueModes.ShiftEntireSkeleton)
                            {
                                for (int b = 0; b < followerSkel.Bones.Count; b++)
                                {
                                    followerSkel.Bones[b].FKMatrix *= fkShift;
                                    followerSkel.CopyBoneToShaderMatrices(b);
                                }
                            }
                            else if (Mode == GlueModes.ShiftChildBones)
                            {
                                //followerSkel.SimpleRippleAdjustFKRecursive(followerBoneIndex, fkShift, copyToShaderMatrices: true);
                                followerSkel.RippleAdjustFKOfBone(followerBoneIndex, oldFK * fkShift, copyToShaderMatrices: true);
                            }
                           
                            
                            // Alternate FK glue:
                            // var startPos = Vector3.Transform(Vector3.Forward, followerBone.FKMatrix);
                            // var endPos = Vector3.Transform(Vector3.Forward, leaderBone.FKMatrix);
                            // var fkShift = Matrix.CreateTranslation(endPos - startPos);
                            
                            
                            if (Main.Debug.HavokBoneGluerPrintEachGlue)
                                Console.WriteLine($"NewHavokBoneGluer - Moved FK of '{followerSkel.DebugName}'->'{entry.FollowerBoneName}' to the location of '{leaderSkel.DebugName}'->'{entry.LeaderBoneName}'.");
                        }
                    }
                }
            }
            catch (Exception handled_ex) when (Main.EnableErrorHandler.HavokBoneGluer)
            {
                Main.HandleError(nameof(Main.EnableErrorHandler.HavokBoneGluer), handled_ex);
            }
        }

        public NewBoneGluer(Model leader, Model follower, GlueModes mode, GlueMethods method)
            : this(leader, follower, new List<BoneGlueEntry>(), mode, method)
        {

        }



        public class BoneGlueEntry
        {
            public readonly string GUID = Guid.NewGuid().ToString();
            public bool Enabled = true;
            public bool EnableDebugDraw = false;
            public string LeaderBoneName;
            public string FollowerBoneName;
            public BoneGlueEntry()
            {

            }
            public BoneGlueEntry(string leaderBoneName, string followerBoneName)
            {
                LeaderBoneName = leaderBoneName;
                FollowerBoneName = followerBoneName;
            }
        }

        public void Dispose()
        {
            // if (FollowerModel.AnimContainer?.Skeleton != null)
            //     FollowerModel.AnimContainer.Skeleton.HkxBoneTransformSet -= FollowerModelHavokSkeleton_HkxBoneTransformSet;
            //if (LeaderModel.AnimContainer?.Skeleton != null)
            //    LeaderModel.AnimContainer.Skeleton.HkxBoneTransformSet -= LeaderModelHavokSkeleton_HkxBoneTransformSet;


            BoneGlueEntries?.Clear();
            BoneGlueEntries = null;

            LeaderModel = null;
            FollowerModel = null;
        }
    }
}
