using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

// Visual Studio用、BitmapSourceVisualizerに「ファイルに保存」と「コピー」を追加した - 午後わてんのブログ
// https://gogowaten.hatenablog.com/entry/2026/05/20/233726

namespace BitmapSourceVisualizer
{
    /// <summary>
    /// BitmapSourceVisualizerWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class BitmapSourceVisualizerWindow : Window
    {
        //private double MyPixelGridVisibleMinScale = 10.0;
        private bool IsMouseDraged; // 枠Rectのドラッグ移動フラグ
        private Point MyClickedPoint; // MiniMapをクリックした場所
        private double MyRateMoveX; // スクロール可動範囲の比率
        private double MyRateMoveY;
        private double MyClickedScrollXOffset; // スクロール位置の記録
        private double MyClickedScrollYOffset;

        private Point MyPoint; // マウスドラッグ移動処理で使う
        private readonly double ImageScaleMin = 0.01; // 拡大率下限
        private readonly double ImageScaleMax = 100.0; // 拡大率上限




        public BitmapSourceVisualizerWindow()
        {
            InitializeComponent();
            ImageControl.ContextMenu = CreateContextMenu();
            DataContext = this;
            //MyTextBlockScale.FontSize = this.FontSize * 1.2;
            Loaded += BitmapSourceVisualizerWindow_Loaded;

        }

        private void BitmapSourceVisualizerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MyBind();
        }
        private void MyBind()
        {
            SetBinding(MyClickedSolidColorBrushProperty, new Binding() { Source = this, Path = new PropertyPath(MyClickedColorProperty), Converter = new MyConvColorToSolidBrush() });
            SetBinding(MyClickedHsvProperty, new Binding() { Source = this, Path = new PropertyPath(MyClickedColorProperty), Converter = new MyConvColorToHsv() });

            //// 拡大後のサイズ
            //MultiBinding mb;
            //mb = new() { Converter = new MyConvScaledSize(), Mode = BindingMode.OneWay };
            //Binding scaleBind = new() { Source = this, Path = new PropertyPath(MyImageScaleProperty) };
            //mb.Bindings.Add(scaleBind);
            //mb.Bindings.Add(new Binding() { Source = MyBitmapSource, Path = new PropertyPath(WidthProperty) });
            //SetBinding(MyBitmapScaledWidthProperty, mb);

            //mb = new() { Converter = new MyConvScaledSize(), Mode = BindingMode.OneWay };
            //mb.Bindings.Add(scaleBind);
            //mb.Bindings.Add(new Binding() { Source = MyBitmapSource, Path = new PropertyPath(HeightProperty) });
            //SetBinding(MyBitmapScaledHeightProperty, mb);

        }

        public void SetImage(BitmapSource bitmap)
        {
            MyBitmapSource = bitmap;
            ImageControl.Source = bitmap;
            //Title = $"BitmapSource Visualizer - {bitmap.PixelWidth} x {bitmap.PixelHeight} , PixelFormat {bitmap.Format} , DPI {bitmap.DpiX}";
            MiniMapImage.Source = bitmap;
        }



        #region 依存関係プロパティ

        // 拡大後のサイズ
        public int MyBitmapScaledWidth
        {
            get { return (int)GetValue(MyBitmapScaledWidthProperty); }
            set { SetValue(MyBitmapScaledWidthProperty, value); }
        }
        public static readonly DependencyProperty MyBitmapScaledWidthProperty =
            DependencyProperty.Register(nameof(MyBitmapScaledWidth), typeof(int), typeof(BitmapSourceVisualizerWindow), new PropertyMetadata(0));

        public int MyBitmapScaledHeight
        {
            get { return (int)GetValue(MyBitmapScaledHeightProperty); }
            set { SetValue(MyBitmapScaledHeightProperty, value); }
        }
        public static readonly DependencyProperty MyBitmapScaledHeightProperty =
            DependencyProperty.Register(nameof(MyBitmapScaledHeight), typeof(int), typeof(BitmapSourceVisualizerWindow), new PropertyMetadata(0));


        // ピクセルグリッドを表示するために必要な最低倍率
        public double MyPixelGridVisibleMinScale
        {
            get { return (double)GetValue(MyPixelGridVisibleMinScaleProperty); }
            set { SetValue(MyPixelGridVisibleMinScaleProperty, value); }
        }
        public static readonly DependencyProperty MyPixelGridVisibleMinScaleProperty =
            DependencyProperty.Register(nameof(MyPixelGridVisibleMinScale), typeof(double), typeof(BitmapSourceVisualizerWindow), new PropertyMetadata(10.0));

        // ピクセルグリッドの線の幅
        public double MyPixelGridWidth
        {
            get { return (double)GetValue(MyPixelGridWidthProperty); }
            set { SetValue(MyPixelGridWidthProperty, value); }
        }
        public static readonly DependencyProperty MyPixelGridWidthProperty =
            DependencyProperty.Register(nameof(MyPixelGridWidth), typeof(double), typeof(BitmapSourceVisualizerWindow), new PropertyMetadata(0.0));

        // 画像のドラッグ移動確認フラグ
        public bool IsImageDrag
        {
            get { return (bool)GetValue(IsImageDragProperty); }
            set { SetValue(IsImageDragProperty, value); }
        }
        public static readonly DependencyProperty IsImageDragProperty =
            DependencyProperty.Register(nameof(IsImageDrag), typeof(bool), typeof(BitmapSourceVisualizerWindow), new PropertyMetadata(false));


        // メイン画像
        public BitmapSource MyBitmapSource
        {
            get { return (BitmapSource)GetValue(MyBitmapSourceProperty); }
            set { SetValue(MyBitmapSourceProperty, value); }
        }
        public static readonly DependencyProperty MyBitmapSourceProperty =
            DependencyProperty.Register(nameof(MyBitmapSource), typeof(BitmapSource), typeof(BitmapSourceVisualizerWindow), new PropertyMetadata(null));

        // クリックしたピクセルの座標
        public int MyClickedPixelX
        {
            get { return (int)GetValue(MyClickedPixelXProperty); }
            set { SetValue(MyClickedPixelXProperty, value); }
        }
        public static readonly DependencyProperty MyClickedPixelXProperty =
            DependencyProperty.Register(nameof(MyClickedPixelX), typeof(int), typeof(BitmapSourceVisualizerWindow), new PropertyMetadata(0));

        public int MyClickedPixelY
        {
            get { return (int)GetValue(MyClickedPixelYProperty); }
            set { SetValue(MyClickedPixelYProperty, value); }
        }
        public static readonly DependencyProperty MyClickedPixelYProperty =
            DependencyProperty.Register(nameof(MyClickedPixelY), typeof(int), typeof(BitmapSourceVisualizerWindow), new PropertyMetadata(0));

        // クリックしたピクセルの色のブラシ
        public SolidColorBrush MyClickedSolidColorBrush
        {
            get { return (SolidColorBrush)GetValue(MyClickedSolidColorBrushProperty); }
            set { SetValue(MyClickedSolidColorBrushProperty, value); }
        }
        public static readonly DependencyProperty MyClickedSolidColorBrushProperty =
            DependencyProperty.Register(nameof(MyClickedSolidColorBrush), typeof(SolidColorBrush), typeof(BitmapSourceVisualizerWindow), new PropertyMetadata(null));

