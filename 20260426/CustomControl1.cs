using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260426
{
    public class CustomCanvasThumb : CustomThumb
    {
        private Canvas MyTemplateCanvas = null!;
        public ResizeAdorner MyResizeAdorner { get; set; } = null!;
        private UIElement MyInternalElement = null!;

        static CustomCanvasThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomCanvasThumb), new FrameworkPropertyMetadata(typeof(CustomCanvasThumb)));
        }
        public CustomCanvasThumb()
        {
            MyResizeAdorner = new ResizeAdorner(this);
            PreviewMouseLeftButtonDown += CustomCanvasThumb_PreviewMouseLeftButtonDown;
            Loaded += CustomCanvasThumb_Loaded;

        }

        private void CustomCanvasThumb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyData?.RootData?.CurrentItemData = MyData;
        }

        private void CustomCanvasThumb_Loaded(object sender, RoutedEventArgs e)
        {
            InitResizeAdorner();
            InitContextMenu();
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_Canvas") is Canvas canvas)
            {
                MyTemplateCanvas = canvas;
                if (MyTemplateCanvas.Children[0] is UIElement element)
                {
                    MyInternalElement = element;

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

        private void InitContextMenu()
        {
            ContextMenu context = new();
            MenuItem item = new() { Header = "perfectly" };
            item.Click += (o, e) => { PerfectlyFit(); };
            context.Items.Add(item);
            this.ContextMenu = context;
        }

        private void InitResizeAdorner()
        {
            if (AdornerLayer.GetAdornerLayer(this) is AdornerLayer layer)
            {
                layer.Add(MyResizeAdorner);
                //MyResizeAdorner.Visibility = Visibility.Collapsed;

                MyResizeAdorner.LeftLocateChanged += ResizeHandle_LeftLocateChanged;
                MyResizeAdorner.TopLocateChanged += ResizeHandle_TopLocateChanged;

                MyResizeAdorner.SetBinding(ResizeAdorner.ResizeHandleSizeProperty,
                    new Binding() { Source = this, Path = new PropertyPath(ResizeHandleSizeProperty) });
            }
        }


        #region プロパティ


        public double ResizeHandleSize
        {
            get { return (double)GetValue(ResizeHandleSizeProperty); }
            set { SetValue(ResizeHandleSizeProperty, value); }
        }
        public static readonly DependencyProperty ResizeHandleSizeProperty =
            DependencyProperty.Register(nameof(ResizeHandleSize), typeof(double),
                typeof(CustomCanvasThumb), new PropertyMetadata(12.0));
        #endregion プロパティ


        #region パブリックメソッド

        // ぴったりサイズ
        public void PerfectlyFit()
        {
            if (MyInternalElement is GeoThumb gt)
            {
                var bounds = gt.MyGeoLine.GetRenderBounds();
                MyData.Width = bounds.Width;
                MyData.Height = bounds.Height;
                // 位置合わせは保留
            }
        }

        // 図形の頂点ハンドルを更新
        //public void UpdateVertexHandle()
        //{
        //    if (MyInternalUIElement is GeoThumb gt)
        //    {
        //        gt.UpdateVertexHandles();
        //    }
        //}

        //public void ChangeResizeHandleVisible()
        //{
        //    if (MyResizeAdorner.Visibility == Visibility.Visible)
        //    {
        //        MyResizeAdorner.Visibility = Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        MyResizeAdorner.Visibility = Visibility.Visible;
        //    }
        //}

        //public void HiddenResizeHndle()
        //{
        //    MyResizeAdorner.Visibility = Visibility.Collapsed;
        //}

        //public void VisibleResizeHandle()
        //{
        //    MyResizeAdorner.Visibility = Visibility.Visible;
        //}

        #endregion パブリックメソッド

        #region プライベートメソッド

        // リサイズハンドルの移動でCanvasの座標が変更される時には、
        // 中の要素をその場に留めるために反対方向に移動させる
        private void ResizeHandle_LeftLocateChanged(object? sender, double e)
        {
            //MyData.X += e;
            Canvas.SetLeft(MyInternalElement, Canvas.GetLeft(MyInternalElement) - e);
        }

        private void ResizeHandle_TopLocateChanged(object? sender, double e)
        {
            Canvas.SetTop(MyInternalElement, Canvas.GetTop(MyInternalElement) - e);
        }

        #endregion プライベートメソッド
    }




    [ContentProperty(nameof(MyContentElement))]
    public class CustomThumbEx : Thumb
    {
        public ResizeAdorner MyResizeAdorner { get; set; }

        static CustomThumbEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumbEx), new FrameworkPropertyMetadata(typeof(CustomThumbEx)));
        }
        public CustomThumbEx()
        {
            MyResizeAdorner = new(this);
            Loaded += CustomThumbEx_Loaded;
            DragDelta += CustomThumbEx_DragDelta;
            PreviewMouseLeftButtonDown += CustomThumbEx_PreviewMouseLeftButtonDown;
        }

        private void CustomThumbEx_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyData?.RootData?.CurrentItemData = MyData;
        }

        private void CustomThumbEx_DragDelta(object sender, DragDeltaEventArgs e)
        {
            MyData.X += e.HorizontalChange;
            MyData.Y += e.VerticalChange;
            e.Handled = true;
        }

        private void CustomThumbEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitResizeAdorner();
            InitContextMenu();
            PerfectlyFit();
        }

        private void InitContextMenu()
        {
            ContextMenu menu = new();
            MenuItem item = new() { Header = "menu", Name = "nenu" };
            item.Click += (s, e) => { PerfectlyFit(); };
            menu.Items.Add(item);
            this.ContextMenu = menu;
        }

        // リサイズハンドルの初期化
        private void InitResizeAdorner()
        {
            if (AdornerLayer.GetAdornerLayer(this) is AdornerLayer layer)
            {
                layer.Add(MyResizeAdorner);
                //MyResizeAdorner.Visibility = Visibility.Collapsed;

                // リサイズ時、子要素の逆移動
                MyResizeAdorner.LeftLocateChanged += ResizeHandle_LeftLocateChanged;
                MyResizeAdorner.TopLocateChanged += ResizeHandle_TopLocateChanged;

                MyResizeAdorner.SetBinding(ResizeAdorner.ResizeHandleSizeProperty,
                    new Binding() { Source = this, Path = new PropertyPath(ResizeHandleSizeProperty) });
            }
        }

        #region 依存関係プロパティ

        public Data MyData
        {
            get { return (Data)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(Data), typeof(CustomThumbEx), new PropertyMetadata(null));

        public FrameworkElement MyContentElement
        {
            get { return (FrameworkElement)GetValue(MyContentElementProperty); }
            set { SetValue(MyContentElementProperty, value); }
        }
        public static readonly DependencyProperty MyContentElementProperty =
            DependencyProperty.Register(nameof(MyContentElement), typeof(FrameworkElement), typeof(FrameworkElement), new PropertyMetadata(null));

        public double ResizeHandleSize
        {
            get { return (double)GetValue(ResizeHandleSizeProperty); }
            set { SetValue(ResizeHandleSizeProperty, value); }
        }
        public static readonly DependencyProperty ResizeHandleSizeProperty =
            DependencyProperty.Register(nameof(ResizeHandleSize), typeof(double),
                typeof(ResizeCanvas), new PropertyMetadata(20.0));
        #endregion 依存関係プロパティ


        #region パブリックメソッド


        // ぴったりサイズ
        public void PerfectlyFit()
        {
            if (MyContentElement is GeoThumb gt)
            {
                var bounds = gt.MyGeoLine.GetRenderBounds();
                MyData.Width = bounds.Width;
                MyData.Height = bounds.Height;
                // 位置合わせは保留
            }
            else
            {

            }
        }

        // 図形の頂点ハンドルを更新
        //public void UpdateVertexHandle()
        //{
        //    if (MyInternalUIElement is GeoThumb gt)
        //    {
        //        gt.UpdateVertexHandles();
        //    }
        //}

        //public void ChangeResizeHandleVisible()
        //{
        //    if (MyResizeAdorner.Visibility == Visibility.Visible)
        //    {
        //        MyResizeAdorner.Visibility = Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        MyResizeAdorner.Visibility = Visibility.Visible;
        //    }
        //}
        #endregion パブリックメソッド

        #region プライベートメソッド
        // リサイズハンドルの移動でCanvasの座標が変更される時には、
        // 中の要素をその場に留めるために反対方向に移動させる
        private void ResizeHandle_TopLocateChanged(object? sender, double e)
        {
            Canvas.SetTop(MyContentElement, Canvas.GetTop(MyContentElement) - e);
        }

        private void ResizeHandle_LeftLocateChanged(object? sender, double e)
        {

            Canvas.SetLeft(MyContentElement, Canvas.GetLeft(MyContentElement) - e);
        }



        //private void CanvasThumb_DragDelta(object sender, DragDeltaEventArgs e)
        //{
        //    //Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
        //    //Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
        //}
        #endregion プライベートメソッド




    }

    // 中止
    // リサイズハンドルを持つCanvas
    // ハンドルの表示切り替えはnewじゃなくて、VisibilityのVisibleとCollapsedで切り替える
    // 子要素は1個に限定
    // LeftとTop要素のリサイズ時は子要素を移動させる
    public class ResizeCanvas : Canvas
    {
        public ResizeAdorner MyResizeAdorner { get; set; }
        private UIElement MyInternalUIElement = null!;

        public ResizeCanvas()
        {
            MyResizeAdorner = new(this);
            Loaded += ResizeCanvas_Loaded;


        }

        private void ResizeCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            InitResizeAdorner();
            if (Children[0] is UIElement element)
            {
                MyInternalUIElement = element;
            }
        }

        // リサイズハンドルの初期化
        private void InitResizeAdorner()
        {
            if (AdornerLayer.GetAdornerLayer(this) is AdornerLayer layer)
            {
                layer.Add(MyResizeAdorner);
                //MyResizeAdorner.Visibility = Visibility.Collapsed;

                // リサイズ時、子要素の逆移動
                MyResizeAdorner.LeftLocateChanged += ResizeHandle_LeftLocateChanged;
                MyResizeAdorner.TopLocateChanged += ResizeHandle_TopLocateChanged;

                MyResizeAdorner.SetBinding(ResizeAdorner.ResizeHandleSizeProperty,
                    new Binding() { Source = this, Path = new PropertyPath(ResizeHandleSizeProperty) });
            }
        }



        public double ResizeHandleSize
        {
            get { return (double)GetValue(ResizeHandleSizeProperty); }
            set { SetValue(ResizeHandleSizeProperty, value); }
        }
        public static readonly DependencyProperty ResizeHandleSizeProperty =
            DependencyProperty.Register(nameof(ResizeHandleSize), typeof(double),
                typeof(ResizeCanvas), new PropertyMetadata(20.0));



        #region パブリックメソッド

        // ぴったりサイズ
        public void PerfectlyFit()
        {
            if (MyInternalUIElement is GeoThumb gt)
            {
                var bounds = gt.MyGeoLine.GetRenderBounds();
                Width = bounds.Width;
                Height = bounds.Height;
                // 位置合わせは保留
            }
        }

        // 図形の頂点ハンドルを更新
        //public void UpdateVertexHandle()
        //{
        //    if (MyInternalUIElement is GeoThumb gt)
        //    {
        //        gt.UpdateVertexHandles();
        //    }
        //}

        //public void ChangeResizeHandleVisible()
        //{
        //    if (MyResizeAdorner.Visibility == Visibility.Visible)
        //    {
        //        MyResizeAdorner.Visibility = Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        MyResizeAdorner.Visibility = Visibility.Visible;
        //    }
        //}

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



        //private void CanvasThumb_DragDelta(object sender, DragDeltaEventArgs e)
        //{
        //    //Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
        //    //Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
        //}
        #endregion プライベートメソッド
    }




    public class CanvasThumb : Thumb
    {
        private Canvas MyTemplateCanvas = null!;
        //public ResizeAdorner MyResizeAdorner { get; set; }
        private UIElement MyInternalUIElement = null!;

        #region コンストラクタ

        static CanvasThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasThumb), new FrameworkPropertyMetadata(typeof(CanvasThumb)));
        }
        public CanvasThumb()
        {
            //MyResizeAdorner = new(this);
            //Loaded += (s, e) => { InitResizeAdorner(); };
            DragDelta += CanvasThumb_DragDelta;
        }



        //private void InitResizeAdorner()
        //{
        //    if (AdornerLayer.GetAdornerLayer(this) is AdornerLayer layer)
        //    {
        //        layer.Add(MyResizeAdorner);
        //        MyResizeAdorner.Visibility = Visibility.Collapsed;

        //        MyResizeAdorner.LeftLocateChanged += ResizeHandle_LeftLocateChanged;
        //        MyResizeAdorner.TopLocateChanged += ResizeHandle_TopLocateChanged;

        //        MyResizeAdorner.SetBinding(ResizeAdorner.ResizeHandleSizeProperty,
        //            new Binding() { Source = this, Path = new PropertyPath(ResizeHandleSizeProperty) });
        //    }
        //}

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

        // ぴったりサイズ
        public void PerfectlyFit()
        {
            if (MyInternalUIElement is GeoThumb gt)
            {
                var bounds = gt.MyGeoLine.GetRenderBounds();
                Width = bounds.Width;
                Height = bounds.Height;
                // 位置合わせは保留
            }
        }

        // 図形の頂点ハンドルを更新
        //public void UpdateVertexHandle()
        //{
        //    if (MyInternalUIElement is GeoThumb gt)
        //    {
        //        gt.UpdateVertexHandles();
        //    }
        //}

        //public void ChangeResizeHandleVisible()
        //{
        //    if (MyResizeAdorner.Visibility == Visibility.Visible)
        //    {
        //        MyResizeAdorner.Visibility = Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        MyResizeAdorner.Visibility = Visibility.Visible;
        //    }
        //}

        //public void HiddenResizeHndle()
        //{
        //    MyResizeAdorner.Visibility = Visibility.Collapsed;
        //}

        //public void VisibleResizeHandle()
        //{
        //    MyResizeAdorner.Visibility = Visibility.Visible;
        //}

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
            //Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            //Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
        }
        #endregion プライベートメソッド
    }


    public class GeoThumb : Thumb
    {
        //private VertexAdorner? _vertexAdorner; // 頂点移動用ハンドル

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
        //public bool MyVisibleVertexHandle
        //{
        //    get { return (bool)GetValue(MyVisibleVertexHandleProperty); }
        //    set { SetValue(MyVisibleVertexHandleProperty, value); }
        //}
        //public static readonly DependencyProperty MyVisibleVertexHandleProperty =
        //    DependencyProperty.Register(nameof(MyVisibleVertexHandle), typeof(bool), typeof(GeoThumb), new PropertyMetadata(false, OnMyVisibleVertexHandle));

        //private static void OnMyVisibleVertexHandle(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (d is GeoThumb thumb)
        //    {
        //        if ((bool)e.NewValue)
        //        {
        //            thumb.ShowVertexHandle();
        //        }
        //        else
        //        {
        //            thumb.HideVertexHandle();
        //        }
        //    }
        //}

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


        //public void UpdateVertexHandles()
        //{
        //    _vertexAdorner?.UpdateHandles();
        //}

        //public void ShowVertexHandle()
        //{
        //    if (AdornerLayer.GetAdornerLayer(MyGeoLine) is AdornerLayer layer)
        //    {
        //        _vertexAdorner = new VertexAdorner(MyGeoLine);
        //        _vertexAdorner.SetBinding(VertexAdorner.MyHandleSizeProperty, new Binding() { Source = this, Path = new PropertyPath(MyShapeVertexHandleSizeProperty) });
        //        layer.Add(_vertexAdorner);
        //    }
        //}

        //public void HideVertexHandle()
        //{
        //    if (AdornerLayer.GetAdornerLayer(MyGeoLine) is AdornerLayer layer && _vertexAdorner is not null)
        //    {
        //        layer.Remove(_vertexAdorner);
        //        _vertexAdorner = null;
        //    }
        //}

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


    public class FlatHandle : Thumb
    {
        static FlatHandle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlatHandle), new FrameworkPropertyMetadata(typeof(FlatHandle)));
        }
        public FlatHandle()
        {

        }


        public Brush MyFillBrush
        {
            get { return (Brush)GetValue(MyFillBrushProperty); }
            set { SetValue(MyFillBrushProperty, value); }
        }
        public static readonly DependencyProperty MyFillBrushProperty =
            DependencyProperty.Register(nameof(MyFillBrush), typeof(Brush), typeof(FlatHandle), new PropertyMetadata(Brushes.Transparent));

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


    [ContentProperty(nameof(MyContent))]
    public class CustomThumb : Thumb
    {
        //// Ctrl+クリック移動後の削除判定用
        //// 移動開始時に自身は選択状態だった場合にtrue
        //private bool isSelectedAtDragStart;

        //// 移動開始時にCtrlキーが押されていたフラグ
        //private bool isDragStartWithPressedCtrl;

        static CustomThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumb), new FrameworkPropertyMetadata(typeof(CustomThumb)));
        }
        public CustomThumb()
        {


            DragDelta += CustomThumb_DragDelta;
        }

        private void CustomThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            MyData.X += e.HorizontalChange;
            MyData.Y += e.VerticalChange;
            e.Handled = true;
        }

        #region 依存関係プロパティ


        public Data MyData
        {
            get { return (Data)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(Data), typeof(CustomThumb), new PropertyMetadata(null));


        public FrameworkElement MyContent
        {
            get { return (FrameworkElement)GetValue(MyContentProperty); }
            set { SetValue(MyContentProperty, value); }
        }
        public static readonly DependencyProperty MyContentProperty =
            DependencyProperty.Register(nameof(MyContent), typeof(FrameworkElement), typeof(CustomThumb), new PropertyMetadata(null));
        #endregion 依存関係プロパティ

        #region パブリックメソッド
        //// たぶん右クリックメニューから実行
        //// 図形Thumb専用、図形にピッタリサイズにする
        //public void PerfectlyFit()
        //{
        //    if (MyContent is ResizeCanvas geot)
        //    {
        //        geot.PerfectlyFit();
        //    }
        //}

        #endregion パブリックメソッド
    }


    public class RootItemsControl : ItemsControl
    {
        static RootItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RootItemsControl), new FrameworkPropertyMetadata(typeof(RootItemsControl)));
        }
        public RootItemsControl()
        {

        }


        public RootData MyData
        {
            get { return (RootData)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(RootData), typeof(RootItemsControl), new PropertyMetadata(null));

    }




}
