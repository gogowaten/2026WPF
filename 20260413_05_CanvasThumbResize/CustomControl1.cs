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

namespace _20260413_05_CanvasThumbResize
{
    public class CanvasThumb : Thumb
    {
        private Canvas MyTemplateCanvas = null!;
        public ResizeAdorner MyResizeAdorner { get; set; }

        static CanvasThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasThumb), new FrameworkPropertyMetadata(typeof(CanvasThumb)));
        }
        public CanvasThumb()
        {
            MyResizeAdorner = new ResizeAdorner(this);
            Loaded += (s, e) => { InitResizeAdorner(); };
            DragDelta += CanvasThumb_DragDelta;
            InitResizeAdorner();
        }



        private void InitResizeAdorner()
        {
            if (AdornerLayer.GetAdornerLayer(this) is AdornerLayer layer)
            {
                layer.Add(MyResizeAdorner);
                MyResizeAdorner.Visibility = Visibility.Collapsed;

                MyResizeAdorner.LeftLocateChanged += CanvasThumb_LeftLocateChanged;
                MyResizeAdorner.TopLocateChanged += CanvasThumb_TopLocateChanged;

                MyResizeAdorner.SetBinding(ResizeAdorner.ResizeHandleSizeProperty, new Binding() { Source = this, Path = new PropertyPath(ResizeHandleSizeProperty) });
            }
        }

        //private void InitResizeAdorner()
        //{
        //    if (AdornerLayer.GetAdornerLayer(this) is AdornerLayer layer)
        //    {
        //        //MyResizeAdorner.SetBinding(ResizeAdorner.ResizeHandleSizeProperty, new Binding() { Source = this, Path = new PropertyPath(ResizeHandleSize) });
        //        layer.Add(MyResizeAdorner);
        //        Binding b = new();
        //        b.Source = this;
        //        b.Path = new PropertyPath(ResizeHandleSizeProperty);
        //        b.Mode = BindingMode.TwoWay;
        //        BindingOperations.SetBinding(MyResizeAdorner, ResizeAdorner.ResizeHandleSizeProperty, b);

        //        MyResizeAdorner.LeftLocateChanged += CanvasThumb_LeftLocateChanged;
        //        MyResizeAdorner.TopLocateChanged += CanvasThumb_TopLocateChanged;
        //    }
        //}


        #region プロパティ

        public double ResizeHandleSize
        {
            get { return (double)GetValue(ResizeHandleSizeProperty); }
            set { SetValue(ResizeHandleSizeProperty, value); }
        }
        public static readonly DependencyProperty ResizeHandleSizeProperty =
            DependencyProperty.Register(nameof(ResizeHandleSize), typeof(double), typeof(CanvasThumb), new PropertyMetadata(20.0));

        #endregion プロパティ


        public void HiddenResizeHndle()
        {
            MyResizeAdorner.Visibility = Visibility.Collapsed;
        }

        public void VisibleResizeHandle()
        {
            MyResizeAdorner.Visibility = Visibility.Visible;
        }

        private void CanvasThumb_TopLocateChanged(object? sender, double e)
        {
            foreach (var item in MyTemplateCanvas.Children.OfType<UIElement>())
            {
                Canvas.SetTop(item, Canvas.GetTop(item) - e);
            }
        }

        private void CanvasThumb_LeftLocateChanged(object? sender, double e)
        {
            IEnumerable<UIElement> items = MyTemplateCanvas.Children.OfType<UIElement>();
            foreach (UIElement item in items)
            {
                Canvas.SetLeft(item, Canvas.GetLeft(item) - e);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_Canvas") is Canvas canvas)
            {
                MyTemplateCanvas = canvas;
            }
            else { throw new Exception(); }
        }

        private void CanvasThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
        }
    }


public class CustomControl1 : Control
    {
        static CustomControl1()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
        }
    }
}
