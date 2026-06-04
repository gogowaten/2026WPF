using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace _20260510
{
    #region Data追加時のZIndexの指定モード

    /// <summary>
    /// Enumとboolの変換、ModeZIndexのラジオボタンで使っている
    /// </summary>
    public class MyConvEnumToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || parameter is null) { return false; }
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || !(bool)value)
            {
                return DependencyProperty.UnsetValue;
            }

            string? mode = parameter.ToString();
            if (string.IsNullOrEmpty(mode))
            {
                return DependencyProperty.UnsetValue;
            }
            return Enum.Parse(targetType, mode);
        }
    }

    /// <summary>
    /// Data追加時のZIndexの指定モード
    /// </summary>
    public enum ModeAddZIndex { Upper = 0, Under, Top, Bottom }
    #endregion Data追加時のZIndexの指定モード

    public partial class RootData : GroupData
    {
        // 選択状態の要素の枠線表示の有無
        [ObservableProperty] private bool _isVisbleSelectedBorder = true;
        // Groupの枠線表示の有無
        [ObservableProperty] private bool _isVisbleGroupBorder = true;

        [ObservableProperty] private Brush _groupBorderNormalColor = Brushes.DeepSkyBlue;
        [ObservableProperty] private Brush _groupBorderEditingColor = Brushes.Red;
        //[ObservableProperty] private Brush _groupBorderColor = Brushes.DeepSkyBlue;

        // 編集グループにData追加する時の、Dataの追加座標決定に使う、Currentからの距離
        [ObservableProperty] private double _shiftHorizontal = 32.0;
        [ObservableProperty] private double _shiftVertical = 32.0;
        [ObservableProperty] private ModeAddZIndex _shiftZIndexMode = ModeAddZIndex.Upper;


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
            RootData = this;
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

            // Clickedを編集モード移行の可否判定通知
            MigreteEditingFromClickedCommand.NotifyCanExecuteChanged();
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
                //oldValue.WakuIro = GroupBorderNormalColor;
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
                //newValue.WakuIro = GroupBorderEditingColor;

                // 子要素を選択可能にする
                foreach (var item in newValue.DataList)
                {
                    item.IsSelectable = true;
                }

                // 中止：個々で処理するべきではない気がする
                //// クリックItemが子要素に在れば、それを選択状態にしてCurrentに指定する
                //if (ClickedItemData is not null && newValue.DataList.Contains(ClickedItemData))
                //{
                //    AddDataToSelectedItems(ClickedItemData);
                //}
            }

            // 編集可否判定通知
            EditingUpperGroupCommand.NotifyCanExecuteChanged();
            MigreteEditingFromClickedCommand.NotifyCanExecuteChanged();
            EditingCurrentGroupCommand.NotifyCanExecuteChanged();
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




        #region Can～コマンドの実行可否の判定

        // Clickedの編集モード移行可否判定通知
        public bool CanMigrateEditingFromClicked()
        {
            return ClickedItemData is GroupData group
                && !group.IsEditing;
        }


        // CurrentItemDataが編集モード移行可否
        public bool CanEditingCurrentGroup() => CurrentItemData is GroupData;

        // Rootを画像として保存の可否判定
        private bool CanSaveRootToPngImageFile()
        {
            // true：Itemが存在 ＆ Contentに自身が入っている
            return DataList.Count > 0 && Content is not null;
        }

        // 編集モードをParentに移行するの可否判定
        public bool CanEditingUpperGroup() => EditingGroupData?.ParentData is not null;

        // Currentを画像として保存の可否判定
        private bool CanCurrentSave()
        {
            return (CurrentItemData is not null) && CurrentItemData.Content is not null;
        }

        // 全削除の可否判定
        private bool CanRemoveAll()
        {
            return DataList.Count >= 1;
        }


        // 選択状態のItemすべてを削除できるかの判定
        private bool CanSelectedItemsRemove()
        {
            return SelectedItemsData.Count > 0;
        }

        // グループ解除の可否判定
        private bool CanUnGroup()
        {
            if (SelectedItemsData.Count == 0) { return false; }
            if (CurrentItemData is not GroupData) { return false; }
            if (EditingGroupData is null) { return false; }
            return true;
        }

        /// <summary>
        /// グループ化の可否判定
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

        // ZDownの可否判定
        private bool CanZDown()
        {
            // 編集中モードのグループが在る
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

        // ZUpの可否判定
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


        // TextBlock追加できるかの判定用
        private bool CanAddTextBlockData()
        {
            // 文字が入力されている ＆ 編集モードのグループがある
            return !string.IsNullOrEmpty(AddText) && (EditingGroupData is not null);
        }

        #endregion Can～コマンドの実行可否の判定

        #region メソッド


        #region グループ化

        // 指定グループをグループ解除する
        private void UnGroup(GroupData group)
        {
            if (group.ParentData is null) { return; }

            // 対象グループのZIndexを記録、これが分解したData群の追加先基準になる
            int zIndex = group.Z;

            var parent = group.ParentData;

            // 親要素から対象グループのData削除
            group.ParentData.DataList.Remove(group);

            // Selectedを空する
            ClearSelectedItems();

            // CurrentDataにするDataのZIndex
            int newCurrentDataZIndex = zIndex;

            // 親要素にData群を追加、同時に選択状態にする
            for (int i = 0; i < group.DataList.Count; i++)
            {
                var data = group.DataList[i];
                if (data.IsClicked) { newCurrentDataZIndex += i; } // Clickedがあれば、それのZIndexを記録
                data.IsSelectable = true; // 選択状態可能にしておく
                data.X += group.X; // Data群のx,y調整
                data.Y += group.Y;
                AddDataToGroup(parent, data, i + zIndex, isUpdateBounds: false);// zの調整は追加メソッド先で行われる
                //AddDataToEditingGroup(data, i + zIndex); 
                AddDataToSelectedItems(data); // 選択状態にする
            }

            //CurrentItemData = DataList[newCurrentDataZIndex];

            UnGroupCurrentCommand.NotifyCanExecuteChanged();
        }


        /// <summary>
        /// グループ解除する。Currentがグループの場合のみ
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

                    // Data追加、zの調整は追加メソッド先で行われる
                    AddDataToEditingGroup(data, i + zIndex, IsUpdateBounds: false);
                    AddDataToSelectedItems(data); // 選択状態にする
                }

                CurrentItemData = DataList[newCurrentDataZIndex];

                UnGroupCurrentCommand.NotifyCanExecuteChanged();
            }
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
            var newBounds = Manager.GetBounds(SelectedItemsData);
            if (newBounds.IsEmpty) { return; }

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
                //newGroup.AddData(item);
                item.RootData = this;
                item.ParentData = newGroup;
                newGroup.DataList.Add(item);
            }

            // 親要素に新グループを追加（挿入）
            AddDataToEditingGroup(newGroup, newGroupZIndex, IsUpdateBounds: true);

            // 新グループをSelectedにする            
            AddDataToSelectedItems(newGroup);

            // 通知
            AddGroupFromSelectedItemsCommand.NotifyCanExecuteChanged();

        }



        #endregion グループ化

        #region Z移動

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



        #endregion Z移動

        #region 編集モード

        /// <summary>
        /// Clickedを編集モードへ移行
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanMigrateEditingFromClicked))]
        public void MigreteEditingFromClicked()
        {
            if (ClickedItemData is GroupData group)
            {
                MigrateEditingGroup(group);
            }
        }

        // 指定グループを編集モードにする
        public void MigrateEditingGroup(GroupData group)
        {
            EditingGroupData = group;
        }


        // 1つ上を編集モードにする
        /// <summary>
        /// 編集モードをParentに移行する
        /// もし、子要素が1個の場合はグループ解除により移行する
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEditingUpperGroup))]
        public void EditingUpperGroup()
        {
            if (EditingGroupData?.ParentData is GroupData parent)
            {
                // 処理後にCurrentにするため、元のグループDataを記録
                Data nextCurrentData = EditingGroupData;

                // 子要素が1個の場合はグループ解除してから移行する
                if (EditingGroupData.DataList.Count == 1)
                {
                    nextCurrentData = EditingGroupData.DataList[0];
                    UnGroup(EditingGroupData);
                }

                // 移行してから、 元のグループを選択状態にする
                EditingGroupData = parent;
                AddDataToSelectedItems(nextCurrentData);
            }
        }


        // Currentを編集モードにする
        [RelayCommand(CanExecute = nameof(CanEditingCurrentGroup))]
        public void EditingCurrentGroup()
        {
            if (CurrentItemData is GroupData group) { EditingGroupData = group; }
        }

        #endregion 編集モード

        #region SelectedItems

        /// <summary>
        /// DataをSelectedItemsに追加する
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


        #region 削除

        /// <summary>
        /// 全削除
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRemoveAll))]
        public void RemoveAll()
        {

            DataList.Clear();
            // その他Clearするのは、Selected、Current、Clicked、
            ClearSelectedItems();
            CurrentItemData = null;
            MyClickedItem = null;
            ClickedItemData = null;


            // EditingGroupをRootに移行
            MigrateEditingGroup(this);

            UpdateBoundsEditingToRoot();
        }

        // 選択状態のItemすべてをDataListから削除 ＆ 選択リストもクリア
        /// <summary>
        /// 選択されているItemを削除する
        /// すべての子要素が選択されている場合はGroup自体を削除する
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSelectedItemsRemove))]
        public void RemoveSelectedItems()
        {
            int selectedCount = SelectedItemsData.Count;
            if (selectedCount == 0) { return; }


            int nokori = EditingGroupData.DataList.Count - selectedCount;
            // 削除後にItemが1個以上残る or Rootが編集モード
            if (nokori >= 1 || EditingGroupData is RootData)
            {
                // 削除後にSelectedにするDataを決めておく
                int removeDataZ = SelectedItemsData[0].Z;
                Data? nextCurrent = null;
                if (selectedCount == 1 && nokori >= 1)
                {
                    if (removeDataZ >= 1)
                    {
                        nextCurrent = EditingGroupData.DataList[removeDataZ - 1];
                    }
                    else
                    {
                        nextCurrent = EditingGroupData.DataList[removeDataZ + 1];
                    }
                }

                // 通常削除
                // リストから削除
                foreach (var item in SelectedItemsData)
                {
                    _ = EditingGroupData.DataList.Remove(item);
                    if (item.IsClicked)
                    {
                        ClickedItemData = null;
                        MyClickedItem = null;
                    }
                }

                // 選択状態解除
                ClearSelectedItems();

                // 次のItemをSelectedとCurrentにする
                if (nextCurrent is not null)
                {
                    _ = AddDataToSelectedItems(nextCurrent);
                }
            }
            // Item全削除 & Root以外が編集モードの時
            else
            {
                // 先にItemを削除
                foreach (var data in SelectedItemsData)
                {
                    _ = EditingGroupData.DataList.Remove(data);
                    if (data.IsClicked)
                    {
                        ClickedItemData = null;
                        MyClickedItem = null;
                    }
                }
                // 今のEditingGroupを保持
                GroupData group = EditingGroupData;

                // Editingを1個上に以降
                EditingUpperGroup();

                // 元のGroup自体を削除
                _ = EditingGroupData.DataList.Remove(group);
            }

            // 処理後のBounds更新
            UpdateBoundsToRoot(EditingGroupData);

        }

        #endregion 削除

        #region 画像として保存

        /// <summary>
        /// Rootをpng画像として保存
        /// ホントはDataクラスで行いたいけど、Bitmap作成で自身を送っているからできない
        /// →できた、Contentプロパティに自身を入れておくだけだった
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSaveRootToPngImageFile))]
        public void SaveRootToPngImageFile()
        {
            if (DataList.Count <= 0) { return; }
            if (Content is null) { return; }

            // 枠表示保持
            bool groupWaku = IsVisbleSelectedBorder;
            bool selectWaku = IsVisbleSelectedBorder;

            // ファイル保存Dialog作成
            SaveFileDialog dialog = Manager.MakeSaveFileDialogFileNameyyyyMMddHHmmss();

            // Dialog表示、pngで保存
            if (dialog.ShowDialog() == true)
            {
                // 枠を非表示
                IsVisbleGroupBorder = false;
                IsVisbleSelectedBorder = false;

                string filePath = dialog.FileName;
                var bmp = Manager.MakeBitmapFromLayoutTransformElement(Content);

                PngBitmapEncoder encoder = new();
                encoder.Frames.Add(BitmapFrame.Create(bmp));

                using FileStream stream = File.OpenWrite(filePath);
                encoder.Save(stream);
            }

            // 枠表示を戻す
            IsVisbleGroupBorder = groupWaku;
            IsVisbleSelectedBorder = selectWaku;
        }

        /// <summary>
        /// Currentをpng画像として保存
        /// </summary>
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

        #endregion 画像として保存

        #region Data追加

        // 追加先のZを選定
        private int GetInsertZIndex()
        {
            if (CurrentItemData is null) { return 0; }
            return ShiftZIndexMode switch
            {
                ModeAddZIndex.Upper => CurrentItemData.Z + 1,
                ModeAddZIndex.Under => CurrentItemData.Z,
                ModeAddZIndex.Top => EditingGroupData.DataList.Count,
                ModeAddZIndex.Bottom => 0,
                _ => 0,
            };
        }

        // 追加先のZの種類は以下、ShiftZに従って決定
        // CurrentのZの1個上
        // CurrentのZの1個下
        // 編集グループ内の一番上
        // 編集グループ内の一番下
        /// <summary>
        /// Dataを編集グループに追加、Currentの近傍に追加、Zの指定がない場合はShiftZに従う
        /// 通常はZ指定は必要ない
        /// </summary>
        /// <param name="data"></param>
        /// <param name="zIndex"></param>
        /// <param name="isUpdateBounds">Data追加後にRootまでBounds更新する</param>
        public void AddDataToCurrentNeighborhood(Data data, int zIndex = -1, bool isUpdateBounds = true)
        {
            data.RootData = this;
            data.X = 0;
            data.Y = 0;
            if (CurrentItemData is not null)
            {
                data.X = ShiftHorizontal + CurrentItemData.X;
                data.Y = ShiftVertical + CurrentItemData.Y;
            }

            // 追加先Z、指定無しor範囲外の場合は選定
            if (zIndex == -1 || zIndex > EditingGroupData.DataList.Count)
            {
                zIndex = GetInsertZIndex();
            }
            // 編集グループにData追加
            EditingGroupData.DataList.Insert(zIndex, data);

            // Selectedを空にする
            ClearSelectedItems();

            // Selectedに追加する
            AddDataToSelectedItems(data);

            // Boundsの更新
            if (isUpdateBounds)
            {
                UpdateBoundsEditingToRoot();
            }
        }

        /// <summary>
        /// EditingGroupのDataListにDataを挿入
        /// </summary>
        /// <param name="data"></param>
        /// <param name="insert"></param>
        public void AddDataToEditingGroup(Data data, int insert, bool IsUpdateBounds = true)
        {
            data.RootData = this;
            EditingGroupData.DataList.Insert(insert, data);


            // Boundsの更新
            if (IsUpdateBounds)
            {
                UpdateBoundsEditingToRoot();
            }
        }

        ///// <summary>
        ///// EditingGroupのDataListの末尾にDataを追加
        ///// </summary>
        ///// <param name="data"></param>
        //public void AddDataToEditingGroup(Data data)
        //{
        //    data.RootData = this;
        //    EditingGroupData.DataList.Add(data);
        //}


        /// <summary>
        /// 任意のグループにDataを追加する
        /// </summary>
        /// <param name="group"></param>
        /// <param name="data"></param>
        /// <param name="insert"></param>
        private void AddDataToGroup(GroupData group, Data data, int insert, bool isUpdateBounds)
        {
            data.RootData = this;
            group.DataList.Insert(insert, data);
            if (isUpdateBounds) { UpdateBoundsToRoot(group); }
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

        #endregion Data追加

        #region Bounds更新


        /// <summary>
        /// 指定GroupのBounds更新して、Rootまで行くBounds更新
        /// </summary>
        /// <param name="group"></param>
        public void UpdateBoundsToRoot(GroupData group)
        {
            // 子要素全体のBounds取得
            var rect = Manager.GetBounds(group.DataList);
            if (rect.IsEmpty) { return; }

            // サイズ更新
            group.Width = rect.Width;
            group.Height = rect.Height;

            // 子要素の座標更新
            if (rect.Top != 0 || rect.Left != 0)
            {
                foreach (Data item in group.DataList) { item.X -= rect.Left; item.Y -= rect.Top; }
            }

            // 自身の座標更新
            group.X += rect.Left;
            group.Y += rect.Top;

            // 親要素へ伝播
            if (group.ParentData is not null)
            {
                UpdateBoundsToRoot(group.ParentData);
            }
            //group.ParentData?.UpdateBoundsToRoot(group.ParentData);

        }

        public void UpdateBoundsEditingToRoot()
        {
            UpdateBoundsToRoot(EditingGroupData);
        }

        #endregion Bounds更新


        #region グループのサイズ更新

        /// <summary>
        /// 全グループのサイズの再計算、更新
        /// </summary>
        public void UpdateSizeAllDescendant()
        {
            UpdateSizeAllDescendant(this);
        }

        /// <summary>
        /// 指定グループの全子孫のサイズの再計算、更新
        /// </summary>
        /// <param name="group"></param>
        public void UpdateSizeAllDescendant(GroupData group)
        {
            foreach (var data in group.DataList)
            {
                if (data is GroupData groupData)
                {
                    UpdateSizeAllDescendant(groupData);
                }
            }
            group.UpdateSize();
        }

        #endregion グループのサイズ更新

        #endregion メソッド




    }




    /*Group*/



    public partial class GroupData : Data
    {
        //[ObservableProperty] private Brush _wakuIro = Brushes.Green;
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

                    // 追加先Index以降のItemのZをIndexに合わせるために＋１する
                    for (int i = e.NewStartingIndex + 1; i < DataList.Count; i++)
                    {
                        DataList[i].Z++;
                    }

                    // 自身が編集モードなら、子要素を選択可能にする
                    if (this.IsEditing)
                    {
                        newData.IsSelectable = true;
                    }
                }
                RootData?.SaveRootToPngImageFileCommand.NotifyCanExecuteChanged();
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

                    //oldData.ParentData = null; // Parentをリサイズしてからnullにする
                }
                RootData?.SaveRootToPngImageFileCommand.NotifyCanExecuteChanged();
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


        //partial void OnIsEditingChanged(bool oldValue, bool newValue)
        //{
        //    if (newValue)
        //    {
        //        WakuIro = Brushes.Red;
        //    }
        //    else
        //    {
        //        WakuIro = Brushes.Blue;
        //    }
        //}

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



        #region パブリックメソッド



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
        [ObservableProperty] private Pen _strokePen; // 保存対象外にする

        public ShapeData()
        {
            _strokePen = new(_stroke, _strokeThickness)
            {
                EndLineCap = _strokeEndLineCap,
                StartLineCap = _strokeStartLineCap,
                LineJoin = _strokeLineJoin,
                MiterLimit = _strokeMiterLimit,
            };
        }

        partial void OnStrokeLineJoinChanged(PenLineJoin oldValue, PenLineJoin newValue)
        {
            StrokePen.LineJoin = newValue;
        }
        partial void OnStrokeMiterLimitChanged(double oldValue, double newValue)
        {
            StrokePen.MiterLimit = newValue;
        }
        partial void OnStrokeStartLineCapChanged(PenLineCap oldValue, PenLineCap newValue)
        {
            StrokePen.StartLineCap = newValue;
        }
        partial void OnStrokeEndLineCapChanged(PenLineCap oldValue, PenLineCap newValue)
        {
            StrokePen.EndLineCap = newValue;
        }
        partial void OnStrokeThicknessChanged(double oldValue, double newValue)
        {
            StrokePen.Thickness = newValue;
        }

        //partial void OnStrokeThicknessChanging(double oldValue, double newValue)
        //{
                        
        //    StrokePen.Thickness = newValue;
            
        //}

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