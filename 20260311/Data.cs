using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace _20260311
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

        #region テスト用初期化

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
            DataList.Add(new PolylineData() { Name = "ボリライン", X = 0, Y = 0, StrokeThickness = 50, Stroke = Brushes.MediumPurple });
            DataList.Add(new GeoShapeData() { Name = "ベジェ曲線", X = 100, Y = 100, Stroke = Brushes.MediumAquamarine, StrokeThickness = 20.0, Points = [new Point(50, 70), new Point(250, 150), new Point(50, 250), new Point(50, 200), new Point(50, 150), new Point(150, 100), new Point(250, 250),], StrokeEndLineCap = PenLineCap.Round , Background = Brushes.Gray});



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
        #endregion テスト用初期化


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
            if (newValue is not null)
            {
                newValue.IsCurrent = true;
                UnGroupCommand.NotifyCanExecuteChanged();
                if(newValue is GeoShapeData geo)
                {
                    CanChageGeoShapeData();
                }
            }
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

        /// <summary>
        /// オフセットの切り替え、GeoShapeData専用
        /// 図形の位置が左上(0,0)になるのと、通常の位置の切り替えになる
        /// 図形の位置が変わるののでThumbのいちも相対的に変更するため、DataのX,Yを変更している
        /// </summary>
        //[RelayCommand(CanExecute = nameof(CanChageGeoShapeData))]
        [RelayCommand]
        private void ChangeGeoShapeOffset()
        {
            if (CurrentItem is GeoShapeData data)
            {
                data.IsOffset = !data.IsOffset;
            }
        }
        public bool CanChageGeoShapeData()
        {
            return CurrentItem is GeoShapeData;
        }

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
                    AddGroupFromSelectedItemsCommand.NotifyCanExecuteChanged();

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
                    AddGroupFromSelectedItemsCommand.NotifyCanExecuteChanged();
                    UnGroupCommand.NotifyCanExecuteChanged();
                }
            }
        }


        #region メソッド

        #region グループ化

        private bool CanUnGroup()
        {
            if (SelectedItems.Count == 0) { return false; }
            if (CurrentItem is not GroupData) { return false; }
            if (EditingGroup is null) { return false; }
            return true;
        }

        /// <summary>
        /// 現在選択されているグループ項目をグループ解除し、子要素を親グループに移動させ、選択状態と位置を更新します。
        /// </summary>
        /// <remarks>このメソッドは、現在の項目がグループであり、かつ編集グループが存在する場合にのみ使用できます。
        /// グループ解除後、グループのすべての子要素が選択され、親グループに対する位置が調整されます。
        /// グループ自体は親グループから削除されます。この操作により、
        /// 選択状態が更新され、必要に応じてコマンド状態の変更が通知されます。
        /// </remarks>
        [RelayCommand(CanExecute = nameof(CanUnGroup))]
        private void UnGroup()
        {
            if (CurrentItem is not GroupData) { return; }
            if (EditingGroup is null) { return; }

            if (CurrentItem is GroupData targetGroupData)
            {
                // ClickedItemチェック
                if (ClickedItem == targetGroupData) { ClickedItem = null; }

                // 親要素のDataListにバラした要素を順番に挿入
                int z = targetGroupData.Z;
                for (int i = targetGroupData.DataList.Count - 1; i >= 0; i--)
                {
                    var item = targetGroupData.DataList[i];
                    EditingGroup.DataList.Insert(z, item);
                    item.IsSelectable = true;
                    item.IsSelected = true;
                }

                // 子要素の座標調整
                foreach (var item in targetGroupData.DataList)
                {
                    item.X += targetGroupData.X;
                    item.Y += targetGroupData.Y;
                }

                // 子要素全体のZを整える
                for (int i = 0; i < EditingGroup.DataList.Count; i++)
                {
                    EditingGroup.DataList[i].Z = i;
                }
                // 選択Itemを整える、解除したグループの子要素を選択状態にする
                ClearSelectedItems();
                foreach (var item in targetGroupData.DataList)
                {
                    AddDataToSelectedItems(item);
                }

                // 解除するDataを外す
                EditingGroup.DataList.Remove(targetGroupData);
                //RemoveDataFromSelect(targetGroupData);
                targetGroupData.IsClicked = false;
                targetGroupData.IsSelectable = false;
                targetGroupData.IsSelected = false;
                targetGroupData.DataList.Clear(); // 要る？

                // 選択ItemにClickedItemが在ればそれをCurrentItemにする
                if (ClickedItem is not null && ClickedItem.IsSelected) { CurrentItem = ClickedItem; }


                UnGroupCommand.NotifyCanExecuteChanged();
            }




        }

        private bool CanAddGroupFromSelectedItems()
        {
            if (EditingGroup is null) { return false; }
            if (SelectedItems.Count <= 1) { return false; }
            if (EditingGroup.DataList.Count < 1) { return false; }
            if (EditingGroup.DataList.Count == SelectedItems.Count) { return false; }
            return true;
        }

        /// <summary>
        /// 編集グループ内で現在選択されている項目から新しいグループを作成し、データリストを更新します。
        /// </summary>
        /// <remarks>このメソッドは、選択された項目を新しいグループにまとめ、位置とZオーダーを再計算し、
        /// 編集グループのデータリストを更新された構造に置き換えます。その後、新しいグループが選択されます。
        /// </remarks>
        [RelayCommand(CanExecute = nameof(CanAddGroupFromSelectedItems))]
        private void AddGroupFromSelectedItems()
        {
            if (EditingGroup is null) { return; }
            if (SelectedItems.Count <= 1) { return; }
            if (EditingGroup.DataList.Count < 1) { return; }
            if (EditingGroup.DataList.Count == SelectedItems.Count) { return; }

            // 新グループのZを先に計算しておく
            // 新グループのZ = 選択Itemの最上層Z - (選択個数 - 1)
            int groupZ = SelectedItems.Max(n => n.Z) - (SelectedItems.Count - 1);


            // 新リスト作成、非選択Itemを詰め込む
            var newDataList = new ObservableCollection<Data>();
            foreach (var item in EditingGroup.DataList)
            {
                if (item.IsSelected == false) { newDataList.Add(item); }
            }


            // 新グループ作成、そのDataListに選択Itemを順番に追加
            GroupData newGroup = new()
            {
                RootData = this.RootData,
                IsSelectable = true,
                ParentData = EditingGroup,
            };
            var sortedItems = SelectedItems.OrderBy(n => n.Z).ToArray();
            for (int i = 0; i < sortedItems.Length; i++)
            {
                newGroup.DataList.Add(sortedItems[i]);
            }

            // 新グループを新リストに挿入            
            newDataList.Insert(groupZ, newGroup);

            // 新グループと子要素の座標調整
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double right = 0;
            double bottom = 0;
            foreach (var item in newGroup.DataList)
            {
                if (minX > item.X) { minX = item.X; }
                if (minY > item.Y) { minY = item.Y; }
                if (right < item.X + item.Width) { right = item.X + item.Width; }
                if (bottom < item.Y + item.Height) { bottom = item.Y + item.Height; }
            }
            newGroup.X = minX; newGroup.Y = minY;
            newGroup.Width = right - minX;
            newGroup.Height = bottom - minY;

            // 子要素の座標調整、ついでにIsSelectableとIsSelectedをfalseに変更
            foreach (var item in newGroup.DataList)
            {
                item.X -= minX;
                item.Y -= minY;
                item.IsSelectable = false;
                item.IsSelected = false;
            }

            // 新リストの要素のZを整える
            for (int i = 0; i < newDataList.Count; i++) { newDataList[i].Z = i; }

            // 今の全体リストと新リストを入れ替えて完了
            EditingGroup.DataList = newDataList;

            // 選択Itemリストを整える
            ClearSelectedItems();
            AddDataToSelectedItems(newGroup);// 新グループを選択状態にする

            AddGroupFromSelectedItemsCommand.NotifyCanExecuteChanged();
        }
        #endregion グループ化

        #region Z

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
        #endregion Z

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

        /// <summary>
        /// SelectedItemsに指定したDataを追加する
        /// </summary>
        /// <param name="data"></param>
        public void AddDataToSelectedItems(Data data)
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
        // 追加後はSelectedをクリアして、追加Itemを選択状態にする、Currentにする
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
            ClearSelectedItems();
            AddDataToSelectedItems(data);

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
                if (item.IsClicked) { ClickedItem = null; }
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
                    //newData.Z = DataList.Count - 1;
                    newData.Z = e.NewStartingIndex;
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

            // 子要素の座標更新
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

    #region 図形
    public partial class GeoShapeData : ShapeData
    {
        [ObservableProperty] private PointCollection _points = [];
        [ObservableProperty] private PenLineCap _strokeEndLineCap = PenLineCap.Flat;
        [ObservableProperty] private PenLineCap _strokeStartLineCap = PenLineCap.Flat;
        [ObservableProperty] private PenLineJoin _strokeLineJoin = PenLineJoin.Miter;
        [ObservableProperty] private double _strokeMiterLimit = 1.0;
        [ObservableProperty] private bool _isOffset;


        partial void OnIsOffsetChanged(bool value)
        {
            if (value)
            {
                X += OriginBounds.X;
                Y += OriginBounds.Y;
            }
            else
            {
                X -= OriginBounds.X;
                Y -= OriginBounds.Y;
            }
        }
    }

    //public partial class GeoLineData : ShapeData
    //{
    //    [ObservableProperty] private PointCollection _points = [];
    //    [ObservableProperty] private PenLineCap _strokeEndLineCap = PenLineCap.Flat;
    //    [ObservableProperty] private PenLineCap _strokeStartLineCap = PenLineCap.Flat;
    //    [ObservableProperty] private PenLineJoin _strokeLineJoin = PenLineJoin.Miter;
    //    [ObservableProperty] private double _strokeMiterLimit =1.0;

    //}

    public partial class PolylineData : ShapeData
    {
        public PolylineData()
        {
            StrokeThickness = 20;
            Stroke = Brushes.Red;
            Points = new PointCollection([new Point(), new Point(100, 0)]);
        }
        [ObservableProperty] private PointCollection _points = [];
    }

    public partial class EllipseData : ShapeData { }

    public partial class RectangleData : ShapeData { }


    public abstract partial class ShapeData : Data
    {
        [ObservableProperty] private Brush? _fill;
        [ObservableProperty] private Brush _stroke = new SolidColorBrush(Color.FromArgb(200, 0, 250, 200));
        [ObservableProperty] private double _strokeThickness = 1.0;
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
        [ObservableProperty] private Brush? _background;
        [ObservableProperty] private Rect _bounds = new();
        [ObservableProperty] private Rect _originBounds = new();
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