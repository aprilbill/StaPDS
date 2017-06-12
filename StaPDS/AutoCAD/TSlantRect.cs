using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace AutoCAD
{
    public class TSlantRect
    {
        public TSlantRect(PointF[] aPoints)
        {
            for (int i = 0; i < Points.Length; i++) Points[i] = aPoints[i];

            //float w1 = Global.Distance(Points[0], Points[1]);
            //float w2 = Distance(Points[1], Points[2]);

            //if (w1 > w2)//0 到 1 是长边
            //{
            //    W = w1;
            //    w = w2;

            //    Cta = (float)Math.Atan2(Points[0].Y - Points[1].Y, Points[0].X - Points[1].X);
            //    Cta *= 180 / (float)Math.PI;
            //}
            //else //1 到 2 是长边
            //{
            //    W = w2;
            //    w = w1;

            //    Cta = (float)Math.Atan2(Points[1].Y - Points[2].Y, Points[1].X - Points[2].X);
            //    Cta *= 180 / (float)Math.PI;
            //    if (Cta < 0) Cta += 360;
            //}
        }

        public float W, w;//长边，短边
        public float Cta;//矩形倾斜的角度
        public PointF[] Points = new PointF[4];//包含的点  
        
     
    }
}
