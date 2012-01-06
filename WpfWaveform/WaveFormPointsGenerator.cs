using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfWaveform
{
    class Peak
    {
        public double LeftMin  { get; private set; }
        public double LeftMax  { get; private set; }
        public double RightMin { get; private set; }
        public double RightMax { get; private set; }

        public Peak(double leftMin, double leftMax, double rightMin, double rightMax)
        {
            this.LeftMin  = leftMin;
            this.LeftMax  = leftMax;
            this.RightMin = rightMin;
            this.RightMax = rightMax;
        }
    }

    class WaveFormPointsGenerator
    {
        public IEnumerable<Peak> GetPeaks(string fileName, int millisecondsPerUpdate)
        {
            List<Peak> peaks = new List<Peak>();
            peaks.Add(new Peak(0,0,0,0));
            using (var reader = new Mp3FileReader(fileName))
            {
                int stepSize = (reader.WaveFormat.AverageBytesPerSecond / 1000) * millisecondsPerUpdate;
                WaveBuffer buffer = new WaveBuffer(stepSize);
                int read;
                while ((read = reader.Read(buffer.ByteBuffer, 0, stepSize)) > 0)
                {
                    int samples = read / 2; // assume 16 bit
                    double maxLeft = 0;
                    double minLeft = 0;
                    double maxRight = 0;
                    double minRight = 0;
                    for (int sample = 0; sample < samples; sample += 2)
                    {
                        double sampleLeft = buffer.ShortBuffer[sample] / 32768.0;
                        maxLeft = Math.Max(maxLeft, sampleLeft);
                        minLeft = Math.Min(minLeft, sampleLeft);
                        double sampleRight = buffer.ShortBuffer[sample + 1] / 32768.0;
                        maxRight = Math.Max(maxRight, sampleLeft);
                        minRight = Math.Min(minRight, sampleLeft);
                    }
                    peaks.Add(new Peak(minLeft, maxLeft, minRight, maxRight));
                }
            }
            peaks.Add(new Peak(0,0,0,0));
            return peaks;
        }

        public Path GetBezierPath(IEnumerable<double> magnitude, double xOffset, double xStep, double yOffset, double yMult, Brush stroke, Brush fill)
        {
            var points = GetPoints(magnitude, 0, 2, 110, -100).ToArray();
            var geometry = GetBezierPathGeometry(points);
            var path = new Path() { Stroke = stroke, StrokeThickness = 1, Data = geometry, Fill = fill };
            return path;
        }

        public Path GetLinearPath(IEnumerable<double> magnitude, double xOffset, double xStep, double yOffset, double yMult, Brush stroke, Brush fill)
        {
            var points = GetPoints(magnitude, 0, 2, 110, -100).ToArray();
            var geometry = GetLinearPathGeometry(points);
            var path = new Path() { Stroke = stroke, StrokeThickness = 1, Data = geometry, Fill = fill };
            return path;
        }

        private IEnumerable<Point> GetPoints(IEnumerable<double> magnitude, double xOffset, double xStep, double yOffset, double yMult)
        {
            List<Point> points = new List<Point>();
            foreach (var m in magnitude)
            {
                points.Add(new Point(xOffset, yOffset + m * yMult));
                xOffset += xStep;
            }
            return points;
        }

        private PathGeometry GetBezierPathGeometry(Point[] points)
        {
            // Get Bezier Spline Control Points.
            Point[] cp1, cp2;
            BezierSpline.GetCurveControlPoints(points, out cp1, out cp2);

            // Draw curve by Bezier.
            PathSegmentCollection lines = new PathSegmentCollection();
            for (int i = 0; i < cp1.Length; ++i)
            {
                lines.Add(new BezierSegment(cp1[i], cp2[i], points[i + 1], true));
            }
            PathFigure f = new PathFigure(points[0], lines, false);
            PathGeometry g = new PathGeometry(new PathFigure[] { f });
            return g;
        }

        private PathGeometry GetLinearPathGeometry(Point[] points)
        {
            // Draw curve by Bezier.
            PathSegmentCollection lines = new PathSegmentCollection();
            lines.Add(new PolyLineSegment(points, true));
            PathFigure f = new PathFigure(points[0], lines, false);
            PathGeometry g = new PathGeometry(new PathFigure[] { f });
            return g;
        }
    }
}
