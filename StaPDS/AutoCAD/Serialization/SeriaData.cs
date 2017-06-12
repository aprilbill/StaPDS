using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;

namespace AutoCAD.Serialization
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct SeriaData
    {
        public Type classType;
        public Color penColor;
        public PointF[] Points;
        public bool IsClosed;
        public float Radius;
        public float startAngle;
        public float sweepAngle;
    }

 

}
