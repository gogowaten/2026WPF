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

namespace _20260309_DataTemplate_CustomControl
{

    public class AAA : ItemsControl
    {
        static AAA()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AAA), new FrameworkPropertyMetadata(typeof(AAA)));
        }
        public AAA()
        {

        }

    }



}
