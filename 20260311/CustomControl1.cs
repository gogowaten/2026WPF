using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260311
{
    [ContentProperty(nameof(MyContent))]
    public class CustomThumb : Thumb
    {

        static CustomThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumb), new FrameworkPropertyMetadata(typeof(CustomThumb)));
        }

        // Ctrl+クリック移動後の削除判定用
        // 移動開始時に自身は選択状態だった場合にtrue
        private bool isSelectedAtDragStart;

        // 移動開始時にCtrlキーが押されていたフラグ
        private bool isDragStartWithPressedCtrl;

        #region 依存関係プロパティ


        public Data MyData
        {
            get { return (Data)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(Data), typeof(CustomThumb), new PropertyMetadata(null));


        public FrameworkElement MyContent
        {
            get { return (FrameworkElement)GetValue(MyContentProperty); }
            set { SetValue(MyContentProperty, value); }
        }
        public static readonly DependencyProperty MyContentProperty =
            DependencyProperty.Register(nameof(MyContent), typeof(FrameworkElement), typeof(CustomThumb), new PropertyMetadata(null));
        #endregion 依存関係プロパティ

        //public bool MyIsSelected
        //{
        //    get { return (bool)GetValue(MyIsSelectedProperty); }
        //    set { SetValue(MyIsSelectedProperty, value); }
        //}
        //public static readonly DependencyProperty MyIsSelectedProperty =
        //    DependencyProperty.Register(nameof(MyIsSelected), typeof(bool), typeof(CustomThumb), new PropertyMetadata(false));



        public CustomThumb()
        {
            //this.DataContext = this;
            DragStarted += CustomThumb_DragStarted;
            DragDelta += TThumb_DragDelta;
            DragCompleted += CustomThumb_DragCompleted;
            PreviewMouseLeftButtonDown += CustomThumb_PreviewMouseLeftButtonDown;
        }

        #region キーイベント

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.F2)
            {
                if (MyData is GroupData group && group.IsCurrent) { group.RootData?.ChangeEditingGroup(group); }
            }
            else if (e.Key == Key.Escape)
            {
                if (MyData.ParentData is GroupData parent && parent.IsEditing)
                {
                    MyData.RootData?.ChangeEditingGroup(parent);
                }
            }
        }
        #endregion キーイベント


        #region マウスイベント
        #region クリックイベント


        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            var sou = e.Source;
            var ori = e.OriginalSource;
            var dc = this.DataContext;
            var myd = MyData;

            this.Focus();
            var isfo = this.IsFocused;
            var iskeyfo = this.IsKeyboardFocused;

            if (MyData.RootData is RootData root)
            {
                // ClickedItemの更新
                if (e.OriginalSource is FrameworkElement elm && elm.DataContext is Data data)
                {
                    root.ClickedItem = data;
                }

                //// 選択状態の更新
                //UpdateSelectedItems2(MyData, root);

            }

        }

        //protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        //{
        //    base.OnPreviewMouseLeftButtonUp(e);
        //    if (MyData.RootData is RootData root)
        //    {
        //        UpdateSelectedItems(MyData, root);
        //    }

        //}

        private void UpdateSelectedItems(Data ClickedData, RootData root)
        {

            // 選択状態の更新
            if (ClickedData.IsSelectable)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    // 選択リストに自身が既に在る？
                    if (root.SelectedItems.Contains(ClickedData))
                    {
                        // 選択リストの要素数が2個以上で
                        if (root.SelectedItems.Count > 1)
                        {
                            root.RemoveSelect(ClickedData); // 選択リストから削除
                        }
                        // 自身だけが選択されている状態なら、何もしない、そのままを維持
                    }
                    else
                    {
                        root.AddSelect(ClickedData); // 選択リストに追加
                    }
                }
                // 通常クリック時
                else
                {
                    root.ClearSelectedItems();
                    root.AddSelect(ClickedData);
                }
            }

        }


        private void UpdateSelectedItems2(Data ClickedData, RootData root)
        {
            // 選択状態の更新
            if (ClickedData.IsSelectable)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    // 選択リストに自身が既に在る？
                    if (root.SelectedItems.Contains(ClickedData))
                    {
                        //// 選択リストの要素数が2個以上で
                        //if (root.SelectedItems.Count > 1)
                        //{
                        //    root.RemoveSelect(ClickedData); // 選択リストから削除
                        //}
                        //// 自身だけが選択されている状態なら、何もしない、そのままを維持
                    }
                    else
                    {
                        root.AddSelect(ClickedData); // 選択リストに追加
                    }
                }
                // 通常クリック時
                else
                {
                    root.ClearSelectedItems();
                    root.AddSelect(ClickedData);
                }
            }

        }

        private void CustomThumb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Groupの中の要素の場合は、先にGroupのクリックが来た後に要素のクリックが来る
            var sou = e.Source;
            var ori = e.OriginalSource;
            var dc = this.DataContext;
            var myd = MyData;
            this.Focus();
            var isfo = this.IsFocused;
            var iskeyfo = this.IsKeyboardFocused;
            var iii = this.Focusable;

        }

        #endregion クリックイベント

        #region ドラッグ移動

        // SelectedItemsへの追加をする
        private void CustomThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            if (MyData.IsSelectable == false) { return; }

            if (MyData.RootData is RootData root)
            {
                // 自身が既に選択リストに在ったかを記録
                isSelectedAtDragStart = root.SelectedItems.Contains(MyData);

                // Ctrlキーの状態を記録
                isDragStartWithPressedCtrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

                // 未選択をCtrlクリックの場合、選択リストに追加
                if (isDragStartWithPressedCtrl)
                {
                    if (MyData.IsSelected == false)
                    {
                        root.AddSelect(MyData);
                    }
                }
                // 既選択を通常クリックの場合、自身だけを選択状態にする
                else
                {
                    if (MyData.IsSelected == false)
                    {
                        root.ClearSelectedItems();
                        root.AddSelect(MyData);
                    }
                }

            }


        }


        private void TThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (MyData.RootData is RootData root)
            {
                if (root.SelectedItems.Contains(MyData))
                {
                    foreach (var item in root.SelectedItems)
                    {
                        item.X += e.HorizontalChange;
                        item.Y += e.VerticalChange;
                    }
                    e.Handled = true;
                }

            }
            //MyData.X += e.HorizontalChange;
            //MyData.Y += e.VerticalChange;
            //e.Handled= true;
        }

        private void CustomThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            // 選択リストからの削除
            // 移動していない
            if (e.HorizontalChange == 0 && e.VerticalChange == 0)
            {
                // Ctrlクリック
                if (isDragStartWithPressedCtrl == true)
                {
                    // クリック以前から既選択だった
                    if (isSelectedAtDragStart == true)
                    {
                        if (MyData.RootData is RootData root)
                        {
                            if (root.SelectedItems.Count > 1)
                            {
                                root.RemoveSelect(MyData);
                            }
                        }
                    }
                }
                // 通常クリック
                else
                {
                    if (MyData.RootData is RootData root)
                    {
                        root.ClearSelectedItems();
                        root.AddSelect(MyData);
                    }
                }
            }

            //    // 選択状態の解除
            //    // 移動していない
            //    if (e.HorizontalChange == 0 && e.VerticalChange == 0)
            //{
            //    if (MyData.RootData is RootData root)
            //    {
            //        // 選択リストの要素数が2個以上で
            //        if (root.SelectedItems.Count > 1)
            //        {
            //            if (isSelectedAtDragStart)
            //            {
            //                root.RemoveSelect(MyData); // 選択リストから削除
            //            }
            //        }
            //    }
            //}
        }

        #endregion ドラッグ移動
        #endregion マウスイベント
    }









    public class AAAItemsCtrl : ItemsControl
    {

        public RootData MyRootData
        {
            get { return (RootData)GetValue(MyRootDataProperty); }
            set { SetValue(MyRootDataProperty, value); }
        }
        public static readonly DependencyProperty MyRootDataProperty =
            DependencyProperty.Register(nameof(MyRootData), typeof(RootData), typeof(AAAItemsCtrl), new PropertyMetadata(null));


        //public DataService AAADataService
        //{
        //    get { return (DataService)GetValue(AAADataServiceProperty); }
        //    set { SetValue(AAADataServiceProperty, value); }
        //}
        //public static readonly DependencyProperty AAADataServiceProperty =
        //    DependencyProperty.Register(nameof(AAADataService), typeof(DataService), typeof(AAAItemsCtrl));


        static AAAItemsCtrl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AAAItemsCtrl), new FrameworkPropertyMetadata(typeof(AAAItemsCtrl)));
        }
        public AAAItemsCtrl()
        {
            PreviewMouseLeftButtonDown += AAAItemsCtrl_PreviewMouseLeftButtonDown;
        }

        private void AAAItemsCtrl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var sou = e.Source;
            var ori = e.OriginalSource;

        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            var sou = e.Source;
            var ori = e.OriginalSource;
            var dc = this.DataContext;
            var root = MyRootData;

            if (ori is FrameworkElement elem && elem.DataContext is Data oo)
            {
                var neko = oo;
            }

            if (ori is UIElement elm)
            {
                // 要素からコンテナ取得？
                var youso = ContainerFromElement(elm);
                var youso2 = ContainerFromElement(this, elm);
                var item = ItemsControl.ItemsControlFromItemContainer(youso);
                var yo = ItemsControl.GetItemsOwner(youso);
                var io = ItemsControl.GetItemsOwner(item);
            }
        }
    }


}
