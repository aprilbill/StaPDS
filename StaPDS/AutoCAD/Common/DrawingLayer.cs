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
    public struct DrawingLayer
    { 
        public int colorIndex;
        public string name;
        public DrawingLayer(string aName, int aIndex)
        {
            this.name = aName;
            this.colorIndex = aIndex;
        }
    }
}
