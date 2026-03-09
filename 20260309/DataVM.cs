using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace _20260309
{
    public partial class DataVM : ObservableObject
    {
        [ObservableProperty] private GroupData _myRootData = new();

        public DataVM()
        {
            MyInit();

        }
        private void MyInit()
        {
            RectangleData rRed = new() { Name = "RedRect", X = 0, Y = 0, Width = 60, Height = 60, Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)) };
            RectangleData rBlue = new() { Name = "BlueRect", X = 20, Y = 20, Width = 60, Height = 60, Fill = new SolidColorBrush(Color.FromArgb(50, 0, 0, 255)) };
            EllipseData maruRed = new() { Name = "RedEllipse", X = 0, Y = 0, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)) };
            EllipseData maruBlue = new() { Name = "BlueEllipse", X = 20, Y = 20, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(50, 0, 0, 255)) };
            EllipseData maruGreen = new() { Name = "GreenEllipse", X = 40, Y = 140, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0)) };

            GroupData groupRect = new() { Name = "GropuA", X = 0, Y = 0 };
            groupRect.DataList.Add(rRed);
            groupRect.DataList.Add(rBlue);

            GroupData groupEllipse = new() { Name = "GropuB", X = 100, Y = 0 };
            groupEllipse.DataList.Add(maruRed);
            groupEllipse.DataList.Add(maruBlue);

            MyRootData.DataList.Add(groupRect);
            MyRootData.DataList.Add(groupEllipse);
            MyRootData.DataList.Add(maruGreen);
        }

        [RelayCommand]
        public void AddEllipse()
        {
            MyRootData.DataList.Add(new EllipseData() { Width = 50, Height = 50, Fill = Brushes.Red });
        }
    }
}
