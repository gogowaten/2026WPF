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

namespace _20260305
{

    public class RectangleGroupView : ItemsControl
    {
        static RectangleGroupView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RectangleGroupView), new FrameworkPropertyMetadata(typeof(RectangleGroupView)));
        }
        public RectangleGroupView()
        {

        }
    }

    public class CustomControl1 : Control
    {
        static CustomControl1()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
        }
    }

    public class RectangleView : ContentControl
    {
        static RectangleView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RectangleView), new FrameworkPropertyMetadata(typeof(RectangleView)));
        }
        public RectangleView()
        {
            
        }
    }
}
