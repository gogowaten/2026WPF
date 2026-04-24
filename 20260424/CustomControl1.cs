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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260424
{
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
            //Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            //Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
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
            //if (MyData is not null)
            //{
            //    MyData.InternalX += e.HorizontalChange;
            //    MyData.InternalY += e.VerticalChange;
            //    e.Handled = true;
            //}
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
        public override string ToString()
        {
            //return base.ToString();
            return MyData.Name;
        }

        static CustomThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumb), new FrameworkPropertyMetadata(typeof(CustomThumb)));
        }

        // Ctrl+クリック移動後の削除判定用
        // 移動開始時に自身は選択状態だった場合にtrue
        private bool isSelectedAtDragStart;

        // 移動開始時にCtrlキーが押されていたフラグ
        private bool isDragStartWithPressedCtrl;

        #region 依存関係プロパティ


        public Data MyData
        {
            get { return (Data)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(Data), typeof(CustomThumb), new PropertyMetadata(null));
        //private static void OnThumbDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (d is CustomThumb thumb && thumb.MyData is GeoShapeData data && data.RootData is RootData root)
        //    {
        //        //root.ChangeGeoShapeOffsetCommand.Execute(true);
        //        root.CanChageGeoShapeData();
        //    }
        //}


        public FrameworkElement MyContent
        {
            get { return (FrameworkElement)GetValue(MyContentProperty); }
            set { SetValue(MyContentProperty, value); }
        }
        public static readonly DependencyProperty MyContentProperty =
            DependencyProperty.Register(nameof(MyContent), typeof(FrameworkElement), typeof(CustomThumb), new PropertyMetadata(null));
        #endregion 依存関係プロパティ

        //public bool MyIsSelected
        //{
        //    get { return (bool)GetValue(MyIsSelectedProperty); }
        //    set { SetValue(MyIsSelectedProperty, value); }
        //}
        //public static readonly DependencyProperty MyIsSelectedProperty =
        //    DependencyProperty.Register(nameof(MyIsSelected), typeof(bool), typeof(CustomThumb), new PropertyMetadata(false));


        // コンストラクタ
        public CustomThumb()
        {
            //this.DataContext = this;
            DragStarted += CustomThumb_DragStarted;
            DragDelta += TThumb_DragDelta;
            DragCompleted += CustomThumb_DragCompleted;
            PreviewMouseLeftButtonDown += CustomThumb_PreviewMouseLeftButtonDown;

            // 以下はXAMLのほうで処理するように変更した
            // TextBlockなどサイズが確定していない要素を
            // まっさらなRootに追加した直後にRootのサイズを決定するのに使う
            //Loaded += CustomThumb_Loaded; // これは動くけど、DataTemplateからだとクリックのたびに実行される
            //Initialized += CustomThumb_Initialized;// こっちだとまだ描画されていない感じ

        }

        //// 起動時
        //private void CustomThumb_Loaded(object sender, RoutedEventArgs e)
        //{
        //    // TextBlockなどのサイズがNaNの要素が追加された時用
        //    // Dataが追加された時点で親要素のサイズ計測がされるけど、これらの要素はその時点でのサイズは0で
        //    // 正しいサイズが設定されるのは描画後で、それがここなので、ここで親要素のサイズ計測


        //    if (MyData is TextData text && text.RootData is RootData root)
        //    {
        //        if (text.Width != 0 && root.DataList.Count == 1)
        //        {
        //            root.UpdateSize();
        //        }

        //    }
        //    // 起動時に1回だけ実行されればいいので、ここで解除
        //    // とは言ってもDateTemplateで表示しているとクリックのたびに再作成している？から意味ないかも？
        //    Loaded -= CustomThumb_Loaded;
        //}


        //protected override bool HandlesScrolling
        //{
        //    get
        //    {
        //        //return base.HandlesScrolling;
        //        return true;
        //    }
        //}

        #region キーイベント

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.F2)
            {
                // 自身がCurrentなら編集モードにする
                MyData.RootData?.MigrateEditingGroupCurrent();
            }
            else if (e.Key == Key.Escape)
            {
                // Parentを編集モードにする
                MyData.RootData?.MigrateEditingGroupUpper();
                e.Handled = true;
                //if (MyData is GroupData)
                //{
                //    MyData.RootData?.MigrateEditingGroupUpper();
                //    e.Handled = true;
                //}
            }
        }
        #endregion キーイベント


        #region マウスイベント
        #region クリックイベント


        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            var sou = e.Source;
            var ori = e.OriginalSource;
            var dc = this.DataContext;
            var myd = MyData;

            this.Focus();
            var isfo = this.IsFocused;
            var iskeyfo = this.IsKeyboardFocused;

            if (MyData.RootData is RootData root)
            {
                // ClickedItemの更新
                if (e.OriginalSource is FrameworkElement elm && elm.DataContext is Data data)
                {
                    root.ClickedItem = data;
                }

            }

        }





        private void CustomThumb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //// Groupの中の要素の場合は、先にGroupのクリックが来た後に要素のクリックが来る
            //var sou = e.Source;
            //var ori = e.OriginalSource;
            //var dc = this.DataContext;
            //var myd = MyData;
            //this.Focus();
            //var isfo = this.IsFocused;
            //var iskeyfo = this.IsKeyboardFocused;
            //var iii = this.Focusable;

        }

        #endregion クリックイベント

        #region ドラッグ移動

        // ドラッグ移動開始時
        private void CustomThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            if (MyData.IsSelectable == false) { return; }

            if (MyData.RootData is RootData root)
            {
                // 自身の選択状態を記録 自身が既に選択リストに在ったかを記録
                isSelectedAtDragStart = root.SelectedItems.Contains(MyData);

                // Ctrlキーの状態を記録
                isDragStartWithPressedCtrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

                // 選択状態を更新
                if (MyData.IsSelected == false)
                {
                    // 未選択をCtrlクリックした場合は、選択リストに追加して選択状態にする
                    if (isDragStartWithPressedCtrl)
                    {
                        root.AddDataToSelectedItems(MyData);
                    }
                    // 未選択を通常クリックした場合は、自身だけを選択状態にしたいので
                    // 選択リストをクリアした後、自身をリストに追加
                    else
                    {
                        root.ClearSelectedItems();
                        root.AddDataToSelectedItems(MyData);
                    }
                }
            }


        }


        private void TThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (MyData.RootData is RootData root)
            {
                if (root.SelectedItems.Contains(MyData))
                {
                    foreach (var item in root.SelectedItems)
                    {
                        item.X += e.HorizontalChange;
                        item.Y += e.VerticalChange;
                    }
                    e.Handled = true;
                }

            }
        }

        // ドラッグ移動完了後
        private void CustomThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            // 選択リストから削除して、未選択状態にする処理
            // 削除対象になる条件はAとBの2種類

            // AB共通条件
            // * 移動していない
            // * クリック前から既選択だった

            // パターンA
            // * クリック時にCtrlキーが押されていた
            // * 選択リストの要素数が2個以上(選択要素を0個にしたくないだけなので、これは変えても良いかも)

            // パターンB
            // * 通常クリックだった(Ctrlキーが押されていない)
            // * 選択リストに自身が在る


            // 移動なし＋既選択
            if (e.HorizontalChange == 0 && e.VerticalChange == 0 && isSelectedAtDragStart && MyData.RootData is RootData root)
            {
                if (MyData.IsSelected)
                {
                    if (isDragStartWithPressedCtrl == false)
                    {
                        // 通常クリックだった場合、自身だけを選択状態
                        root.ClearSelectedItems();
                        root.AddDataToSelectedItems(MyData);
                        e.Handled = true;// ここで止める。
                    }
                    else
                    {
                        // Ctrlクリックだった場合で選択要素が2個以上あるなら、自身を選択リストから削除
                        if (root.SelectedItems.Count > 1)
                        {
                            root.RemoveDataFromSelect(MyData);
                            e.Handled = true;// ここで止める。
                        }
                        // 選択要素が自身だけ(1個だけ)なら何もしないで終了
                        else
                        {
                            e.Handled = true;// ここで止める。
                        }
                    }
                }
            }
            //else
            //{
            //    //e.Handled = true;// これは要らない
            //}

            // 移動した場合はBounds更新
            if (e.HorizontalChange != 0 || e.VerticalChange != 0)
            {
                var sou = e.Source;
                var ori = e.OriginalSource;
                var myd = MyData;

                MyData.ParentData?.UpdateBounds(MyData.ParentData);
            }
        }

        #endregion ドラッグ移動
        #endregion マウスイベント
    }









    public class AAAItemsCtrl : ItemsControl
    {

        public RootData MyRootData
        {
            get { return (RootData)GetValue(MyRootDataProperty); }
            set { SetValue(MyRootDataProperty, value); }
        }
        public static readonly DependencyProperty MyRootDataProperty =
            DependencyProperty.Register(nameof(MyRootData), typeof(RootData), typeof(AAAItemsCtrl), new PropertyMetadata(null));


        //public DataService AAADataService
        //{
        //    get { return (DataService)GetValue(AAADataServiceProperty); }
        //    set { SetValue(AAADataServiceProperty, value); }
        //}
        //public static readonly DependencyProperty AAADataServiceProperty =
        //    DependencyProperty.Register(nameof(AAADataService), typeof(DataService), typeof(AAAItemsCtrl));


        static AAAItemsCtrl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AAAItemsCtrl), new FrameworkPropertyMetadata(typeof(AAAItemsCtrl)));
        }
        public AAAItemsCtrl()
        {
            PreviewMouseLeftButtonDown += AAAItemsCtrl_PreviewMouseLeftButtonDown;
            // 子要素クリック時のスクロールバーの自動移動をキャンセル
            RequestBringIntoView += (s, e) => { e.Handled = true; };
        }


        private void AAAItemsCtrl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var sou = e.Source;
            var ori = e.OriginalSource;

        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            var sou = e.Source;
            var ori = e.OriginalSource;
            var dc = this.DataContext;
            var root = MyRootData;

            if (ori is FrameworkElement elem && elem.DataContext is Data oo)
            {
                var neko = oo;
            }

            if (ori is UIElement elm)
            {
                // 要素からコンテナ取得？
                var youso = ContainerFromElement(elm);
                var youso2 = ContainerFromElement(this, elm);
                var item = ItemsControl.ItemsControlFromItemContainer(youso);
                var yo = ItemsControl.GetItemsOwner(youso);
                var io = ItemsControl.GetItemsOwner(item);
            }
        }
    }


 
}
