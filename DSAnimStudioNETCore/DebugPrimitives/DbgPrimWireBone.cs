using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimWireBone : DbgPrimWire
    {
        public const float ThicknessRatio = 0.05f;

        private static DbgPrimGeometryData GeometryData = null;

        public DbgPrimWireBone(string name, Transform location, Color color, float length, float thickness)
        {
            KeepBuffersAlive = false;

            Transform = location;
            NameColor = color;
            OverrideColor = color;
            Name = name;

            Vector3 pt_start = Vector3.Zero;
            Vector3 pt_cardinal1 = Vector3.Up * thickness + Vector3.Right * thickness;
            Vector3 pt_cardinal2 = Vector3.Forward * thickness + Vector3.Right * thickness;
            Vector3 pt_cardinal3 = Vector3.Down * thickness + Vector3.Right * thickness;
            Vector3 pt_cardinal4 = Vector3.Backward * thickness + Vector3.Right * thickness;
            Vector3 pt_tip = Vector3.Right * length;

            //Start to cardinals
            AddLine(pt_start, pt_cardinal1);
            AddLine(pt_start, pt_cardinal2);
            AddLine(pt_start, pt_cardinal3);
            AddLine(pt_start, pt_cardinal4);

            //Cardinals to end
            AddLine(pt_cardinal1, pt_tip);
            AddLine(pt_cardinal2, pt_tip);
            AddLine(pt_cardinal3, pt_tip);
            AddLine(pt_cardinal4, pt_tip);

            //Connecting the cardinals
            AddLine(pt_cardinal1, pt_cardinal2);
            AddLine(pt_cardinal2, pt_cardinal3);
            AddLine(pt_cardinal3, pt_cardinal4);
            AddLine(pt_cardinal4, pt_cardinal1);

            FinalizeBuffers(true);
        }

        public DbgPrimWireBone(string name, Transform location, Color color)
        {
            KeepBuffersAlive = true;

            Transform = location;
            NameColor = color;
            OverrideColor = color;
            Name = name;

            if (GeometryData != null)
            {
                SetBuffers(GeometryData.VertBuffer, GeometryData.IndexBuffer);
            }
            else
            {
                Vector3 pt_start = Vector3.Zero;
                //Vector3 pt_cardinal1 = Vector3.Up * ThicknessRatio + Vector3.Right * ThicknessRatio;
                //Vector3 pt_cardinal2 = Vector3.Forward * ThicknessRatio + Vector3.Right * ThicknessRatio;
                //Vector3 pt_cardinal3 = Vector3.Down * ThicknessRatio + Vector3.Right * ThicknessRatio;
                //Vector3 pt_cardinal4 = Vector3.Backward * ThicknessRatio + Vector3.Right * ThicknessRatio;
                Vector3 pt_tip = Vector3.Right;

                ////Start to cardinals
                //AddLine(pt_start, pt_cardinal1);
                //AddLine(pt_start, pt_cardinal2);
                //AddLine(pt_start, pt_cardinal3);
                //AddLine(pt_start, pt_cardinal4);

                ////Cardinals to end
                //AddLine(pt_cardinal1, pt_tip);
                //AddLine(pt_cardinal2, pt_tip);
                //AddLine(pt_cardinal3, pt_tip);
                //AddLine(pt_cardinal4, pt_tip);

                ////Connecting the cardinals
                //AddLine(pt_cardinal1, pt_cardinal2);
                //AddLine(pt_cardinal2, pt_cardinal3);
                //AddLine(pt_cardinal3, pt_cardinal4);
                //AddLine(pt_cardinal4, pt_cardinal1);



                AddLine(pt_start, pt_tip);




                FinalizeBuffers(true);

                GeometryData = new DbgPrimGeometryData()
                {
                    VertBuffer = VertBuffer,
                    IndexBuffer = IndexBuffer,
                };
            }

            
        }
    }
}
