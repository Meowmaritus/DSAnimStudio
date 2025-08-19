using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimWireFan : DbgPrimWire
    {
        //private DbgPrimGeometryData Geom;
        

        public DbgPrimWireFan(Color color, ParamData.AtkParam.Hit hit)
        {
            KeepBuffersAlive = false;
            

            float angleRange = MathHelper.ToRadians(hit.AC6Fan_SpanAngle);
            float a = -(angleRange / 2f);
            float b = (a + (angleRange * 0.25f));
            float c = (a + (angleRange * 0.50f));
            float d = (a + (angleRange * 0.75f));
            float e = (angleRange / 2f);

            var m = Matrix.CreateRotationX(MathHelper.ToRadians(hit.AC6Fan_RotationX))
                * Matrix.CreateRotationY(MathHelper.ToRadians(hit.AC6Fan_RotationY)) 
                * Matrix.CreateRotationZ(MathHelper.ToRadians(hit.AC6Fan_RotationZ));

            Vector3 reach = new Vector3(hit.AC6Fan_Reach, 1, -hit.AC6Fan_Reach);

            Vector3 vaHigh = Vector3.Transform(new Vector3(MathF.Sin(a), hit.AC6Fan_ThicknessTop, MathF.Cos(a)) * reach, m);
            Vector3 vbHigh = Vector3.Transform(new Vector3(MathF.Sin(b), hit.AC6Fan_ThicknessTop, MathF.Cos(b)) * reach, m);
            Vector3 vcHigh = Vector3.Transform(new Vector3(MathF.Sin(c), hit.AC6Fan_ThicknessTop, MathF.Cos(c)) * reach, m);
            Vector3 vdHigh = Vector3.Transform(new Vector3(MathF.Sin(d), hit.AC6Fan_ThicknessTop, MathF.Cos(d)) * reach, m);
            Vector3 veHigh = Vector3.Transform(new Vector3(MathF.Sin(e), hit.AC6Fan_ThicknessTop, MathF.Cos(e)) * reach, m);
            Vector3 vaLow = Vector3.Transform(new Vector3(MathF.Sin(a), -hit.AC6Fan_ThicknessBottom, MathF.Cos(a)) * reach, m);
            Vector3 vbLow = Vector3.Transform(new Vector3(MathF.Sin(b), -hit.AC6Fan_ThicknessBottom, MathF.Cos(b)) * reach, m);
            Vector3 vcLow = Vector3.Transform(new Vector3(MathF.Sin(c), -hit.AC6Fan_ThicknessBottom, MathF.Cos(c)) * reach, m);
            Vector3 vdLow = Vector3.Transform(new Vector3(MathF.Sin(d), -hit.AC6Fan_ThicknessBottom, MathF.Cos(d)) * reach, m);
            Vector3 veLow = Vector3.Transform(new Vector3(MathF.Sin(e), -hit.AC6Fan_ThicknessBottom, MathF.Cos(e)) * reach, m);
            Vector3 baseHigh = Vector3.Transform(new Vector3(0, hit.AC6Fan_ThicknessTop, 0), m);
            Vector3 baseLow = Vector3.Transform(new Vector3(0, -hit.AC6Fan_ThicknessBottom, 0), m);

            AddLine(baseHigh, vaHigh);
            AddLine(vaHigh, vbHigh);
            AddLine(vbHigh, vcHigh);
            AddLine(vcHigh, vdHigh);
            AddLine(vdHigh, veHigh);
            AddLine(veHigh, baseHigh);

            AddLine(baseLow, vaLow);
            AddLine(vaLow, vbLow);
            AddLine(vbLow, vcLow);
            AddLine(vcLow, vdLow);
            AddLine(vdLow, veLow);
            AddLine(veLow, baseLow);

            AddLine(baseHigh, baseLow);
            AddLine(vaHigh, vaLow);
            AddLine(vbHigh, vbLow);
            AddLine(vcHigh, vcLow);
            AddLine(vdHigh, vdLow);
            AddLine(veHigh, veLow);


            FinalizeBuffers(true);
        }
    }
}
