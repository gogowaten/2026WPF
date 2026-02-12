using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace _20260211
{
    class ViewModel
    {
    }

    //public partial class MainViewModel : ObservableObject
    //{
    //    [ObservableProperty] private PointCollection _points = new();
    //    [ObservableProperty] private bool _isDrawing = false;

    //    // 描画開始
    //    [RelayCommand]
    //    private void StartDrawing()
    //    {
    //        Points = new PointCollection();
    //        IsDrawing = true;
    //    }

    //    // 頂点追加、Canvasクリック時に呼び出す
    //    [RelayCommand]
    //    private void AddPoint(Point point)
    //    {
    //        if (!IsDrawing) { return; }
    //        Points.Add(point);
    //        // PointCollectionの変更を通知するために再代入
    //        OnPropertyChanged(nameof(Points));
    //    }

    //    // 描画終了
    //    [RelayCommand]
    //    private void StopDrawing()
    //    {
    //        IsDrawing = false;
    //    }

    //    // JSON保存
    //    [RelayCommand]
    //    private void SavePoints()
    //    {
    //        var data = Points.Select(p => new PointData(p.X, p.Y)).ToString();
    //        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    //        File.WriteAllText("points.json", json);
    //        MessageBox.Show("points.json に保存しました");
    //    }
    //}



    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty] private Brush _selectedBrush = Brushes.Blue; // 塗りつぶし
        [ObservableProperty] private double _currentArrowSize = 20.0; // 鏃サイズ
        [ObservableProperty]
        private ObservableCollection<Point> _points = [];

        [ObservableProperty]
        private bool _isDrawing = false;

        // 描画開始
        [RelayCommand]
        private void StartDrawing()
        {
            //   Points = new ObservableCollection<Point>();
            Points.Clear();
            IsDrawing = true;
        }

        /* // 矢印Geometry作成
         public Geometry CreateArrowGeometry(Point start, Point end, double thickness)
         {
             var geometry = new StreamGeometry();
             using (StreamGeometryContext ctx = geometry.Open())
             {
                 // 矢印のサイズ設定（太さに比例させるのがコツ）
                 double arrowLength = thickness * 4;
                 double arrowWidth = thickness * 3;

                 Vector lineVec = end - start;
                 lineVec.Normalize();

                 // 垂直ベクトル
                 Vector normalVec = new Vector(-lineVec.Y, lineVec.X);

                 // 矢印の底辺の中心点
                 Point basePoint = end - (lineVec * arrowLength);
                 // 矢印の左右の角
                 Point leftCorner = basePoint + (normalVec * arrowWidth / 2);
                 Point rightCorner = basePoint - (normalVec * arrowWidth / 2);

                 // 描画開始
                 ctx.BeginFigure(start, true, false); // 線を描く
                 ctx.LineTo(basePoint, true, false);  // 矢印の底まで

                 // 矢印部分（二等辺三角形）
                 ctx.LineTo(leftCorner, true, false);
                 ctx.LineTo(end, true, false);        // 先端
                 ctx.LineTo(rightCorner, true, false);
                 ctx.LineTo(basePoint, true, false);
             }
             geometry.Freeze();
             return geometry;
         }*/


        // 色変更
        [RelayCommand]
        private void ChangeColor(string colorName)
        {   
            SelectedBrush = (Brush)new BrushConverter().ConvertFromString(colorName)!;
        }

        // 頂点追加 (Canvasクリック時に呼び出す)
        [RelayCommand]
        private void AddPoint(object param)
        {
            // param には MouseButtonEventArgs が入ってくる
            if (!IsDrawing || param is not MouseButtonEventArgs e) { return; }
            //if (!IsDrawing || e == null) { return; }

            // イベントが発生した源 (Canvas) を取得
            if (e.Source is not IInputElement canvas) { return; }

            // Canvas内でのクリック座標取得して追加
            Points.Add(e.GetPosition(canvas));
        }

        // 描画終了
        [RelayCommand]
        private void StopDrawing() { IsDrawing = false; }

        // JSON保存
        [RelayCommand]
        private void SavePoints()
        {
            var data = Points.Select(p => new PointData(p.X, p.Y)).ToList();
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText("points.json", json);
            MessageBox.Show("points.json に保存しました。");
        }
    }
}
