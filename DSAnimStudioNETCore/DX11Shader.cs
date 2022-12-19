using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class DX11Shader : IDisposable
    {
        public static void TEST()
        {
            var test = new DX11Shader(System.IO.File.ReadAllBytes(@"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game\shader\gxflvershader-shaderbnd-dcx\GXFlver_ColDifSpcPomMulMaskDOL_Fwd.ppo"));
        }

        public DX11Shader(byte[] bytecode)
        {
            var test = new SharpDX.Direct3D11.PixelShader(DX11.Device, bytecode);
            var refl = new SharpDX.D3DCompiler.ShaderReflection(bytecode);
            List<string> variableNames = new List<string>();

            List<SharpDX.D3DCompiler.ShaderParameterDescription> inputParameters = new List<SharpDX.D3DCompiler.ShaderParameterDescription>();
            List<SharpDX.D3DCompiler.ShaderParameterDescription> outputParameters = new List<SharpDX.D3DCompiler.ShaderParameterDescription>();

            var sb = new StringBuilder();

            sb.AppendLine("INPUT:");
            for (int i = 0; i < refl.Description.InputParameters; i++)
            {
                var desc = refl.GetInputParameterDescription(i);
                inputParameters.Add(desc);

                sb.AppendLine($"{desc.SemanticName}{desc.SemanticIndex}");
            }

            sb.AppendLine("OUTPUT:");

            for (int i = 0; i < refl.Description.OutputParameters; i++)
            {
                var desc = refl.GetOutputParameterDescription(i);
                outputParameters.Add(desc);

                sb.AppendLine($"{desc.SemanticName}{desc.SemanticIndex}");
            }

            for (int i = 0; i < refl.Description.ConstantBuffers; i++)
            {
                SharpDX.D3DCompiler.ConstantBuffer cb = refl.GetConstantBuffer(i);
                for (int j = 0; j < cb.Description.VariableCount; j++)
                {
                    SharpDX.D3DCompiler.ShaderReflectionVariable variable = cb.GetVariable(j);
                    variableNames.Add(variable.Description.Name);
                }
            }

            var inputBindings = new List<InputBindingDescription>();
            for (var i = 0; i < refl.Description.BoundResources; i++)
            {
                var rdesc = refl.GetResourceBindingDescription(i);
                inputBindings.Add(rdesc);
            }
            var bytecodeTest = new ShaderBytecode(bytecode);
            var test_Disassemble = bytecodeTest.Disassemble();

            var diffuse = refl.GetVariable("g_DiffuseTexture");
            //var diffuseVariables = new List<SharpDX.D3DCompiler.ShaderReflectionVariable>();
            //for (int i = 0; i < diffuse.Buffer.Description.VariableCount; i++)
            //{
            //    var v = diffuse.Buffer.GetVariable(i);
            //    diffuseVariables.Add(v);
            //}
            //var index = diffuse.GetInterfaceSlot(0);
            //GFX.DX11Device.ImmediateContext.VertexShader.Set(test);

            var testStr = sb.ToString();



            Console.WriteLine("test");

        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DX11Shader()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
