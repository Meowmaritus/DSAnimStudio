//#define NO_EXCEPTION_CATCH

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

namespace DSAnimStudio
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
        public class TextureInfo
        {
            public TPF.Texture Texture;
            public byte[] DDSBytes;
            public TPF.TPFPlatform Platform;
        }

        public TextureFetchRequestType RequestType { get; private set; }
        public TPF TPFReference { get; private set; }
        public TextureInfo TexInfo { get; private set; }
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
            TexInfo = new TextureInfo()
            {
                DDSBytes = ddsBytes,
                Platform = TPF.TPFPlatform.PC,
                Texture = null,
            };
        }

        private TextureInfo FetchTexInfo()
        {
            //if (TPFReference == null)
            //    return null;

            //if (TPFReference.Platform == TPF.TPFPlatform.PS4)
            //{
            //    lock (_lock_conversion)
            //    {
                    
            //        foreach (var tex in TPFReference)
            //        {
            //            Console.WriteLine("Enter: " + tex.Name);
            //            tex.Bytes = tex.Headerize(TPFReference.Platform);
            //            Console.WriteLine("Leave: " + tex.Name);
            //        }
            //    }
            //}
            //else if (TPFReference.Platform == TPF.TPFPlatform.PS3)
            //{
            //    lock (_lock_conversion)
            //    {
            //        foreach (var tex in TPFReference)
            //        {
            //            Console.WriteLine("Enter: " + tex.Name);
            //            tex.Bytes = tex.Headerize(TPFReference.Platform);
            //            Console.WriteLine("Leave: " + tex.Name);
            //        }
            //    }
            //}
            //else if (TPFReference.Platform == TPF.TPFPlatform.Xbone)
            //{
            //    // Because there are actually xbone textures in the PC version for some dumb reason
            //    return null;
            //}

            if (RequestType == TextureFetchRequestType.TPF)
            {
                if (TPFReference == null)
                    return null;

                var matchedTextures = TPFReference.Textures.Where(x => x.Name == TexName).ToList();

                if (matchedTextures.Count > 0)
                {
                    var tex = matchedTextures.First();
                    //var texBytes = tex.Bytes;

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

                    return new TextureInfo()
                    {
                        DDSBytes = tex.Bytes,
                        Platform = TPFReference.Platform,
                        Texture = tex,
                    };
                }
                else
                {
                    return null;
                }
            }
            else if (RequestType == TextureFetchRequestType.DDS)
            {
                return TexInfo;
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

        internal static int GetBlockSize(byte tpfTexFormat)
        {
            switch (tpfTexFormat)
            {
                case 105:
                    return 4;
                case 0:
                case 1:
                case 22:
                case 25:
                case 103:
                case 108:
                case 109:
                    return 8;
                case 5:
                case 100:
                case 102:
                case 106:
                case 107:
                case 110:
                    return 16;
                default:
                    throw new NotImplementedException($"TPF Texture format {tpfTexFormat} BlockSize unknown.");
            }
        }

        public Texture2D Fetch()
        {
            if (CachedTexture != null)
                return CachedTexture;

            var texInfo = FetchTexInfo();
            if (texInfo == null)
                return null;

            //DDS header = null;
#if !NO_EXCEPTION_CATCH
            try
            {
#endif
                int height = texInfo.Texture?.Header?.Height ?? 0;
                int width = texInfo.Texture?.Header?.Width ?? 0;
                int dxgiFormat = texInfo.Texture?.Header?.DXGIFormat ?? 0;
                int mipmapCount = texInfo.Texture?.Mipmaps ?? 0;
                uint fourCC = DDS.PIXELFORMAT.FourCCDX10;
                int arraySize = texInfo.Texture?.Header?.TextureCount ?? 0;

                int dataStartOffset = 0;

                var br = new BinaryReaderEx(false, texInfo.DDSBytes);

                bool hasHeader = br.ReadASCII(4) == "DDS ";

                int blockSize = !hasHeader ? GetBlockSize(texInfo.Texture.Format) : -1;

                if (hasHeader)
                {
                    DDS header = new DDS(texInfo.DDSBytes);
                    height = header.dwHeight;
                    width = header.dwWidth;
                    mipmapCount = header.dwMipMapCount;
                    fourCC = header.ddspf.dwFourCC;

                    if (header.header10 != null)
                    {
                        arraySize = (int)header.header10.arraySize;
                        dxgiFormat = (int)header.header10.dxgiFormat;
                    }

                    dataStartOffset = header.DataOffset;
                }
                else
                {
                    if (texInfo.Platform == TPF.TPFPlatform.PS4)
                    {
                        switch (texInfo.Texture.Format)
                        {
                            case 0:
                            case 1:
                            case 25:
                            case 103:
                            case 108:
                            case 109:
                                fourCC = DDS.PIXELFORMAT.FourCCDX10; //DX10
                                break;
                            case 5:
                            case 100:
                            case 102:
                            case 106:
                            case 107:
                            case 110:
                                fourCC = DDS.PIXELFORMAT.FourCCDX10; //DX10
                                break;
                            case 22:
                                fourCC = 0x71;
                                break;
                            case 105:
                                fourCC = 0;
                                break;
                        }
                    }
                    else if (texInfo.Platform == TPF.TPFPlatform.PS3)
                    {
                        switch (texInfo.Texture.Format)
                        {
                            case 0:
                            case 1:
                                fourCC = 0x31545844;
                                break;
                            case 5:
                                fourCC = 0x35545844;
                                break;
                            case 9:
                            case 10:
                                fourCC = 0;
                                break;
                        }
                    }

                    if (mipmapCount == 0)
                    {
                        // something Hork came up with :fatcat:
                        mipmapCount = (int)(1 + Math.Floor(Math.Log(Math.Max(width, height), 2)));
                    }

                    dataStartOffset = 0;
                }

                SurfaceFormat surfaceFormat;
                if (fourCC == DDS.PIXELFORMAT.FourCCDX10)
                {
                    // See if there are DX9 textures
                    int fmt = dxgiFormat;
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
                        Console.WriteLine($"Unable to load {TexName} because it uses DX10+ exclusive texture type.");
                        IsDX10 = true;
                        CachedTexture = Main.DEFAULT_TEXTURE_MISSING;
                        TPFReference = null;
                        return CachedTexture;
                    }
                }
                else
                {
                    surfaceFormat = GetSurfaceFormatFromString(System.Text.Encoding.ASCII.GetString(BitConverter.GetBytes(fourCC)));
                }

                bool mipmaps = mipmapCount > 0;

                // apply memes
                if (texInfo.Platform == TPF.TPFPlatform.PC)
                {
                    width = GetNextMultipleOf4(width);
                    height = GetNextMultipleOf4(height);
                    mipmaps = true;
                }
                else if (texInfo.Platform == TPF.TPFPlatform.PS4)
                {
                    width = (int)(Math.Ceiling(width / 4f) * 4f);
                    height = (int)(Math.Ceiling(height / 4f) * 4f);
                }
                else if (texInfo.Platform == TPF.TPFPlatform.PS3)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new NotImplementedException();
                }

                Texture2D tex = new Texture2D(GFX.Device, width, height,
                    mipmapCount > 0,
                    surfaceFormat);

                int currentWidth = width;
                int currentHeight = height;
                int paddedWidth = 0;
                int paddedHeight = 0;
                int paddedSize = 0;
                int copyOffset = dataStartOffset;

                if (arraySize > 1)
                {
                    for (int i = 0; i < arraySize; i++)
                    {
                        currentWidth = width;
                        currentHeight = height;

                        void SetPaddedSize_Cubemap(int w, int h)
                        {
                            if (texInfo.Texture.Format == 22)
                            {
                                paddedWidth = (int)(Math.Ceiling(w / 8f) * 8f);
                                paddedHeight = (int)(Math.Ceiling(h / 8f) * 8f);
                                paddedSize = paddedWidth * paddedHeight * blockSize;
                            }
                            else
                            {
                                paddedWidth = (int)(Math.Ceiling(w / 32f) * 32f);
                                paddedHeight = (int)(Math.Ceiling(h / 32f) * 32f);
                                paddedSize = (int)(Math.Ceiling(paddedWidth / 4f) * Math.Ceiling(paddedHeight / 4f) * blockSize);
                            }
                        }

                        SetPaddedSize_Cubemap(currentWidth, currentHeight);

                        copyOffset = dataStartOffset + paddedSize * i;

                        for (int j = 0; j < mipmapCount; j++)
                        {
                            if (j > 0)
                            {
                                SetPaddedSize_Cubemap(currentWidth, currentHeight);

                                copyOffset += paddedSize * j;
                            }

                            var deswizzler = new DDSDeswizzler(texInfo.Texture.Format, br.GetBytes(copyOffset, paddedSize), blockSize);

                            byte[] deswizzledMipMap = null;

                            if (texInfo.Platform == TPF.TPFPlatform.PS4)
                            {
                                deswizzler.CreateOutput();
                                deswizzler.DDSWidth = paddedWidth;
                                deswizzler.DeswizzleDDSBytesPS4(currentWidth, currentHeight);
                                deswizzledMipMap = deswizzler.OutputBytes;
                            }
                            else if (texInfo.Platform == TPF.TPFPlatform.PS3)
                            {
                                deswizzler.CreateOutput();
                                deswizzler.DDSWidth = paddedWidth;
                                //deswizzler.DeswizzleDDSBytesPS3(currentWidth, currentHeight);
                                deswizzledMipMap = deswizzler.OutputBytes;
                            }

                            var finalBytes = (deswizzledMipMap ?? deswizzler.InputBytes);

                            using (var tempMemStream = new System.IO.MemoryStream(finalBytes.Length))
                            {
                                var tempWriter = new BinaryWriter(tempMemStream);

                                if (texInfo.Texture.Format == 22)
                                {
                                    for (int h = 0; h < currentHeight; h++)
                                    {
                                        tempWriter.Write(finalBytes, h * paddedWidth * blockSize, currentWidth * blockSize);
                                    }
                                }
                                else
                                {
                                    for (int h = 0; h < (int)Math.Ceiling(currentHeight / 4f); h++)
                                    {
                                        tempWriter.Write(finalBytes, (int)(h * Math.Ceiling(paddedWidth / 4f) * blockSize), (int)(Math.Ceiling(currentWidth / 4f) * blockSize));
                                    }
                                }

                                tex.SetData(j, i, null, tempMemStream.ToArray(), 0, finalBytes.Length);
                            }

                            copyOffset += (arraySize - i) * paddedSize + paddedSize * 2;

                            if (currentWidth > 1)
                                currentWidth /= 2;

                            if (currentHeight > 1)
                                currentHeight /= 2;
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < mipmapCount; j++)
                    {
                        if (texInfo.Platform == TPF.TPFPlatform.PC)
                        {
                            paddedSize = GetNextMultipleOf4(currentWidth) * GetNextMultipleOf4(currentHeight);

                            if (surfaceFormat == SurfaceFormat.Dxt1 || surfaceFormat == SurfaceFormat.Dxt1SRgb)
                                paddedSize /= 2;

                            tex.SetData(j, 0, null, br.GetBytes(copyOffset, paddedSize), 0, paddedSize);
                            copyOffset += paddedSize;
                        }
                        else
                        {
                            if (texInfo.Texture.Format == 105)
                            {
                                paddedWidth = currentWidth;
                                paddedHeight = currentHeight;
                                paddedSize = paddedWidth * paddedHeight * blockSize;
                            }
                            else
                            {
                                paddedWidth = (int)(Math.Ceiling(currentWidth / 32f) * 32f);
                                paddedHeight = (int)(Math.Ceiling(currentHeight / 32f) * 32f);
                                paddedSize = (int)(Math.Ceiling(paddedWidth / 4f) * Math.Ceiling(paddedHeight / 4f) * blockSize);
                            }

                            var deswizzler = new DDSDeswizzler(texInfo.Texture.Format, br.GetBytes(copyOffset, paddedSize), blockSize);

                            byte[] deswizzledMipMap = null;

                            if (texInfo.Platform == TPF.TPFPlatform.PS4)
                            {
                                deswizzler.CreateOutput();
                                deswizzler.DDSWidth = paddedWidth;
                                deswizzler.DeswizzleDDSBytesPS4(currentWidth, currentHeight);
                                deswizzledMipMap = deswizzler.OutputBytes;
                            }
                            else if (texInfo.Platform == TPF.TPFPlatform.PS3)
                            {
                                deswizzler.CreateOutput();
                                deswizzler.DDSWidth = paddedWidth;
                                //deswizzler.DeswizzleDDSBytesPS3(currentWidth, currentHeight);
                                deswizzledMipMap = deswizzler.OutputBytes;
                            }

                            var finalBytes = (deswizzledMipMap ?? deswizzler.InputBytes);

                            using (var tempMemStream = new System.IO.MemoryStream())
                            {
                                var tempWriter = new BinaryWriter(tempMemStream);


                                if (texInfo.Platform == TPF.TPFPlatform.PS4)
                                {
                                    if (texInfo.Texture.Format == 105)
                                    {
                                        tempWriter.Write(finalBytes);
                                    }
                                    else
                                    {
                                        for (int h = 0; h < (int)Math.Ceiling(currentHeight / 4f); h++)
                                        {
                                            tempWriter.Write(finalBytes, (int)(h * Math.Ceiling(paddedWidth / 4f) * blockSize), (int)(Math.Ceiling(currentWidth / 4f) * blockSize));
                                        }
                                    }
                                }
                                else if (texInfo.Platform == TPF.TPFPlatform.PS3)
                                {
                                    throw new NotImplementedException();
                                }
                                else
                                {
                                    throw new NotImplementedException();
                                }

                                tex.SetData(j, 0, null, tempMemStream.ToArray(), 0, (int)tempMemStream.Length);
                            }

                            copyOffset += paddedSize;
                        }



                        if (currentWidth > 1)
                            currentWidth /= 2;

                        if (currentHeight > 1)
                            currentHeight /= 2;
                    }
                }



                CachedTexture?.Dispose();

                CachedTexture = tex;

                return CachedTexture;
#if !NO_EXCEPTION_CATCH
            }
            catch (Exception ex)
            {
                if (!TexturePool.Failures.ContainsKey(texInfo))
                    TexturePool.Failures.Add(texInfo, ex);

                return null;
            }
#endif
        }

        public void Dispose()
        {
            TPFReference = null;

            CachedTexture?.Dispose();
            CachedTexture = null;
        }
    }
}
