using System;
using System.Collections.Generic;
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

namespace _20260222_ResizePanel
{

    public class Node : Control
    {
        private ResizePanel _resizePanel = null!;
        static Node()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Node), new FrameworkPropertyMetadata(typeof(Node)));
        }

        public Node()
        {

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_Panel") is ResizePanel panel)
            {
                _resizePanel = panel;
            }
            else { throw new ApplicationException("テンプレートが見つからん"); }
        }

        public void AddChild(UIElement child, double x, double y)
        {
            ResizePanel.SetX(child, x);
            ResizePanel.SetY(child, y);
            _resizePanel.Children.Add(child);
        }

        public void RemoveChild(UIElement child)
        {
            _resizePanel.Children.Remove(child);
        }
    }



}
