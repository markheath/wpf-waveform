using System;
using System.Collections.Generic;
using System.Linq;
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
using NAudio.Wave;

namespace WpfWaveform
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            using (var reader = new Mp3FileReader(@"E:\Audio\Music\Coldplay\X&Y\04-Fix You.mp3"))
            {
                int stepSize = reader.WaveFormat.AverageBytesPerSecond / 5;
                byte[] byteBuffer = new byte[stepSize];
                WaveBuffer buffer = new WaveBuffer(byteBuffer);
                int read;
                List<Point> points = new List<Point>();
                int x = 0;
                while ((read = reader.Read(byteBuffer, 0, stepSize)) > 0)
                {
                    int samples = read / 2; // assume 16 bit
                    double maxLeft = 0;
                    for (int sample = 0; sample < samples; sample+=2)
                    {
                        double sampleLeft = buffer.ShortBuffer[sample] / 32768.0;
                        double sampleRight = buffer.ShortBuffer[sample+1] / 32768.0;
                        maxLeft = Math.Max(maxLeft, sampleLeft);
                    }
                    points.Add(new Point(x, maxLeft * 400));
                    x += 2;
                }

                // Get Bezier Spline Control Points.
                Point[] cp1, cp2;
                BezierSpline.GetCurveControlPoints(points.ToArray(), out cp1, out cp2);

                // Draw curve by Bezier.
                PathSegmentCollection lines = new PathSegmentCollection();
                for (int i = 0; i < cp1.Length; ++i)
                {
                    lines.Add(new BezierSegment(cp1[i], cp2[i], points[i + 1], true));
                }
                PathFigure f = new PathFigure(points[0], lines, false);
                PathGeometry g = new PathGeometry(new PathFigure[] { f });
                Path path = new Path() { Stroke = Brushes.Red, StrokeThickness = 1, Data = g, Fill=Brushes.Beige };
                canvas.Children.Add(path);
            }
        }

        /*
         * canvas.Children.Clear();

            Point[] points = curve();
            if (points.Length < 2)
                return;

            // Get Bezier Spline Control Points.
            Point[] cp1, cp2;
            ovp.BezierSpline.GetCurveControlPoints(points, out cp1, out cp2);

            // Draw curve by Bezier.
            PathSegmentCollection lines = new PathSegmentCollection();
            for (int i = 0; i < cp1.Length; ++i)
            {
                lines.Add(new BezierSegment(cp1[i], cp2[i], points[i + 1], true));
            }
            PathFigure f = new PathFigure(points[0], lines, false);
            PathGeometry g = new PathGeometry(new PathFigure[] { f });
            Path path = new Path() { Stroke = Brushes.Red, StrokeThickness = 1, Data = g, Fill=Brushes.Beige };
            canvas.Children.Add(path);
         */
    }
}
