using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260227
{

    public class NodeContainer : ListBox
    {
        private Point _startPoint;
        private bool _isDragging;

        static NodeContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeContainer),
                new FrameworkPropertyMetadata(typeof(NodeContainer)));
        }

        public NodeContainer()
        {

        }

        #region マウスドラッグ移動
        // クリック時
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            // クリックされたListBoxItem要素を取得する
            var item = ContainerFromElement((DependencyObject)e.OriginalSource) as ListBoxItem;
            if (item == null) { return; }

            _isDragging = true;
            _startPoint = e.GetPosition(this);// NodeContainer(Canvas)上でのクリック座標取得

            // クリックした要素が未選択なら、それを選択状態にする
            if (!item.IsSelected)
            {
                if((Keyboard.Modifiers & ModifierKeys.Control) == 0)
                {
                    UnselectAll();
                }
                item.IsSelected = true;
            }

            item.CaptureMouse(); // マウスが枠外にでてもイベントを追えるようにキャプチャしておく

            e.Handled= true; // イベントはここで終了させ、ListBoxには渡さない
        }

        // 移動時
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (_isDragging == false) { return; }

            Point currentPoint = e.GetPosition(this);
            Vector delta = currentPoint - _startPoint; // 移動量を計算

            if (delta.Length > 0)
            {
                // 選択されている要素全てを移動
                //foreach (var selectedItem in SelectedItems.OfType<Data>())
                //{

                //}
                foreach (var selectedItem in SelectedItems.Cast<Data>())
                {
                    selectedItem.X += delta.X;
                    selectedItem.Y += delta.Y;
                }

                _startPoint = currentPoint; // 基準点を更新
            }

            e.Handled = true; // 移動中も他へは渡さない
        }

        // マウスが離された時
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {

            if (_isDragging)
            {
                var item = ContainerFromElement((DependencyObject)e.OriginalSource) as ListBoxItem;
                item?.ReleaseMouseCapture();
                _isDragging = false;
            }

            base.OnPreviewMouseUp(e);
        }
        #endregion マウスドラッグ移動
    }



}