        // クリックしたピクセルの色
        public Color MyClickedColor
        {
            get { return (Color)GetValue(MyClickedColorProperty); }
            set { SetValue(MyClickedColorProperty, value); }
        }
        public static readonly DependencyProperty MyClickedColorProperty =
            DependencyProperty.Register(nameof(MyClickedColor), typeof(Color), typeof(BitmapSourceVisualizerWindow), new PropertyMetadata(null));

        public HSV MyClickedHsv
        {
            get { return (HSV)GetValue(MyClickedHsvProperty); }
            set { SetValue(MyClickedHsvProperty, value); }
        }
        public static readonly DependencyProperty MyClickedHsvProperty =
            DependencyProperty.Register(nameof(MyClickedHsv), typeof(HSV), typeof(BitmapSourceVisualizerWindow), new PropertyMetadata(null));



        // 取得した色の表示用（マウスカーソル位置のピクセルの色）
        public SolidColorBrush MySolidColorBrush
        {
            get { return (SolidColorBrush)GetValue(MySolidColorBrushProperty); }
            set { SetValue(MySolidColorBrushProperty, value); }
        }
        public static readonly DependencyProperty MySolidColorBrushProperty =
            DependencyProperty.Register(nameof(MySolidColorBrush), typeof(SolidColorBrush), typeof(Window), new PropertyMetadata(null));

        // 取得した色の表示用（マウスカーソル位置のピクセルの色）
        public Color MyColor
        {
            get { return (Color)GetValue(MyColorProperty); }
            set { SetValue(MyColorProperty, value); }
        }
        public static readonly DependencyProperty MyColorProperty =
            DependencyProperty.Register(nameof(MyColor), typeof(Color), typeof(Window), new PropertyMetadata(Colors.Transparent));


        // マウスカーソル位置の画像のピクセル座標
        public int MyPixelX
        {
            get { return (int)GetValue(MyPixelXProperty); }
            set { SetValue(MyPixelXProperty, value); }
        }
        public static readonly DependencyProperty MyPixelXProperty =
            DependencyProperty.Register(nameof(MyPixelX), typeof(int), typeof(Window), new PropertyMetadata(0));

        public int MyPixelY
        {
            get { return (int)GetValue(MyPixelYProperty); }
            set { SetValue(MyPixelYProperty, value); }
        }
        public static readonly DependencyProperty MyPixelYProperty =
            DependencyProperty.Register(nameof(MyPixelY), typeof(int), typeof(Window), new PropertyMetadata(0));

        public Point MyImageClickPoint
        {
            get { return (Point)GetValue(MyImageClickPointProperty); }
            set { SetValue(MyImageClickPointProperty, value); }
        }
        public static readonly DependencyProperty MyImageClickPointProperty =
            DependencyProperty.Register(nameof(MyImageClickPoint), typeof(Point), typeof(Window), new PropertyMetadata(null));

        // 画像のスケール（表示倍率）
        public double MyImageScale
        {
            get { return (double)GetValue(MyImageScaleProperty); }
            set { SetValue(MyImageScaleProperty, value); }
        }
        public static readonly DependencyProperty MyImageScaleProperty =
            DependencyProperty.Register(nameof(MyImageScale), typeof(double), typeof(BitmapSourceVisualizerWindow), new PropertyMetadata(1.0, OnMyImageScale));

        // スケール変更時
        private static void OnMyImageScale(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 倍率変更時、スクロールの位置調整
            if (d is BitmapSourceVisualizerWindow main)
            {
                double oldScale = (double)e.OldValue;
                double newScale = (double)e.NewValue;
                AdjustOffset2(main.MyScroll, main.MyBitmapSource, oldScale, newScale);

                // ピクセルグリッドの線の幅の更新、
                // 常に1.0になるように調整、最低倍率以下のときは0にして非表示
                if (newScale >= main.MyPixelGridVisibleMinScale)
                {
                    main.MyPixelGridWidth = 1.0 / newScale;
                }
                else
                {
                    main.MyPixelGridWidth = 0.0;
                }

                // スケール後のサイズプロパティ更新
                int scaleW = (int)(main.MyBitmapSource.PixelWidth * newScale);
                int scaleH = (int)(main.MyBitmapSource.PixelHeight * newScale);
                string sizeText = $"{scaleW} x {scaleH}";
                main.MyStatusScaledSize.Text = sizeText;
            }
        }

        #endregion 依存関係プロパティ

        #region 右クリックメニュー

