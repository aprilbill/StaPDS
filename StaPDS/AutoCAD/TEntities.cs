using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoCAD.Common;
using AutoCAD.Serialization;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace AutoCAD
{
    public class TEntities//相当于一个楼层
    {
        // Fields
        public PictureBox pictureBox;//底图
        private DxfReader dxfReader;//读取dxf
        public ContextMenuStrip contextMenuStrip直线 = new ContextMenuStrip();
        public bool AutoRefresh;
        public Bitmap Bg;
        private Pen CapturePen = new Pen(Color.DarkOrange, 2f);
       
        public CaptureResult cResult;
        public PointF[] curPos = new PointF[3];
        public bool Dragging;
        public bool Drawing;
      
        public List<TFigure> Figures = new List<TFigure>();
        public Graphics Gr;
        private Pen lcPen = new Pen(Color.FromArgb(180, Color.Red));
        private Tools mTool = Tools.None;
        private List<PointF> newPolyLine = new List<PointF>();
        
        private SolidBrush pkBrush = new SolidBrush(Color.Transparent);
        public PointF pkMax;
        public PointF pkMin;
        private Pen pkPen = new Pen(Color.Black);
        public PointF[] Points = new PointF[4];
        private float r;
        private float Scale = 1f;
        public List<TFigure> selectedFigures = new List<TFigure>();
        private List<SeriaData> seriaDatas = new List<SeriaData>();
        public Bitmap tBg;
        public Graphics tGr;
        private Timer timer1 = new Timer();
        public Timer timer2 = new Timer();

    // Methods
    public TEntities(string pass)
    {
        if (pass.Equals("Silver 0254"))
        {
            this.timer1.Enabled = true;
            this.timer1.Tick += new EventHandler(this.timer1_Tick);
            this.timer2.Enabled = true;
            this.timer2.Tick += new EventHandler(this.timer2_Tick);
            this.contextMenuStrip直线.Items.Add("确认(&Y)");
            this.contextMenuStrip直线.Items[0].Click += new EventHandler(this.contextMenuStrip直线_确认_Click);
            this.contextMenuStrip直线.Items.Add("取消(&N)");
            this.contextMenuStrip直线.Items[1].Click += new EventHandler(this.contextMenuStrip直线_取消_Click);
        }
    }

    public void Active()
    {
        this.timer1.Enabled = true;
        this.timer2.Enabled = true;
    }

    public void Add(TFigure aFigure)
    {
        this.Figures.Add(aFigure);
    }

    public void CalculateM()
    {
        this.Max_Min(ref this.Points[2], ref this.Points[3]);
        float num5 = this.Points[1].X - this.Points[0].X;
        float num6 = this.Points[3].X - this.Points[2].X;
        float num = num5 / num6;
        float num7 = this.Points[1].Y - this.Points[0].Y;
        float num8 = this.Points[3].Y - this.Points[2].Y;
        float num3 = num7 / num8;
        num = (num < num3) ? num : num3;
        float num9 = (num5 - (num * num6)) / 2f;
        float num10 = (num7 - (num * num8)) / 2f;
        float dx = (this.Points[0].X - (num * this.Points[2].X)) + num9;
        float dy = (this.Points[0].Y - (num * this.Points[2].Y)) + num10;
        Global.M[0] = new Matrix(num, 0f, 0f, num, dx, dy);
        Matrix matrix = new Matrix(1f, 0f, 0f, -1f, 0f, this.Points[1].Y);
        Global.M[0].Multiply(matrix, MatrixOrder.Append);
        Global.M[1] = Global.M[0].Clone();
        Global.M[1].Invert();
    }

    public void CancelPickup()
    {
        foreach (TFigure figure in this.Figures)
        {
            figure.CancelPickup();
        }
        this.selectedFigures.Clear();
    }

    private CaptureResult Capture(PointF curCoo)
    {
        CaptureResult result = Global.Caputue(curCoo, this.newPolyLine.ToArray());
        if (!result.IsCaptured)
        {
            foreach (TFigure figure in this.Figures)
            {
                if (figure.IsContain(curCoo))
                {
                    result = figure.Capture(curCoo);
                    if (result.IsCaptured)
                    {
                        return result;
                    }
                }
            }
        }
        return result;
    }

    private void contextMenuStrip直线_取消_Click(object sender, EventArgs e)
    {
        this.newPolyLine.Clear();
    }

    private void contextMenuStrip直线_确认_Click(object sender, EventArgs e)
    {
        PointF[] array = this.newPolyLine.ToArray();
        Array.Resize<PointF>(ref array, array.Length + 1);
        this.Add(new TLines(Color.Black, array));
        this.Redraw();
        this.newPolyLine.Clear();
    }

    public void DrawCapturedRect()
    {
        if (this.cResult.IsCaptured)
        {
            float num = 5f * Global.M[1].Elements[0];
            this.CapturePen.Width = num / 2.5f;
            this.tGr.DrawRectangle(this.CapturePen, (float) (this.cResult.Coo.X - num), (float) (this.cResult.Coo.Y - num), (float) (2f * num), (float) (2f * num));
        }
    }

    public void DrawMe(Graphics aGr)
    {
        foreach (TFigure figure in this.Figures)
        {
            figure.DrawMe(aGr);
        }
    }

    public void DrawNewPolyLine()
    {
        if (this.newPolyLine.Count > 1)
        {
            this.tGr.DrawLines(this.lcPen, this.newPolyLine.ToArray());
        }
        if (this.newPolyLine.Count > 0)
        {
            this.tGr.DrawLine(this.lcPen, this.curPos[0], this.curPos[1]);
        }
    }

    public void DrawPkRect(Color pkColor)
    {
        this.pkBrush.Color = pkColor;
        this.tGr.FillRectangle(this.pkBrush, this.pkMin.X, this.pkMin.Y, this.pkMax.X - this.pkMin.X, this.pkMax.Y - this.pkMin.Y);
        this.pkPen.Width = Global.M[1].Elements[0];
        this.tGr.DrawRectangle(this.pkPen, this.pkMin.X, this.pkMin.Y, this.pkMax.X - this.pkMin.X, this.pkMax.Y - this.pkMin.Y);
    }

    public void Max_Min(ref PointF Min, ref PointF Max)
    {
        Max = new PointF(float.MinValue, float.MinValue);
        Min = new PointF(float.MaxValue, float.MaxValue);
        foreach (TFigure figure in this.Figures)
        {
            figure.Max_Min(ref Max, ref Min);
        }
    }

    public void Open(string fileName)
    {
        if (fileName.Contains(".dxf"))
        {
            DxfReader dxfReader = new DxfReader();
            if (!this.dxfReader.Open(fileName))
            {
                MessageBox.Show("无法打开文件！");
                return;
            }
            this.dxfReader.Read(this);
        }
        else if (fileName.Contains(".se"))
        {
            IFormatter formatter = new BinaryFormatter();
            Stream serializationStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            this.SeriaDataList = (List<SeriaData>) formatter.Deserialize(serializationStream);
            this.ReadSeriaData();
            serializationStream.Close();
        }
        this.timer2.Enabled = true;
    }

    private bool Pickup(PointF aCursorCoo)
    {
        foreach (TFigure figure in this.Figures)
        {
            if (figure.Pickup(aCursorCoo))
            {
                this.selectedFigures.Add(figure);
                return true;
            }
        }
        return false;
    }

    private void Pickup(Region pkReg)
    {
        foreach (TFigure figure in this.Figures)
        {
            figure.Pickup(pkReg);
        }
    }

    private void pictureBox_MouseClick(object sender, MouseEventArgs e)
    {
        if ((e.Button == MouseButtons.Left) && (this.cTool == Tools.Default))
        {
            if ((this.AutoRefresh && !this.Drawing) && this.Pickup(this.CursorCoo))
            {
                this.Redraw();
            }
            else if (this.Drawing)
            {
                this.Drawing = false;
                if (this.AutoRefresh)
                {
                    Region pkReg = new Region(new RectangleF(this.pkMin.X, this.pkMin.Y, this.pkMax.X - this.pkMin.X, this.pkMax.Y - this.pkMin.Y));
                    this.Pickup(pkReg);
                    this.pkMin = this.pkMax;
                    this.Redraw();
                }
            }
            else
            {
                this.Drawing = true;
                this.curPos[0] = this.curPos[1] = this.cResult.Coo;
            }
        }
    }

    private void pictureBox_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            if ((e.Button == MouseButtons.Right) && (this.cTool == Tools.Line))
            {
                this.contextMenuStrip直线.Show(this.pictureBox, e.X, e.Y);
            }
        }
        else
        {
            switch (this.cTool)
            {
                case Tools.Hand:
                    this.Dragging = true;
                    this.curPos[0] = this.curPos[1] = (PointF) new Point(e.X, e.Y);
                    return;

                case Tools.Translate:
                case Tools.Rotate:
                case Tools.Scale:
                    if (!this.Drawing)
                    {
                        PointF tf2;
                        this.Drawing = true;
                        this.curPos[2] = tf2 = this.cResult.Coo;
                        this.curPos[0] = this.curPos[1] = tf2;
                        return;
                    }
                    this.Drawing = false;
                    if (this.AutoRefresh)
                    {
                        this.Redraw();
                    }
                    return;

                case Tools.Default:
                    return;

                case Tools.Line:
                    this.curPos[0] = this.curPos[1] = this.cResult.Coo;
                    this.newPolyLine.Add(this.curPos[0]);
                    return;

                default:
                    return;
            }
        }
    }

    private void pictureBox_MouseMove(object sender, MouseEventArgs e)
    {
        this.pictureBox.Focus();
    }

    private void pictureBox_MouseUp(object sender, MouseEventArgs e)
    {
        if ((e.Button == MouseButtons.Left) && (this.cTool == Tools.Hand))
        {
            this.Dragging = false;
        }
    }

    private void pictureBox_MouseWheel(object sender, MouseEventArgs e)
    {
        this.Zoom(e.Delta);
    }

    public void ReadSeriaData()
    {
        this.Figures.Clear();
        Type[] types = new Type[] { typeof(SeriaData) };
        object[] parameters = new object[1];
        foreach (SeriaData data in this.seriaDatas)
        {
            parameters[0] = data;
            ConstructorInfo constructor = data.classType.GetConstructor(types);
            this.Figures.Add((TFigure) constructor.Invoke(parameters));
        }
        this.CalculateM();
    }

    public void Redraw()
    {
        this.Gr.Clear(Color.White);
        this.Gr.Transform = Global.M[0];
        if (Global.tM.IsIdentity)
        {
            this.DrawMe(this.Gr);
        }
        else
        {
            this.lcPen.Width = Global.M[1].Elements[0];
            this.Gr.DrawLine(this.lcPen, this.curPos[0], this.curPos[2]);
            this.Transform(Global.tM, this.Gr);
            Global.tM.Reset();
        }
        this.pictureBox.Image = this.Bg;
        Global.M[1] = Global.M[0].Clone();
        Global.M[1].Invert();
        this.Gr.ResetTransform();
    }

    public void Refresh(Color pkColor)
    {
        this.tGr.DrawImage(this.Bg, 0, 0);
        this.tGr.Transform = Global.M[0];
        this.DrawPkRect(pkColor);
        this.DrawCapturedRect();
        this.DrawNewPolyLine();
        this.pictureBox.Image = this.tBg;
        this.tGr.ResetTransform();
    }

    public void RemoveSeletced()
    {
        for (int i = 0; i < this.Figures.Count; i++)
        {
            if (this.Figures[i].IsSelected)
            {
                this.Figures.RemoveAt(i--);
            }
        }
        if (this.AutoRefresh)
        {
            this.Redraw();
        }
    }

    public void Reset()
    {
        this.CancelPickup();
        this.pkMax = this.pkMin;
        this.Dragging = this.Drawing = false;
        if (this.AutoRefresh)
        {
            this.Redraw();
        }
    }

    public void ResetDisplay(Size clientSize, Control value)
    {
        this.pictureBox = new PictureBox();
        this.pictureBox.Dock = DockStyle.Fill;
        this.pictureBox.Cursor = Global.myCursors[1];
        value.Controls.Clear();
        value.Controls.Add(this.pictureBox);
        this.pictureBox.MouseMove += new MouseEventHandler(this.pictureBox_MouseMove);
        this.pictureBox.MouseClick += new MouseEventHandler(this.pictureBox_MouseClick);
        this.pictureBox.MouseUp += new MouseEventHandler(this.pictureBox_MouseUp);
        this.pictureBox.MouseDown += new MouseEventHandler(this.pictureBox_MouseDown);
        this.pictureBox.MouseWheel += new MouseEventHandler(this.pictureBox_MouseWheel);
        this.Points[0] = new PointF(0f, 0f);
        this.Points[1] = new PointF((float) clientSize.Width, (float) clientSize.Height);
        this.Bg = new Bitmap(clientSize.Width, clientSize.Height);
        this.tBg = new Bitmap(this.Bg);
        this.Gr = Graphics.FromImage(this.Bg);
        this.tGr = Graphics.FromImage(this.tBg);
        this.Gr.SmoothingMode = SmoothingMode.HighSpeed;
        this.Gr.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
    }

    public void Save(string fileName)
    {
        IFormatter formatter = new BinaryFormatter();
        Stream serializationStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        formatter.Serialize(serializationStream, this.SeriaDataList);
        serializationStream.Close();
    }

    public bool Selected_any_Figure()
    {
        foreach (TFigure figure in this.Figures)
        {
            if (figure.IsSelected)
            {
                return true;
            }
        }
        return false;
    }

    public void Stop()
    {
        this.timer1.Enabled = false;
        this.timer2.Enabled = false;
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        if (this.Dragging && (this.cTool == Tools.Hand))
        {
            this.curPos[1] = (PointF) this.pictureBox.PointToClient(Control.MousePosition);
            Global.M[0].Translate(this.curPos[1].X - this.curPos[0].X, this.curPos[1].Y - this.curPos[0].Y, MatrixOrder.Append);
            if (this.AutoRefresh)
            {
                this.Redraw();
            }
            this.curPos[0] = this.curPos[1];
        }
        if (this.Drawing)
        {
            this.curPos[2] = this.cResult.Coo;
            switch (this.cTool)
            {
                case Tools.Translate:
                    Global.tM.Translate(this.curPos[2].X - this.curPos[1].X, this.curPos[2].Y - this.curPos[1].Y);
                    if (this.AutoRefresh)
                    {
                        this.Redraw();
                    }
                    this.curPos[1] = this.curPos[2];
                    break;

                case Tools.Rotate:
                {
                    float f = Global.FCta(this.curPos[0], this.curPos[1], this.curPos[2]);
                    if (float.IsNaN(f))
                    {
                        f = 0f;
                    }
                    Global.tM.RotateAt(f, this.curPos[0]);
                    if (this.AutoRefresh)
                    {
                        this.Redraw();
                    }
                    this.curPos[1] = this.curPos[2];
                    break;
                }
                case Tools.Scale:
                {
                    double num3 = Math.Sqrt(Math.Pow((double) (this.curPos[0].X - this.curPos[1].X), 2.0) + Math.Pow((double) (this.curPos[0].Y - this.curPos[1].Y), 2.0));
                    double num4 = Math.Sqrt(Math.Pow((double) (this.curPos[0].X - this.curPos[2].X), 2.0) + Math.Pow((double) (this.curPos[0].Y - this.curPos[2].Y), 2.0));
                    float scaleX = 1f;
                    if ((num3 > 5.0) && (num4 > 5.0))
                    {
                        if (this.curPos[2].Y <= this.curPos[0].Y)
                        {
                            scaleX = (float) (num3 / num4);
                        }
                        else
                        {
                            scaleX = (float) (num4 / num3);
                        }
                    }
                    Global.tM.Translate(this.curPos[0].X, this.curPos[0].Y);
                    Global.tM.Scale(scaleX, scaleX);
                    Global.tM.Translate(-this.curPos[0].X, -this.curPos[0].Y);
                    if (this.AutoRefresh)
                    {
                        this.Redraw();
                    }
                    this.curPos[1] = this.curPos[2];
                    break;
                }
                case Tools.Default:
                    this.pkMin = this.curPos[0];
                    this.pkMax = this.curPos[1] = this.cResult.Coo;
                    for (int i = 0; i < 2; i++)
                    {
                        if (this.pkMax.X < this.curPos[i].X)
                        {
                            this.pkMax.X = this.curPos[i].X;
                        }
                        if (this.pkMax.Y < this.curPos[i].Y)
                        {
                            this.pkMax.Y = this.curPos[i].Y;
                        }
                        if (this.pkMin.X > this.curPos[i].X)
                        {
                            this.pkMin.X = this.curPos[i].X;
                        }
                        if (this.pkMin.Y > this.curPos[i].Y)
                        {
                            this.pkMin.Y = this.curPos[i].Y;
                        }
                    }
                    break;
            }
        }
        if (this.cTool == Tools.Line)
        {
            this.curPos[1] = this.cResult.Coo;
        }
        if (this.AutoRefresh && (this.cTool != Tools.Hand))
        {
            this.Refresh(Color.FromArgb(100, Color.DarkBlue));
        }
    }

    private void timer2_Tick(object sender, EventArgs e)
    {
        this.cResult = this.Capture(this.CursorCoo);
    }

    public void Transform(Matrix T, Graphics aGr)
    {
        foreach (TFigure figure in this.Figures)
        {
            if (figure.IsSelected)
            {
                figure.Transform(T, aGr);
            }
            else
            {
                figure.DrawMe(aGr);
            }
        }
    }

    public void Zoom(int Delta)
    {
        if (Delta > 0)
        {
            this.r = 1.25f;
            this.Scale *= this.r;
        }
        else if (Delta < 0)
        {
            this.r = 0.8f;
            this.Scale *= this.r;
        }
        PointF cursorCoo = this.CursorCoo;
        Global.M[0].Translate(cursorCoo.X, cursorCoo.Y, MatrixOrder.Prepend);
        Global.M[0].Scale(this.r, this.r, MatrixOrder.Prepend);
        Global.M[0].Translate(-cursorCoo.X, -cursorCoo.Y, MatrixOrder.Prepend);
        if (this.AutoRefresh)
        {
            this.Redraw();
        }
    }

    public void 平移()
    {
        this.cTool = Tools.Translate;
        this.pictureBox.Cursor = Global.myCursors[2];
    }

    public void 实时平移()
    {
        this.cTool = Tools.Hand;
        this.pictureBox.Cursor = Global.myCursors[1];
    }

    public void 缩放()
    {
        this.cTool = Tools.Scale;
        this.pictureBox.Cursor = Global.myCursors[2];
    }

    public void 旋转()
    {
        this.cTool = Tools.Rotate;
        this.pictureBox.Cursor = Global.myCursors[2];
    }

    public void 选择()
    {
        this.cTool = Tools.Default;
        this.pictureBox.Cursor = Global.myCursors[0];
    }

    public void 直线()
    {
        this.cTool = Tools.Line;
        this.pictureBox.Cursor = Global.myCursors[2];
    }

    // Properties
    public bool CanSave
    {
        get
        {
            return (this.Figures.Count > 0);
        }
    }

    public Tools cTool
    {
        get
        {
            return this.mTool;
        }
        set
        {
            this.mTool = value;
            this.pkMax = this.pkMin;
            this.Dragging = this.Drawing = false;
        }
    }

    public PointF CursorCoo
    {
        get
        {
            PointF[] pts = new PointF[] { this.pictureBox.PointToClient(Control.MousePosition) };
            Global.M[1].TransformPoints(pts);
            return pts[0];
        }
    }

    public List<SeriaData> SeriaDataList
    {
        get
        {
            this.seriaDatas.Clear();
            foreach (TFigure figure in this.Figures)
            {
                this.seriaDatas.Add(figure.GetSeriaData());
            }
            return this.seriaDatas;
        }
        set
        {
            this.seriaDatas = value;
        }
    }
}


}
