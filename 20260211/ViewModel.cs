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
        [ObservableProperty]
        private PointCollection _points = [];
        public PointCollection MyPoints = [new Point(), new Point(100, 50)];
        
        [ObservableProperty]
        private bool _isDrawing = false;

        // 描画開始
        [RelayCommand]
        private void StartDrawing()
        {
            Points = new PointCollection();
            IsDrawing = true;
        }

        //// 頂点追加 (Canvasクリック時に呼び出す)
        //[RelayCommand]
        //private void AddPoint(Point point)
        //{
        //    if (!IsDrawing) return;
        //    Points.Add(point);
        //    // PointCollectionの変更を通知するために再代入
        //    OnPropertyChanged(nameof(Points));
        //}

        // 頂点追加 (Canvasクリック時に呼び出す)
        [RelayCommand]
        private void AddPoint(object param)
        {
            // param には MouseButtonEventArgs が入ってくる
            if (!IsDrawing || param is not MouseButtonEventArgs e) { return; }

            // イベントが発生した源 (Canvas) を取得
            var canvas = e.Source as IInputElement;
            if (canvas == null) { return; }

            // Canvas内でのクリック座標取得
            Point point = e.GetPosition(canvas);

            MyPoints.Add(point);
            Points.Add(point);
            //OnPropertyChanged(nameof(Points));
            Points = new PointCollection(Points);
        }

        // 描画終了
        [RelayCommand]
        private void StopDrawing()
        {
            IsDrawing = false;
        }

        // JSON保存
        [RelayCommand]
        private void SavePoints()
        {
            List<PointData> data = Points.Select(p => new PointData(p.X, p.Y)).ToList();
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText("points.json", json);
            MessageBox.Show("points.json に保存しました。");
        }
    }
}