        /// <summary>
        /// 右クリックメニュー作成
        /// </summary>
        /// <returns></returns>
        private ContextMenu CreateContextMenu()
        {
            ContextMenu menu = new();
            MenuItem item = new() { Header = "📃 原寸コピー"};
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    BitmapToPngImageToClipboard(MyBitmapSource);
                }
            };

            item = new() { Header = "📃 表示コピー", ToolTip = "表示倍率のサイズでコピー" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    CopyToClipboardTransfomedImage();
                    //CopyToClipboardMyBitmapSource();
                }
            };

            item = new() { Header = "📃 コピー(見えている部分そのまま)" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    CopyToClipboardExterior(MyScroll);
                }
            };


            menu.Items.Add(new Separator());


            item = new() { Header = "💾 原寸保存(png)" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    MyStatusText.Text = SaveBitmapSource(MyBitmapSource) ? "保存処理完了" : "保存処理はキャンセルされた";
                }
            };

            item = new() { Header = "💾 表示保存(png)" , ToolTip = "表示倍率のサイズで保存" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    SaveTransformedImage(); // 非同期処理バージョン
                    //SaveExteriorToImageFile();
                }
            };

            item = new() { Header = "💾 保存(見えている部分そのまま)(png)" };
            menu.Items.Add(item);
            item.Click += (s, e) => { SaveMyScrollToImageFile(); };


            menu.Items.Add(new Separator());


            item = new() { Header = $"📃 色値コピー（#ARGB）" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    SetTextToClipboard(MyClickedColor.ToString());
                }
            };

            item = new() { Header = $"📃 色値コピー（A, R, G, B）" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                if (MyBitmapSource is not null)
                {
                    string argb = $"{MyClickedColor.A}, {MyClickedColor.R}, {MyClickedColor.G}, {MyClickedColor.B}";
                    SetTextToClipboard(argb);
                }
            };

            menu.Items.Add(new Separator());

            item = new() { Header = $"ウィンドウサイズ小（654 x 367）" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                Width = 654;Height = 367;
            };

            item = new() { Header = $"ウィンドウサイズ中（854 x 647）" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                Width = 854;Height = 647;
            };

            item = new() { Header = $"ウィンドウサイズ大（1140 x 647）" };
            menu.Items.Add(item);
            item.Click += (s, e) =>
            {
                Width = 1140;Height = 647;
            };




            //item = new() { Header = "画像コピー(this)" };
            //menu.Items.Add(item);
            //item.Click += (s, e) =>
            //{
            //    if (MyBitmapSource is not null)
            //    {
            //        CopyToClipboardExterior(this);
            //    }
            //};


            return menu;
        }
        #endregion 右クリックメニュー


        // wpf Clipboard.SetText 応答に時間がかかる at DuckDuckGo
        // https://duckduckgo.com/?q=wpf+Clipboard.SetText+%E5%BF%9C%E7%AD%94%E3%81%AB%E6%99%82%E9%96%93%E3%81%8C%E3%81%8B%E3%81%8B%E3%82%8B&ia=web
        // c# - クリップボードのSetTextは失敗するが、SetDataObjectは失敗する - Stack Overflow
        // https://stackoverflow.com/questions/44538513/clipboard-settext-fails-whereas-setdataobject-does-not?rq=3
        // クリップボードに文字列をコピー（セットする）
        private static void SetTextToClipboard(string text)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    //Clipboard.SetText(text);// これは応答時間が長過ぎる、3秒くらいかかる
                    Clipboard.SetDataObject(text);
                    break;// 成功したらループを抜ける
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(100); // 100ms 待機してリトライ
                }
            }
        }

        #region 画像処理


        private bool SaveBitmapSource(BitmapSource bitmap)
        {
            Microsoft.Win32.SaveFileDialog dialog = new()
            {
                AddExtension = true,
                DefaultExt = "png",
            };

            if (dialog.ShowDialog() == true)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using FileStream stream = new(dialog.FileName, FileMode.Create, FileAccess.Write);
                encoder.Save(stream);
                return true;
            }
            else { return false; }
        }

        //アルファ値を失わずに画像のコピペできた、.NET WPFのClipboard - 午後わてんのブログ
        //        https://gogowaten.hatenablog.com/entry/2021/02/10/134406
        //より
        private async void BitmapToPngImageToClipboard(BitmapSource source)
        {
            MyStatusText.Text = "コピー処理中";
            MyProgressBar.IsIndeterminate = true;
            Mouse.OverrideCursor = Cursors.Wait;

            //画像をPNGにエンコード
            PngBitmapEncoder pngEnc = new();
            pngEnc.Frames.Add(BitmapFrame.Create(source));
            //エンコードした画像をMemoryStreamにSava
            using var ms = new System.IO.MemoryStream();
            pngEnc.Save(ms);

            // 2. 新しいSTAスレッドを作成して、バックグラウンドでクリップボード処理を実行
            bool isSuccess = false;
            Exception exception = null;
            await Task.Run(() =>
            {
                Thread thread = new(() =>
                {
                    try
                    {
                        //MemoryStreamをクリップボードにコピー
                        Clipboard.SetData("PNG", ms);
                        isSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            });

            if (!isSuccess && exception != null)
            {
                MessageBox.Show($"エラーが発生しました: {exception.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                MyStatusText.Text = "コピー処理中にエラー発生";
            }
            else
            {
                MyStatusText.Text = "コピー完了";
            }

            MyMainGrid.IsEnabled = true;
            MyProgressBar.IsIndeterminate = false;
            Mouse.OverrideCursor = null;
        }

        //private static void BitmapToPngImageToClipboard(BitmapSource source)
        //{   
        //    //画像をPNGにエンコード
        //    PngBitmapEncoder pngEnc = new();
        //    pngEnc.Frames.Add(BitmapFrame.Create(source));
        //    //エンコードした画像をMemoryStreamにSava
        //    using var ms = new System.IO.MemoryStream();
        //    pngEnc.Save(ms);
        //    //MemoryStreamをクリップボードにコピー
        //    Clipboard.SetData("PNG", ms);
        //}




        //// 要素からBitmap作成
        //public static RenderTargetBitmap MakeBitmapFromElement(double width, double height, FrameworkElement item)
        //{
        //    int w = (int)width;
        //    int h = (int)height;
        //    double dpi = 96.0 * PresentationSource.FromVisual(item).CompositionTarget.TransformFromDevice.M11;
        //    RenderTargetBitmap bmp = new(w, h, dpi, dpi, PixelFormats.Pbgra32);
        //    bmp.Render(item);
        //    return bmp;
        //}

        // 完成版：要素からBitmap作成、LayoutTransformによる回転拡大対応
        public RenderTargetBitmap MakeBitmapFromLayoutTransformElement(FrameworkElement element)
        {
            double dpi = 96.0 * PresentationSource.FromVisual(element).CompositionTarget.TransformFromDevice.M11;
            Rect bounds = new(0, 0, element.ActualWidth, element.ActualHeight); // 元のBounds
            Rect ltBounds = element.LayoutTransform.TransformBounds(bounds); // 変形後のBounds
            DrawingVisual dv = new();
            dv.Offset = new Vector(-ltBounds.X, -ltBounds.Y);
            RenderOptions.SetBitmapScalingMode(dv, BitmapScalingMode.NearestNeighbor);// 無意味
            RenderOptions.SetEdgeMode(dv, EdgeMode.Aliased);// 無意味

            VisualBrush brush = new(element)
            {
                Stretch = Stretch.None,
                //ViewportUnits = BrushMappingMode.Absolute,// 完全透明画像になる
                //ViewboxUnits = BrushMappingMode.Absolute,// 描画位置が違う
            };
            RenderOptions.SetBitmapScalingMode(brush, BitmapScalingMode.NearestNeighbor);// 無意味
            RenderOptions.SetEdgeMode(brush, EdgeMode.Aliased);// 無意味

            using (DrawingContext context = dv.RenderOpen())
            {
                context.DrawRectangle(brush, null, ltBounds);
                context.PushTransform(element.LayoutTransform);
            }
            RenderTargetBitmap bmp =
                new(MyCeiling(ltBounds.Width), MyCeiling(ltBounds.Height), dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(dv);
            return bmp;
        }

        // 最近傍補間でBitmapSourceを拡大したBitmapを返す
        // DrawingGroupを使う、これに最近傍補間でスケールしてBitmapSourceを描画したものを
        // DrawingVisualのDrawDrawingで描画、これで最近傍補間になる
        // もしDrawingGroupを使わずにDrawingVisual最近傍補間でDrawVisualでびょうがしても最近傍補間にはならない
        public RenderTargetBitmap MakeScaleBitmap(BitmapSource originalBitmap, double scale)
        {
            // 1. 拡大後のサイズを計算
            int newWidth = (int)(originalBitmap.PixelWidth * scale);
            int newHeight = (int)(originalBitmap.PixelHeight * scale);

            DrawingGroup drawingGroup = new();
            // 最近傍補間を設定
            RenderOptions.SetBitmapScalingMode(drawingGroup, BitmapScalingMode.NearestNeighbor);
            using (DrawingContext drawingContext = drawingGroup.Open())
            {
                // 2倍のサイズに引き伸ばして描画
                drawingContext.DrawImage(originalBitmap, new Rect(0, 0, newWidth, newHeight));
            }

            // 2. 描き込み用の Visual を作成
            DrawingVisual drawingVisual = new();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                context.DrawDrawing(drawingGroup);
            }

            // 3. RenderTargetBitmap に焼き付ける
            RenderTargetBitmap renderTargetBitmap = new(
                newWidth,
                newHeight,
                originalBitmap.DpiX,
                originalBitmap.DpiY,
                PixelFormats.Pbgra32);

            renderTargetBitmap.Render(drawingVisual);
            renderTargetBitmap.Freeze();
            return renderTargetBitmap;
        }


        //public RenderTargetBitmap MakeBitmapFromLayoutTransformElement2(FrameworkElement element)
        //{
        //    double dpi = 96.0 * PresentationSource.FromVisual(element).CompositionTarget.TransformFromDevice.M11;
        //    Rect bounds = new(0, 0, element.ActualWidth, element.ActualHeight); // 元のBounds
        //    Rect ltBounds = element.LayoutTransform.TransformBounds(bounds); // 変形後のBounds
        //    DrawingVisual dv = new();
        //    dv.Offset = new Vector(-ltBounds.X, -ltBounds.Y);

        //    RenderOptions.SetBitmapScalingMode(dv, BitmapScalingMode.NearestNeighbor);// 無意味
        //    RenderOptions.SetEdgeMode(dv, EdgeMode.Aliased);// 無意味

        //    using (DrawingContext context = dv.RenderOpen())
        //    {
        //        BitmapCacheBrush bcBrush = new(element)
        //        {
        //            BitmapCache = new() { EnableClearType = false, SnapsToDevicePixels = true },// 無意味
        //            AutoLayoutContent = true,// 無意味

        //        };

        //        RenderOptions.SetBitmapScalingMode(bcBrush, BitmapScalingMode.NearestNeighbor);// 無意味
        //        RenderOptions.SetEdgeMode(bcBrush, EdgeMode.Aliased);// 無意味
        //        context.DrawRectangle(bcBrush, null, ltBounds);
        //    }
        //    RenderTargetBitmap bmp =
        //        new(MyCeiling(ltBounds.Width), MyCeiling(ltBounds.Height), dpi, dpi, PixelFormats.Pbgra32);
        //    bmp.Render(dv);
        //    return bmp;
        //}


        // doubleを切り上げてintに変換
        public static int MyCeiling(double value)
        {
            return (int)Math.Ceiling(value);
        }

        #endregion 画像処理

        #region ボタンクリック

        private void ButtonCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource is not null)
            {
                BitmapToPngImageToClipboard(MyBitmapSource);
            }
        }



        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            MyStatusText.Text = SaveBitmapSource(MyBitmapSource) ? "保存処理完了" : "保存処理はキャンセルされた";
        }




        private void ButtonSetScale_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int scale))
            {
                MyImageScale = scale;
            }
        }

        private void ButtonSetMathScale_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && double.TryParse(button.Tag.ToString(), out double value))
            {
                var scale = MyImageScale * value;
                if (scale <= 1.0)
                {
                    MyImageScale = GetClampedImageScale(scale);
                }
                else
                {
                    MyImageScale = GetClampedImageScale(MyCeiling(scale));
                }
            }
        }

        // 処理中のUI状態をセットするヘルパーメソッド

        private void SetUiState(string statusText)
        {
            MyStatusText.Text = statusText; // 処理中表示メッセージ
            MyProgressBar.IsIndeterminate = true;
            Mouse.OverrideCursor = Cursors.Wait;
            MyMainGrid.IsEnabled = false; // 二重クリック防止など
        }

        /// <summary>
        /// LayoutTransformによる変形後の要素からBitmap作成して、非同期処理でクリップボードにコピー
        /// </summary>
        private async void CopyToClipboardTransfomedImage()
        {
            // 1. 最初にUIスレッド側でプログレスバーやカーソルを設定
            SetUiState("コピー処理中");

            try
            {
                // 2. メモリチェックや確認（UI要素が絡むためUIスレッドで行う）
                if (!CheckMemoryAndConfirmConsent(ImageControl))
                {
                    // キャンセルされた場合は元に戻す
                    ResetUiState();
                    return;
                }

                // 1. 【非常に重要】UIスレッド側にいる時点で、Dispatcherをローカル変数に保持しておく
                // Visualizerとして使う場合はApplication.Current.Dispatcherだと
                // 取得されるDispatcherはVisual StudioのDispatcherになってしまうので、
                // ここで自身のDispatcherを取得しておく必要がある
                Dispatcher dispatcher = this.Dispatcher;

                // 現在のスケール値をローカル変数にコピー（スレッド間安全のため）
                double scale = MyImageScale;

                // sourceToProcess や scale の準備
                BitmapSource sourceToProcess = MyBitmapSource;
                if (sourceToProcess != null && !sourceToProcess.IsFrozen && sourceToProcess.CanFreeze)
                {
                    sourceToProcess = sourceToProcess.Clone();
                    sourceToProcess.Freeze();
                }


                // 2. バックグラウンド処理を開始
                await Task.Run(async () =>
                {
                    // sourceToProcess が null の場合はエラーを回避する
                    if (sourceToProcess == null)
                    {
                        throw new InvalidOperationException("コピー元の画像データがありません。");
                    }

                    BitmapSource scaledSource = null;

                    // 3. 事前に取得しておいた dispatcher を使ってUIスレッドを呼び出すのは、
                    // BitmapSource系の処理はバックグラウンドスレッドでは扱えないため
                    await dispatcher.InvokeAsync(() =>
                    {
                        // 最近傍補間の効いた拡大ビットマップをUIスレッド側で作成
                        scaledSource = MakeScaleBitmap(sourceToProcess, scale);
                    });


                    // 念のため、生成されたビットマップが null でないかチェック
                    if (scaledSource == null)
                    {
                        throw new InvalidOperationException("画像の拡大処理に失敗しました。");
                    }

                    // 4. PNGエンコード（ここからはバックグラウンドなのでプログレスバーは止まらない）
                    PngBitmapEncoder pngEnc = new();
                    pngEnc.Frames.Add(BitmapFrame.Create(scaledSource));

                    using var ms = new System.IO.MemoryStream();
                    pngEnc.Save(ms);

                    // 5. クリップボードへの書き込み（STAスレッド）
                    TaskCompletionSource<bool> tcs = new();
                    //Exception threadException = null;

                    Thread thread = new(() =>
                    {
                        try
                        {
                            ms.Position = 0;
                            Clipboard.SetData("PNG", ms);
                            tcs.SetResult(true);// タスクを成功として完了する
                        }
                        catch (Exception ex)
                        {
                            //threadException = ex;
                            tcs.SetException(ex);// タスクを失敗として完了する
                        }
                    });

                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();

                    // thread.Join() の代わりに TaskCompletionSource を await すると
                    // バックグラウンドスレッドの効率が良くなり、フリーズのリスクを減らせます
                    await tcs.Task;
                });

                MyStatusText.Text = "コピー完了";
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                MyStatusText.Text = "コピー処理中にエラー発生";
            }
            finally
            {
                // 5. 成功・失敗に関わらずUIの状態を元に戻す
                ResetUiState();
            }
        }

        private void ButtonCopyToClipboardExterior_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboardTransfomedImage();
        }

        #region 非同期コピー処理過去
        //private async void ButtonCopyToClipboardExterior_Click(object sender, RoutedEventArgs e)
        //{
        //    //CopyToClipboardMyBitmapSource();
        //    // 1. 最初にUIスレッド側でプログレスバーやカーソルを設定
        //    MyStatusText.Text = "コピー処理中";
        //    MyProgressBar.IsIndeterminate = true;
        //    Mouse.OverrideCursor = Cursors.Wait;
        //    MyMainGrid.IsEnabled = false; // 二重クリック防止など

        //    try
        //    {
        //        // 2. メモリチェックや確認（UI要素が絡むためUIスレッドで行う）
        //        if (!CheckMemoryAndConfirmConsent(ImageControl))
        //        {
        //            // キャンセルされた場合は元に戻す
        //            ResetUiState();
        //            return;
        //        }

        //        // 3. 元の画像データを取得し、別スレッドで扱えるようにフリーズする
        //        // ※MyBitmapSourceがUIスレッド依存の場合、ここでFreezeしておきます
        //        BitmapSource sourceToProcess = MyBitmapSource;
        //        if (!sourceToProcess.IsFrozen && sourceToProcess.CanFreeze)
        //        {
        //            sourceToProcess = sourceToProcess.Clone();
        //            sourceToProcess.Freeze();
        //        }

        //        // 現在のスケール値をローカル変数にコピー（スレッド間安全のため）
        //        double scale = MyImageScale;

        //        // 4. 重い処理（リサイズ、エンコード、クリップボード転送）を丸ごと非同期(Task.Run)で実行
        //        await Task.Run(async () =>
        //        {
        //            // A. 画像のスケール・エンコード処理
        //            // ※MakeScaleBitmap内で新しいBitmapを作成する場合、
        //            //   そのメソッド内で新しいBitmapをFreeze()するか、スレッド依存に注意してください
        //            //BitmapSource scaledSource = MakeScaleBitmap(sourceToProcess, scale);
        //            BitmapSource scaledSource = null;

        //            // 拡大処理(MakeScaleBitmap)だけをUIスレッドに一時的に投げる
        //            await Application.Current.Dispatcher.InvokeAsync(() =>
        //            {
        //                scaledSource = MakeScaleBitmap(sourceToProcess, scale);
        //                // MakeScaleBitmapの最後で.Freeze()しているので、ここで別スレッドに渡せるようになります
        //            });

        //            PngBitmapEncoder pngEnc = new();
        //            pngEnc.Frames.Add(BitmapFrame.Create(scaledSource));

        //            using var ms = new System.IO.MemoryStream();
        //            pngEnc.Save(ms);

        //            // B. クリップボードへの書き込み（STAスレッドが必要）
        //            Thread thread = new(() =>
        //            {
        //                // MemoryStreamは position 0 から読み込める状態にする
        //                ms.Position = 0;
        //                Clipboard.SetData("PNG", ms);
        //            });

        //            thread.SetApartmentState(ApartmentState.STA);
        //            thread.Start();
        //            thread.Join(); // STAスレッドの完了を待つ
        //        });

        //        MyStatusText.Text = "コピー完了";
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        //        MyStatusText.Text = "コピー処理中にエラー発生";
        //    }
        //    finally
        //    {
        //        // 5. 成功・失敗に関わらずUIの状態を元に戻す
        //        ResetUiState();
        //    }

        //}

        #endregion 過去



        // UI状態をリセットするヘルパーメソッド
        private void ResetUiState()
        {
            MyMainGrid.IsEnabled = true;
            MyProgressBar.IsIndeterminate = false;
            Mouse.OverrideCursor = null;
        }

        /// <summary>
        /// 非同期処理で拡大画像を保存
        /// </summary>
        private async void SaveTransformedImage()
        {
            SetUiState("ファイルに保存中…");

            try
            {
                if (!CheckMemoryAndConfirmConsent(ImageControl))
                {
                    ResetUiState();
                    return;
                }

                Dispatcher dispatcher = this.Dispatcher;
                double scale = MyImageScale;
                bool saveResult = false;

                BitmapSource sourceToProcess = MyBitmapSource;
                if (sourceToProcess != null && !sourceToProcess.IsFrozen && sourceToProcess.CanFreeze)
                {
                    sourceToProcess = sourceToProcess.Clone();
                    sourceToProcess.Freeze();
                }

                await Task.Run(async () =>
                {
                    if (sourceToProcess is null)
                    {
                        throw new InvalidOperationException("元の画像がない！");
                    }

                    BitmapSource scaledSource = null;

                    await dispatcher.InvokeAsync(() =>
                    {
                        scaledSource = MakeScaleBitmap(sourceToProcess, scale);
                    });

                    if (scaledSource is null)
                    {
                        throw new InvalidOperationException("画像の拡大処理に失敗した");
                    }

                    // 保存処理はバックグラウンドスレッド
                    TaskCompletionSource<bool> tcs = new();
                    //Exception threadException = null;
                    Thread thread = new(() =>
                    {
                        try
                        {
                            saveResult = SaveBitmapSource(scaledSource);
                            tcs.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            //threadException = ex;
                            tcs.SetException(ex);
                        }
                    });

                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    await tcs.Task;
                });
                if (saveResult)
                {
                    MyStatusText.Text = "保存処理完了";
                }
                else
                {
                    MyStatusText.Text = "保存処理はキャンセルされた";
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                MyStatusText.Text = "保存処理中にエラー発生";
            }
            finally
            {
                ResetUiState();
            }
        }

        // 変形後の要素を画像としてファイルに保存
        private void ButtonSaveExterior_Click(object sender, RoutedEventArgs e)
        {
            //SaveExteriorToImageFile();
            SaveTransformedImage();
        }

        private void Button_ClickCopyClickedPixelARGB(object sender, RoutedEventArgs e)
        {
            string argb = $"{MyClickedColor.A}, {MyClickedColor.R}, {MyClickedColor.G}, {MyClickedColor.B}";
            SetTextToClipboard(argb);
        }
        private void Button_ClickCopyClickedPixelHex(object sender, RoutedEventArgs e)
        {
            SetTextToClipboard(MyClickedColor.ToString());
        }

        private void RepeatButton_ClickDown(object sender, RoutedEventArgs e)
        {
            ChangeScaleWithMouseWheel(-1);
        }

        private void RepeatButton_ClickUp(object sender, RoutedEventArgs e)
        {
            ChangeScaleWithMouseWheel(1);
        }


        #endregion ボタンクリック

        private double GetClampedImageScale(double scale)
        {
            return Clamp(scale, ImageScaleMin, ImageScaleMax);
        }

        // クランプ。値を上限下限内に収めて返す
        // パターン1：NaN があれば NaN を返す（標準的な挙動）
        private static double Clamp(double value, double min, double max)
        {
            // いずれかの引数が NaN の場合は NaN を返す
            if (double.IsNaN(value) || double.IsNaN(min) || double.IsNaN(max))
            {
                return double.NaN;
            }

            if (min > max) { (max, min) = (min, max); }

            double result = value;
            if (value < min) { result = min; }
            else if (value > max) { result = max; }
            return result;
        }



        // LayoutTransformによる変形後の要素からBitmap作成して、クリップボードにコピー
        public void CopyToClipboardExterior(FrameworkElement element)
        {
            // 処理に伴うメモリ量と処理続行の確認してから
            if (CheckMemoryAndConfirmConsent(element))
            {
                //BitmapToPngImageToClipboard(MakeBitmapFromLayoutTransformElement2(element));
                BitmapToPngImageToClipboard(MakeBitmapFromLayoutTransformElement(element));
                //BitmapToPngImageToClipboard(MakeScaleImageBitmap());
                //BitmapToPngImageToClipboard(MakeScaleBitmap(MyBitmapSource, MyImageScale));
            }
        }


        //private void SaveExteriorToImageFile()
        //{
        //    // 処理に伴うメモリ量と処理続行の確認してから
        //    if (CheckMemoryAndConfirmConsent(ImageControl))
        //    {
        //        SaveBitmapSource(MakeScaleBitmap(MyBitmapSource, MyImageScale));
        //        //SaveBitmapSource(MakeBitmapFromLayoutTransformElement(MyMainGrid));
        //        //SaveBitmapSource(MakeBitmapFromLayoutTransformElement(ImageControl));
        //    }
        //}

        private void SaveMyScrollToImageFile()
        {
            //SaveBitmapSource(MakeBitmapFromLayoutTransformElement(MyScroll));
            MyStatusText.Text = SaveBitmapSource(MakeBitmapFromLayoutTransformElement(MyScroll))
                ? "保存処理完了" : "保存処理はキャンセルされた";
        }



        #region チェック系

        /// <summary>
        /// 要素のBitmap化の処理で1GB以上のメモリを使用する場合にtrueを返す
        /// LayoutTransformにより変形された要素に対応
        /// RenderTransformにより変形された要素には未対応
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static bool IsOver1GigaByte(FrameworkElement element)
        {
            Rect bounds = new(0, 0, element.ActualWidth, element.ActualHeight); // 元のBounds
            var ltBounds = element.LayoutTransform.TransformBounds(bounds); // 変形後のBounds

            if (ltBounds.Width * ltBounds.Height * 4 > 1000 * 1000 * 1000)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 要素のBitmap化で1GB以上のメモリを使用する場合の処理続行の確認
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static bool CheckMemoryAndConfirmConsent(FrameworkElement element)
        {
            if (IsOver1GigaByte(element))
            {
                if (MessageBox.Show("使用メモリが1GBを超えるけど、処理続行する？", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    return true;
                }
                else { return false; }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 空きメモリ容量取得、MB
        /// </summary>
        /// <returns></returns>
        public float GetFreeMemoryCapacity()
        {
            float result = 0;
            using (System.Diagnostics.PerformanceCounter ramcounter = new("Memory", "Available MBytes"))
            {
                float availableMemoryMB = ramcounter.NextValue();
                result = availableMemoryMB;
                Debug.WriteLine($"空きメモリ：{availableMemoryMB} MB");
            }
            return result;
        }
        #endregion チェック系

        #region マウスホイールでの倍率変更

        // マウスホイールでの倍率変更
        private void MyScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ChangeScaleWithMouseWheel(e.Delta);

            e.Handled = true;
        }

        // マウスホイールでの倍率変更
        private bool ChangeScaleWithMouseWheel(int delta)
        {
            // 今の倍率と増減値
            // 20～：10
            // 1～20：1
            // 0.01～1：2倍、半分
            var resultScale = MyImageScale;
            // 拡大時
            if (delta > 0)
            {
                // 今の倍率が
                // 1以上なら+1
                // 1未満なら2倍にする
                if (resultScale >= 1) { resultScale++; }
                else if (resultScale > 0.5) { resultScale = (int)(resultScale * 2.0); }
                else { resultScale *= 2; }
            }
            // 縮小時
            else
            {
                // 今の倍率が
                // 2以上なら-1してから小数以下を切り捨て
                if (resultScale >= 2) { resultScale = (int)(resultScale - 1.0); }
                // 2未満1より大きいなら1.0にする
                else if (resultScale > 1) { resultScale = 1.0; }
                // 1以下なら半分にする
                else { resultScale = resultScale / 2.0; }

            }
            var clamped = GetClampedImageScale(resultScale);
            if (clamped == MyImageScale) { return false; }
            MyImageScale = clamped;
            return true;

        }


        #endregion マウスホイールでの倍率変更

        #region クリック系

        // マウスクリック時
        private void ImageControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // クリック位置と色を記録
            int px = (int)MyImageClickPoint.X;
            int py = (int)MyImageClickPoint.Y;
            if (px >= MyBitmapSource.PixelWidth) { px = MyBitmapSource.PixelWidth - 1; }
            if (py >= MyBitmapSource.PixelHeight) { py = MyBitmapSource.PixelHeight - 1; }
            MyClickedPixelX = px;
            MyClickedPixelY = py;
            MyClickedColor = GetPixelColor(MyBitmapSource, px, py);

        }


        // 左クリックダウン時
        // 通常のMouseDownでは反応しないので、Preview版でクリック座標を記録
        private void ImageControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyPoint = e.GetPosition(this);
            ImageControl.CaptureMouse();
        }

        // 左クリックアップ時
        private void ImageControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ImageControl.Cursor = Cursors.Arrow;
            ImageControl.ReleaseMouseCapture();

            IsImageDrag = false;
            //描画更新、OnRenderが実行される
            MyDraw.InvalidateVisual();
        }

        #endregion クリック系


        #region マウスドラッグ移動でスクロールバーを移動させる



        //      WPF、ScrollViewerの中の要素をマウスドラッグ移動しているように見せかける - 午後わてんのブログ
        //https://gogowaten.hatenablog.com/entry/15755956


        // ドラッグ移動時
        // マウスの移動距離をスクロールバーに加算する
        private void ImageControl_MouseMove(object sender, MouseEventArgs e)
        {
            //マウスドラッグ移動の距離だけスクロールさせるには
            //(直前のカーソル位置 - 今のカーソル位置) + (スクロールバーのOffset位置)
            //この値をSetOffsetする
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // 一時的にテキストARGBを非表示することで、向こうのフラグで描画を停止させる
                IsImageDrag = true;

                ImageControl.Cursor = Cursors.ScrollAll;//カーソル形状を変更
                //今のマウスの座標
                var nowPoint = e.GetPosition(this);
                //マウスの移動距離＝直前の座標と今の座標の差
                var xd = MyPoint.X - nowPoint.X;
                var yd = MyPoint.Y - nowPoint.Y;
                //xd *= 2;//2倍速
                //yd *= 2;

                //移動距離＋今のスクロール位置
                xd += MyScroll.HorizontalOffset;
                yd += MyScroll.VerticalOffset;

                //スクロール位置の指定
                MyScroll.ScrollToHorizontalOffset(xd);
                MyScroll.ScrollToVerticalOffset(yd);

                MyPoint = nowPoint;//直前の座標を今の座標に変更
            }

            // マウスカーソル位置を記録してから、カーソル位置のピクセルの色を取得
            MyImageClickPoint = e.GetPosition(ImageControl);
            UpdateMyColor();
        }

        #endregion マウスドラッグ移動でスクロールバーを移動させる

        #region 色の取得

        // WPFとVB.NET、表示した画像をクリックした場所の色の取得はややこしい - 午後わてんのブログ
        // https://gogowaten.hatenablog.com/entry/13952774
        // カーソル位置のピクセルの色を取得
        private void UpdateMyColor()
        {
            int px = (int)MyImageClickPoint.X;
            int py = (int)MyImageClickPoint.Y;
            if (px >= MyBitmapSource.PixelWidth) { px = MyBitmapSource.PixelWidth - 1; }
            if (py >= MyBitmapSource.PixelHeight) { py = MyBitmapSource.PixelHeight - 1; }

            if (px != MyPixelX || py != MyPixelY)
            {
                MyPixelX = px; // マウスカーソル位置のピクセル座標
                MyPixelY = py;
                MyColor = GetPixelColor(MyBitmapSource, MyPixelX, MyPixelY);
                MySolidColorBrush = new SolidColorBrush(MyColor);
            }
        }

        // 指定したピクセル座標の色を返す
        private static Color GetPixelColor(BitmapSource bmp, int x, int y)
        {
            CroppedBitmap cropBmp = new(bmp, new Int32Rect(x, y, 1, 1));
            FormatConvertedBitmap bgraBmp = new(cropBmp, PixelFormats.Bgra32, null, 0);
            byte[] pixels = new byte[40];
            bgraBmp.CopyPixels(pixels, 4, 0);
            return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
        }

        #endregion 色の取得

        #region 拡大率変更時にスクロール位置固定


        // 倍率変更時、スクロールの位置を固定する
        private static void AdjustOffset2(ScrollViewer scroll, BitmapSource bmp, double oldScale, double newScale)
        {
            // スクロールバーの幅を取得
            double oldSize = bmp.PixelWidth * oldScale;
            double oldMaxOffset = oldSize - scroll.ActualWidth;
            double scrollBarWidth = scroll.ScrollableWidth - oldMaxOffset;
            // スクロールバーの幅を取得には下記で良いはずなんだけど、何故かずれる
            //double scrollBarWidth = SystemParameters.HorizontalScrollBarHeight;

            // 横スクロール位置
            // 倍率変更前のスクロール位置（割合）を取得
            double rateOffset = 0;
            if (scroll.ScrollableWidth > 0)
            {
                rateOffset = scroll.HorizontalOffset / scroll.ScrollableWidth;
            }

            // 倍率変更後のスクロールの最大値を計算
            // 最大値 = (画像サイズ) - (ScrollViewerサイズ)
            double newSize = bmp.PixelWidth * newScale;
            double maxOffset = newSize - scroll.ActualWidth + scrollBarWidth;

            // スクロール位置設定
            // スクロール位置 = 最大値 * 割合
            if (maxOffset > 0)
            {
                //MessageBox.Show($"scrollBarWidth = {scrollBarWidth}{Environment.NewLine}" +
                //    $"HorizontalScrollBarHeight = {SystemParameters.HorizontalScrollBarHeight}");
                double pos = maxOffset * rateOffset;
                scroll.ScrollToHorizontalOffset(pos);
            }

            // 縦スクロール位置
            newSize = bmp.PixelHeight * newScale;
            oldSize = bmp.PixelHeight * oldScale;
            oldMaxOffset = oldSize - scroll.ActualHeight;
            scrollBarWidth = scroll.ScrollableHeight - oldMaxOffset;

            rateOffset = 0;
            if (scroll.ScrollableHeight > 0)
            {
                rateOffset = scroll.VerticalOffset / scroll.ScrollableHeight;
            }

            maxOffset = newSize - scroll.ActualHeight + scrollBarWidth;
            if (maxOffset > 0)
            {
                double pos = maxOffset * rateOffset;
                scroll.ScrollToVerticalOffset(pos);
            }
        }

        //private static void AdjustOffset(ScrollViewer scroll, BitmapSource bmp, double scale, int currentXPos, int currentYPos)
        //{
        //    var bmpViewSize = bmp.PixelWidth * scale;
        //    var maxOffset = bmpViewSize - scroll.ActualWidth;
        //    if (maxOffset > 0)
        //    {
        //        double ratePos = currentXPos / (double)bmp.PixelWidth;
        //        double pos = maxOffset * ratePos;
        //        //MessageBox.Show($"maxOffset = {maxOffset}{Environment.NewLine}" + 
        //        //    $" ratePos = {ratePos}" + 
        //        //    $" currentXPos = {currentXPos}" +
        //        //    $" MyPixelXProperty = {MyPixelXProperty}");
        //        scroll.ScrollToHorizontalOffset(pos);

        //    }

        //    bmpViewSize = bmp.PixelHeight * scale;
        //    maxOffset = bmpViewSize - scroll.ActualHeight;
        //    if (maxOffset > 0)
        //    {
        //        double ratePos = currentYPos / (double)bmp.PixelHeight;
        //        double pos = maxOffset * ratePos;
        //        scroll.ScrollToVerticalOffset(pos);
        //    }
        //}


        #endregion 拡大率変更時にスクロール位置固定

        private void MyScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (ImageControl.ActualHeight == 0 || ImageControl.ActualWidth == 0) { return; }

            // ナビ枠更新
            NaviWaku();

            //描画更新、OnRenderが実行される
            MyDraw.InvalidateVisual();
        }



        private void NaviWaku()
        {
            // スクロール位置の割合、スクロールバーなしなら0
            double rateScrollX = MyScroll.ScrollableWidth == 0 ? 0 : MyScroll.HorizontalOffset / MyScroll.ScrollableWidth;
            double rateScrollY = MyScroll.ScrollableHeight == 0 ? 0 : MyScroll.VerticalOffset / MyScroll.ScrollableHeight;

            // ナビ枠の位置の最小値
            double zeroXPos = (MiniMapCanvas.Width - MiniMapImage.ActualWidth) / 2.0;
            double zeroYPos = (MiniMapCanvas.Height - MiniMapImage.ActualHeight) / 2.0;

            // スケール後の画像サイズ            
            double scaledImageWidth = MyBitmapSource.PixelWidth * MyImageScale;
            double scaledImageHeight = MyBitmapSource.PixelHeight * MyImageScale;

            // ScrollViewer内に見えている部分のサイズ
            double viewWidth = MyScroll.ViewportWidth;
            double viewHeight = MyScroll.ViewportHeight;

            // ScrollViewer内に見えている部分（Viewport）の画像サイズの全体からの比率
            // 見えている部分 / 全体サイズ
            double rateViewWidth = viewWidth / scaledImageWidth;
            double rateViewHeight = viewHeight / scaledImageHeight;

            // ナビ枠のサイズ決定
            double naviWidth = MiniMapImage.ActualWidth * rateViewWidth;
            if (rateViewWidth > 1.0) { naviWidth = MiniMapImage.ActualWidth; }
            ViewBoundsRect.Width = naviWidth;
            double naviHeight = MiniMapImage.ActualHeight * rateViewHeight;
            if (rateViewHeight > 1.0) { naviHeight = MiniMapImage.ActualHeight; }
            ViewBoundsRect.Height = naviHeight;

            // スクロール最大幅
            double maxScrollX = MiniMapImage.ActualWidth - ViewBoundsRect.Width;
            double maxScrollY = MiniMapImage.ActualHeight - ViewBoundsRect.Height;

            // スクロール可動範囲の比率を記録は、枠移動で使うため
            // ScrollViewer / ナビ枠
            MyRateMoveX = MyScroll.ScrollableWidth / maxScrollX;
            MyRateMoveY = MyScroll.ScrollableHeight / maxScrollY;

            // ナビ枠の位置を指定
            double xPos = zeroXPos + maxScrollX * rateScrollX;
            double yPos = zeroYPos + maxScrollY * rateScrollY;
            Canvas.SetLeft(ViewBoundsRect, xPos);
            Canvas.SetTop(ViewBoundsRect, yPos);

        }


        #region 枠移動
        // 実質は枠移動じゃなくてスクロール調整

        // MiniMapCanvasクリック時、
        // クリック位置に枠が来るようにスクロール位置を調整する
        private void MiniMapCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsImageDrag = true;
            IsMouseDraged = true;
            MyClickedPoint = e.GetPosition(MiniMapCanvas);
            // CuptureMouseは実行するとMouseMoveイベントが発行されるのでGetPositionより後にする
            MiniMapCanvas.CaptureMouse();

            // 横スクロールバーの位置調整
            if (MyScroll.ScrollableWidth > 0)
            {
                // 枠内クリックの場合は調整しない
                // スクロール位置だけ記録する
                if (MiniMapCanvas.InputHitTest(MyClickedPoint) is Rectangle)
                {
                    MyClickedScrollXOffset = MyScroll.HorizontalOffset;
                    return;
                }

                // ここからスクロール位置調整処理
                // クリック位置と枠の位置の差
                var xDiff = MyClickedPoint.X - Canvas.GetLeft(ViewBoundsRect);

                // スクロール位置 = 今のスクロール位置 + （位置の差 * ScrollViewerと枠サイズの率）
                double xOffset = MyScroll.HorizontalOffset + (xDiff * MyRateMoveX);
                // クリック位置に枠の中心を合わせる
                xOffset -= ViewBoundsRect.Width * MyRateMoveX / 2.0;
                //xOffset -= ViewBoundsRect.ActualWidth * MyRateMoveX / 2.0;

                // スクロール位置（値）をクランプしてセット
                xOffset = Clamp(xOffset, 0, MyScroll.ScrollableWidth);
                MyScroll.ScrollToHorizontalOffset(xOffset);

                // スクロール位置を記録しておく
                MyClickedScrollXOffset = xOffset;
            }

            if (MyScroll.ScrollableHeight > 0)
            {
                if (MiniMapCanvas.InputHitTest(MyClickedPoint) is Rectangle)
                {
                    MyClickedScrollYOffset = MyScroll.VerticalOffset;
                    return;
                }

                var yDiff = MyClickedPoint.Y - Canvas.GetTop(ViewBoundsRect);

                double yOffset = MyScroll.VerticalOffset + (yDiff * MyRateMoveY);
                yOffset -= ViewBoundsRect.Height * MyRateMoveY / 2.0;
                //yOffset -= ViewBoundsRect.ActualHeight * MyRateMoveY / 2.0;

                yOffset = Clamp(yOffset, 0, MyScroll.ScrollableHeight);
                MyScroll.ScrollToVerticalOffset(yOffset);

                MyClickedScrollYOffset = yOffset;
            }


        }

        // ナビ上でのマウス移動時のスクロールバー位置調整
        private void MiniMapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // スクロールバーなしなら何もしない
            if (MyScroll.ScrollableHeight == 0 || MyScroll.ScrollableWidth == 0) { return; }

            // ドラッグ移動フラグ確認、無ければ何もしない
            if (IsMouseDraged == false) { return; }

            // マウス移動距離、0なら何もしない、終わり
            var pos = e.GetPosition(MiniMapCanvas);
            var horizontalChanged = pos.X - MyClickedPoint.X;
            var verticalChanged = pos.Y - MyClickedPoint.Y;
            if (horizontalChanged == 0 && verticalChanged == 0) { return; }

            // マウス移動していた場合、スクロール位置調整
            else
            {
                // スクロール位置計算
                // スクロール位置 = クリックした時の位置 + （マウス移動距離 * 比率）
                double xOffset = MyClickedScrollXOffset + (horizontalChanged * MyRateMoveX);
                double yOffset = MyClickedScrollYOffset + (verticalChanged * MyRateMoveY);

                // スクロール位置（値）をクランプしてセット
                xOffset = Clamp(xOffset, 0, MyScroll.ScrollableWidth);
                yOffset = Clamp(yOffset, 0, MyScroll.ScrollableHeight);
                MyScroll.ScrollToHorizontalOffset(xOffset);
                MyScroll.ScrollToVerticalOffset(yOffset);
            }
        }

        // クリック離した時
        private void MiniMapCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // ドラッグ移動フラグ解除
            IsImageDrag = false;
            MyDraw.InvalidateVisual();

            IsMouseDraged = false;
            MiniMapCanvas.ReleaseMouseCapture();
        }



        #endregion 枠移動

        // ナビ上でのマウスホイール操作で拡大率変化
        private void MiniMapImage_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _ = ChangeScaleWithMouseWheel(e.Delta);
        }

        // ナビ枠上でのマウスホイール操作で拡大率変化
        private void ViewBoundsRect_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ChangeScaleWithMouseWheel(e.Delta)) { e.Handled = true; }
        }

        private void Button_ClickChangeDrawText(object sender, RoutedEventArgs e)
        {
            var type = MyDraw.MyDrawTextType;
            if (type == DrawTextType.None)
            {
                MyDraw.MyDrawTextType = DrawTextType.RGBA;
            }
            else if (type == DrawTextType.RGBA)
            {
                MyDraw.MyDrawTextType = DrawTextType.HSVA;
            }
            else if (type == DrawTextType.HSVA)
            {
                MyDraw.MyDrawTextType = DrawTextType.None;
            }
        }

        //private void Button_ClickChangeWindowSize(object sender, RoutedEventArgs e)
        //{
        //    // 854x647 default  4:3
        //    // 654x367 min      16:9 min
        //    // 1140x647 large   16:9
        //    if (Width == 654 && Height == 367) { Width = 854; Height = 647; }
        //    else if (Width == 854 && Height == 647) { Width = 1140; Height = 647; }
        //    else if (Width == 1140 && Height == 647) { Width = 654; Height = 367; }
        //    else { Width = 854; Height = 647; }
        //}

        //private void Button_ClickWindowSmall(object sender, RoutedEventArgs e)
        //{
        //    Width = 654; Height = 367;
        //}

        //private void Button_ClickWindowMiddle(object sender, RoutedEventArgs e)
        //{
        //    Width = 854;Height = 647;
        //}

        //private void Button_ClickWindowLarge(object sender, RoutedEventArgs e)
        //{
        //    Width = 1140;Height = 647;
        //}
    }
}
