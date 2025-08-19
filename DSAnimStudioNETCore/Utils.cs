using Microsoft.Xna.Framework;
using SharpDX.Direct2D1.Effects;
using SoulsAssetPipeline;
using SoulsAssetPipeline.Animation;
using SoulsAssetPipeline.Animation.SIBCAM;
using SoulsFormats;
using System;
using System.IO;
using System.Text;

namespace DSAnimStudio
{
    public static class Utils
    {
        public static Vector3 MemeVector3Normalize(Vector3 v)
        {
            if (v == Vector3.Zero)
                return Vector3.Zero;
            return Vector3.Normalize(v);
        }
        
        public static Matrix MatrixMemeLerp(Matrix a, Matrix b, float s)
        {
            return NewBlendableTransform.Lerp(a.ToNewBlendableTransform(), b.ToNewBlendableTransform(), s)
                .GetXnaMatrixFull();
        }
        
        public static Matrix GetDebugMatrixFuzz(float x, float y, float z)
        {
            return Matrix.CreateTranslation(((Main.RandFloat() * 2) - 1) * x, ((Main.RandFloat() * 2) - 1) * y, ((Main.RandFloat() * 2) - 1) * z);
        }
        
        public static Matrix GetDebugMatrixFuzz(float xyz)
        {
            return GetDebugMatrixFuzz(xyz, xyz, xyz);
        }
        
        public static Matrix DebugMatrixFuzz =>
            Matrix.CreateTranslation(Main.RandFloat(), Main.RandFloat(), Main.RandFloat());
        
        public static bool IsReadBinder(byte[] bytes, out IBinder binder)
        {
            if (DCX.Is(bytes))
            {
                var decompressedBytes = DCX.Decompress(bytes, out DCX.Type dcxType);
                if (BND3.IsRead(decompressedBytes, out BND3 asBND3))
                {
                    binder = asBND3;
                    asBND3.Compression = dcxType;
                    return true;
                }
                else if (BND4.IsRead(decompressedBytes, out BND4 asBND4))
                {
                    binder = asBND4;
                    asBND4.Compression = dcxType;
                    return true;
                }
            }
            else if (BND3.IsRead(bytes, out BND3 outerAsBND3))
            {
                binder = outerAsBND3;
                return true;
            }
            else if (BND4.IsRead(bytes, out BND4 outerAsBND4))
            {
                binder = outerAsBND4;
                return true;
            }

            binder = null;
            return false;
        }

        public static bool IsReadBinder(string path, out IBinder binder)
        {
            if (DCX.Is(path))
            {
                var bytes = DCX.Decompress(path, out DCX.Type dcxType);
                if (BND3.IsRead(bytes, out BND3 asBND3))
                {
                    binder = asBND3;
                    asBND3.Compression = dcxType;
                    return true;
                }
                else if (BND4.IsRead(bytes, out BND4 asBND4))
                {
                    binder = asBND4;
                    asBND4.Compression = dcxType;
                    return true;
                }
            }
            else
            {
                var bytes = File.ReadAllBytes(path);
                if (BND3.IsRead(bytes, out BND3 asBND3))
                {
                    binder = asBND3;
                    return true;
                }
                else if (BND4.IsRead(bytes, out BND4 asBND4))
                {
                    binder = asBND4;
                    return true;
                }
            }

            binder = null;
            return false;
        }

        public static byte[] WriteBinder(IBinder binder)
        {
            if (binder is BND3 asBND3)
                return asBND3.Write();
            else if (binder is BND4 asBND4)
                return asBND4.Write();
            return null;
        }

        public static IBinder ReadBinder(byte[] bytes)
        {
            if (IsReadBinder(bytes, out IBinder binder))
                return binder;
            else
                return null;
        }

        public static IBinder ReadBinder(string path)
        {
            if (IsReadBinder(path, out IBinder binder))
                return binder;
            else
                return null;
        }

        public static Matrix SlowAccurateMatrixLerp(Matrix a, Matrix b, float s)
        {
            if (a.Decompose(out Vector3 aS, out Quaternion aR, out Vector3 aT))
            {
                if (b.Decompose(out Vector3 bS, out Quaternion bR, out Vector3 bT))
                {
                    var transA = new NewBlendableTransform(aT.ToCS(), aS.ToCS(), Quaternion.Normalize(aR).ToCS());
                    var transB = new NewBlendableTransform(bT.ToCS(), bS.ToCS(), Quaternion.Normalize(bR).ToCS());
                    var transBlend = NewBlendableTransform.Lerp(transA, transB, s);
                    return (transBlend.GetMatrixScale() * transBlend.GetMatrix()).ToXna();
                }
            }

            return Matrix.Lerp(a, b, s);
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            double num;
            temp3 = Utils.MoveIntoRange(temp3);
            if (temp3 < 0.166666666666667)
            {
                num = temp1 + (temp2 - temp1) * 6 * temp3;
            }
            else if (temp3 >= 0.5)
            {
                num = (temp3 >= 0.666666666666667 ? temp1 : temp1 + (temp2 - temp1) * (0.666666666666667 - temp3) * 6);
            }
            else
            {
                num = temp2;
            }
            return num;
        }

