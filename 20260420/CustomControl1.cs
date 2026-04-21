using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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

namespace _20260420
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
            MyResizeAdorner = new(this);
            Loaded += (s, e) => { InitResizeAdorner(); };
            DragDelta += CanvasThumb_DragDelta;

        }


        private void InitResizeAdorner()
        {
            if(AdornerLayer.GetAdornerLayer(this) is AdornerLayer layer)
            {
                layer.Add(MyResizeAdorner);
                MyResizeAdorner.Visibility = Visibility.Collapsed;

                MyResizeAdorner.LeftLocateChanged += CanvasThumb_LeftLocateChanged;
                MyResizeAdorner.TopLocateChanged += CanvasThumb_TopLocateChanged;

                MyResizeAdorner.SetBinding(ResizeAdorner.ResizeHandleSizeProperty,
                    new Binding() { Source = this ,Path = new PropertyPath(ResizeHandeleSizeProperty)});
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_Canvas") is Canvas canvas)
            {
                MyTemplateCanvas = canvas;
            }
            else
            {
                throw new InvalidOperationException("TemplateのCanvasが見つからない");
            }
        }

        public double ResizeHandeleSize
        {
            get { return (double)GetValue(ResizeHandeleSizeProperty); }
            set { SetValue(ResizeHandeleSizeProperty, value); }
        }
        public static readonly DependencyProperty ResizeHandeleSizeProperty =
            DependencyProperty.Register(nameof(ResizeHandeleSize), typeof(double),
                typeof(CanvasThumb), new PropertyMetadata(12.0));

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

      

        private void CanvasThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
        }
    }




    public class GeoThumb : Thumb
    {


        static GeoThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GeoThumb), new FrameworkPropertyMetadata(typeof(GeoThumb)));
        }

        public GeoThumb()
        {
            Loaded += GeoThumb_Loaded;
            DragDelta += GeoThumb_DragDelta;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_GeoLine") is GeoLine geo)
            {
                MyGeoLine = geo;
            }
            else
            {
                throw new InvalidOperationException("GeoLineが見つからん");
            }
        }

        #region プロパティ

        public GeoLine MyGeoLine
        {
            get { return (GeoLine)GetValue(MyGeoLineProperty); }
            set { SetValue(MyGeoLineProperty, value); }
        }
        public static readonly DependencyProperty MyGeoLineProperty =
            DependencyProperty.Register(nameof(MyGeoLine), typeof(GeoLine), typeof(GeoThumb), new PropertyMetadata(null));
        public GeoLineData MyData
        {
            get { return (GeoLineData)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty = DependencyProperty.Register(
                nameof(MyData), typeof(GeoLineData), typeof(GeoThumb), new PropertyMetadata(null));

     
        #endregion プロパティ


        private void GeoThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (MyData is not null)
            {
                MyData.InternalX += e.HorizontalChange;
                MyData.InternalY += e.VerticalChange;
                e.Handled = true;
            }
        }

        private void GeoThumb_Loaded(object sender, RoutedEventArgs e)
        {

            if (DataContext is GeoLineData data)
            {
                MyData = data;
                //this.Measure(MyData.GeometryBounds.Size);
                //MyData.RefreshWidthHeight();
                //MyData.UpdateGeometrySize();
                //InvalidateMeasure(); // 効果なし
                //InvalidateVisual();
            }
            //MyData.UpdatePen();
        }
        private void Test()
        {
            Debug.WriteLine("Test");
        }
    }



}
