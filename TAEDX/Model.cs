using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace TAEDX
{
    public class Model : IDisposable
    {
        public bool IsVisible { get; set; } = true;
        public BoundingBox Bounds { get; private set; }

        private List<ModelInstance> Instances = new List<ModelInstance>();
        public int InstanceCount => Instances.Count;

        VertexBuffer InstanceBuffer;
        public VertexBufferBinding InstanceBufferBinding { get; private set; }

        public Transform ShittyTransform = Transform.Default;

        public void ApplyWorldToInstances(Matrix world)
        {
            foreach (var inst in Instances)
            {
                inst.Data.WorldMatrix = world;
            }
        }

        public enum ModelType
        {
            ModelTypeFlver,
            ModelTypeCollision,
        };
        ModelType Type;

        
        public void AddNewInstance(ModelInstance ins)
        {
            Instances.Add(ins);

            if (InstanceBuffer != null)
                InstanceBuffer.Dispose();

            InstanceBuffer = new VertexBuffer(GFX.Device, ModelInstance.InstanceVertexDeclaration, Instances.Count, BufferUsage.WriteOnly);
            InstanceBuffer.SetData(Instances.Select(x => x.Data).ToArray());
            InstanceBufferBinding = new VertexBufferBinding(InstanceBuffer, 0, 1);
        }

        public void ReinitInstanceData()
        {
            InstanceBuffer = new VertexBuffer(GFX.Device, ModelInstance.InstanceVertexDeclaration, Instances.Count, BufferUsage.WriteOnly);
            InstanceBuffer.SetData(Instances.Select(x => x.Data).ToArray());
            InstanceBufferBinding = new VertexBufferBinding(InstanceBuffer, 0, 1);
        }

        private List<FlverSubmeshRenderer> Submeshes = new List<FlverSubmeshRenderer>();

        public IEnumerable<FlverSubmeshRenderer> GetSubmeshes()
        {
            return Submeshes;
        }

        public Model(FLVER2 flver)
        {
            Type = ModelType.ModelTypeFlver;

            Submeshes = new List<FlverSubmeshRenderer>();
            var subBoundsPoints = new List<Vector3>();
            foreach (var submesh in flver.Meshes)
            {
                // Blacklist some materials that don't have good shaders and just make the viewer look like a mess
                MTD mtd = null;// InterrootLoader.GetMTD(Path.GetFileName(flver.Materials[submesh.MaterialIndex].MTD));
                if (mtd != null)
                {
                    if (mtd.ShaderPath.Contains("FRPG_Water_Env"))
                        continue;
                    if (mtd.ShaderPath.Contains("FRPG_Water_Reflect.spx"))
                        continue;
                }
                var smm = new FlverSubmeshRenderer(this, flver, submesh);
                Submeshes.Add(smm);
                subBoundsPoints.Add(smm.Bounds.Min);
                subBoundsPoints.Add(smm.Bounds.Max);
            }

            //DEBUG//
            //Console.WriteLine($"{flver.Meshes[0].DefaultBoneIndex}");
            //Console.WriteLine();
            //Console.WriteLine();
            //foreach (var mat in flver.Materials)
            //{
            //    Console.WriteLine($"{mat.Name}: {mat.MTD}");
            //}
            /////////

            if (Submeshes.Count == 0)
            {
                Bounds = new BoundingBox();
                IsVisible = false;
            }
            else
            {
                Bounds = BoundingBox.CreateFromPoints(subBoundsPoints);
            }
        }

        public Model(FLVER0 flver)
        {
            Type = ModelType.ModelTypeFlver;

            Submeshes = new List<FlverSubmeshRenderer>();
            var subBoundsPoints = new List<Vector3>();
            foreach (var submesh in flver.Meshes)
            {
                // Blacklist some materials that don't have good shaders and just make the viewer look like a mess
                MTD mtd = null;// InterrootLoader.GetMTD(Path.GetFileName(flver.Materials[submesh.MaterialIndex].MTD));
                if (mtd != null)
                {
                    if (mtd.ShaderPath.Contains("FRPG_Water_Env"))
                        continue;
                    if (mtd.ShaderPath.Contains("FRPG_Water_Reflect.spx"))
                        continue;
                }

                if (submesh.ToTriangleList().Length > 0)
                {
                    var smm = new FlverSubmeshRenderer(this, flver, submesh);
                    Submeshes.Add(smm);
                    subBoundsPoints.Add(smm.Bounds.Min);
                    subBoundsPoints.Add(smm.Bounds.Max);
                }
            }

            //DEBUG//
            //Console.WriteLine($"{flver.Meshes[0].DefaultBoneIndex}");
            //Console.WriteLine();
            //Console.WriteLine();
            //foreach (var mat in flver.Materials)
            //{
            //    Console.WriteLine($"{mat.Name}: {mat.MTD}");
            //}
            /////////

            if (Submeshes.Count == 0)
            {
                Bounds = new BoundingBox();
                IsVisible = false;
            }
            else
            {
                Bounds = BoundingBox.CreateFromPoints(subBoundsPoints);
            }
        }

        //public Model(HKX hkx)
        //{
        //    Type = ModelType.ModelTypeCollision;

        //    Submeshes = new List<FlverSubmeshRenderer>();
        //    var subBoundsPoints = new List<Vector3>();
        //    foreach (var col in hkx.DataSection.Objects)
        //    {
        //        if (col is HKX.FSNPCustomParamCompressedMeshShape)
        //        {
        //            var smm = new FlverSubmeshRenderer(this, hkx, (HKX.FSNPCustomParamCompressedMeshShape)col);
        //            Submeshes.Add(smm);
        //            subBoundsPoints.Add(smm.Bounds.Min);
        //            subBoundsPoints.Add(smm.Bounds.Max);
        //        }

        //        if (col is HKX.HKPStorageExtendedMeshShapeMeshSubpartStorage)
        //        {
        //            var smm = new FlverSubmeshRenderer(this, hkx, (HKX.HKPStorageExtendedMeshShapeMeshSubpartStorage)col);
        //            Submeshes.Add(smm);
        //            subBoundsPoints.Add(smm.Bounds.Min);
        //            subBoundsPoints.Add(smm.Bounds.Max);
        //        }
        //    }

        //    if (Submeshes.Count == 0)
        //    {
        //        Bounds = new BoundingBox();
        //        IsVisible = false;
        //    }
        //    else
        //    {
        //        Bounds = BoundingBox.CreateFromPoints(subBoundsPoints);
        //    }
        //}

        public void DebugDraw()
        {
            foreach (var ins in Instances)
            {
                ins.DrawDebugInfo();
            }
            //TODO
        }

        public void Draw()
        {
            var lod = 0;// GFX.World.GetLOD(modelLocation);
            GFX.World.ApplyViewToShader(GFX.FlverShader, ShittyTransform);
            foreach (var submesh in Submeshes)
            {
                if (Type == ModelType.ModelTypeFlver)
                {
                    submesh.Draw(lod, GFX.FlverShader);
                }
                else
                {
                    submesh.Draw(lod, GFX.CollisionShader);
                }
            }
        }

        public void TryToLoadTextures()
        {
            foreach (var sm in Submeshes)
                sm.TryToLoadTextures();
        }

        public void Dispose()
        {
            if (Submeshes != null)
            {
                for (int i = 0; i < Submeshes.Count; i++)
                {
                    if (Submeshes[i] != null)
                        Submeshes[i].Dispose();
                }

                Submeshes = null;
            }

            InstanceBuffer?.Dispose();
            InstanceBuffer = null;
        }
    }
}
