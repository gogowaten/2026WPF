using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using System.Windows;

namespace _20260321
{

    public partial class RootData : GroupData
    {
        // TextBlock追加時に使う文字列用
        [NotifyCanExecuteChangedFor(nameof(AddTextBlockDataCommand))]
        [ObservableProperty] private string _addText = "ここに文字列";


        //[ObservableProperty]
        //private Data? _currentItem; // 筆頭

        //[ObservableProperty] private Data? _clickedItem; // 大抵は最後にクリックしたItem

        //[NotifyCanExecuteChangedFor(nameof(ZAgeCommand))]
        [ObservableProperty] private ObservableCollection<Data> _selectedItems = [];

        //[NotifyCanExecuteChangedFor(nameof(ZUpCommand))]
        //[NotifyCanExecuteChangedFor(nameof(ZtoTopCommand))]
        //[ObservableProperty] private GroupData? _editingGroup;



        //public DataService MyService { get; } = new();


        public RootData()
        {
            Name = "RootData";
            this.RootData = this; // 自身をRootにしておく
            
        }




        #region メソッド






        // TextBlockを追加するテスト
        // 追加後はSelectedをクリアして、追加Itemを選択状態にする、Currentにする
        [RelayCommand]
        public void AddTextBlockData(string name)
        {
            TextBlockData data = new()
            {
                Name = name,
                Text = name,
                Foreground = Brushes.MidnightBlue,
                RootData = this,
                FontSize = 30,
                IsSelectable = true
            };
            DataList.Add(data);
        }




        #endregion メソッド

        //// テスト用初期化
        //private void MyInit()
        //{
        //    this.RootData = this; // 自身をRootにしておく
        //    this.CurrentItem = this; // 自身を筆頭にしておく
        //    this.IsEditing = true; // 起動時は自身が編集状態グループ

        //    RectangleData rRed = new() { Name = "赤四角", X = 0, Y = 0, Width = 60, Height = 60, Fill = new SolidColorBrush(Color.FromArgb(250, 255, 0, 0)) };
        //    RectangleData rBlue = new() { Name = "青四角", X = 20, Y = 20, Width = 60, Height = 60, Fill = new SolidColorBrush(Color.FromArgb(250, 0, 0, 255)) };
        //    EllipseData maruRed = new() { Name = "黄玉", X = 0, Y = 0, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(250, 255, 200, 0)) };
        //    EllipseData maruBlue = new() { Name = "水玉", X = 120, Y = 20, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(250, 0, 200, 255)) };
        //    EllipseData maruGreen = new() { Name = "翠玉", X = 40, Y = 140, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(250, 100, 200, 150)) };

        //    GroupData groupRect = new() { RootData = this, Name = "GropuA", X = 0, Y = 0 };
        //    groupRect.DataList.Add(rRed);
        //    groupRect.DataList.Add(rBlue);

        //    GroupData groupEllipse = new() { RootData = this, Name = "GropuB", X = 100, Y = 0 };
        //    groupEllipse.DataList.Add(maruRed);
        //    groupEllipse.DataList.Add(maruBlue);

        //    GroupData groupB_1 = new() { RootData = this, Name = "GroupB_1", X = 0, Y = 100 };
        //    groupB_1.DataList.Add(new EllipseData() { Name = "青丸", X = 0, Y = 0, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(250, 0, 0, 255)) });
        //    groupB_1.DataList.Add(new EllipseData() { Name = "赤丸", X = 100, Y = 100, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(250, 255, 0, 0)) });
        //    groupEllipse.DataList.Add(groupB_1);

        //    DataList.Add(groupRect);
        //    DataList.Add(groupEllipse);
        //    DataList.Add(maruGreen);
        //    TextBlockData textBlockData = new() { Name = "Text1", X = 0, Y = 0, Text = "Text1", FontSize = 30 };
        //    DataList.Add(textBlockData);


        //    // 直下のItemのIsSelectableをtrueにする
        //    foreach (var item in DataList)
        //    {
        //        item.IsSelectable = true;
        //    }

        //    DataSyokika(this);
        //}

        //// 起動時のDataの辻褄合わせ
        //// すべてのItemのRootDataを自身にする
        //private void DataSyokika(GroupData data)
        //{
        //    foreach (var item in data.DataList)
        //    {
        //        item.RootData = this;
        //        if (item is GroupData group) { DataSyokika(group); }
        //    }
        //}


    }




