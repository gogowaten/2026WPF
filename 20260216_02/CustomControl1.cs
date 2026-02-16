using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

namespace _20260216_02
{
    /// <summary>
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:_20260216_02"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:_20260216_02;assembly=_20260216_02"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    //public class CustomControl1 : Control
    //{
    //    static CustomControl1()
    //    {
    //        DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
    //    }
    //}



    // ViewModelを描画するだけのViewクラス

    // この NodeContainer は次の特徴を満たします：
    //Control を継承（外観は ControlTemplate で定義）
    //ViewModel（NodeViewModel）を描画する View
    //入れ子構造を再帰的に描画
    //子ノードの UI を自動生成
    //DragBehavior と NodeProps.Position を利用
    //ViewModel の X/Y と UI の位置が同期
    //ViewModel.Children の変更に追従して UI を再構築
    public class NodeContainer : Control
    {
        private NodePanel? _panel;
        static NodeContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NodeContainer),
                new FrameworkPropertyMetadata(typeof(NodeContainer)));
        }



        public NodeViewModel ViewModel
        {
            get { return (NodeViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(NodeViewModel), typeof(NodeContainer), new PropertyMetadata(null, OnViewModelChanged));

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NodeContainer)d;
            control.HookViewModelEvents(e.OldValue as NodeViewModel, e.NewValue as NodeViewModel);
            control.RebuildUI();
        }

        // ViewModel.Childrenの変更を監視する
        private void HookViewModelEvents(NodeViewModel? oldVM, NodeViewModel? newVM)
        {
            oldVM?.Children.CollectionChanged -= Children_CollectionChanged;
            newVM?.Children.CollectionChanged += Children_CollectionChanged;
        }


        private void Children_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RebuildUI();
        }



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_Panel") is NodePanel p)
            {
                _panel = p;
                RebuildUI();
            }
        }

        // ViewModelのChildrenをUIに反映する
        private void RebuildUI()
        {
            if (_panel == null || ViewModel == null) { return; }

            _panel.Children.Clear();

            foreach (var childVM in ViewModel.Children)
            {
                var childView = CreateChildNode(childVM);
                _panel.Children.Add(childView);
            }
        }

        private NodeContainer CreateChildNode(NodeViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.Text))
            {
                // TextBlockノードして表示
                TextBlock tb = new()
                {
                    Text = vm.Text,
                    Background = Brushes.LightYellow,
                    Padding = new Thickness(4)
                };

                DragBehavior.SetIsEnabled(tb, true);
                NodeProps.SetPosition(tb, new Point(vm.X, vm.Y));

                return new NodeContainer() { ViewModel = vm };
            }

            // 通常のNodeContainer
            var child = new NodeContainer
            {
                DataContext = vm,
                ViewModel = vm,
                Width = 120,
                Height = 50,
                Background = Brushes.LightBlue
            };

            // ドラッグ移動を有効化
            DragBehavior.SetIsEnabled(child, true);

            // 位置をViewModelから反映
            NodeProps.SetPosition(child, new Point(vm.X, vm.Y));

            return child;
        }
    }


}
