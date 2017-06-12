using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using AutoCAD.Serialization;
using AutoCAD.Common;

namespace AutoCAD
{
    public class TLines : TFigure
    {   
        public TLines (List <Vector2> _mpoint,Color _color,Factory _factory):base(_mpoint,_color ,_factory )
        {
            FigureType = "Line";
            GeometrySink sink = orginGeomtery.Open();
            sink.BeginFigure(_mpoint[0],FigureBegin.Filled );
            for(int i=1;i<_mpoint .Count -1;i++)
            {
                sink.AddLine(_mpoint[i]);
            }
            sink.EndFigure(FigureEnd.Open);
            sink.Close();
        }
        public override TLines Clone()
        {
            TLines tmpLines = new TLines(this.mPoints, this.FigColor, this.factory);
            tmpLines.layer = this.layer;
            return tmpLines;
        }
        public bool isCrossed(TLines yline)
        {
            GeometryRelation GetResult = curGeometry.Compare(yline.curGeometry);          
            if (GetResult != GeometryRelation.Disjoint && GetResult != GeometryRelation.Unknown)
            {
                return true;
            }
            return false;
        }   
    }
}
