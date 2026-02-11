using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;


namespace _20260210_tuduki_mvvm
{
    public partial class RectModel : ObservableObject
    {
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        [ObservableProperty] private bool _isSelected;

        //[ObservableProperty] private bool _isVisible;
        //[ObservableProperty] private bool _isDragging;
        //[ObservableProperty] private bool _isMoving;

        // 親のViewModelから渡される「移動してほしい」という処理の参照
        public Action<double, double>? MoveRequested;

        public void RequestMove(double dx, double dy)
        {
            MoveRequested?.Invoke(dx, dy);
        }
    }
}
