using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using AutoCAD.Common;

namespace AutoCAD
{
   public class DxfReader
{
    // Fields
    private TEntities outEntities;
    private StreamReader Sr;

    // Methods
    public bool Open(string FileName)
    {
        try
        {
            this.Sr = new StreamReader(FileName, Encoding.Default);
        }
        catch (Exception exception1)
        {
            return false;
        }
        return true;
    }

    public void Read(TEntities aEntities)
    {
      
            string str;
            this.outEntities = aEntities;
            while ((str = this.Sr.ReadLine()) != null)
            {
                if (str.Equals("  0"))
                {
                    if (this.Sr.ReadLine().Equals("LAYER"))
                    {
                        Global.AddLayer(this.ReadLayer());
                    }
                }
                else if (str.Equals("ENTITIES"))
                {
                    break;
                }
            }
            while ((str = this.Sr.ReadLine()) != null)
            {
                if (str.Equals("  0"))
                {
                    str = this.Sr.ReadLine();
                    if (str.Equals("LINE"))
                    {
                        this.outEntities.Add(this.ReadLine());
                    }
                    else
                    {
                        if (str.Equals("LWPOLYLINE"))
                        {
                            this.outEntities.Add(this.ReadPolyline());
                            continue;
                        }
                        if (str.Equals("CIRCLE") || str.Equals("ARC"))
                        {
                            this.outEntities.Add(this.ReadArc(str));
                            continue;
                        }
                        if (str.Equals("ENDSEC"))
                        {
                            break;
                        }
                    }
                }
            }
            this.Sr.Close();
            this.outEntities.CalculateM();
       
    }

    private TArc ReadArc(string cType)
    {
        string str;
        Color white = Color.White;
        while ((str = this.Sr.ReadLine()) != null)
        {
            if (str.Equals("  8"))
            {
                white = Global.FindLayerColor(this.Sr.ReadLine());
            }
            else if (str.Equals("AcDbCircle"))
            {
                break;
            }
        }
        PointF[] aPoints = new PointF[] { this.ReadPoint() };
        float aRadius = 0f;
        float astartAngle = 0f;
        float asweepAngle = 360f;
        while ((str = this.Sr.ReadLine()) != null)
        {
            if (str.Equals(" 40"))
            {
                aRadius = Convert.ToSingle(this.Sr.ReadLine());
                if (cType.Equals("CIRCLE"))
                {
                    return new TArc(white, aPoints, aRadius, astartAngle, asweepAngle);
                }
            }
            else
            {
                if (str.Equals(" 50"))
                {
                    astartAngle = Convert.ToSingle(this.Sr.ReadLine());
                    continue;
                }
                if (str.Equals(" 51"))
                {
                    asweepAngle = Convert.ToSingle(this.Sr.ReadLine());
                    break;
                }
            }
        }
        if (asweepAngle < astartAngle)
        {
            asweepAngle = (360f + asweepAngle) - astartAngle;
        }
        else
        {
            asweepAngle -= astartAngle;
        }
        return new TArc(white, aPoints, aRadius, astartAngle, asweepAngle);
    }

    private DrawingLayer ReadLayer()
    {
        string str;
        string aName = "";
        int aIndex = 0;
        while ((str = this.Sr.ReadLine()) != null)
        {
            if (str.Equals("  2"))
            {
                aName = this.Sr.ReadLine();
            }
            else if (str.Equals(" 62"))
            {
                aIndex = Convert.ToInt32(this.Sr.ReadLine());
                break;
            }
        }
        return new DrawingLayer(aName, aIndex);
    }

    private TLines ReadLine()
    {
        string str;
        Color white = Color.White;
        while ((str = this.Sr.ReadLine()) != null)
        {
            if (str.Equals("  8"))
            {
                white = Global.FindLayerColor(this.Sr.ReadLine());
            }
            else if (str.Equals("AcDbLine"))
            {
                break;
            }
        }
        PointF[] aPoints = new PointF[3];
        for (int i = 0; i < 2; i++)
        {
            aPoints[i] = this.ReadPoint();
        }
        return new TLines(white, aPoints);
    }

    private PointF ReadPoint()
    {
        string str;
        float[] numArray = new float[2];
        while ((str = this.Sr.ReadLine()) != null)
        {
            if (str.Equals(" 10") || str.Equals(" 11"))
            {
                str = this.Sr.ReadLine();
                numArray[0] = Convert.ToSingle(str);
            }
            else if (str.Equals(" 20") || str.Equals(" 21"))
            {
                str = this.Sr.ReadLine();
                numArray[1] = Convert.ToSingle(str);
                break;
            }
        }
        return new PointF(numArray[0], numArray[1]);
    }

    private TLines ReadPolyline()
    {
        string str;
        int num = 0;
        int num2 = 0;
        Color white = Color.White;
        while ((str = this.Sr.ReadLine()) != null)
        {
            if (str.Equals("  8"))
            {
                white = Global.FindLayerColor(this.Sr.ReadLine());
            }
            else
            {
                if (str.Equals(" 90"))
                {
                    num = Convert.ToInt32(this.Sr.ReadLine());
                    continue;
                }
                if (str.Equals(" 70"))
                {
                    num2 = Convert.ToInt32(this.Sr.ReadLine());
                    break;
                }
            }
        }
        PointF[] array = new PointF[num + 1];
        for (int i = 0; i < num; i++)
        {
            array[i] = this.ReadPoint();
        }
        bool aIsClosed = false;
        if (num2 == 1)
        {
            aIsClosed = true;
            array[array.Length - 1] = array[0];
            Array.Resize<PointF>(ref array, array.Length + 1);
        }
        return new TLines(aIsClosed, white, array);
    }
}



}
