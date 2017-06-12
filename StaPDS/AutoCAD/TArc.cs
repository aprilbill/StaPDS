using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoCAD.Common;
using AutoCAD.Serialization;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;


namespace AutoCAD
{
    public class TArc : TFigure
    {
        // Fields
        private float Radius;//半径
        public float startAngle;
        public float sweepAngle;
        // Methods
        public TArc(Factory _factory)        
        {
            FigureType = "Arc";
            GeometrySink sink = orginGeomtery.Open();
            sink.BeginFigure(_mpoint[0],FigureBegin.Filled );
            for(int i=1;i<_mpoint .Count -1;i++)
            {
                sink.AddArc (new ArcSegment ())(_mpoint[i]);
            }
            sink.EndFigure(FigureEnd.Open);
            sink.Close();
        }
        
        
    }
}
