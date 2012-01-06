using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace WpfWaveform
{
    class AutoSizeCanvas : Canvas
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            double width = 0.0;
            double height = 0.0;

            foreach (var child in Children.OfType<FrameworkElement>())
            {
                child.Measure(availableSize);
                double x = x = GetLeft(child) + child.DesiredSize.Width; 
                if (!double.IsInfinity(x) && !double.IsNaN(x))
                {
                    width = Math.Max(width, x);
                }
                double y = GetTop(child) + child.DesiredSize.Height;
                if (!double.IsInfinity(y) && !double.IsNaN(y))
                {
                    height = Math.Max(height, y);
                }
            }

            return new Size(width, height);
        }
    }
}
