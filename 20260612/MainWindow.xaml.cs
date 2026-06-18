using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260612
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool IsMouseDraged;
        private Point MyClickedPoint;
        private double MyMinWakuX;
        private double MyMinWakuY;
        private double MaxWakuX;
        private double MaxWakuY;
        private double MyRateMoveX;
        private double MyRateMoveY;
        private double MyClickedScrollXOffset;
        private double MyClickedScrollYOffset;


        private readonly string MyImagePath = @"D:\ブログ用\テスト用画像\collection5.png";
        //private readonly string MyImagePath = @"D:\ブログ用\テスト用画像\テスト結果用\NEC_0541_2017_07_21_午後わてん_p6_32color.png";
        //private readonly string MyImagePath = @"D:\ブログ用\テスト用画像\連結テスト\WP_20210327_11_20_32_Pro_2021_03_27_午後わてん.jpg";
        public BitmapImage MyImage { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            //MyImage = new BitmapImage(new Uri(MyImagePath));


            MyImage = new();
            MyImage.BeginInit();
            MyImage.UriSource = new Uri(MyImagePath);
            MyImage.EndInit();
            MainImage.Source = MyImage;
            MiniMapImage.Source = MyImage;

        }

        #region 依存関係プロパティ

        public int MyX
        {
            get { return (int)GetValue(MyXProperty); }
            set { SetValue(MyXProperty, value); }
        }
        public static readonly DependencyProperty MyXProperty =
            DependencyProperty.Register(nameof(MyX), typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        public int MyY
        {
            get { return (int)GetValue(MyYProperty); }
            set { SetValue(MyYProperty, value); }
        }
        public static readonly DependencyProperty MyYProperty =
            DependencyProperty.Register(nameof(MyY), typeof(int), typeof(MainWindow), new PropertyMetadata(0));


        public double MyScale
        {
            get { return (double)GetValue(MyScaleProperty); }
            set { SetValue(MyScaleProperty, value); }
        }
        public static readonly DependencyProperty MyScaleProperty =
            DependencyProperty.Register(nameof(MyScale), typeof(double), typeof(MainWindow), new PropertyMetadata(1.0));



        #endregion 依存関係プロパティ




        //private static void OnMyScale(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    //if (d is MainWindow main && main.MyRect.IsMouseOver)
        //    //{
        //    //    AdjustOffset(main.MyScroll, main.MyRect, main.MyScale, main.MyX, main.MyY);
        //    //}
        //}

        //private static void AdjustOffset(ScrollViewer scroll, Rectangle rect, double scale, int currentXPos, int currentYPos)
        //{
        //    var bmpViewSize = rect.Width * scale;
        //    var maxOffset = bmpViewSize - scroll.ActualWidth;
        //    if (maxOffset > 0)
        //    {
        //        var ratePos = currentXPos / rect.Width;
        //        var pos = maxOffset * ratePos;
        //        scroll.ScrollToHorizontalOffset(pos);
        //    }

        //    bmpViewSize = rect.Height * scale;
        //    maxOffset = bmpViewSize - scroll.ActualHeight;
        //    if (maxOffset > 0)
        //    {
        //        var ratePos = currentYPos / rect.Height;
        //        var pos = maxOffset * ratePos;
        //        scroll.ScrollToVerticalOffset(pos);
        //    }
        //}



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var imageActW = MainImage.ActualWidth;
        }

        private void MyRect_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //var point = e.GetPosition(MyScroll);
            //var neko = e.GetPosition(MyRect);

            //var rectmouse = MyRect.IsMouseOver;
        }

        private void MyRect_MouseMove(object sender, MouseEventArgs e)
        {
            //var pos = e.GetPosition(MyRect);
            //MyX = (int)pos.X;
            //MyY = (int)pos.Y;
        }




        private void MyRect_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                MyScale++;
                e.Handled = true;
            }
            else if (e.Delta < 0 && MyScale - 1 > 0)
            {
                MyScale--;
                e.Handled = true;
            }
        }

        private void MyScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // 画像がまだロードされていない、またはサイズが0の場合は処理しない
            if (MainImage.ActualWidth == 0 || MainImage.ActualHeight == 0) return;

            //// 1. メイン画像全体の「拡大後」のサイズを取得
            //double totalWidth = MainImage.ActualWidth * ImageScale.ScaleX;
            //double totalHeight = MainImage.ActualHeight * ImageScale.ScaleY;

            //// 2. ScrollViewerで見えている範囲（Viewport）のサイズを取得
            //double viewportWidth = MyScroll.ViewportWidth;
            //double viewportHeight = MyScroll.ViewportHeight;

            //// 3. 現在のスクロール位置（左上からのオフセット）を取得
            //double offsetX = MyScroll.HorizontalOffset;
            //double offsetY = MyScroll.VerticalOffset;

            //// --- ここからミニマップ（縮小側）への縮尺変換 ---

            ////// 4. ミニマップ内での実際の画像表示サイズを取得（Uniformに合わせる）
            ////// ※Canvasの200x150に対して、画像のアスペクト比によって余白ができるのを考慮
            ////double miniImageWidth = MiniMapImage.ActualWidth;
            ////double miniImageHeight = MiniMapImage.ActualHeight;

            //// アスペクト比から実際の描画サイズを割り出す場合は以下のように調整
            //double imgAspect = MainImage.ActualWidth / MainImage.ActualHeight;
            //double canvasAspect = MiniMapCanvas.Width / MiniMapCanvas.Height;

            //double actualMiniW, actualMiniH;
            //double leftMargin = 0, topMargin = 0;

            //if (imgAspect > canvasAspect)
            //{
            //    actualMiniW = MiniMapCanvas.Width;
            //    actualMiniH = MiniMapCanvas.Width / imgAspect;
            //    topMargin = (MiniMapCanvas.Height - actualMiniH) / 2; // 上下の黒帯
            //}
            //else
            //{
            //    actualMiniH = MiniMapCanvas.Height;
            //    actualMiniW = MiniMapCanvas.Height * imgAspect;
            //    leftMargin = (MiniMapCanvas.Width - actualMiniW) / 2; // 左右の黒帯
            //}

            //// 5. メイン画面とミニマップ画像の比率を算出
            //double ratioX = actualMiniW / totalWidth;
            //double ratioY = actualMiniH / totalHeight;

            //// 6. 枠（Rectangle）のサイズを計算
            //// 全体が見えている場合はミニマップ画像と同じサイズ、拡大しているときはその比率分だけ小さくする
            //ViewBoundsRect.Width = Math.Min(viewportWidth * ratioX, actualMiniW);
            //ViewBoundsRect.Height = Math.Min(viewportHeight * ratioY, actualMiniH);

            //// 7. 枠（Rectangle）の位置を計算（余白分をプラスする）
            //double rectLeft = leftMargin + (offsetX * ratioX);
            //double rectTop = topMargin + (offsetY * ratioY);

            //Canvas.SetLeft(ViewBoundsRect, rectLeft);
            //Canvas.SetTop(ViewBoundsRect, rectTop);

            Navi();
        }

        private void Navi()
        {
            // スクロール位置の割合、スクロールバーなしなら0
            double rateScrollX = MyScroll.ScrollableWidth == 0 ? 0 : MyScroll.HorizontalOffset / MyScroll.ScrollableWidth;
            double rateScrollY = MyScroll.ScrollableHeight == 0 ? 0 : MyScroll.VerticalOffset / MyScroll.ScrollableHeight;

            // ナビ枠の位置の最小値
            double zeroXPos = (MiniMapCanvas.Width - MiniMapImage.ActualWidth) / 2.0;
            double zeroYPos = (MiniMapCanvas.Height - MiniMapImage.ActualHeight) / 2.0;
            MyMinWakuX = zeroXPos;
            MyMinWakuY = zeroYPos;

            // スケール後の画像サイズ            
            double scaledImageWidth = MyImage.PixelWidth * ImageScale.ScaleX;
            double scaledImageHeight = MyImage.PixelHeight * ImageScale.ScaleY;

            // ScrollViewer内に見えている部分のサイズ
            double viewWidth = MyScroll.ViewportWidth;
            double viewHeight = MyScroll.ViewportHeight;

            // ScrollViewer内に見えている部分（Viewport）の、全体からの比率
            double rateViewWidth = viewWidth / scaledImageWidth;
            double rateViewHeight = viewHeight / scaledImageHeight;

            // ナビ枠のサイズ決定
            double naviWidth = MiniMapImage.ActualWidth * rateViewWidth;
            if (rateViewWidth > 1.0) { naviWidth = MiniMapImage.ActualWidth; }
            ViewBoundsRect.Width = naviWidth;
            double naviHeight = MiniMapImage.ActualHeight * rateViewHeight;
            if (rateViewHeight > 1.0) { naviHeight = MiniMapImage.ActualHeight; }
            ViewBoundsRect.Height = naviHeight;

            // ナビ枠の位置の最大値、可動範囲の最大値
            double maxScrollX = MiniMapImage.ActualWidth - ViewBoundsRect.Width;
            double maxScrollY = MiniMapImage.ActualHeight - ViewBoundsRect.Height;

            MyRateMoveX = MyScroll.ScrollableWidth / maxScrollX;
            MyRateMoveY = MyScroll.ScrollableHeight / maxScrollY;

            // ナビ枠の位置を指定
            double xPos = zeroXPos + maxScrollX * rateScrollX;
            double yPos = zeroYPos + maxScrollY * rateScrollY;
            Canvas.SetLeft(ViewBoundsRect, xPos);
            Canvas.SetTop(ViewBoundsRect, yPos);

        }

        private void ViewBoundsRect_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {


        }

        private void ViewBoundsRect_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void ViewBoundsRect_MouseMove(object sender, MouseEventArgs e)
        {

        }
        private static double Clamp(double value, double min, double max)
        {
            if (min > max) { (max, min) = (min, max); }

            if (value < min) { value = min; }
            if (value > max) { value = max; }
            return value;
        }

        // MiniMapCanvasクリック時、クリック位置に枠が来るようにスクロール位置を調整する
        private void MiniMapCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsMouseDraged = true;
            MyClickedPoint = e.GetPosition(MiniMapCanvas);
            
            // 枠内クリックの場合は移動処理はしない
            if (MiniMapCanvas.InputHitTest(MyClickedPoint) is Rectangle)
            {
                MyClickedScrollXOffset = MyScroll.HorizontalOffset;
                MyClickedScrollYOffset = MyScroll.VerticalOffset;
                return;
            }

            // クリック位置と枠の位置の差
            var xDiff = MyClickedPoint.X - Canvas.GetLeft(ViewBoundsRect);
            var yDiff = MyClickedPoint.Y - Canvas.GetTop(ViewBoundsRect);

            // スクロール位置 = 今のスクロール位置 + （位置の差 * ScrollViewerと枠サイズの率）
            double xOffset = MyScroll.HorizontalOffset + (xDiff * MyRateMoveX);
            double yOffset = MyScroll.VerticalOffset + (yDiff * MyRateMoveY);
            // クリック位置に枠の中心を合わせる
            xOffset -= ViewBoundsRect.ActualWidth * MyRateMoveX / 2.0;
            yOffset -= ViewBoundsRect.ActualHeight * MyRateMoveY / 2.0;

            xOffset = Clamp(xOffset, 0, MyScroll.ScrollableWidth);
            yOffset = Clamp(yOffset, 0, MyScroll.ScrollableHeight);

            MyScroll.ScrollToHorizontalOffset(xOffset);
            MyScroll.ScrollToVerticalOffset(yOffset);
            MyClickedScrollXOffset = xOffset;
            MyClickedScrollYOffset = yOffset;

        }


        private void MiniMapCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsMouseDraged = false;
        }

        private void MiniMapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDraged == false) { return; }
            else
            {
                var neko = e.GetPosition(MiniMapCanvas);
                var xDiff = neko.X - MyClickedPoint.X;
                var yDiff = neko.Y - MyClickedPoint.Y;
                double xOffset = MyClickedScrollXOffset + xDiff * MyRateMoveX;
                double yOffset = MyClickedScrollYOffset + yDiff * MyRateMoveY;
                xOffset = Clamp(xOffset, 0, MyScroll.ScrollableWidth);
                yOffset = Clamp(yOffset, 0, MyScroll.ScrollableHeight);
                MyScroll.ScrollToHorizontalOffset(xOffset);
                MyScroll.ScrollToVerticalOffset(yOffset);
            }

        }
        // 移動量に応じてスクロール位置を調整
        //MyScroll.ScrollToHorizontalOffset();

    }
}

