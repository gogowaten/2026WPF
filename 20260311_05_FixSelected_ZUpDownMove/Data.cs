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

namespace _20260311_05_FixSelected_ZUpDownMove
{
    public partial class RootData : GroupData
    {
        // TextBlock追加時に使う文字列用
        [NotifyCanExecuteChangedFor(nameof(AddTextBlockDataCommand))]
        [ObservableProperty] private string _addText = "ここに文字列";


        [ObservableProperty]
        private Data? _currentItem; // 筆頭

        [ObservableProperty] private Data? _clickedItem; // 大抵は最後にクリックしたItem

        //[NotifyCanExecuteChangedFor(nameof(ZAgeCommand))]
        [ObservableProperty] private ObservableCollection<Data> _selectedItems = [];

        [NotifyCanExecuteChangedFor(nameof(ZUpCommand))]
        [NotifyCanExecuteChangedFor(nameof(ZtoTopCommand))]
        [ObservableProperty] private GroupData? _editingGroup;



        //public DataService MyService { get; } = new();


        public RootData()
        {
            Name = "RootDataです";
            EditingGroup = this;
            MyInit();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }


        #region On～プロパティの変更時

        // クリックItem
        partial void OnClickedItemChanged(Data? oldValue, Data? newValue)
        {
            if (oldValue is not null)
            {
                oldValue.IsClicked = false;
            }
            if (newValue is not null)
            {
                newValue.IsClicked = true;
            }
        }


        // 筆頭変更後
        partial void OnCurrentItemChanged(Data? oldValue, Data? newValue)
        {
            if (oldValue is not null) { oldValue.IsCurrent = false; }
            if (newValue is not null) { newValue.IsCurrent = true; }
        }

        // 編集Group変更時
        partial void OnEditingGroupChanged(GroupData? oldValue, GroupData? newValue)
        {
            ClearSelectedItems();
            CurrentItem = null;
            ClickedItem = null;

            if (oldValue is not null)
            {
                oldValue.IsEditing = false;
                foreach (var item in oldValue.DataList)
                {
                    item.IsSelectable = false;
                }

            }
            if (newValue is not null)
            {
                newValue.IsEditing = true;
                foreach (var item in newValue.DataList)
                {
                    item.IsSelectable = true;
                }
            }
        }

        #endregion On～プロパティの変更時

        // 全選択解除
        [RelayCommand]
        public void ClearSelectedItems()
        {
            foreach (var item in SelectedItems)
            {
                item.IsSelected = false;
                item.IsCurrent = false;
            }

            //SelectedItems.Clear(); // Clearメソッドは使わない
            var tempList = new List<Data>(SelectedItems);
            foreach (Data item in tempList) { _ = SelectedItems.Remove(item); }

            CurrentItem = null;
        }

