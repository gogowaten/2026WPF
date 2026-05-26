using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace _20260510
{
    public partial class RootData : GroupData
    {
        // 選択状態の要素の枠線表示の有無
        [ObservableProperty] private bool _isVisbleSelectedBorder = true;
        // Groupの枠線表示の有無
        [ObservableProperty] private bool _isVisbleGroupBorder = true;

        // 編集グループにData追加する時の、Dataの追加座標決定に使う、Currentからの距離
        [ObservableProperty] private double _shiftHorizontal = 32.0;
        [ObservableProperty] private double _shiftVertical = 32.0;


        //// システムのDPI
        //public double MyDPI { get; set; } = 96.0;

        // TextBlock追加時に使う文字列用
        [NotifyCanExecuteChangedFor(nameof(AddTextBlockDataCommand))]
        [ObservableProperty] private string _addText = "ここに文字列";

        [ObservableProperty] private GroupData _editingGroupData;
        //[ObservableProperty] private GroupData _editingGroupData; // 編集中のGroupData


        [ObservableProperty] private Data? _currentItemData; // CurrentData

        [ObservableProperty] private Data? _clickedItemData; // クリックしたData
        [ObservableProperty] public CustomThumb? _myClickedItem;

        //[NotifyCanExecuteChangedFor(nameof(ZAgeCommand))]
        [ObservableProperty] private ObservableCollection<Data> _selectedItemsData = []; // 選択ItemData

        //[NotifyCanExecuteChangedFor(nameof(ZUpCommand))]
        //[NotifyCanExecuteChangedFor(nameof(ZtoTopCommand))]




        //public DataService MyService { get; } = new();


        public RootData()
        {
            Name = "RootDataです";
            EditingGroupData = this;
            //MyInit();
            SelectedItemsData.CollectionChanged += SelectedItems_CollectionChanged;

        }

        #region 起動時

        #endregion 起動時


        #region On～プロパティの変更時

        // クリックItem
        partial void OnClickedItemDataChanged(Data? oldValue, Data? newValue)
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


        // Current変更後
        partial void OnCurrentItemDataChanged(Data? oldValue, Data? newValue)
        {
            if (oldValue is not null)
            {
                oldValue.IsCurrent = false;
            }

            if (newValue is not null)
            {
                newValue.IsCurrent = true;

                // グループ解除可否判定通知
                UnGroupCurrentCommand.NotifyCanExecuteChanged();

                // 編集可否判定通知
                EditingCurrentGroupCommand.NotifyCanExecuteChanged();

                SaveCurrentItemToPngImageFileCommand.NotifyCanExecuteChanged();
            }
        }


        // 編集中グループの変更時
        partial void OnEditingGroupDataChanged(GroupData? oldValue, GroupData newValue)
        {
            // 選択リストを空にする
            ClearSelectedItems(); // Currentはnullになる

            // 旧グループ、
            if (oldValue is not null)
            {
                oldValue.IsEditing = false;
                // 子要素のIs系の更新
                foreach (var item in oldValue.DataList)
                {
                    item.IsSelectable = false;
                    item.IsSelected = false;
                    item.IsCurrent = false;
                }
            }

            // 新グループ、
            if (newValue is not null)
            {
                newValue.IsEditing = true;
                // 子要素を選択可能にする
                foreach (var item in newValue.DataList)
                {
                    item.IsSelectable = true;
                }

                // クリックItemが子要素に在れば、それを選択状態にしてCurrentに指定する
                if (ClickedItemData is not null && newValue.DataList.Contains(ClickedItemData))
                {
                    AddDataToSelectedItems(ClickedItemData);
                }
            }

            // 編集可否判定通知
            EditingUpperGroupCommand.NotifyCanExecuteChanged();

        }


        #endregion On～プロパティの変更時



        public bool CanChageGeoShapeData()
        {
            return CurrentItemData is GeoShapeData;
        }

        //選択状態のData変更時
        private void SelectedItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems?[0] is Data newData)
                {
                    newData.IsSelected = true;
                    RemoveSelectedItemsCommand.NotifyCanExecuteChanged(); // 削除Command実行判定
                    ZUpSelectedItemsCommand.NotifyCanExecuteChanged();
                    ZtoTopCommand.NotifyCanExecuteChanged();
                    ZDownSelectedItemsCommand.NotifyCanExecuteChanged();
                    ZtoBottomCommand.NotifyCanExecuteChanged();
                    AddGroupFromSelectedItemsCommand.NotifyCanExecuteChanged();
                    UnGroupCurrentCommand.NotifyCanExecuteChanged();
                    SaveCurrentItemToPngImageFileCommand.NotifyCanExecuteChanged();
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems?[0] is Data oldData)
                {
                    oldData.IsSelected = false;
                    oldData.IsCurrent = false;
                    if (CurrentItemData == oldData) { CurrentItemData = null; }
                    RemoveSelectedItemsCommand.NotifyCanExecuteChanged(); // 削除Command実行判定
                    ZUpSelectedItemsCommand.NotifyCanExecuteChanged();
                    ZtoTopCommand.NotifyCanExecuteChanged();
                    ZDownSelectedItemsCommand.NotifyCanExecuteChanged();
                    ZtoBottomCommand.NotifyCanExecuteChanged();
                    AddGroupFromSelectedItemsCommand.NotifyCanExecuteChanged();
                    //UnGroupCommand.NotifyCanExecuteChanged();
                    UnGroupCurrentCommand.NotifyCanExecuteChanged();
                    SaveCurrentItemToPngImageFileCommand.NotifyCanExecuteChanged();
                }
            }
        }


        #region メソッド





        private bool CanCurrentSave()
        {
            return (CurrentItemData is not null) && CurrentItemData.Content is not null;
        }

        [RelayCommand(CanExecute = nameof(CanCurrentSave))]
        public void SaveCurrentItemToPngImageFile()
        {
            if (CurrentItemData is null) { return; }
            if (CurrentItemData.Content is null) { return; }

            // ファイル保存Dialog作成
            SaveFileDialog dialog = Manager.MakeSaveFileDialogFileNameyyyyMMddHHmmss();

            // Dialog表示、pngで保存
            if (dialog.ShowDialog() == true)
            {

                string filePath = dialog.FileName;
                var bmp = Manager.MakeBitmapFromLayoutTransformElement(CurrentItemData.Content);

                PngBitmapEncoder encoder = new();
                encoder.Frames.Add(BitmapFrame.Create(bmp));

                using FileStream stream = File.OpenWrite(filePath);
                encoder.Save(stream);
            }

        }

        #region パブリックメソッド
        // 追加先のZの種類は以下を選べるようにしたい
        // CurrentのZの1個上
        // CurrentのZの1個下
        // 編集グループ内の一番上
        // 編集グループ内の一番下

        // Dataを編集グループに追加、Currentの近傍に追加
        public void AddDataToCurrentNeighborhood(Data data)
        {
            if(CurrentItemData is null) { return; }
            double x = CurrentItemData.X;
            double y = CurrentItemData.Y;
            x += ShiftHorizontal;
            y += ShiftVertical;
            data.X = x;
            data.Y = y;
            data.RootData = this;
            EditingGroupData.AddData
        }

        /// <summary>
        /// EditingGroupのDataListにDataを挿入
        /// </summary>
        /// <param name="data"></param>
        /// <param name="insert"></param>
        public void AddDataToEditingGroup(Data data, int insert)
        {
            data.RootData = this;
            EditingGroupData.DataList.Insert(insert, data);
        }

        /// <summary>
        /// EditingGroupのDataListの末尾にDataを追加
        /// </summary>
        /// <param name="data"></param>
        public void AddDataToEditingGroup(Data data)
        {
            data.RootData = this;
            EditingGroupData.DataList.Add(data);
        }




        #endregion パブリックメソッド

        #region グループ化


        private bool CanUnGroup()
        {
            if (SelectedItemsData.Count == 0) { return false; }
            if (CurrentItemData is not GroupData) { return false; }
            if (EditingGroupData is null) { return false; }
            return true;
        }


        /// <summary>
        /// 今のグループを解除する
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanUnGroup))]
        private void UnGroupCurrent()
        {
            if (CanUnGroup() == false) { return; }

            if (CurrentItemData is GroupData targetGroupData)
            {
                // 対象グループのZIndexを記録、これが分解したData群の追加先基準になる
                int zIndex = targetGroupData.Z;

                // 親要素から対象グループのData削除
                EditingGroupData.DataList.Remove(targetGroupData);

                // Selectedを空する
                ClearSelectedItems();

                // CurrentDataにするDataのZIndex
                int newCurrentDataZIndex = zIndex;

                // 親要素にData群を追加、同時に選択状態にする
                for (int i = 0; i < targetGroupData.DataList.Count; i++)
                {
                    var data = targetGroupData.DataList[i];
                    if (data.IsClicked) { newCurrentDataZIndex += i; } // Clickedがあれば、それのZIndexを記録
                    data.IsSelectable = true; // 選択状態可能にしておく
                    data.X += targetGroupData.X; // Data群のx,y調整
                    data.Y += targetGroupData.Y;
                    AddDataToEditingGroup(data, i + zIndex); // zの調整は追加メソッド先で行われる
                    AddDataToSelectedItems(data); // 選択状態にする
                }

                CurrentItemData = DataList[newCurrentDataZIndex];

                UnGroupCurrentCommand.NotifyCanExecuteChanged();
            }
        }


        ///// <summary>
        ///// 現在選択されているグループ項目をグループ解除し、子要素を親グループに移動させ、選択状態と位置を更新します。
        ///// </summary>
        ///// <remarks>このメソッドは、現在の項目がグループであり、かつ編集グループが存在する場合にのみ使用できます。
        ///// グループ解除後、グループのすべての子要素が選択され、親グループに対する位置が調整されます。
        ///// グループ自体は親グループから削除されます。この操作により、
        ///// 選択状態が更新され、必要に応じてコマンド状態の変更が通知されます。
        ///// </remarks>
        //[RelayCommand(CanExecute = nameof(CanUnGroup))]
        //private void UnGroup()
        //{
        //    if (CurrentItemData is not GroupData) { return; }
        //    if (EditingGroupData is null) { return; }

        //    if (CurrentItemData is GroupData targetGroupData)
        //    {
        //        // ClickedItemチェック
        //        if (ClickedItemData == targetGroupData) { ClickedItemData = null; }

        //        // 親要素のDataListにバラした要素を順番に挿入
        //        int z = targetGroupData.Z;
        //        for (int i = targetGroupData.DataList.Count - 1; i >= 0; i--)
        //        {
        //            var item = targetGroupData.DataList[i];
        //            EditingGroupData.DataList.Insert(z, item);
        //            item.IsSelectable = true;
        //            item.IsSelected = true;
        //        }

        //        // 子要素の座標調整
        //        foreach (var item in targetGroupData.DataList)
        //        {
        //            item.X += targetGroupData.X;
        //            item.Y += targetGroupData.Y;
        //        }

        //        // 子要素全体のZを整える
        //        for (int i = 0; i < EditingGroupData.DataList.Count; i++)
        //        {
        //            EditingGroupData.DataList[i].Z = i;
        //        }
        //        // 選択Itemを整える、解除したグループの子要素を選択状態にする
        //        ClearSelectedItems();
        //        foreach (var item in targetGroupData.DataList)
        //        {
        //            AddDataToSelectedItems(item);
        //        }

        //        // 解除するDataを外す
        //        EditingGroupData.DataList.Remove(targetGroupData);
        //        //RemoveDataFromSelect(targetGroupData);
        //        targetGroupData.IsClicked = false;
        //        targetGroupData.IsSelectable = false;
        //        targetGroupData.IsSelected = false;
        //        targetGroupData.DataList.Clear(); // 要る？

        //        // 選択ItemにClickedItemが在ればそれをCurrentItemにする
        //        if (ClickedItemData is not null && ClickedItemData.IsSelected) { CurrentItemData = ClickedItemData; }


        //        UnGroupCommand.NotifyCanExecuteChanged();
        //    }




        //}

        /// <summary>
        /// グループ化チェック
        /// </summary>
        /// <returns></returns>
        private bool CanAddGroupFromSelectedItems()
        {
            return EditingGroupData is not null
                && SelectedItemsData.Count > 1
                && EditingGroupData.DataList.Count >= 1;

            //if (EditingGroupData is null) { return false; } // 編集中グループがない
            //if (SelectedItemsData.Count <= 1) { return false; } // 選択Item個数が1個以下
            //if (EditingGroupData.DataList.Count < 1) { return false; } // 編集中グループのこ要素数が1未満

            //return true;

        }

        /// <summary>
        /// グループ化
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanAddGroupFromSelectedItems))]
        private void AddGroupFromSelectedItems()
        {
            if (CanAddGroupFromSelectedItems() == false) { return; }

            // 新グループのZを先に計算しておく
            // 新グループのZ = 選択Itemの最上層Z - (選択個数 - 1)
            int newGroupZIndex = SelectedItemsData.Max(n => n.Z) - (SelectedItemsData.Count - 1);

            // 選択アイテムのBoundsを計算、これが新GのBoundsになるし、その子要素の座標調整にも使う
            var newBounds = GetBounds(SelectedItemsData);

            // 新グループ作成
            var newGroup = new GroupData()
            {
                ParentData = EditingGroupData,
                RootData = this,

                X = newBounds.X,
                Y = newBounds.Y,
                Width = newBounds.Width,
                Height = newBounds.Height,
            };

            // SelectedをZIndex順にソートした一時的なリストを作成
            var tempSortedList = SelectedItemsData.OrderBy(item => item.Z).ToArray();

            // ItemDataはは元グループから削除してから、新グループに追加する、
            // この順番が逆だとparentがnullになってしまう

            // 該当DataをSelectedと親要素のDataListから削除
            for (int i = tempSortedList.Length - 1; i >= 0; i--)
            {
                EditingGroupData.DataList.Remove(tempSortedList[i]); // このときparentがnullになる
                SelectedItemsData.Remove(tempSortedList[i]);
            }

            // 順にx,yを調整してから、新グループに追加
            foreach (var item in tempSortedList)
            {
                item.X -= newBounds.X;
                item.Y -= newBounds.Y;
                item.IsSelectable = false;
                item.IsSelected = false;
                newGroup.AddData(item);
            }

            // 親要素に新グループを追加（挿入）
            AddDataToEditingGroup(newGroup, newGroupZIndex);

            // 新グループをSelectedにする            
            AddDataToSelectedItems(newGroup);

            // 通知
            AddGroupFromSelectedItemsCommand.NotifyCanExecuteChanged();

        }



        #endregion グループ化




        #region Z

        // 選択Itemを最背面へ移動
        [RelayCommand(CanExecute = nameof(CanZDown))]
        private void ZtoBottom()
        {
            if (EditingGroupData is null) { return; }

            // 選択Item全体の移動距離を計算、一番下のItemが0になる値
            // = 0 - 一番下のItemのZ
            ZMove(0 - SelectedItemsData.Min(n => n.Z));
        }

        // 背面へ移動
        [RelayCommand(CanExecute = nameof(CanZDown))]
        private void ZDownSelectedItems()
        {
            if (EditingGroupData is null) { return; }

            ZMove(-1);
        }


        /// </summary>
        /// <remarks>編集グループ内の選択されたアイテムのZオーダーを更新し、
        /// 関連するコマンドの状態を更新します</remarks>
        /// <param name="distination">Zオーダー内で選択されたアイテムを移動するオフセット。正の値はアイテムを前方に移動させ、
        /// 負の値はアイテムを後方に移動させます。</param>
        private void ZMove(int distination)
        {
            if (EditingGroupData is null) { return; }

            // 新リスト作成、非選択Itemを詰め込む
            var newList = new ObservableCollection<Data>();
            foreach (var item in EditingGroupData.DataList)
            {
                if (item.IsSelected == false)
                {
                    newList.Add(item);
                }
            }

            // 新リストに選択Itemを順番に挿入、場所は移動距離(方向)を加味
            var sorted = SelectedItemsData.OrderBy(n => n.Z).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                newList.Insert(sorted[i].Z + distination, sorted[i]);
            }

            // ItemのZをIndexに合わせる
            for (int i = 0; i < newList.Count; i++) { newList[i].Z = i; }

            // リストの入れ替え
            EditingGroupData.DataList = newList;


            ZDownSelectedItemsCommand.NotifyCanExecuteChanged();
            ZtoBottomCommand.NotifyCanExecuteChanged();
            ZUpSelectedItemsCommand.NotifyCanExecuteChanged();
            ZtoTopCommand.NotifyCanExecuteChanged();
        }

        private bool CanZDown()
        {
            // 編集モードのグループが在る
            if (EditingGroupData is null) { return false; }

            // 選択Item在る
            int selectCount = SelectedItemsData.Count;
            if (selectCount == 0) { return false; }

            // 選択Item個数は子要素個数より少ない
            if (selectCount >= EditingGroupData.DataList.Count) { return false; }

            // 選択Itemに最下層のItemが含まれていない
            foreach (var item in SelectedItemsData)
            {
                if (item.Z == 0) { return false; }
            }
            return true;
        }




        // 選択Itemを最前面へ移動
        [RelayCommand(CanExecute = nameof(CanZUp))]
        private void ZtoTop()
        {
            if (EditingGroupData is null) { return; }

            // 選択Itemが最前面になるまでの上げ幅を取得
            int mi = SelectedItemsData.Max(n => n.Z);
            int agehaba = EditingGroupData.DataList.Count - 1 - mi;
            ZMove(agehaba);
        }


        // Z、選択Itemを上に移動、ZIndexを1増やす
        [RelayCommand(CanExecute = nameof(CanZUp))]
        public void ZUpSelectedItems()
        {
            if (EditingGroupData is null) { return; }
            ZMove(1);
        }



        private bool CanZUp()
        {
            // 編集モードのグループが在る
            if (EditingGroupData is null) { return false; }

            // 選択Item在る
            int selectCount = SelectedItemsData.Count;
            if (selectCount == 0) { return false; }

            // 選択Item個数は子要素個数より少ない
            if (selectCount >= EditingGroupData.DataList.Count) { return false; }

            // 選択Itemに最上層のItemが含まれていない
            int max = EditingGroupData.DataList.Count - 1;
            foreach (var item in SelectedItemsData)
            {
                if (item.Z == max) { return false; }
            }
            return true;
        }
        #endregion Z

        #region 編集モード

        // 指定グループを編集モードにする
        public void MigrateEditingGroup(GroupData group) { EditingGroupData = group; }


        public bool CanEditingUpperGroup() => EditingGroupData?.ParentData is not null;

        // 1つ上を編集モードにする
        [RelayCommand(CanExecute = nameof(CanEditingUpperGroup))]
        public void EditingUpperGroup()
        {
            if (EditingGroupData?.ParentData is GroupData upper)
            {
                EditingGroupData = upper;
            }
        }

        public bool CanEditingCurrentGroup() => CurrentItemData is GroupData;

        // Currentを編集モードにする
        [RelayCommand(CanExecute = nameof(CanEditingCurrentGroup))]
        public void EditingCurrentGroup()
        {
            if (CurrentItemData is GroupData group) { EditingGroupData = group; }
        }

        #endregion 編集モード

        #region SelectedItems

        /// <summary>
        /// SelectedItemsに指定したDataを追加する
        /// 追加後にCurrentItemDataに指定する
        /// </summary>
        /// <param name="data"></param>
        public bool AddDataToSelectedItems(Data data)
        {
            // 二重登録禁止チェック
            if (SelectedItemsData.Contains(data)) { return false; }

            // 選択可能な場合のみ追加して、Currentに指定する
            if (data.IsSelectable)
            {
                SelectedItemsData.Add(data);
                CurrentItemData = data;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 指定Dataを選択リストから削除後、IsCurrentをFalseにする
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool RemoveDataFromSelectedWithUpdataIs(Data data)
        {
            if (SelectedItemsData.Remove(data))
            {
                // 削除完了後にIs系を更新
                data.IsCurrent = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 選択リストを空にする
        /// 削除したDataのIsCurrentをFalseにする
        /// CurrentItemDataをnullにする
        /// </summary>
        [RelayCommand]
        public void ClearSelectedItems()
        {
            // 今の選択リストから一時的なリストを作成
            var tempList = new List<Data>(SelectedItemsData);

            // 選択リストからDataを削除＆Is系を更新
            foreach (Data item in tempList)
            {
                _ = RemoveDataFromSelectedWithUpdataIs(item);
            }

            CurrentItemData = null;
        }

        // 
        // 

        /// <summary>
        /// 指定Data以外を選択リストから削除する
        /// </summary>
        /// <remarks>未選択Itemを通常クリック時と
        /// 移動なし＋通常クリックだったときに使用</remarks>
        /// <param name="data"></param>
        public void RemoveAllOtherFromSelected(Data data)
        {
            // 今の選択リストから、指定Dataを除いたリストを作成
            var tempList = new List<Data>(SelectedItemsData);
            _ = tempList.Remove(data);

            // 選択リストからDataを削除＆Is系を更新
            foreach (Data item in tempList)
            {
                _ = RemoveDataFromSelectedWithUpdataIs(item);
            }
        }



        /// <summary>
        /// 指定Dataを選択リストから削除する
        /// 削除後にCurrentを更新する、一個前のDataをCurrentにする、それがなければ一個後ろのData
        /// </summary>
        /// <param name="data"></param>
        public void RemoveDataFromSelect(Data data)
        {
            var dataIndex = SelectedItemsData.IndexOf(data) - 1;


            if (SelectedItemsData.Remove(data))
            {
                // CurrentItemを更新
                // 一個前をCurrentにする、一個前がなければ一個後をCurrentにする
                if (dataIndex < 0) { dataIndex++; }
                CurrentItemData = SelectedItemsData[dataIndex];
            }
        }


        #endregion SelectedItems



        // 選択状態のItemすべてをDataListから削除 ＆ 選択リストもクリア
        [RelayCommand(CanExecute = nameof(CanSelectedItemsRemove))]
        public void RemoveSelectedItems()
        {
            if (EditingGroupData is null) { return; }

            // リストから削除
            foreach (var item in SelectedItemsData)
            {
                EditingGroupData.DataList.Remove(item);
                if (item.IsClicked)
                {
                    ClickedItemData = null;
                    MyClickedItem = null;
                }
            }

            // 選択状態解除
            ClearSelectedItems();
        }


        // 選択状態のItemすべてを削除できるかの判定
        private bool CanSelectedItemsRemove()
        {
            return SelectedItemsData.Count > 0;
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
            EditingGroupData?.DataList.Add(data);
            data.IsSelectable = true;
            ClearSelectedItems();
            AddDataToSelectedItems(data);

        }

        // TextBlock追加できるかの判定用
        private bool CanAddTextBlockData()
        {
            // 文字が入力されている ＆ 編集モードのグループがある
            return !string.IsNullOrEmpty(AddText) && (EditingGroupData is not null);
        }




        // テスト用：Data追加
        public new void AddData(Data data)
        {
            data.RootData = this;

            if (data is GroupData group)
            {
                SetRootData(group);
            }

            void SetRootData(GroupData gd)
            {
                foreach (Data item in gd.DataList)
                {
                    item.RootData = this;
                    if (item is GroupData internalData)
                    {
                        SetRootData(internalData);
                    }
                }
            }

            DataList.Add(data);
        }

        #endregion メソッド




    }




    /*Group*/



    public partial class GroupData : Data
    {
        [ObservableProperty] private bool _isEditing; // 編集状態
        [ObservableProperty] private ObservableCollection<Data> _dataList = [];

        partial void OnDataListChanged(ObservableCollection<Data>? oldValue, ObservableCollection<Data> newValue)
        {
            if (oldValue is not null)
            {
                oldValue.CollectionChanged -= DataList_CollectionChanged;
            }

            if (newValue is not null)
            {
                newValue.CollectionChanged += DataList_CollectionChanged;
            }
        }

        public GroupData()
        {
            Name = "GroupData";
            DataList.CollectionChanged += DataList_CollectionChanged;
        }



        internal void DataList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Insertの場合もここAddになる
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems?[0] is Data newData)
                {
                    newData.Z = e.NewStartingIndex; // Zを追加先Indexに合わせる
                    newData.ParentData = this;
                    newData.ParentData.UpdateSize();

                    // 追加先Index以降のItemのZをIndexに合わせるために＋１する
                    for (int i = e.NewStartingIndex + 1; i < DataList.Count; i++)
                    {
                        DataList[i].Z++;
                    }

                    // 自身が編集中なら、子要素を選択可能にする
                    if (this.IsEditing)
                    {
                        newData.IsSelectable = true;
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems?[0] is Data oldData)
                {
                    // 削除した要素のIndexから上の要素のZを1下げる
                    int currentZ = e.OldStartingIndex;
                    for (int i = currentZ; i < DataList.Count; i++)
                    {
                        DataList[i].Z--;
                    }

                    // 以下のIs系は念のため
                    oldData.IsSelectable = false;
                    oldData.IsSelected = false;
                    oldData.IsCurrent = false;

                    oldData.ParentData?.UpdateSize();
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


        ///// <summary>
        ///// 特別、TextBlockなどサイズが確定していない要素を
        ///// まっさらなRootに追加した直後にRootのサイズを決定するのに使う
        ///// DataTemplateのXAMLからBehaviorで使う
        /////   xmlns:i="http://schemas.microsoft.com/xaml/behaviors">
        /////      <i:Interaction.Triggers>
        /////        <i:EventTrigger EventName = "Loaded" >
        /////          < i:InvokeCommandAction Command = "{Binding RootData.UpdateRootSizeForNaNSizeElementCommand}" />
        /////        </ i:EventTrigger>
        /////      </i:Interaction.Triggers>
        ///// </summary>
        //[RelayCommand]
        //private void UpdateRootSizeForNaNSizeElement()
        //{
        //    if (DataList.Count == 1 && Width == 0)
        //    {
        //        Width = DataList[0].Width;
        //        Height = DataList[0].Height;
        //    }
        //}


        /// <summary>
        /// グループ自身のサイズ更新
        /// </summary>
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
            Width = right;
            Height = bottom;
        }


        /// <summary>
        /// 指定GroupのBounds更新して、Rootまで行くBounds更新
        /// </summary>
        /// <param name="group"></param>
        public void UpdateBoundsToRoot(GroupData group)
        {
            double right = 0;
            double bottom = 0;
            double left = double.MaxValue;
            double top = double.MaxValue;
            foreach (var item in group.DataList)
            {
                left = Math.Min(left, item.X);
                top = Math.Min(top, item.Y);
                right = Math.Max(right, item.X + item.Width);
                bottom = Math.Max(bottom, item.Y + item.Height);
            }

            // サイズ更新
            group.Width = right - left;
            group.Height = bottom - top;

            // 子要素の座標更新
            foreach (Data item in group.DataList) { item.X -= left; item.Y -= top; }

            // 自身の座標更新
            X += left;
            Y += top;

            // 親要素へ伝播
            group.ParentData?.UpdateBoundsToRoot(group.ParentData);
        }

        public void UpdateBoundsToRoot()
        {
            UpdateBoundsToRoot(this);
        }

        // DataListのBoundsを計算
        public Rect GetBounds(ObservableCollection<Data> datas)
        {
            if (datas.Count == 0) { return new Rect(); }
            double right = 0;
            double bottom = 0;
            double left = double.MaxValue;
            double top = double.MaxValue;
            foreach (var item in datas)
            {
                left = Math.Min(left, item.X);
                top = Math.Min(top, item.Y);
                right = Math.Max(right, item.X + item.Width);
                bottom = Math.Max(bottom, item.Y + item.Height);
            }
            Rect r = new(left, top, right, bottom)
            {
                Width = right - left,
                Height = bottom - top
            };
            return r;
        }

        #region パブリックメソッド

        public void AddData(Data data)
        {
            data.RootData = this.RootData;
            // RootのDataListに追加するときはok
            // Groupに追加するときは、GroupのDataにRootがあればそれでいいけど
            // ない場合はnullのまま
            // で、Rootに追加するのがGroupだった場合は、GroupのRootDataを指定するのはもちろんで、
            // 子要素以下全てのDataのRootDataを指定する
            data.ParentData = this;
            DataList.Add(data);
        }

        #endregion パブリックメソッド

    }


    #region 図形


    public partial class GeoLineData : GeoShapeData
    {

        public GeoLineData()
        {
            Name = "GeoLineData";
            //#if DEBUG
            //            Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.ReflectedType?.Name}__{MethodBase.GetCurrentMethod()?.Name}");
            //#endif
        }

        //[ObservableProperty] private bool _isCanDragMove;

        //[ObservableProperty] private bool _isVisibleVertexHandles;

        //[ObservableProperty] private double _vertexHandleSize = 50.0; // これはアプリ全体の設定に移動させたほうが良い？
        //[ObservableProperty] private Brush _vertexHandleFillBrush; // これはアプリ全体の設定に移動させたほうが良い？


    }


    public partial class GeoShapeData : ShapeData
    {
        [ObservableProperty] private ObservableCollection<Point> _points = [];
        [ObservableProperty] private bool _isVertexHandle;

    }



    public partial class EllipseData : ShapeData
    {
        public EllipseData() { Name = "EllipseData"; }
    }

    public partial class RectangleData : ShapeData
    {
        public RectangleData()
        {
            Name = "RectangleData";
        }
    }


    public abstract partial class ShapeData : Data
    {
        [ObservableProperty] private Brush? _fill;
        [ObservableProperty] private Brush _stroke = new SolidColorBrush(Color.FromArgb(200, 0, 250, 200));
        [ObservableProperty] private double _strokeThickness = 1.0;
        [ObservableProperty] private PenLineCap _strokeEndLineCap = PenLineCap.Flat;
        [ObservableProperty] private PenLineCap _strokeStartLineCap = PenLineCap.Flat;
        [ObservableProperty] private PenLineJoin _strokeLineJoin = PenLineJoin.Miter;
        [ObservableProperty] private double _strokeMiterLimit = 10.0;
        //[ObservableProperty] private Pen? _strokePen; // 保存対象外にする


    }
    #endregion 図形


    public partial class TextBlockData : TextData
    {
        public TextBlockData()
        {
            Name = "TextBlockData";
        }
    }
    public abstract partial class TextData : Data
    {
        [ObservableProperty] private string _text = string.Empty;
        [ObservableProperty] private string _fontName = SystemFonts.MessageFontFamily.ToString();
        [ObservableProperty] private double _fontSize = SystemFonts.MessageFontSize;
        [ObservableProperty] private Brush? _foreground = Brushes.Black;
        [ObservableProperty] private Brush? _background = Brushes.Transparent;



    }


    /// <summary>
    /// dataclassbase
    /// </summary>
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
        [ObservableProperty] bool _isCurrent = false; // Current
        [ObservableProperty] bool _isClicked = false; // クリックされた要素
        [ObservableProperty] private double _offsetX;
        [ObservableProperty] private double _offsetY;
        //[ObservableProperty] private double _angle; // 回転角度
        public Rect LayoutTransformedRect { get; internal set; }

        // 表示している要素自体を記録用、画像として保存とかに使う
        [ObservableProperty] private FrameworkElement? _content;


        // 自身の座標変更時は親要素を変更しないほうが良さそう、負荷が高いのも在る
        // 移動後に変更する


    }
}