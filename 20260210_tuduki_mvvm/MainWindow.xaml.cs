using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260210_tuduki_mvvm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point startPoint;

        public MainWindow()
        {
            InitializeComponent();

            MainCanvas.MouseDown += (s, e) =>
            {
                if (e.Source is Canvas canvas)
                {
                    foreach (var child in canvas.Children.OfType<DraggableRectangle>())
                    {
                        child.IsSelected = false;
                    }
                }
            };
        }

        #region 矩形選択
        
        // ドラッグ処理の実装 (C#)
        // マウスの開始地点を記録し、移動量に合わせて selectionBox のサイズと位置を更新します。
        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == MainCanvas)
            {
                startPoint = e.GetPosition(MainCanvas);

                // 選択矩形の初期化
                Canvas.SetLeft(selectionBox, startPoint.X);
                Canvas.SetTop(selectionBox, startPoint.Y);
                selectionBox.Width = 0;
                selectionBox.Height = 0;
                selectionBox.Visibility = Visibility.Visible;

                _ = MainCanvas.CaptureMouse();
            }
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!MainCanvas.IsMouseCaptured) return;

            Point currentPoint = e.GetPosition(MainCanvas);

            // 負の方向(左上方向)へのドラッグ移動にも対応する計算
            double x = Math.Min(startPoint.X, currentPoint.X);
            double y = Math.Min(startPoint.Y, currentPoint.Y);
            double width = Math.Abs(startPoint.X - currentPoint.X);
            double height = Math.Abs(startPoint.Y - currentPoint.Y);

            Canvas.SetLeft(selectionBox, x);
            Canvas.SetTop(selectionBox, y);
            selectionBox.Width = width;
            selectionBox.Height = height;
        }
        // 負の方向へのドラッグ
        // マウスを右下から左上へドラッグした場合、currentPoint - startPoint は負になります。WPFの Width/Height に負の値を入れるとエラーになるため、Math.Min と Math.Abs を使って**「常に左上の座標と正のサイズ」**を計算するのがコツです。

        // 当たり判定（ヒットテスト）の実装
        // マウスを離した瞬間に、selectionBox の範囲内に各 DraggableRectangle が含まれているかを判定します。
        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!MainCanvas.IsMouseCaptured) return;

            // 選択範囲のRectを作成
            Rect selectionRect = new(
                Canvas.GetLeft(selectionBox),
                Canvas.GetTop(selectionBox),
                selectionBox.Width,
                selectionBox.Height);

            // Canvas内のDraggableRectangleをすべてチェック
            foreach (var item in MainCanvas.Children.OfType<DraggableRectangle>())
            {
                // アイテムの現在の配置位置を取得
                Rect itemRect = new(
                    Canvas.GetLeft(item),
                    Canvas.GetTop(item),
                    item.ActualWidth,
                    item.ActualHeight);

                // 交差判定(IntersectsWith)または、包含判定(Contains)
                item.IsSelected = selectionRect.IntersectsWith(itemRect);
            }

            // 後片付け
            selectionBox.Visibility = Visibility.Collapsed;
            MainCanvas.ReleaseMouseCapture();
        }
        // IntersectsWith vs Contains
        // IntersectsWith: 選択枠が少しでも図形に触れれば選択されます。直感的で使いやすいです。
        // Contains: 図形が完全に枠の中に入らないと選択されません。精密な選択に向いています。

        // パフォーマンスのヒント
        // 要素数が数百、数千と増える場合は、VisualTreeHelper.HitTest を使った空間分割的な判定が必要になりますが、数十個程度であれば上記の foreach ループで十分高速に動作します。
        #endregion 矩形選択


        //// 削除
        //private void RemoveItem_Click(object sender, RoutedEventArgs e)
        //{
        //    // 選択されている要素をリストアップ
        //    var selectedItems = MainCanvas.Children.OfType<DraggableRectangle>()
        //        .Where(x => x.IsSelected)
        //        .ToList();

        //    // Canvasから削除
        //    foreach (var item in selectedItems)
        //    {
        //        MainCanvas.Children.Remove(item);
        //    }
        //}

        //// 追加
        //private void AddItem_Click(object sender, RoutedEventArgs e)
        //{
        //    //var newRect = new DraggableRectangle() { Width = 100, Height = 100 };

        //    //// 初期配置は、とりあえず左上に
        //    //Canvas.SetLeft(newRect, 50);
        //    //Canvas.SetTop(newRect, 50);
        //    //MainCanvas.Children.Add(newRect);

        //}




        //private void Save_Click(object sender, RoutedEventArgs e)
        //{
        //    var dialog = new Microsoft.Win32.SaveFileDialog();
        //    dialog.Filter = "JSONファイル (*.json)|*.json";
        //    if (dialog.ShowDialog() == true)
        //    {
        //        var data = (MainViewModel)this.DataContext;
        //        data.Save(dialog.FileName);
        //    }
        //}

    }
}