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
using System.Windows.Shapes;

namespace BitmapSourceVisualizer
{
    /// <summary>
    /// BitmapSourceVisualizerWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class BitmapSourceVisualizerWindow : Window
    {
        public BitmapSourceVisualizerWindow()
        {
            InitializeComponent();
        }

        public void SetImage(BitmapSource bitmap)
        {
            ImageControl.Source = bitmap;
            Title = $"BitmapSource Visualizer - {bitmap.PixelWidth} x {bitmap.PixelHeight}";
        }

    }
}
