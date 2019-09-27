using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimDummyPolyCluster : DbgPrimWire
    {
        public List<FLVER2.Dummy> DummyPoly;
        public List<int> DummyPolyID;
        private float RenderSize = 1.0f;

        public static float GlobalRenderSizeMult = 1.0f;

        private List<FLVER2.Bone> BoneList = new List<FLVER2.Bone>();

        public int FlverBoneIndex = -1;

        public Matrix FlverBoneParentMatrix = Matrix.Identity;

        public int BaseDummyPolyID = 0;

        //public int ID { get; private set; }

        //private float _helperSize;
        //public float HelperSize
        //{
        //    get => _helperSize;
        //    set
        //    {
        //        if (value > 0)
        //        {
        //            if (!Children.Contains(Helper))
        //                Children.Add(Helper);
        //        }
        //        else if (value <= 0)
        //        {
        //            if (Children.Contains(Helper))
        //                Children.Remove(Helper);
        //        }

        //        _helperSize = value;
        //    }
        //}

        Matrix GetDummyPolyMatrix(FLVER2.Dummy dummy, bool useScale = true)
        {
            var dummyMatrix = GetBoneParentMatrix(BoneList[dummy.DummyBoneIndex]);

            return (useScale ? Matrix.CreateScale(RenderSize * GlobalRenderSizeMult) : Matrix.Identity) *
            Matrix.CreateLookAt(
            Vector3.Zero,
            new Vector3(dummy.Forward.X, dummy.Forward.Y, dummy.Forward.Z),
            dummy.UseUpwardVector ? new Vector3(dummy.Upward.X, dummy.Upward.Y, dummy.Upward.Z) : Vector3.Up)
            * Matrix.CreateTranslation(new Vector3(dummy.Position.X, dummy.Position.Y, dummy.Position.Z))
            * dummyMatrix;
        }

        static void FixAllStupidDummyCoordinatesInFLVER2(FLVER2 flver, float scale, bool mirrorX)
        {
            Matrix GetBoneParentMatrix(SoulsFormats.FLVER2.Bone b)
            {
                SoulsFormats.FLVER2.Bone parentBone = b;

                var result = Matrix.Identity;

                do
                {
                    result *= Matrix.CreateScale(parentBone.Scale.X, parentBone.Scale.Y, parentBone.Scale.Z);
                    result *= Matrix.CreateRotationX(parentBone.Rotation.X);
                    result *= Matrix.CreateRotationZ(parentBone.Rotation.Z);
                    result *= Matrix.CreateRotationY(parentBone.Rotation.Y);
                    result *= Matrix.CreateTranslation(parentBone.Translation.X, parentBone.Translation.Y, parentBone.Translation.Z);

                    if (parentBone.ParentIndex >= 0)
                    {
                        parentBone = flver.Bones[parentBone.ParentIndex];
                    }
                    else
                    {
                        parentBone = null;
                    }
                }
                while (parentBone != null);

                return result;
            }

            Matrix GetDummyPolyMatrix(FLVER2.Dummy dummy)
            {
                var dummyMatrix = GetBoneParentMatrix(flver.Bones[dummy.DummyBoneIndex]);

                return Matrix.CreateLookAt(
                Vector3.Zero,
                new Vector3(dummy.Forward.X, dummy.Forward.Y, dummy.Forward.Z),
                dummy.UseUpwardVector ? new Vector3(dummy.Upward.X, dummy.Upward.Y, dummy.Upward.Z) : Vector3.Up)
                * Matrix.CreateTranslation(new Vector3(dummy.Position.X, dummy.Position.Y, dummy.Position.Z))
                * dummyMatrix;
            }

            foreach (var dmy in flver.Dummies)
            {
                if (dmy.DummyBoneIndex >= 0)
                {
                    Matrix m = GetDummyPolyMatrix(dmy) * Matrix.CreateScale(scale * (mirrorX ? -1 : 1), scale, scale);

                    Vector3 finalDummyPosition = Vector3.Transform(Vector3.Zero, m);
                    Vector3 finalDummyUp = Vector3.TransformNormal(Vector3.Up, m);
                    Vector3 finalDummyForward = Vector3.TransformNormal(Vector3.Forward, m);

                    dmy.Position = new System.Numerics.Vector3(finalDummyPosition.X, finalDummyPosition.Y, finalDummyPosition.Z);
                    dmy.Upward = new System.Numerics.Vector3(finalDummyUp.X, finalDummyUp.Y, finalDummyUp.Z);
                    dmy.Forward = new System.Numerics.Vector3(finalDummyForward.X, finalDummyForward.Y, finalDummyForward.Z);

                    dmy.UseUpwardVector = true;

                    dmy.AttachBoneIndex = -1;
                }
            }
        }

        protected override void PreDraw()
        {
            //if (HelperSize > 0.05f)
            //    HelperSize *= 0.25f;
            //else
            //    HelperSize = 0;

            ////Test
            ////HelperSize = 0.25f;

            //UpdateHelper();
        }

        private Matrix GetBoneParentMatrix(SoulsFormats.FLVER2.Bone b)
        {
            SoulsFormats.FLVER2.Bone parentBone = b;

            var result = Matrix.Identity;

            do
            {
                result *= Matrix.CreateScale(parentBone.Scale.X, parentBone.Scale.Y, parentBone.Scale.Z);
                result *= Matrix.CreateRotationX(parentBone.Rotation.X);
                result *= Matrix.CreateRotationZ(parentBone.Rotation.Z);
                result *= Matrix.CreateRotationY(parentBone.Rotation.Y);
                result *= Matrix.CreateTranslation(parentBone.Translation.X, parentBone.Translation.Y, parentBone.Translation.Z);

                if (parentBone.ParentIndex >= 0)
                {
                    parentBone = BoneList[parentBone.ParentIndex];
                }
                else
                {
                    parentBone = null;
                }
            }
            while (parentBone != null);

            return result;
        }

        //public IDbgPrim AddHitbox(int dmy1, int dmy2, float radius)
        //{
        //    if (dmy1 >= 0 && dmy2 < 0)
        //    {
        //        var dummy1 = DummyPoly.First(d => d.ReferenceID == dmy1);
        //        return AddHitboxSphere(dummy1, radius);
        //    }
        //    else if (dmy1 < 0 && dmy2 >= 0)
        //    {
        //        var dummy2 = DummyPoly.First(d => d.ReferenceID == dmy2);
        //        return AddHitboxSphere(dummy2, radius);
        //    }
        //    else if (dmy1 >= 0 && dmy2 >= 0)
        //    {
        //        var dummy1 = DummyPoly.First(d => d.ReferenceID == dmy1);
        //        var dummy2 = DummyPoly.First(d => d.ReferenceID == dmy2);
        //        return AddHitboxCapsule(dummy1, dummy2, radius);
        //    }

        //    return null;
        //}

        //private DbgPrimWireCapsule AddHitboxCapsule(FLVER2.Dummy dmy1, FLVER2.Dummy dmy2, float radius)
        //{
        //    Vector3 dmy1Pos = GetDummyPosition(dmy1);
        //    Vector3 dmy2Pos = GetDummyPosition(dmy2);
        //    Vector3 dummyDif = dmy2Pos - dmy1Pos;
        //    Quaternion capsuleAngle = Quaternion.CreateFromAxisAngle(Vector3.Normalize(dummyDif), 0);
        //    float capsuleLength = dummyDif.Length();

        //    var capsule = new DbgPrimWireCapsule(new Transform()
        //    {
        //        OverrideMatrixWorld = Matrix.CreateFromQuaternion(capsuleAngle)
        //        * Matrix.CreateTranslation(dmy1Pos)
        //    }, capsuleLength, radius, 24, Color.White)
        //    {
        //        Category = DbgPrimCategory.DummyPolyHelper,
        //        OverrideColor = Color.Red,
        //    };

        //    Children.Add(capsule);

        //    return capsule;
        //}

        //private DbgPrimWireSphere AddHitboxSphere(FLVER2.Dummy dmy, float radius)
        //{
        //    Vector3 dmyPos = GetDummyPosition(dmy);

        //    var sphere = new DbgPrimWireSphere(new Transform()
        //    {
        //        OverrideMatrixWorld = Matrix.CreateTranslation(dmyPos)
        //    }, radius, 24, 24, Color.White)
        //    {
        //        Category = DbgPrimCategory.DummyPolyHelper,
        //        OverrideColor = Color.Red,
        //    };

        //    Children.Add(sphere);

        //    return sphere;
        //}

        public Vector3 GetDummyPosition(FLVER2.Dummy dmy, bool isAbsolute)
        {
            var m = GetDummyPolyMatrix(dmy);
            if (isAbsolute)
                m *= Transform.WorldMatrix;
            return Vector3.Transform(Vector3.Zero, m);
        }

        public Vector3 GetDummyPosition(int dmy, bool isAbsolute)
        {
            var dummy = DummyPoly.First(d => (BaseDummyPolyID + d.ReferenceID) == dmy);
            var m = GetDummyPolyMatrix(dummy);
            if (isAbsolute)
                m *= Transform.WorldMatrix;
            return Vector3.Transform(Vector3.Zero, m);
        }

        public Matrix GetDummyMatrix(int dmy, bool isAbsolute)
        {
            var dummy = DummyPoly.First(d => (BaseDummyPolyID + d.ReferenceID) == dmy);
            var m = GetDummyPolyMatrix(dummy);
            if (isAbsolute)
                m *= Transform.WorldMatrix;
            return m;
        }

        private void AddDummy(FLVER2.Dummy dummy)
        {
            float forwardLength = dummy.Forward.Length();
            var m = GetDummyPolyMatrix(dummy);

            //var dummyMatrix = GetBoneParentMatrix(BoneList[dummy.DummyBoneIndex]);
            //Children.Add(new DbgPrimWireSphere(new Transform(Vector3.Transform(Vector3.Zero, m), Vector3.Zero), 1, 24, 24, Color.White)
            //{
            //    Category = DbgPrimCategory.DummyPolyHelper,
            //    OverrideColor = Color.Red,
            //    ExtraData = dummy.ReferenceID,
            //});

            AddLine(Vector3.Transform(Vector3.Zero, m), Vector3.Transform(Vector3.Up * forwardLength * 1.5f, m), Color.Lime);
            AddLine(Vector3.Transform(Vector3.Zero, m), Vector3.Transform(Vector3.Forward * forwardLength * 1.5f, m), Color.Blue);
            AddLine(Vector3.Transform(Vector3.Zero, m), Vector3.Transform(Vector3.Left * forwardLength * 1.5f, m), Color.Red);
            AddDbgLabel(m, 1, dummy.ReferenceID.ToString(), new Color(dummy.Color.R, dummy.Color.G, dummy.Color.B, dummy.Color.A));
        }

        public DbgPrimDummyPolyCluster(float size, List<FLVER2.Dummy> dummies, List<FLVER2.Bone> bones, int baseDmyPolyID)
        {
            BaseDummyPolyID = baseDmyPolyID;
            BoneList = bones;
            RenderSize = size;
            //NameColor = new Color(dummy.Color.R, dummy.Color.G, dummy.Color.B, (byte)255);
            //Name = dummy.ReferenceID.ToString();
            DummyPoly = dummies;
            DummyPolyID = dummies.Select(d => (int)(d.ReferenceID + baseDmyPolyID)).ToList();
            Category = DbgPrimCategory.DummyPoly;
            //Transform = new Transform(DummyPolyMatrix);

            foreach (var dmy in DummyPoly)
            {
                AddDummy(dmy);
                //ID = dmy.ReferenceID;
                FlverBoneIndex = dmy.AttachBoneIndex;
            } 

            if (FlverBoneIndex >= 0)
            {
                var bone = bones[FlverBoneIndex];
                float boneLength = (bone.Translation * bone.Scale).Length();
                var bonePrim = new DbgPrimWireBone(bone.Name, Transform.Default, Quaternion.Identity, boneLength / 4f, boneLength, DBG.COLOR_FLVER_BONE);
                bonePrim.Category = DbgPrimCategory.FlverBone;

                bonePrim.OverrideColor = DBG.COLOR_FLVER_BONE;

                var boundingBoxPrim = new DbgPrimWireBox(Transform.Default,
                    new Vector3(bone.BoundingBoxMin.X, bone.BoundingBoxMin.Y, bone.BoundingBoxMin.Z),
                    new Vector3(bone.BoundingBoxMax.X, bone.BoundingBoxMax.Y, bone.BoundingBoxMax.Z), DBG.COLOR_FLVER_BONE_BBOX);

                boundingBoxPrim.Category = DbgPrimCategory.FlverBoneBoundingBox;

                boundingBoxPrim.OverrideColor = DBG.COLOR_FLVER_BONE_BBOX;

                Children.Add(bonePrim);
                Children.Add(boundingBoxPrim);

                FlverBoneParentMatrix = GetBoneParentMatrix(bones[FlverBoneIndex]);
            }
        }

        public void UpdateWithBoneMatrix(Matrix boneMatrix)
        {
            Transform = new Transform(boneMatrix);
            foreach (var c in Children)
            {
                c.Transform = new Transform(FlverBoneParentMatrix);
            }
            //foreach (var c in Children)
            //{
            //    var refId = (short)c.ExtraData;
            //    if (GFX.ModelDrawer.DummyHitSphereInfos.ContainsKey(refId))
            //    {
            //        var info = GFX.ModelDrawer.DummyHitSphereInfos[refId];
            //        c.OverrideColor = info.Color;
            //        var meme = c.Transform;
            //        meme.Scale = Vector3.One * info.Size;
            //        c.Transform = meme;
            //    }
            //    else
            //    {
            //        var meme = c.Transform;
            //        meme.Scale = Vector3.One;
            //        c.Transform = meme;
            //    }
            //}
        }
    }
}
