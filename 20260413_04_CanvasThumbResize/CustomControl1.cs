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

namespace _20260413_04_CanvasThumbResize
{
    public class CanvasThumb : Thumb
    {
        private Canvas MyTemplateCanvas = null!;

        static CanvasThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasThumb), new FrameworkPropertyMetadata(typeof(CanvasThumb)));
        }
        public CanvasThumb()
        {
            DragDelta += CanvasThumb_DragDelta;
        }

        public void RemoveResizeHndle()
        {
            ResizeAdorner.RemoveResizeAdorner(this);
        }

        public void AddResizeHandle()
        {
            ResizeAdorner? adorner = ResizeAdorner.AddResizeAdorner(this);
            adorner?.LeftLocateChanged += Adorner_LeftLocateChanged;
            adorner?.TopLocateChanged += CanvasThumb_TopLocateChanged;
        }

        private void CanvasThumb_TopLocateChanged(object? sender, double e)
        {
            foreach (var item in MyTemplateCanvas.Children.OfType<UIElement>())
            {
                Canvas.SetTop(item, Canvas.GetTop(item) - e);
            }
        }

        private void Adorner_LeftLocateChanged(object? sender, double e)
        {
            IEnumerable<UIElement> items = MyTemplateCanvas.Children.OfType<UIElement>();
            foreach (UIElement item in items)
            {
                Canvas.SetLeft(item, Canvas.GetLeft(item) - e);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_Canvas") is Canvas canvas)
            {
                MyTemplateCanvas = canvas;
            }
            else { throw new Exception(); }
        }

        private void CanvasThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
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
