using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260413_02_CanvasThumbResize
{
    /// <summary>
    /// Thumb自体をリサイズした時TemplateのCanvasはどうなる？
    /// まず、Thumbにサイズを指定する必要がある。もし無指定の場合は
    /// ハンドルを移動させてもサイズはNaNから変化しないし、これはreSizeAdornerの仕様にしている
    /// で、サイズ指定してからハンドル移動するとリサイズされる
    /// けど、中のCanvasはリサイズされないので、中央揃えの表示になる
    /// </summary>
    public class CanvasThumb2 : Thumb
    {
        static CanvasThumb2()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasThumb2), new FrameworkPropertyMetadata(typeof(CanvasThumb2)));
        }
        public CanvasThumb2()
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
}
