using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20260325
{

    public class DataThumb : Thumb
    {

        public Data MyData
        {
            get { return (Data)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register(nameof(MyData), typeof(Data), typeof(DataThumb), new PropertyMetadata(null));

        static DataThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataThumb), new FrameworkPropertyMetadata(typeof(DataThumb)));
        }
        public DataThumb()
        {
            //MyData = new GeoShapeData() { Name = "ベジェ曲線", Stroke = Brushes.MediumAquamarine, StrokeThickness = 20.0, Points = [new Point(50, 70), new Point(250, 150), new Point(50, 250), new Point(50, 200), new Point(50, 150), new Point(150, 100), new Point(250, 250),], StrokeEndLineCap = PenLineCap.Round };

            //DataContext = MyData;
            DragDelta += OnDragDelta;
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (MyData is null) { return; }

            MyData.Left += e.HorizontalChange;
            MyData.Top += e.VerticalChange;
        }
    }

    [ContentProperty(nameof(MyContent))]
    public class CustomThumb : Thumb
    {
        static CustomThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomThumb), new FrameworkPropertyMetadata(typeof(CustomThumb)));
        }


        public object MyContent
        {
            get { return (object)GetValue(MyContentProperty); }
            set { SetValue(MyContentProperty, value); }
        }
        public static readonly DependencyProperty MyContentProperty =
            DependencyProperty.Register(nameof(MyContent), typeof(object), typeof(CustomThumb), new PropertyMetadata(null));



    }
}