        // 選択状態のData変更時
        private void SelectedItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems?[0] is Data newData)
                {
                    newData.IsSelected = true;
                    RemoveSelectedItemsCommand.NotifyCanExecuteChanged(); // 削除Command実行判定
                    ZUpCommand.NotifyCanExecuteChanged();
                    ZtoTopCommand.NotifyCanExecuteChanged();
                    ZDownCommand.NotifyCanExecuteChanged();
                    ZtoBottomCommand.NotifyCanExecuteChanged();
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems?[0] is Data oldData)
                {
                    oldData.IsSelected = false;
                    //oldData.IsCurrent = false;
                    if (CurrentItem == oldData) { CurrentItem = null; }
                    //if (ClickedItem == oldData) { ClickedItem = null; }
                    RemoveSelectedItemsCommand.NotifyCanExecuteChanged(); // 削除Command実行判定
                    ZUpCommand.NotifyCanExecuteChanged();
                    ZtoTopCommand.NotifyCanExecuteChanged();
                    ZDownCommand.NotifyCanExecuteChanged();
                    ZtoBottomCommand.NotifyCanExecuteChanged();
                }
            }
        }


        #region メソッド

        // 選択Itemを最背面へ移動
        [RelayCommand(CanExecute = nameof(CanZDown))]
        private void ZtoBottom()
        {
            if (EditingGroup is null) { return; }

            // 選択Item全体の移動距離を計算、一番下のItemが0になる値
            // = 0 - 一番下のItemのZ
            ZMove(0 - SelectedItems.Min(n => n.Z));
        }

        // 背面へ移動
        [RelayCommand(CanExecute = nameof(CanZDown))]
        private void ZDown()
        {
            if (EditingGroup is null) { return; }

            ZMove(-1);
        }


        /// </summary>
        /// <remarks>編集グループ内の選択されたアイテムのZオーダーを更新し、
        /// 関連するコマンドの状態を更新します</remarks>
        /// <param name="distination">Zオーダー内で選択されたアイテムを移動するオフセット。正の値はアイテムを前方に移動させ、
        /// 負の値はアイテムを後方に移動させます。</param>
        private void ZMove(int distination)
        {
            if (EditingGroup is null) { return; }

            // 新リスト作成、非選択Itemを詰め込む
            var newList = new ObservableCollection<Data>();
            foreach (var item in EditingGroup.DataList)
            {
                if (item.IsSelected == false) { newList.Add(item); }
            }

            // 新リストに選択Itemを順番に挿入、場所は移動距離(方向)を加味
            var sorted = SelectedItems.OrderBy(n => n.Z).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                newList.Insert(sorted[i].Z + distination, sorted[i]);
            }

            // ItemのZをIndexに合わせる
            for (int i = 0; i < newList.Count; i++) { newList[i].Z = i; }

            // リストの入れ替え
            EditingGroup.DataList = newList;

            ZDownCommand.NotifyCanExecuteChanged();
            ZtoBottomCommand.NotifyCanExecuteChanged();
            ZUpCommand.NotifyCanExecuteChanged();
            ZtoTopCommand.NotifyCanExecuteChanged();
        }

        private bool CanZDown()
        {
            // 編集モードのグループが在る
            if (EditingGroup is null) { return false; }

            // 選択Item在る
            int selectCount = SelectedItems.Count;
            if (selectCount == 0) { return false; }

            // 選択Item個数は子要素個数より少ない
            if (selectCount >= EditingGroup.DataList.Count) { return false; }

            // 選択Itemに最下層のItemが含まれていない
            foreach (var item in SelectedItems)
            {
                if (item.Z == 0) { return false; }
            }
            return true;
        }




        // 選択Itemを最前面へ移動
        [RelayCommand(CanExecute = nameof(CanZUp))]
        private void ZtoTop()
        {
            if (EditingGroup is null) { return; }

            // 選択Itemが最前面になるまでの上げ幅を取得
            int mi = SelectedItems.Max(n => n.Z);
            int agehaba = EditingGroup.DataList.Count - 1 - mi;
            ZMove(agehaba);
        }


        // Z、選択Itemを上に移動、ZIndexを1増やす
        [RelayCommand(CanExecute = nameof(CanZUp))]
        private void ZUp()
        {
            if (EditingGroup is null) { return; }
            ZMove(1);
        }



        private bool CanZUp()
        {
            // 編集モードのグループが在る
            if (EditingGroup is null) { return false; }

            // 選択Item在る
            int selectCount = SelectedItems.Count;
            if (selectCount == 0) { return false; }

            // 選択Item個数は子要素個数より少ない
            if (selectCount >= EditingGroup.DataList.Count) { return false; }

            // 選択Itemに最上層のItemが含まれていない
            int max = EditingGroup.DataList.Count - 1;
            foreach (var item in SelectedItems)
            {
                if (item.Z == max) { return false; }
            }
            return true;
        }


        #region 編集モード

        // 指定グループを編集モードにする
        public void MigrateEditingGroup(GroupData group) { EditingGroup = group; }

        // 編集モードを今の1個上に移行する
        public void MigrateEditingGroupUpper()
        {
            if (EditingGroup?.ParentData is GroupData upper)
            {
                EditingGroup = upper;
            }
        }

        // Currentを編集モードにする
        public void MigrateEditingGroupCurrent()
        {
            if (CurrentItem is GroupData group) { EditingGroup = group; }
        }

        #endregion 編集モード

        public void AddSelect(Data data)
        {
            // 二重登録禁止
            if (SelectedItems.Contains(data)) return;
            if (data.IsSelectable == false) { return; }

            SelectedItems.Add(data);
            CurrentItem = data;
        }

        public void RemoveDataFromSelect(Data data)
        {
            var dataIndex = SelectedItems.IndexOf(data) - 1;
            SelectedItems.Remove(data);
            // 筆頭Itemを更新
            // 一個前を筆頭にする、一個前がなければ一個後を筆頭にする
            if (dataIndex < 0) { dataIndex++; }
            CurrentItem = SelectedItems[dataIndex];
        }



        // TextBlockを追加するテスト
        [RelayCommand(CanExecute = nameof(CanAddTextBlockData))]
        public void AddTextBlockData(string name)
        {
            TextBlockData data = new()
            {
                Name = name,
                Text = name,
                Foreground = Brushes.MidnightBlue,
                RootData = this,
                FontSize = 30,
            };
            EditingGroup?.DataList.Add(data);
            data.IsSelectable = true;
        }

        // TextBlock追加できるかの判定用
        private bool CanAddTextBlockData()
        {
            // 文字が入力されている ＆ 編集モードのグループがある
            return !string.IsNullOrEmpty(AddText) && (EditingGroup is not null);
        }


        // 選択状態のItemすべてを削除
        [RelayCommand(CanExecute = nameof(CanSelectedItemsRemove))]
        public void RemoveSelectedItems()
        {
            if (EditingGroup is null) { return; }

            // リストから削除
            foreach (var item in SelectedItems)
            {
                EditingGroup.DataList.Remove(item);
            }

            // 選択状態解除
            ClearSelectedItems();
        }


        // 選択状態のItemすべてを削除できるかの判定
        private bool CanSelectedItemsRemove()
        {
            return SelectedItems.Count > 0;
        }




        #endregion メソッド

        // テスト用初期化
        private void MyInit()
        {
            this.RootData = this; // 自身をRootにしておく
            this.CurrentItem = this; // 自身を筆頭にしておく
            this.IsEditing = true; // 起動時は自身が編集状態グループ

            RectangleData rRed = new() { Name = "赤四角", X = 0, Y = 0, Width = 60, Height = 60, Fill = new SolidColorBrush(Color.FromArgb(250, 255, 0, 0)) };
            RectangleData rBlue = new() { Name = "青四角", X = 20, Y = 20, Width = 60, Height = 60, Fill = new SolidColorBrush(Color.FromArgb(250, 0, 0, 255)) };
            EllipseData maruRed = new() { Name = "黄玉", X = 0, Y = 0, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(250, 255, 200, 0)) };
            EllipseData maruBlue = new() { Name = "水玉", X = 120, Y = 20, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(250, 0, 200, 255)) };
            EllipseData maruGreen = new() { Name = "翠玉", X = 40, Y = 140, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(250, 100, 200, 150)) };

            GroupData groupRect = new() { RootData = this, Name = "GropuA", X = 0, Y = 0 };
            groupRect.DataList.Add(rRed);
            groupRect.DataList.Add(rBlue);

            GroupData groupEllipse = new() { RootData = this, Name = "GropuB", X = 100, Y = 0 };
            groupEllipse.DataList.Add(maruRed);
            groupEllipse.DataList.Add(maruBlue);

            GroupData groupB_1 = new() { RootData = this, Name = "GroupB_1", X = 0, Y = 100 };
            groupB_1.DataList.Add(new EllipseData() { Name = "青丸", X = 0, Y = 0, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(250, 0, 0, 255)) });
            groupB_1.DataList.Add(new EllipseData() { Name = "赤丸", X = 100, Y = 100, Width = 50, Height = 50, Fill = new SolidColorBrush(Color.FromArgb(250, 255, 0, 0)) });
            groupEllipse.DataList.Add(groupB_1);

            DataList.Add(groupRect);
            DataList.Add(groupEllipse);
            DataList.Add(maruGreen);
            TextBlockData textBlockData = new() { Name = "Text1", X = 0, Y = 0, Text = "Text1", FontSize = 30 };
            DataList.Add(textBlockData);


            // 直下のItemのIsSelectableをtrueにする
            foreach (var item in DataList)
            {
                item.IsSelectable = true;
            }

            DataSyokika(this);
        }

        // 起動時のDataの辻褄合わせ
        // すべてのItemのRootDataを自身にする
        private void DataSyokika(GroupData data)
        {
            foreach (var item in data.DataList)
            {
                item.RootData = this;
                if (item is GroupData group) { DataSyokika(group); }
            }
        }


    }




    /*Group*/



    public partial class GroupData : Data
    {
        [ObservableProperty] private bool _isEditing; // 編集状態
        [ObservableProperty] private ObservableCollection<Data> _dataList = [];

        public GroupData()
        {
            Name = "GroupData";
            DataList.CollectionChanged += DataList_CollectionChanged;
        }



        private void DataList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems?[0] is Data newData)
                {
                    newData.Z = DataList.Count - 1;
                    newData.ParentData = this;
                    newData.UpdateParentSize();
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems?[0] is Data oldData)
                {
                    // 削除した要素より上の要素のZを1下げる
                    int currentZ = oldData.Z;
                    foreach (var item in this.DataList)
                    {
                        if (item.Z > currentZ)
                        {
                            item.Z--;
                        }
                    }

                    oldData.UpdateParentSize();
                    oldData.ParentData = null; // Parentをリサイズしてからnullにする
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                var oi = e.OldStartingIndex;
                var ni = e.NewStartingIndex;
                if (ni < oi)
                {
                    for (int i = ni; i <= oi; i++)
                    {
                        this.DataList[i].Z = i;
                    }
                }
                else if (oi < ni)
                {
                    for (int i = oi; i <= ni; i++)
                    {
                        this.DataList[i].Z = i;
                    }
                }

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

        // Bounds更新
        public void UpdateBounds(GroupData group)
        {
            double right = 0;
            double bottom = 0;
            double mx = double.MaxValue;
            double my = double.MaxValue;
            foreach (var item in group.DataList)
            {
                mx = Math.Min(mx, item.X);
                my = Math.Min(my, item.Y);
                right = Math.Max(right, item.X + item.Width);
                bottom = Math.Max(bottom, item.Y + item.Height);
            }

            // サイズ更新
            group.Width = right - mx; group.Height = bottom - my;
            // 座標更新
            foreach (var item in group.DataList) { item.X -= mx; item.Y -= my; }

            // 親要素のBounds更新
            group.ParentData?.UpdateBounds(group.ParentData);
        }



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



    }

    #region 図形

    public partial class EllipseData : ShapeData { }

    public partial class RectangleData : ShapeData { }


    public abstract partial class ShapeData : Data
    {
        [ObservableProperty] private Brush _fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0));
    }
    #endregion 図形

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