        public static Quaternion EulerToQuaternion(Vector3 euler)
        {
            var qy = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), euler.Y);
            var qz = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), euler.Z);
            var qx = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), euler.X);
            return qy * qz * qx;
        }

        public static float MapRange(double s, double a1, double a2, double b1, double b2) => (float)(b1 + (s - a1) * (b2 - b1) / (a2 - a1));

        private static double GetTemp2(float H, float S, float L)
        {
            double temp2;
            temp2 = ((double)L >= 0.5 ? (double)(L + S - L * S) : (double)L * (1 + (double)S));
            return temp2;
        }

        public static Color HSLtoRGB(float H, float S, float L, float alpha)
        {
            byte r, g, b;
            if (S == 0)
            {
                r = (byte)Math.Round(L * 255d);
                g = (byte)Math.Round(L * 255d);
                b = (byte)Math.Round(L * 255d);
            }
            else
            {
                double t1, t2;
                double th = H / 6.0d;

                if (L < 0.5d)
                {
                    t2 = L * (1d + S);
                }
                else
                {
                    t2 = (L + S) - (L * S);
                }
                t1 = 2d * L - t2;

                double tr, tg, tb;
                tr = th + (1.0d / 3.0d);
                tg = th;
                tb = th - (1.0d / 3.0d);

                tr = ColorCalc(tr, t1, t2);
                tg = ColorCalc(tg, t1, t2);
                tb = ColorCalc(tb, t1, t2);
                r = (byte)Math.Round(tr * 255d);
                g = (byte)Math.Round(tg * 255d);
                b = (byte)Math.Round(tb * 255d);
            }
            return new Color(r, g, b, (byte)Math.Round(alpha * 255d));
        }

        private static double ColorCalc(double c, double t1, double t2)
        {

            if (c < 0) c += 1d;
            if (c > 1) c -= 1d;
            if (6.0d * c < 1.0d) return t1 + (t2 - t1) * 6.0d * c;
            if (2.0d * c < 1.0d) return t2;
            if (3.0d * c < 2.0d) return t1 + (t2 - t1) * (2.0d / 3.0d - c) * 6.0d;
            return t1;
        }

        private static double MoveIntoRange(double temp3)
        {
            if (temp3 < 0)
            {
                temp3 += 1;
            }
            else if (temp3 > 1)
            {
                temp3 -= 1;
            }
            return temp3;
        }

        public static string Frankenpath(params string[] pathParts)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < pathParts.Length; i++)
            {
                sb.Append(pathParts[i].Trim('\\'));
                if (i < pathParts.Length - 1)
                    sb.Append('\\');
            }

            return sb.ToString();
        }

        public static string GetShortIngameFileName(string fileName)
        {
            if (fileName == null)
                return null;

            return GetFileNameWithoutAnyExtensions(GetFileNameWithoutDirectoryOrExtension(fileName));
        }

        public static string RemoveInvalidPathChars(string input)
        {
            return string.Concat(input.Split(Path.GetInvalidPathChars()));
        }

        public static string RemoveInvalidFileNameChars(string input)
        {
            return string.Concat(input.Split(Path.GetInvalidFileNameChars()));
        }

        private static readonly char[] _dirSep = new char[] { '\\', '/' };
        public static string GetFileNameWithoutDirectoryOrExtension(string fileName)
        {
            if (fileName == null)
                return null;

            if (fileName.EndsWith("\\") || fileName.EndsWith("/"))
                fileName = fileName.TrimEnd(_dirSep);

            if (fileName.Contains("\\") || fileName.Contains("/"))
                fileName = fileName.Substring(fileName.LastIndexOfAny(_dirSep) + 1);

            if (fileName.Contains("."))
                fileName = fileName.Substring(0, fileName.LastIndexOf('.'));

            return fileName;
        }

        public static string GetOnlyDirectoryName(string fileName)
        {
            try
            {
                var dirName = Path.GetDirectoryName(fileName);
                return dirName;
            }
            catch
            {

            }
            if (fileName.Contains("\\") || fileName.Contains("/"))
                fileName = fileName.Substring(0, fileName.LastIndexOfAny(_dirSep) - 1);

            return fileName;
        }

        public static string GetFileNameWithoutAnyExtensions(string fileName)
        {
            if (fileName == null)
                return null;

            var dirSepIndex = fileName.LastIndexOfAny(_dirSep);
            if (dirSepIndex >= 0)
            {
                var dotIndex = -1;
                bool doContinue = true;
                do
                {
                    dotIndex = fileName.LastIndexOf('.');
                    doContinue = dotIndex > dirSepIndex;
                    if (doContinue)
                        fileName = fileName.Substring(0, dotIndex);
                }
                while (doContinue);
            }
            else
            {
                var dotIndex = -1;
                bool doContinue = true;
                do
                {
                    dotIndex = fileName.LastIndexOf('.');
                    doContinue = dotIndex >= 0;
                    if (doContinue)
                        fileName = fileName.Substring(0, dotIndex);
                }
                while (doContinue);
            }

            return fileName;
        }
    }
}