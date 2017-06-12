using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;

namespace AutoCAD.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CaptureResult
    {
        public PointF Coo;
        public bool IsCaptured;
        public CaptureResult(bool aIsCapured, PointF aCoo)
        {
            this.Coo = aCoo;
            this.IsCaptured = aIsCapured;
        }
    } 

    
}
