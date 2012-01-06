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
                double left = GetLeft(child);
                if (double.IsNaN(left)) left = 0;
                double x = left + child.DesiredSize.Width; 
                if (!double.IsInfinity(x) && !double.IsNaN(x))
                {
                    width = Math.Max(width, x);
                }
                double top = GetTop(child);
                if (double.IsNaN(top)) top = 0;

                double y = top + child.DesiredSize.Height;
                if (!double.IsInfinity(y) && !double.IsNaN(y))
                {
                    height = Math.Max(height, y);
                }
            }

            return new Size(width, height);
        }
    }
}
