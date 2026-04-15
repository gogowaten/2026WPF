using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260413
{
    /// <summary>
    /// Thumb自体をリサイズした時TemplateのCanvasはどうなる？
    /// まず、Thumbにサイズを指定する必要がある。もし無指定の場合は
    /// ハンドルを移動させてもサイズはNaNから変化しないし、これはreSizeAdornerの仕様にしている
    /// で、サイズ指定してからハンドル移動するとリサイズされる
    /// けど、中のCanvasはリサイズされないので、中央揃えの表示になる
    /// </summary>
    public class CanvasThumb2 : Thumb
    {
        static CanvasThumb2()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasThumb2), new FrameworkPropertyMetadata(typeof(CanvasThumb2)));
        }
        public CanvasThumb2()
        {

        }
    }






    public class CanvasThumb : Thumb
    {

        public Canvas MyTemplateCanvas
        {
            get { return (Canvas)GetValue(MyTemplateCanvasProperty); }
            set { SetValue(MyTemplateCanvasProperty, value); }
        }
        public static readonly DependencyProperty MyTemplateCanvasProperty =
            DependencyProperty.Register(nameof(MyTemplateCanvas), typeof(Canvas), typeof(CanvasThumb), new PropertyMetadata(null));

        static CanvasThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasThumb), new FrameworkPropertyMetadata(typeof(CanvasThumb)));
        }

        public CanvasThumb()
        {

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_Panel") is Canvas panel)
            {
                MyTemplateCanvas = panel;
            }
        }

        public void AddResizeAdorner2()
        {
            ResizeAdorner? ado = ResizeAdorner.AddResizeAdorner2(MyTemplateCanvas);
            ado?.LeftLocateChanged += Ado_LeftLocateChanged;


        }

        private void Ado_LeftLocateChanged(object? sender, double e)
        {
            Canvas.SetLeft(MyTemplateCanvas, Canvas.GetLeft(MyTemplateCanvas) + e);
        }

        public void AddResizeAdorner()
        {
            ResizeAdorner.AddResizeAdorner(MyTemplateCanvas);

        }

        public void RemoveResizeAdorner() => ResizeAdorner.RemoveResizeAdorner(MyTemplateCanvas);
    }
}
