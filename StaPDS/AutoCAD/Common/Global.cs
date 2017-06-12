using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;

namespace AutoCAD.Common
{
    public class Global
    {
        // Fields
        internal static Color [] colorIndex = new  Color [9] ;
        public static float Ss = 10f;
        internal static List<DrawingLayer> layers = new List<DrawingLayer>();
        public static Matrix[] M = new Matrix[2];
        public static Cursor[] myCursors=new Cursor [3];
        public static Matrix tM = new Matrix();
        public static float cadScale = 0.01f;

        
        // Methods
        public  static void AddLayer(DrawingLayer Layer)
        {
            layers.Add(Layer);
        }
       
        public static float Distance(PointF p1, PointF p2)        
        {
            float num = p1.X - p2.X;
            float num2 = p1.Y - p2.Y;
            return (float) Math.Sqrt((double) ((num * num) + (num2 * num2)));
        }
        
        public static float FCta(PointF Pc, PointF P1, PointF P2)
        {
            if ((Math.Abs((float) (Pc.X - P1.X)) + Math.Abs((float) (Pc.Y - P1.Y))) < 10f)
            {
                return 0f;
            }
            if ((Math.Abs((float) (Pc.X - P2.X)) + Math.Abs((float) (Pc.Y - P2.Y))) < 10f)
            {
                return 0f;
            }
            float num = (P1.X - Pc.X) / (P1.Y - Pc.Y);
            float num2 = (P2.X - Pc.X) / (P2.Y - Pc.Y);
            float maxValue = float.MaxValue;
            float num4 = 1f + (num * num2);
            maxValue = (num - num2) / num4;
            float num5 = (float) Math.Atan((double) maxValue);
            return (float) (((double) (num5 * 180f)) / Math .PI );
        }
        public  static Color FindLayerColor(string Name)
        {
            return colorIndex[6];
        }
        public static void Initialize()
        {
            tM.Reset();
            M[0] = new Matrix();
            M[1] = new Matrix();
            string startupPath = Application.StartupPath;
            myCursors[0] = new Cursor(startupPath + @"\Cursors\Default.cur");
            myCursors[1] = new Cursor(startupPath + @"\Cursors\Hand.cur");
            myCursors[2] = new Cursor(startupPath + @"\Cursors\Drawing.cur");
            colorIndex[0] = Color.Red;
            colorIndex[1] = Color.Yellow;
            colorIndex[2] = Color.FromArgb(0, 0xff, 0);//绿色 
            colorIndex[3] = Color.FromArgb(0, 0xff, 0xff);//青色
            colorIndex[4] = Color.Blue;
            colorIndex[5] = Color.FromArgb(0xff, 0, 0xff);//洋红
            colorIndex[6] = Color.Black;
            colorIndex[7] = Color.FromArgb(0x80, 0x80, 0x80);//灰色
            colorIndex[8] = Color.FromArgb(0xc0, 0xc0, 0xc0);//浅灰
           
        }
 
     
   
        //for SharpDX
        public static SharpDX.Mathematics.Interop.RawColor4 ToColor4(Color cor)
        {
            return new SharpDX.Mathematics.Interop.RawColor4((float)cor.R / 256f, (float)cor.G / 256f, (float)cor.B / 256f, (float)cor.A / 256f);
        }

        public static SharpDX.Mathematics.Interop.RawRectangle ToRect(Rectangle rect)
        {
            return new SharpDX.Mathematics.Interop.RawRectangle(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
        public static SharpDX.Mathematics.Interop.RawRectangleF ToRectF(RectangleF rect)
        {
            return new SharpDX.Mathematics.Interop.RawRectangleF(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }     
    }
}
