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
    public abstract class TFigure:IDisposable
    {
        // Fields
        //2D工厂实例
        protected Factory factory;
        //唯一标识ID
        public int id;
        //图形类型包括Line,Arc
        public string FigureType;

        //原始路径 （必须）
        protected PathGeometry orginGeomtery;
        //原始中心
        private Vector2 orignalCenter;
        public List<Vector2> mPoints;//所有节点，最新的

        public string layer;//属于仿真层还是基础设施层
        private Color figColor;//图形颜色属性
        public bool IsSelected;//是否选中     
        private bool visible = true;//默认可见属性
        private bool ifFilled = false;//默认不填充属性
        private float figureHeight = 0f;//属性

        //变形后结果
        protected TransformedGeometry transformGeo;//都是基于orginGeomtery的累积变换
        //变换矩阵
        private Matrix3x2 matrix=Matrix .Identity;
        //偏移位置
        private Vector2 offset = new Vector2();
        //旋转角度
        private float rotate = 0f;
        private float lastrotate = 0f;//记录之前旋转过的角度之和
        //缩放比例
        private float scale = 1f;
        
        // Methods
        public TFigure(List<Vector2> aPoints, Color _color, Factory _factory)          
        {
            id = this.GetHashCode();
            IsSelected = false;
            factory = _factory;
            mPoints = new List<Vector2>();
            for (int i = 0; i < aPoints.Count - 1;i++ )
                mPoints .Add(aPoints[i]);
            orignalCenter = new Vector2(0, 0);
            for (int i = 0; i < mPoints.Count; i++)
            {
                orignalCenter.X += mPoints[i].X;
                orignalCenter.Y += mPoints[i].Y;
            }
            float pointNo = (float)mPoints.Count;
            if (mPoints[0] == mPoints[mPoints.Count - 1])
                pointNo -= 1;
            orignalCenter.X = orignalCenter.X / pointNo;
            orignalCenter.Y = orignalCenter.Y / pointNo;          
            orginGeomtery = new PathGeometry(factory);
        }
      
        public bool PickUp(RectangleGeometry _Ref)//判断是否框选中,
        {
            IsSelected = false ;
            GeometryRelation  GetResult = curGeometry.Compare(_Ref);           
            if (GetResult != GeometryRelation.Disjoint && GetResult != GeometryRelation.Unknown)
            {
                IsSelected = true;
                return true;
            }               
            return false;
        }
        public bool PickUp(Vector2 _location)//判断是否点选中
        {
            IsSelected = false;
            if (curGeometry.FillContainsPoint(_location))
            {
                IsSelected = true;
                return true;
            }
            return false;
        }       

       public float Scale//放大
       {
            get { return scale; }
            set
            {
                float rate = value / this.scale;
                scale = value;
                SetOffset(this.offset.X * rate, this.offset.Y * rate);
                for (int i = 0; i < mPoints.Count - 1; i++)
                    mPoints[i] = mPoints[i] * scale;
                CalculateMatrix();
                Transform();
            }
        }
        public float Rotate//旋转
        {
            get { return this.rotate; }
            set
            {
                rotate = value;
                lastrotate+=rotate;
                for (int i = 0; i < mPoints.Count - 1; i++)
                    mPoints[i] = Matrix3x2.TransformPoint(Matrix3x2.Rotation((float)(rotate * Math.PI / 180)),mPoints [i]);
                CalculateMatrix();               
                Transform();
            }
        }
        public Vector2 Offset  //平移
        {
            get { return this.offset; }
            set
            {
                SetOffset(value.X, value.Y);
                for (int i = 0; i < mPoints.Count - 1; i++)
                    mPoints[i] = mPoints[i] +offset;
                Transform();
            }
        }
        public void Transform()//变换，包括移动
        {
            if (this.transformGeo != null)
            {
                this.transformGeo.Dispose();
            }
            if(matrix .IsIdentity ==false )
                this.transformGeo = new TransformedGeometry(factory, this.orginGeomtery, this.matrix);
        }
        public abstract TFigure Clone();//clone the Figure
     
        private void SetOffset(float x, float y)
        {
            matrix.M31 += x - offset.X;
            matrix.M32 += y - offset.Y;
            offset.X = x;
            offset.Y = y;
        }
        private void CalculateMatrix()
        {
            matrix = new Matrix3x2(1, 0, 0, 1, -orignalCenter.X, -orignalCenter.Y);
            if (rotate != 0)
            {
                float rotateRadian = (float)(lastrotate * Math.PI / 180);
                matrix *= Matrix3x2.Rotation(rotateRadian);             
            }
            if (this.scale != 1)
            {
                matrix *= Matrix3x2.Scaling(scale);               
            }
            matrix *= Matrix3x2.Translation(offset.X + scale * orignalCenter.X, offset.Y + scale * orignalCenter.Y);            
        }
       
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }
        public Color FigColor
        {
            get { return this.figColor; }
            set { this.figColor = value; }
        }
        public RawRectangleF GetBound
        {
            get
            {
                return curGeometry.GetBounds();               
            }
        }
        public virtual int HandleCount//点数
        {
            get
            {
                return mPoints.Count;
            }
        }
        public float FigureHeight
        {
            get { return this.figureHeight; }
            set { this.figureHeight = value; }
        }
        public bool Filled
        { 
            get { return ifFilled;}
            set { ifFilled = value; }
        }
        public Vector2 FigureCenter
        {
            get {
                Vector2 _center = new Vector2(0, 0);
                for (int i=0;i<mPoints .Count ;i++)
                {
                    _center.X +=mPoints [i].X ;
                    _center .Y +=mPoints [i].Y;
                }
                float pointNo=(float )mPoints.Count;
                if (mPoints[0] == mPoints[mPoints.Count-1])
                    pointNo-=1;
                _center.X = _center.X / pointNo;
                _center.Y = _center.Y / pointNo;
                return _center;
            }          
        }
        public Geometry curGeometry
        {
            
            get 
            {
                if (transformGeo != null)
                    return transformGeo;
                else
                    return orginGeomtery; 
            }
        }
        
        #region IDisposable 成员
        /// <summary>
        /// 回收当前对象使用的资源。
        /// </summary>
        public void Dispose()
        {
            this.orginGeomtery .Dispose();
            if (this.transformGeo != null)
            {
                this.transformGeo.Dispose();
            }
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
