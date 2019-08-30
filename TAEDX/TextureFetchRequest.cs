//using MeowDSIO;
//using MeowDSIO.DataFiles;
using SoulsFormats;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX
{
    public enum TextureFetchRequestType
    {
        //EntityBnd,
        TPF,
        //TexBnd,
        DDS,
    }
    public class TextureFetchRequest : IDisposable
    {
        public TextureFetchRequestType RequestType { get; private set; }
        public TPF TPFReference { get; private set; }
        public byte[] DDSBytes { get; private set; }
        public string TexName;
        private Texture2D CachedTexture;
        private bool IsDX10;
        private static object _lock_conversion = new object();

        public TextureFetchRequest(TPF tpf, string texName)
        {
            RequestType = TextureFetchRequestType.TPF;
            TPFReference = tpf;
            TexName = texName;
        }

        public TextureFetchRequest(byte[] ddsBytes, string texName)
        {
            RequestType = TextureFetchRequestType.DDS;
            TPFReference = null;
            TexName = texName;
            DDSBytes = ddsBytes;
        }

        private byte[] FetchBytes()
        {
            if (TPFReference == null)
                return null;
            if (TPFReference.Platform == TPF.TPFPlatform.PS4)
            {
                lock (_lock_conversion)
                {
                    Console.WriteLine("Enter: " + TPFReference.Textures[0].Name);
                    //TPFReference.ConvertPS4ToPC();
                    Console.WriteLine("Leave: " + TPFReference.Textures[0].Name);

                    foreach (var tex in TPFReference)
                    {
                        tex.Bytes = tex.Headerize();
                    }
                }
            }
            else if (TPFReference.Platform == TPF.TPFPlatform.PS3)
            {
                lock (_lock_conversion)
                {
                    foreach (var tex in TPFReference)
                    {
                        tex.Bytes = tex.Headerize();
                    }
                }
            }
            else if (TPFReference.Platform == TPF.TPFPlatform.Xbone)
            {
                // Because there are actually xbone textures in the PC version for some dumb reason
                return null;
            }

            if (RequestType == TextureFetchRequestType.TPF)
            {
                if (TPFReference == null)
                    return null;

                var matchedTextures = TPFReference.Textures.Where(x => x.Name == TexName).ToList();

                if (matchedTextures.Count > 0)
                {
                    var tex = matchedTextures.First();
                    var texBytes = tex.Bytes;

                    //foreach (var match in matchedTextures)
                    //{
                    //    match.Bytes = null;
                    //    match.Header = null;
                    //    TPFReference.Textures.Remove(match);
                    //}

                    //if (TPFReference.Textures.Count == 0)
                    //{
                    //    TPFReference.Textures = null;
                    //    TPFReference = null;
                    //}

                    return texBytes;
                }
                else
                {
                    return null;
                }
            }
            else if (RequestType == TextureFetchRequestType.DDS)
            {
                return DDSBytes;
            }
            else
            {
                return null;
            }
        }

        private static SurfaceFormat GetSurfaceFormatFromString(string str)
        {
            switch (str)
            {
                case "DXT1":
                    return SurfaceFormat.Dxt1;
                case "DXT3":
                    return SurfaceFormat.Dxt3;
                case "DXT5":
                    return SurfaceFormat.Dxt5;
                case "ATI1":
                    return SurfaceFormat.ATI1; // Monogame workaround :fatcat:
                case "ATI2":
                    return SurfaceFormat.ATI2;
                default:
                    throw new Exception($"Unknown DDS Type: {str}");
            }
        }

        private static int GetNextMultipleOf4(int x)
        {
            if (x % 4 != 0)
                x += 4 - (x % 4);
            else if (x == 0)
                return 4;
            return x;
        }

        private static int GetNextPowerOf2(int x)
        {
            x--;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            x++;
            return x;
        }

        public Texture2D Fetch()
        {
            if (CachedTexture != null)
                return CachedTexture;

            var textureBytes = FetchBytes();
            if (textureBytes == null)
                return null;

            DDS header = null;

            try
            {
                header = new DDS(textureBytes);
                int height = header.dwHeight;
                int width = header.dwWidth;

                int mipmapCount = header.dwMipMapCount;
                var br = new BinaryReaderEx(false, textureBytes);

                br.Skip((int)header.DataOffset);

                SurfaceFormat surfaceFormat;
                if (header.ddspf.dwFourCC == "DX10")
                {
                    // See if there are DX9 textures
                    int fmt = (int)header.header10.dxgiFormat;
                    if (fmt == 70 || fmt == 71 || fmt == 72)
                        surfaceFormat = SurfaceFormat.Dxt1;
                    else if (fmt == 73 || fmt == 74 || fmt == 75)
                        surfaceFormat = SurfaceFormat.Dxt3;
                    else if (fmt == 76 || fmt == 77 || fmt == 78)
                        surfaceFormat = SurfaceFormat.Dxt5;
                    else if (fmt == 79 || fmt == 80 || fmt == 81)
                        surfaceFormat = SurfaceFormat.ATI1;
                    else if (fmt == 82 || fmt == 83 || fmt == 84)
                        surfaceFormat = SurfaceFormat.ATI2;
                    else if (fmt == 97 || fmt == 98 || fmt == 99)
                        surfaceFormat = SurfaceFormat.BC7;
                    else
                    {
                        // No DX10 texture support in monogame yet
                        IsDX10 = true;
                        CachedTexture = Main.DEFAULT_TEXTURE_MISSING;
                        header = null;
                        TPFReference = null;
                        return CachedTexture;
                    }
                }
                else
                {
                    surfaceFormat = GetSurfaceFormatFromString(header.ddspf.dwFourCC);
                }
                // Adjust width and height because from has some DXTC textures that have dimensions not a multiple of 4 :shrug:
                Texture2D tex = new Texture2D(GFX.Device, GetNextMultipleOf4(width), GetNextMultipleOf4(height), true, surfaceFormat);

                for (int i = 0; i < mipmapCount; i++)
                {
                    try
                    {
                        int numTexels = GetNextMultipleOf4(width >> i) * GetNextMultipleOf4(height >> i);
                        if (surfaceFormat == SurfaceFormat.Dxt1 || surfaceFormat == SurfaceFormat.Dxt1SRgb)
                            numTexels /= 2;
                        byte[] thisMipMap = br.ReadBytes(numTexels);
                        tex.SetData(i, 0, null, thisMipMap, 0, numTexels);
                        thisMipMap = null;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error loading {TexName}: {ex.Message}");
                    }
                }

                CachedTexture?.Dispose();

                CachedTexture = tex;
                
                return CachedTexture;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error loading {TexName}: {ex.Message}");
                return null;
            }
            finally
            {
                header = null;
                TPFReference = null;
            }

            return null;
        }

        public void Dispose()
        {
            TPFReference = null;

            CachedTexture?.Dispose();
            CachedTexture = null;
        }
    }
}
