using mg = Microsoft.Xna.Framework;
using dx = SharpDX.Direct3D11;
using dxc = SharpDX.D3DCompiler;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class DX11FlverRenderer
    {
        FLVER2 Flver;
        public List<MeshEntry> MeshEntries = new List<MeshEntry>();

        

        public class MeshEntry : IDisposable
        {
            public List<dx.Buffer> VertexBuffers = new List<dx.Buffer>();
            public List<dx.VertexBufferBinding> VertexBufferBindings = new List<dx.VertexBufferBinding>();
            public dx.Buffer IndexBuffer = null;
            public SharpDX.Direct3D.PrimitiveTopology PrimTopo;
            public dx.InputLayout InputLayout = null;
            public int IndexCount;

            public dx.VertexShader VPO = null;
            public dx.PixelShader PPO = null;
            public dxc.ShaderReflection VPORefl = null;
            public dxc.ShaderReflection PPORefl = null;
            public List<dxc.ConstantBuffer> VPO_ConstantBuffers = new List<dxc.ConstantBuffer>();
            public List<dxc.ShaderParameterDescription> VPOInputDesc = new List<dxc.ShaderParameterDescription>();

            public void Dispose()
            {
                if (VertexBuffers != null)
                    foreach (var b in VertexBuffers)
                        b?.Dispose();

                if (IndexBuffer != null)
                    IndexBuffer?.Dispose();
            }
        }

        public DX11FlverRenderer(FLVER2 flver)
        {
            Flver = flver;

            

            

            foreach (var m in flver.Meshes)
            {
                var meshEntry = new MeshEntry();

                var mtd = FlverMaterialDefInfo.Lookup(flver.Materials[m.MaterialIndex].MTD);
                var vpo = mtd.VPO;
                var ppo = mtd.PPO;

                meshEntry.VPO = new SharpDX.Direct3D11.VertexShader(DX11.Device, vpo);
                meshEntry.PPO = new SharpDX.Direct3D11.PixelShader(DX11.Device, ppo);
                meshEntry.VPORefl = new dxc.ShaderReflection(vpo);
                meshEntry.PPORefl = new dxc.ShaderReflection(ppo);

                for (int i = 0; i < meshEntry.VPORefl.Description.ConstantBuffers; i++)
                    meshEntry.VPO_ConstantBuffers.Add(meshEntry.VPORefl.GetConstantBuffer(i));

                for (int i = 0; i < meshEntry.VPORefl.Description.InputParameters; i++)
                {
                    meshEntry.VPOInputDesc.Add(meshEntry.VPORefl.GetInputParameterDescription(i));
                }

                var elements = new List<dx.InputElement>();
                foreach (var param in meshEntry.VPOInputDesc)
                {
                    SharpDX.DXGI.Format format = SharpDX.DXGI.Format.Unknown;
                    var paramType = param.ComponentType;
                    if (paramType == dxc.RegisterComponentType.Float32)
                        format = SharpDX.DXGI.Format.R32G32B32A32_Float;
                    else if (paramType == dxc.RegisterComponentType.SInt32)
                        format = SharpDX.DXGI.Format.R32G32B32A32_SInt;
                    else if (paramType == dxc.RegisterComponentType.UInt32)
                        format = SharpDX.DXGI.Format.R32G32B32A32_UInt;
                    else if (paramType == dxc.RegisterComponentType.Unknown)
                        format = SharpDX.DXGI.Format.R32G32B32A32_Typeless;

                    var element = new dx.InputElement(param.SemanticName, param.SemanticIndex, format, param.Register);
                    elements.Add(element);
                }

                var layout = new dx.InputLayout(DX11.Device, vpo, elements.ToArray());
                meshEntry.InputLayout = layout;

                foreach (var vb in m.VertexBuffers)
                {
                    dx.Buffer b = dx.Buffer.Create(DX11.Device, dx.BindFlags.VertexBuffer, vb.LLA_BufferData, vb.LLA_VertexSize * vb.LLA_VertexCount, structureByteStride: vb.LLA_VertexSize);
                    var test = new dx.VertexBufferBinding(b, vb.LLA_VertexSize, 0);

                    
                    meshEntry.VertexBuffers.Add(b);
                    meshEntry.VertexBufferBindings.Add(test);

                }

                foreach (var f in m.FaceSets)
                {
                    var indices = f.Indices.ToArray();
                    dx.Buffer b = dx.Buffer.Create(DX11.Device, dx.BindFlags.IndexBuffer, indices);
                    meshEntry.IndexBuffer = b;
                    meshEntry.IndexCount = f.Indices.Count;
                    meshEntry.PrimTopo = f.TriangleStrip ? SharpDX.Direct3D.PrimitiveTopology.TriangleStrip : SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                    //test idk
                    break;
                }

                MeshEntries.Add(meshEntry);
            }
        }

        public void Draw(mg.Matrix world, mg.Matrix view, mg.Matrix projection)
        {
            var d3dContext = DX11.Device.ImmediateContext;

            foreach (var m in MeshEntries)
            {
                for (int i = 0; i < m.VertexBuffers.Count; i++)
                {
                    d3dContext.InputAssembler.SetVertexBuffers(i, m.VertexBufferBindings[i]);
                }

                d3dContext.InputAssembler.InputLayout = m.InputLayout;

                d3dContext.InputAssembler.SetIndexBuffer(m.IndexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

                d3dContext.InputAssembler.PrimitiveTopology = m.PrimTopo;

                int cbIndex = 0;

                d3dContext.VertexShader.Set(m.VPO);
                d3dContext.PixelShader.Set(m.PPO);

                foreach (var cb in m.VPO_ConstantBuffers)
                {
                    if (cb.Description.Name == "cbSceneParam")
                    {
                        byte[] test = new byte[cb.Description.Size];
                        byte[] test2 = new byte[cb.Description.Size];
                        using (var ms = new System.IO.MemoryStream(test))
                        {
                            var bin = new BinaryWriterEx(false, ms);
                            bin.Position = 16;
                            bin.WriteSingle(GFX.Device.Viewport.Width);
                            bin.WriteSingle(GFX.Device.Viewport.Height);
                            bin.Position = 80;
                            for (int i = 0; i < 12; i++)
                            {
                                bin.WriteSingle(view[i]);
                            }
                            bin.Position = 256;
                            for (int i = 0; i < 16; i++)
                            {
                                bin.WriteSingle(projection[i]);
                            }
                            //bin.Position = 256;
                            //for (int i = 0; i < 16; i++)
                            //{
                            //    bin.WriteSingle(GFX.World.Matrix_Projection[i]);
                            //}
                            bin.Position = 48;
                            bin.WriteSingle(GFX.CurrentWorldView.CameraLocationInWorld.Position.X);
                            bin.WriteSingle(GFX.CurrentWorldView.CameraLocationInWorld.Position.Y);
                            bin.WriteSingle(GFX.CurrentWorldView.CameraLocationInWorld.Position.Z);

                            test2 = ms.ToArray();
                        }

                        d3dContext.VertexShader.SetConstantBuffer(cbIndex, new dx.Buffer(DX11.Device, SharpDX.DataStream.Create(test2, true, true), new dx.BufferDescription(test2.Length, dx.BindFlags.ConstantBuffer, dx.ResourceUsage.Default)));
                    }
                    cbIndex++;
                }

                

                d3dContext.DrawIndexed(m.IndexCount, 0, 0);
            }
        }
    }
}
