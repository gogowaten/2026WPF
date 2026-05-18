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
            }

            this.ContextMenu = CreateMyContextMenu(MyData);
        }

        private void Element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MyData?.ParentData?.UpdateBoundsToRoot();
        }


        private ContextMenu CreateMyContextMenu(Data data)
        {
            var menu = new ContextMenu();
            var item = new MenuItem() { Header = "current保存" };
            item.Click += Item_Click;
            item.SetBinding(IsEnabledProperty, new Binding(nameof(MyData.IsCurrent)) { Source = MyData });
            menu.Items.Add(item);

            //if (typeof(EllipseData) == data.GetType())
            //{
            //    var item = new MenuItem() { Header = "ellipse" };
            //    menu.Items.Add(item);
            //}
            //else if (typeof(GeoLineData) == data.GetType())
            //{
            //    var item = new MenuItem() { Header = "geoline" };
            //    menu.Items.Add(item);
            //    if(MyContent is GeoLineEX geo)
            //    {

            //    }
            //}

            return menu;
        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new()
            {
                AddExtension = true,
                DefaultExt = "png",
                FileName = DateTime.Now.ToString("yyyyMMdd_HHmmss")
            };


            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                var bmp = MakeMyContentRenderBitmap();

                PngBitmapEncoder encoder = new();
                encoder.Frames.Add(BitmapFrame.Create(bmp));

                using FileStream stream = File.OpenWrite(filePath);
                encoder.Save(stream);
            }

        }

        public RenderTargetBitmap MakeMyContentRenderBitmap()
        {
            int width = (int)MyData.Width;
            int height = (int)MyData.Height;
            double dpi = MyData.RootData is null ? 96.0 : MyData.RootData.MyDPI;
            RenderTargetBitmap bmp = new(width, height, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(MyContent);
            return bmp;
        }

        public void Test(RenderTargetBitmap bmp, string filePath)
        {

        }

        public void SaveMyContentToImage(string filePath)
        {
            RenderTargetBitmap bmp = MakeMyContentRenderBitmap();

        }

        #region クリックイベント時


        //private void CustomThumb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    MyData.RootData?.ClickedItemData = MyData;
        //    MyData.RootData?.MyClickedItem = this;
        //}


        //private void CustomThumb_MouseUp(object sender, MouseButtonEventArgs e)
        //{

        //}

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


            MyData?.ParentData?.UpdateBoundsToRoot();
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




    public class RootItemsControl : ItemsControl
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
            // システムのDPIを記録
            //PresentationSource sou = PresentationSource.FromVisual(this);
            //double dpi = 96.0 * sou.CompositionTarget.TransformFromDevice.M11;
            //MyData.MyDPI = dpi;
            MyData.MyDPI = 96.0 * PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice.M11;
        }


        public RootData MyData
        {
            get { return (RootData)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(RootData), typeof(RootItemsControl), new PropertyMetadata(null));


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