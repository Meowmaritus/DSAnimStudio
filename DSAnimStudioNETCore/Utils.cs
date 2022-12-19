using Microsoft.Xna.Framework;
using SoulsAssetPipeline;
using SoulsAssetPipeline.Animation;
using SoulsAssetPipeline.Animation.SIBCAM;
using System;
using System.Text;

namespace DSAnimStudio
{
    public static class Utils
    {
        

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

        public static Color HSLtoRGB(float H, float S, float L)
        {
            double r = 0;
            double g = 0;
            double b = 0;
            if (L != 0f)
            {
                if (S != 0f)
                {
                    double temp2 = Utils.GetTemp2(H, S, L);
                    double temp1 = 2 * (double)L - temp2;
                    r = Utils.GetColorComponent(temp1, temp2, (double)H + 0.333333333333333);
                    g = Utils.GetColorComponent(temp1, temp2, (double)H);
                    b = Utils.GetColorComponent(temp1, temp2, (double)H - 0.333333333333333);
                }
                else
                {
                    double l = (double)L;
                    b = l;
                    g = l;
                    r = l;
                }
            }
            Color color = Color.FromNonPremultiplied(new Vector4((float)r, (float)g, (float)b, 1f));
            return color;
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