    /*Group*/



    public partial class GroupData : Data
    {
        [ObservableProperty] private ObservableCollection<Data> _dataList = [];

        public GroupData()
        {
            Name = "GroupData";
        }





        /// <summary>
        /// 特別、TextBlockなどサイズが確定していない要素を
        /// まっさらなRootに追加した直後にRootのサイズを決定するのに使う
        /// DataTemplateのXAMLからBehaviorで使う
        ///   xmlns:i="http://schemas.microsoft.com/xaml/behaviors">
        ///      <i:Interaction.Triggers>
        ///        <i:EventTrigger EventName = "Loaded" >
        ///          < i:InvokeCommandAction Command = "{Binding RootData.UpdateRootSizeForNaNSizeElementCommand}" />
        ///        </ i:EventTrigger>
        ///      </i:Interaction.Triggers>
        /// </summary>
        [RelayCommand]
        private void UpdateRootSizeForNaNSizeElement()
        {
            if (DataList.Count == 1 && Width == 0)
            {
                Width = DataList[0].Width;
                Height = DataList[0].Height;
            }
        }

        [RelayCommand]
        public void UpdateSize()
        {
            double right = 0;
            double bottom = 0;
            foreach (var item in DataList)
            {
                right = Math.Max(right, item.X + item.Width);
                bottom = Math.Max(bottom, item.Y + item.Height);
            }
            Width = right; Height = bottom;
            //var neko = DataList.Max(n => n.X + n.Width);
        }

        //// Bounds更新
        //public void UpdateBounds(GroupData group)
        //{
        //    double right = 0;
        //    double bottom = 0;
        //    double mx = double.MaxValue;
        //    double my = double.MaxValue;
        //    foreach (var item in group.DataList)
        //    {
        //        mx = Math.Min(mx, item.X);
        //        my = Math.Min(my, item.Y);
        //        right = Math.Max(right, item.X + item.Width);
        //        bottom = Math.Max(bottom, item.Y + item.Height);
        //    }

        //    // サイズ更新
        //    group.Width = right - mx; group.Height = bottom - my;

        //    // 子要素の座標更新
        //    foreach (var item in group.DataList) { item.X -= mx; item.Y -= my; }

        //    // 親要素のBounds更新
        //    group.ParentData?.UpdateBounds(group.ParentData);
        //}



    }

    public partial class TextBlockData : TextData
    {

    }
    public abstract partial class TextData : Data
    {
        [ObservableProperty] private string _text = string.Empty;
        [ObservableProperty] private string _fontName = SystemFonts.MessageFontFamily.ToString();
        [ObservableProperty] private double _fontSize = SystemFonts.MessageFontSize;
        [ObservableProperty] private Brush? _foreground = Brushes.Black;
        [ObservableProperty] private Brush? _background = Brushes.Transparent;

        #region サイズ変更に関わる
        partial void OnTextChanged(string value)
        {
            UpdateParentSize();
        }
        partial void OnFontNameChanged(string value)
        {
            UpdateParentSize();
        }
        partial void OnFontSizeChanged(double value)
        {
            UpdateParentSize();
        }
        #endregion サイズ変更に関わる
    }


    public abstract partial class Data : ObservableObject
    {
        [ObservableProperty] private RootData? _rootData;
        [ObservableProperty] private GroupData? _parentData;
        [ObservableProperty] private double _width;
        [ObservableProperty] private double _height;
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private int _z;
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] bool _isSelected = false; // 選択状態
        [ObservableProperty] bool _isSelectable = false; // 選択状態
        [ObservableProperty] bool _isCurrent = false; // 筆頭
        [ObservableProperty] bool _isClicked = false; // クリックされた要素

        public void UpdateParentSize()
        {
            if (ParentData is null) { return; }

            double right = 0;
            double bottom = 0;
            //double mx = double.MaxValue;
            //double my = double.MaxValue;
            foreach (var item in ParentData.DataList)
            {
                //mx = Math.Min(mx, item.X);
                //my = Math.Min(my, item.Y);
                right = Math.Max(right, item.X + item.Width);
                bottom = Math.Max(bottom, item.Y + item.Height);
            }
            //X = mx; Y = my;
            ParentData.Width = right; ParentData.Height = bottom;
        }



    }
}