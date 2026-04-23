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



    public class FlatHandle : Thumb
    {
        static FlatHandle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlatHandle), new FrameworkPropertyMetadata(typeof(FlatHandle)));
        }
        public FlatHandle()
        {

        }


        public double MyLeft
        {
            get { return (double)GetValue(MyLeftProperty); }
            set { SetValue(MyLeftProperty, value); }
        }
        public static readonly DependencyProperty MyLeftProperty =
            DependencyProperty.Register(nameof(MyLeft), typeof(double), typeof(FlatHandle), new PropertyMetadata(0.0));

        public double MyTop
        {
            get { return (double)GetValue(MyTopProperty); }
            set { SetValue(MyTopProperty, value); }
        }
        public static readonly DependencyProperty MyTopProperty =
            DependencyProperty.Register(nameof(MyTop), typeof(double), typeof(FlatHandle), new PropertyMetadata(0.0));



    }



    public class CanvasThumb : Thumb
    {
        private Canvas MyTemplateCanvas = null!;
        public ResizeAdorner MyResizeAdorner { get; set; }
        private UIElement MyInternalUIElement = null!;

        #region コンストラクタ

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
            if (AdornerLayer.GetAdornerLayer(this) is AdornerLayer layer)
            {
                layer.Add(MyResizeAdorner);
                MyResizeAdorner.Visibility = Visibility.Collapsed;

                MyResizeAdorner.LeftLocateChanged += ResizeHandle_LeftLocateChanged;
                MyResizeAdorner.TopLocateChanged += ResizeHandle_TopLocateChanged;

                MyResizeAdorner.SetBinding(ResizeAdorner.ResizeHandleSizeProperty,
                    new Binding() { Source = this, Path = new PropertyPath(ResizeHandleSizeProperty) });
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_Canvas") is Canvas canvas)
            {
                MyTemplateCanvas = canvas;
                if (MyTemplateCanvas.Children[0] is UIElement element)
                {
                    MyInternalUIElement = MyTemplateCanvas.Children[0];
                }
                else
                {
                    throw new InvalidOperationException("中の要素が見つからない");
                }
            }
            else
            {
                throw new InvalidOperationException("TemplateのCanvasが見つからない");
            }
        }
        #endregion コンストラクタ

        #region プロパティ



        public double ResizeHandleSize
        {
            get { return (double)GetValue(ResizeHandleSizeProperty); }
            set { SetValue(ResizeHandleSizeProperty, value); }
        }
        public static readonly DependencyProperty ResizeHandleSizeProperty =
            DependencyProperty.Register(nameof(ResizeHandleSize), typeof(double),
                typeof(CanvasThumb), new PropertyMetadata(12.0));
        #endregion プロパティ

        #region パブリックメソッド

        public void UpdateVertexHandle()
        {
            if (MyInternalUIElement is GeoThumb gt)
            {
                gt.UpdateVertexHandles();
            }
        }

        public void ChangeResizeHandleVisible()
        {
            if (MyResizeAdorner.Visibility == Visibility.Visible)
            {
                MyResizeAdorner.Visibility = Visibility.Collapsed;
            }
            else
            {
                MyResizeAdorner.Visibility = Visibility.Visible;
            }
        }

        public void HiddenResizeHndle()
        {
            MyResizeAdorner.Visibility = Visibility.Collapsed;
        }

        public void VisibleResizeHandle()
        {
            MyResizeAdorner.Visibility = Visibility.Visible;
        }

        #endregion パブリックメソッド

        #region プライベートメソッド

        // リサイズハンドルの移動でCanvasの座標が変更される時には、
        // 中の要素をその場に留めるために反対方向に移動させる
        private void ResizeHandle_TopLocateChanged(object? sender, double e)
        {
            Canvas.SetTop(MyInternalUIElement, Canvas.GetTop(MyInternalUIElement) - e);
        }

        private void ResizeHandle_LeftLocateChanged(object? sender, double e)
        {

            Canvas.SetLeft(MyInternalUIElement, Canvas.GetLeft(MyInternalUIElement) - e);
        }



        private void CanvasThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
        }
        #endregion プライベートメソッド
    }




    public class GeoThumb : Thumb
    {
        private VertexAdorner? _vertexAdorner; // 頂点移動用ハンドル

        #region コンストラクタ

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
        #endregion コンストラクタ

        #region プロパティ

        // 頂点ハンドルの表示切り替え
        public bool MyVisibleVertexHandle
        {
            get { return (bool)GetValue(MyVisibleVertexHandleProperty); }
            set { SetValue(MyVisibleVertexHandleProperty, value); }
        }
        public static readonly DependencyProperty MyVisibleVertexHandleProperty =
            DependencyProperty.Register(nameof(MyVisibleVertexHandle), typeof(bool), typeof(GeoThumb), new PropertyMetadata(false, OnMyVisibleVertexHandle));

        private static void OnMyVisibleVertexHandle(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GeoThumb thumb)
            {
                if ((bool)e.NewValue)
                {
                    thumb.ShowVertexHandle();
                }
                else
                {
                    thumb.HideVertexHandle();
                }
            }
        }

        // 頂点ハンドルサイズ
        public double MyShapeVertexHandleSize
        {
            get { return (double)GetValue(MyShapeVertexHandleSizeProperty); }
            set { SetValue(MyShapeVertexHandleSizeProperty, value); }
        }
        public static readonly DependencyProperty MyShapeVertexHandleSizeProperty =
            DependencyProperty.Register(nameof(MyShapeVertexHandleSize), typeof(double), typeof(GeoThumb), new PropertyMetadata(12.0));

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


        public void UpdateVertexHandles()
        {
            _vertexAdorner?.UpdateHandles();
        }

        public void ShowVertexHandle()
        {
            if (AdornerLayer.GetAdornerLayer(MyGeoLine) is AdornerLayer layer)
            {
                _vertexAdorner = new VertexAdorner(MyGeoLine);
                _vertexAdorner.SetBinding(VertexAdorner.MyHandleSizeProperty, new Binding() { Source = this, Path = new PropertyPath(MyShapeVertexHandleSizeProperty) });
                layer.Add(_vertexAdorner);
            }
        }

        public void HideVertexHandle()
        {
            if (AdornerLayer.GetAdornerLayer(MyGeoLine) is AdornerLayer layer && _vertexAdorner is not null)
            {
                layer.Remove(_vertexAdorner);
                _vertexAdorner = null;
            }
        }

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
            }
        }

    }



}
