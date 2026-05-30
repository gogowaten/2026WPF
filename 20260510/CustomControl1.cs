using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260510
{
    [ContentProperty(nameof(MyContent))]
    public class CustomThumb : Thumb
    {
        // Ctrl+クリック移動後の削除判定用
        // 移動開始時に自身は選択状態だった場合にtrue
        private bool IsSelectedAtDragStart;

        // 移動開始時にCtrlキーが押されていたフラグ
        private bool IsDragStartWithPressedCtrl;

        public Point migikurikkuiti;
        public ContextMenu MyContextMenu { get; set; } = null!;


        static CustomThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumb), new FrameworkPropertyMetadata(typeof(CustomThumb)));
        }
        public CustomThumb()
        {
            PreviewMouseDown += CustomThumb_PreviewMouseDown;
            MouseRightButtonDown += (s, e) => { migikurikkuiti = e.GetPosition(this); };
            DragStarted += CustomThumb_DragStarted;
            DragDelta += CustomThumb_DragDelta;
            DragCompleted += CustomThumb_DragCompleted;
            //PreviewMouseLeftButtonDown += CustomThumb_PreviewMouseLeftButtonDown;
            //MouseUp += CustomThumb_MouseUp;
            Loaded += CustomThumb_Loaded;



        }


        private void CustomThumb_Loaded(object sender, RoutedEventArgs e)
        {
            if (MyContent is FrameworkElement element)
            {
                element.SizeChanged += Element_SizeChanged;

                // Dataに要素を記録しておく、画像として保存とかに使う
                MyData.Content = this.MyContent;
            }

            // 右クリックメニュー作成
            this.ContextMenu = CreateMyContextMenu(MyData);
        }

        // サイズ変更時、RootまでBoundsの更新
        private void Element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //MyData?.ParentData?.UpdateBoundsToRoot();
            if (MyData?.ParentData is GroupData parent)
            {
                MyData.RootData?.UpdateBoundsToRoot(parent);
            }
        }

        // 右クリックメニュー作成
        private ContextMenu CreateMyContextMenu(Data data)
        {
            var menu = new ContextMenu();
            var item = new MenuItem() { Header = "currentをpngで保存" };
            item.Click += (s, e) => { Manager.SaveMyContentToPngImage(this); };

            //item.SetBinding(IsEnabledProperty,
            //    new Binding(nameof(MyData.IsCurrent)) { Source = MyData });

            item.SetBinding(VisibilityProperty,
                new Binding(nameof(MyData.IsCurrent)) { Source = MyData, Converter = new MyConvBoolToVisible() });

            menu.Items.Add(item);

            if (typeof(EllipseData) == data.GetType())
            {
                item = new MenuItem() { Header = "ellipse" };
                menu.Items.Add(item);
            }
            else if (typeof(GeoLineData) == data.GetType())
            {
                item = new MenuItem() { Header = "geoline" };
                menu.Items.Add(item);
                if (MyContent is GeoLineEX geo)
                {

                }
            }

            return menu;
        }






        #region クリックイベント時


        // クリック直前
        private void CustomThumb_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MyData.RootData is RootData root)
            {
                // 自身の選択状態を記録
                IsSelectedAtDragStart = root.SelectedItemsData.Contains(MyData);

                // Ctrlキーの状態を記録
                IsDragStartWithPressedCtrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

                root.ClickedItemData = MyData;
                root.MyClickedItem = this;
            }
        }

        // ドラッグ移動中
        // Selectedをすべて移動させる
        private void CustomThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (MyData.RootData is null) { return; }

            var selectedItems = MyData.RootData.SelectedItemsData;
            if (selectedItems.Contains(MyData))
            {
                foreach (var item in selectedItems)
                {
                    item.X += e.HorizontalChange;
                    item.Y += e.VerticalChange;
                }
                e.Handled = true;
            }

        }

        // ドラッグ移動開始時
        private void CustomThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            if (MyData.IsSelectable == false) { return; }

            if (MyData.RootData is RootData root)
            {
                // 選択状態リストを更新
                // 未選択状態の時だけ処理
                if (MyData.IsSelected == false)
                {
                    // Ctrlクリックした場合は、選択リストに追加
                    if (IsDragStartWithPressedCtrl)
                    {
                        root.AddDataToSelectedItems(MyData);
                    }
                    // 通常クリックした場合は、自身だけを選択状態にしたいので
                    // 自身を追加後に、自身以外をリストから削除
                    else
                    {
                        if (root.AddDataToSelectedItems(MyData))
                        {
                            root.RemoveAllOtherFromSelected(MyData);
                        }
                    }
                }
            }
        }

        // ドラッグ移動完了時
        private void CustomThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            /*  移動した：何もしない
             *      
             *  移動していない
             *      クリック前未選択：何もしない
             *      
             *      クリック前既選択
             *          今の選択リストのItem個数が1個：何もしない
             *          
             *          今の選択リストのItem個数が2個以上
             *              通常クリックだった：選択リストから自身以外を削除
             *              Ctrlクリックだった：選択リストから自身を削除
             *              
             *  まとめると、処理が必要なのは
             *      移動なし＆クリック前既選択＆選択Item個数が2個以上の場合だけになる
             */

            if (e.HorizontalChange == 0 &&
                e.VerticalChange == 0 &&
                IsSelectedAtDragStart &&
                MyData.RootData is RootData root &&
                root.SelectedItemsData.Count >= 2)
            {
                if (IsDragStartWithPressedCtrl)
                {
                    // Ctrlクリックだった：選択リストから自身を削除
                    root.RemoveDataFromSelect(MyData);
                }
                else
                {
                    // 通常クリックだった：選択リストから自身以外を削除
                    root.RemoveAllOtherFromSelected(MyData);
                }

                e.Handled = true;
            }


            //MyData?.ParentData?.UpdateBoundsToRoot();
            if(MyData?.ParentData is GroupData group)
            {
                MyData?.RootData?.UpdateBoundsToRoot(group);
            }
        }
        #endregion クリックイベント時


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

        #region パブリックメソッド
        //// たぶん右クリックメニューから実行
        //// 図形Thumb専用、図形にピッタリサイズにする
        //public void PerfectlyFit()
        //{
        //    if (MyContent is ResizeCanvas geot)
        //    {
        //        geot.PerfectlyFit();
        //    }
        //}

        #endregion パブリックメソッド
    }




    public partial class RootItemsControl : ItemsControl
    {


        static RootItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RootItemsControl), new FrameworkPropertyMetadata(typeof(RootItemsControl)));
        }
        public RootItemsControl()
        {
            Loaded += RootItemsControl_Loaded;
        }

        private void RootItemsControl_Loaded(object sender, RoutedEventArgs e)
        {            

            // システムのDPIを記録したいけど
            // 、XAML画面でエラーになるけど、動くことは動く

            //MyData.MyDPI = 96.0 * PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice.M11;
        }

        // すべてのDataのRootDataにMyDataを登録する
        private static void SetRootData(GroupData group)
        {
            foreach (var item in group.DataList)
            {
                item.RootData = group.RootData;
                if (item is GroupData groupData)
                {
                    SetRootData(groupData);
                }
            }
        }


        public RootData MyData
        {
            get { return (RootData)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(RootData), typeof(RootItemsControl), new PropertyMetadata(null, OnMyDataChanged));
        private static void OnMyDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RootItemsControl root && e.NewValue is RootData data)
            {
                // MyDataのチェック
                // すべてのDataのRootDataにMyDataを登録する
                foreach (var item in data.DataList)
                {
                    item.RootData = data;
                    if (item is GroupData group)
                    {
                        SetRootData(group);
                    }
                }

                // 自身をDataのContentに登録、自身を画像として保存に使う
                root.MyData.Content = root;

                // 全子孫のサイズの再計算
                root.MyData.UpdateSizeAllDescendant();
            }
        }



    }





    public class FlatHandle : Thumb
    {
        public int MyIndex { get; set; } // 識別用インデックス

        static FlatHandle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlatHandle), new FrameworkPropertyMetadata(typeof(FlatHandle)));
        }
        public FlatHandle()
        {

        }

        public Brush MyFillBrush
        {
            get { return (Brush)GetValue(MyFillBrushProperty); }
            set { SetValue(MyFillBrushProperty, value); }
        }
        public static readonly DependencyProperty MyFillBrushProperty =
            DependencyProperty.Register(nameof(MyFillBrush), typeof(Brush), typeof(FlatHandle), new PropertyMetadata(Brushes.Transparent));

        public double MyLeft
        {
            get { return (double)GetValue(MyLeftProperty); }
            set { SetValue(MyLeftProperty, value); }
        }
        public static readonly DependencyProperty MyLeftProperty =
            DependencyProperty.Register(nameof(MyLeft), typeof(double), typeof(FlatHandle), new PropertyMetadata(0.0));

        public double MyTop
        {
            get { return (double)GetValue(MyTopProperty); }
            set { SetValue(MyTopProperty, value); }
        }
        public static readonly DependencyProperty MyTopProperty =
            DependencyProperty.Register(nameof(MyTop), typeof(double), typeof(FlatHandle), new PropertyMetadata(0.0));



    }




}