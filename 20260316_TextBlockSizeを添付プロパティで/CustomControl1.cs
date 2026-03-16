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

namespace _20260316_TextBlockSizeを添付プロパティで
{

    public class TestControl : ContentControl
    {
        static TestControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TestControl), new FrameworkPropertyMetadata(typeof(TestControl)));
        }
        public TestControl()
        {

        }

    }

}