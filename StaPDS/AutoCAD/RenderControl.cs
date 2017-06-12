using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoCAD.Common;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace AutoCAD
{
    public partial class RenderControl : UserControl,IDisposable 
    {
        public  WindowRenderTarget target;     
        SharpDX.Direct2D1.Factory factory = new SharpDX.Direct2D1.Factory();
        public SharpDX.Direct2D1.SolidColorBrush rBrush;//绘图用
        public SharpDX.Direct2D1.SolidColorBrush pBrush;//选中用
        public  List<TFigure> Figures = new List<TFigure>();
        
        public RenderControl()
        {
            InitializeComponent();
        }

        private void RenderControl_Load(object sender, EventArgs e)
        {
            ResetRenderTarget();
        }
        public void ResetRenderTarget()
        {
            factory = new SharpDX.Direct2D1.Factory();
            RenderTargetProperties renderProp = new RenderTargetProperties()
            {
                DpiX = 0,
                DpiY = 0,
                MinLevel = FeatureLevel.Level_10,
                PixelFormat = new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
                Type = RenderTargetType.Hardware,
                Usage = RenderTargetUsage.None
            };

            //set hwnd target properties (permit to attach Direct2D to window)
            HwndRenderTargetProperties winProp = new HwndRenderTargetProperties()
            {
                Hwnd = this.Handle,
                PixelSize = new Size2(this.ClientSize.Width, this.ClientSize.Height),
                PresentOptions = PresentOptions.Immediately
            };
            //target creation
            target = new WindowRenderTarget(factory, renderProp, winProp);    
          
        }

        private void RenderControl_Paint(object sender, PaintEventArgs e)
        {
            Renders();
        }
        //重绘
        private void Renders()
        {
            target.BeginDraw();
            target.Clear(Global.ToColor4(this.BackColor));
            for (int i = 0; i < Figures.Count;i++ )
            {
               
           
            }
            target.EndDraw();
        }
        //清除内存
        public void Dispose()
        {
            target.Dispose();
            factory.Dispose();
            rBrush.Dispose();
            pBrush.Dispose();
        }
        //重绘
        private void RenderControl_SizeChanged(object sender, EventArgs e)
        {
            if (target != null)
                target.Resize(new Size2(this.ClientSize.Width, this.ClientSize.Height));
        }
        
     
    }
}
