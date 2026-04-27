using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace _20260426
{
    public class ResizeAdorner : Adorner
    {

        private readonly VisualCollection _visualChildren;
        private readonly Dictionary<ResizeDirection, FlatHandle> HT = []; // リサイズハンドル群
        private double ResizeHandleHalfSize { get; set; } // 計算用：ハンドルサイズの半分
        private Size InternalResizeHandleSize { get; set; } // 計算用：ハンドルサイズ

        #region コンストラクタと初期化系

        public ResizeAdorner(UIElement adornedElement) : base(adornedElement)
        {
            this.UseLayoutRounding = true; // ドットに合わせてくっきり表示
            _visualChildren = new VisualCollection(this);
            ResizeHandleHalfSize = ResizeHandleSize / 2.0;
            InternalResizeHandleSize = new Size(ResizeHandleSize, ResizeHandleSize);

            // 対象要素の座標が未指定のときは0を指定する
            if (double.IsNaN(Canvas.GetLeft(adornedElement)))
            {
                Canvas.SetLeft(adornedElement, 0);
                Canvas.SetTop(adornedElement, 0);
            }

            // 8つのハンドルThumbを作成
            foreach (ResizeDirection item in Enum.GetValues<ResizeDirection>())
            {
                HT.Add(item, CreateResizeHandleThumb(item, Cursors.Hand));
            }
        }

        private FlatHandle CreateResizeHandleThumb(ResizeDirection direction, Cursor cursor)
        {
            var thumb = new FlatHandle()
            {
                Tag = direction,
                Cursor = cursor,
                MyFillBrush = new SolidColorBrush(Color.FromArgb(40, 100, 200, 0))
            };
            thumb.DragDelta += OnResize;
            _visualChildren.Add(thumb);
            return thumb;
        }
        //private Thumb CreateResizeHandleThumb(ResizeDirection direction, Cursor cursor)
        //{
        //    var thumb = new Thumb()
        //    {
        //        Background = Brushes.White,
        //        BorderBrush = Brushes.DodgerBlue,
        //        BorderThickness = new Thickness(1),
        //        Tag = direction,
        //        Cursor = cursor
        //    };
        //    thumb.DragDelta += OnResize;
        //    _visualChildren.Add(thumb);
        //    return thumb;
        //}

        private static readonly Dictionary<ResizeDirection, ResizeMatrix> ResizePolicies = new()
        {   
            //                                              Width,  Left,   Height,   Top
            {ResizeDirection.Left,          new ResizeMatrix(-1,      1,       0,      0) },
            {ResizeDirection.Right,         new ResizeMatrix( 1,      0,       0,      0) },
            {ResizeDirection.Top,           new ResizeMatrix( 0,      0,      -1,      1) },
            {ResizeDirection.Bottom,        new ResizeMatrix( 0,      0,       1,      0) },
            {ResizeDirection.TopLeft,       new ResizeMatrix(-1,      1,      -1,      1) },
            {ResizeDirection.TopRight,      new ResizeMatrix( 1,      0,      -1,      1) },
            {ResizeDirection.BottomLeft,    new ResizeMatrix(-1,      1,       1,      0) },
            {ResizeDirection.BottomRight,   new ResizeMatrix( 1,      0,       1,      0) },
        };

        #endregion コンストラクタと初期化系

        private record ResizeMatrix(double WidthF, double LeftF, double HeightF, double TopF);

        public enum ResizeDirection
        {
            Left, Right, Top, Bottom,
            TopLeft, TopRight, BottomLeft, BottomRight
        }

        #region プロパティ

        // リサイズ対象の最小サイズ値
        public double ElementResizeMinSize
        {
            get { return (double)GetValue(ElementMinSizeProperty); }
            set { SetValue(ElementMinSizeProperty, value); }
        }
        public static readonly DependencyProperty ElementMinSizeProperty =
            DependencyProperty.Register(nameof(ElementResizeMinSize), typeof(double), typeof(ResizeAdorner), new PropertyMetadata(10.0));

        // ハンドルサイズ
        public double ResizeHandleSize
        {
            get { return (double)GetValue(ResizeHandleSizeProperty); }
            set { SetValue(ResizeHandleSizeProperty, value); }
        }
        public static readonly DependencyProperty ResizeHandleSizeProperty =
            DependencyProperty.Register(nameof(ResizeHandleSize), typeof(double), typeof(ResizeAdorner), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsArrange, OnResizeHnadleSize));

        private static void OnResizeHnadleSize(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ResizeAdorner resizeAdorner)
            {
                double value = (double)e.NewValue;
                resizeAdorner.ResizeHandleHalfSize = value / 2.0;
                resizeAdorner.InternalResizeHandleSize = new Size(value, value);
            }
        }

        #endregion プロパティ

        #region イベント

        public event EventHandler<double>? LeftLocateChanged;
        public event EventHandler<double>? TopLocateChanged;
        #endregion イベント

        // Thumbの移動、対象要素のサイズ変更
        private void OnResize(object sender, DragDeltaEventArgs e)
        {
            if (sender is not FlatHandle thumb || AdornedElement is not FrameworkElement element) { return; }

            var dir = (ResizeDirection)thumb.Tag;
            if (!ResizePolicies.TryGetValue(dir, out var policy)) { return; }

            // 横方向の計算
            if (policy.WidthF != 0)
            {
                // 左要素の変更時は、横幅と左位置も変更する
                // サイズは、最小サイズ未満にならないようにする
                double deltaX = e.HorizontalChange;
                double newWidth = element.Width + (deltaX * policy.WidthF);
                if (newWidth < ElementResizeMinSize)
                {
                    deltaX = (ElementResizeMinSize - element.Width) * policy.WidthF;
                    newWidth = ElementResizeMinSize;
                }

                element.Width = newWidth; // リサイズ

                // 移動
                if (policy.LeftF != 0)
                {
                    double moveX = deltaX * policy.LeftF;
                    var neko =  Canvas.GetLeft(element);
                    Canvas.SetLeft(element, Canvas.GetLeft(element) + moveX); // X座標変更
                    LeftLocateChanged?.Invoke(this, moveX); // X座標変更通知用                 
                }
            }

            // 縦方向の計算
            if (policy.HeightF != 0)
            {
                double deltaY = e.VerticalChange;
                double newHeight = element.Height + (deltaY * policy.HeightF);

                if (newHeight < ElementResizeMinSize)
                {
                    deltaY = (ElementResizeMinSize - element.Height) * policy.HeightF;
                    newHeight = ElementResizeMinSize;
                }

                element.Height = newHeight;
                if (policy.TopF != 0)
                {
                    double moveY = deltaY * policy.TopF;
                    Canvas.SetTop(element, Canvas.GetTop(element) + moveY);
                    TopLocateChanged?.Invoke(this, moveY);
                }
            }
        }


        // 配置の決定（Thumbを右下に配置）
        // 8個のThumbを正しい位置に並べる際も、finalSize（対象要素のサイズ）を基準に一括配置します。
        protected override Size ArrangeOverride(Size finalSize)
        {
            // Q 1度の動作に2回ArrangeOverrideが処理されているのはなんで？
            // A 1回目：子要素（Thumb等）の再配置
            // 2回目：子要素の再配置により親要素（AdornerElement）の再配置が必要

            double r = ResizeHandleHalfSize; // ハンドルThumbの半径
            Size s = InternalResizeHandleSize; // ハンドルサイズ
            double w = finalSize.Width;
            double h = finalSize.Height;

            HT[ResizeDirection.TopLeft].Arrange(new Rect(new Point(-r, -r), s));
            HT[ResizeDirection.TopRight].Arrange(new Rect(new Point(w - r, -r), s));
            HT[ResizeDirection.BottomLeft].Arrange(new Rect(new Point(-r, h - r), s));
            HT[ResizeDirection.BottomRight].Arrange(new Rect(new Point(w - r, h - r), s));

            double halfW = w / 2.0;
            double halfH = h / 2.0;
            HT[ResizeDirection.Top].Arrange(new Rect(new Point(halfW - r, -r), s));
            HT[ResizeDirection.Left].Arrange(new Rect(new Point(-r, halfH - r), s));
            HT[ResizeDirection.Right].Arrange(new Rect(new Point(w - r, halfH - r), s));
            HT[ResizeDirection.Bottom].Arrange(new Rect(new Point(halfW - r, h - r), s));

            return finalSize;
            //return base.ArrangeOverride(finalSize);
        }



        /// <summary>
        /// 指定された UI 要素に、サイズ変更アドーナがまだ存在しない場合に、追加します。
        /// </summary>
        /// <remarks>このメソッドは、指定された要素にサイズ変更アドーナが既に存在するかどうかを確認してから、新しいアドーナを追加します。
        /// 要素はビジュアルツリーの一部であり、関連付けられたアドーナレイヤーを持っている必要があります。
        /// </remarks>
        /// <param name="element">サイズ変更アドーナを追加する UI 要素。null は指定できません。</param>
        public static ResizeAdorner? AddResizeAdorner(UIElement? element)
        {
            if (element is null) { return null; }

            if (AdornerLayer.GetAdornerLayer(element) is AdornerLayer layer)
            {
                var adorners = layer.GetAdorners(element);
                if (adorners is null || adorners.Length == 0)
                {
                    ResizeAdorner me = new(element);
                    layer.Add(me);
                    if (double.IsNaN(Canvas.GetLeft(element)))
                    {
                        Canvas.SetLeft(element, 0);
                        Canvas.SetTop(element, 0);
                    }
                    return me;
                }
            }
            return null;
        }


        /// <summary>
        /// 指定された UI 要素から、すべてのアドナーを削除します。
        /// </summary>
        /// <remarks>このメソッドは、指定された要素に関連付けられたすべてのアドナーを検索して削除します。
        /// アドナーが見つからない場合は、false を返します。操作を成功させるには、要素がビジュアルツリーの一部である必要があります。
        /// </remarks>
        /// <param name="element">アドナーを削除する UI 要素。null は指定できません。</param>
        /// <returns>削除されたAdornerの個数を返す</returns>
        public static int RemoveResizeAdorner(UIElement? element)
        {

            int result = 0;
            if (element is null) { return result; }

            if (AdornerLayer.GetAdornerLayer(element) is AdornerLayer layer)
            {
                if (layer.GetAdorners(element) is Adorner[] ados)
                {
                    foreach (Adorner item in ados)
                    {
                        layer.Remove(item);
                        result++;
                    }
                }
            }
            return result;
        }


        // Visualの子要素をフレームワークに教えるための定型文
        protected override int VisualChildrenCount => _visualChildren.Count;
        protected override Visual GetVisualChild(int index) => _visualChildren[index];

    }
}