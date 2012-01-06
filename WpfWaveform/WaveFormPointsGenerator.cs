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
    class WaveFormPointsGenerator
    {
        public MipMap GetPeaks(string fileName, int samplesPerPeak)
        {
            if (fileName.EndsWith(".ReaPeaks", StringComparison.CurrentCultureIgnoreCase))
            {
                var reaPeaks = new ReaPeaksFileReader(fileName);
                return reaPeaks.MipMaps[0];
            }

            MipMap m = new MipMap();
            m.DivisionFactor = samplesPerPeak;

            List<PeakValues> peaks = new List<PeakValues>();
            using (var reader = GetReader(fileName))
            {
                int channels = reader.WaveFormat.Channels;
                int stepSize = samplesPerPeak * channels * (reader.WaveFormat.BitsPerSample / 8);
                WaveBuffer buffer = new WaveBuffer(stepSize);
                int read;
                while ((read = reader.Read(buffer.ByteBuffer, 0, stepSize)) > 0)
                {
                    PeakValues peakValues = new PeakValues(reader.WaveFormat.Channels);
                    int samples = read / 2; // assume 16 bit
                    
                    for (int index = 0; index < samples; index++)
                    {
                        int ch = index % channels;
                        peakValues.Channels[ch].Max = Math.Max(peakValues.Channels[ch].Max, buffer.ShortBuffer[index]);
                        peakValues.Channels[ch].Min = Math.Min(peakValues.Channels[ch].Min, buffer.ShortBuffer[index]);
                    }
                    peaks.Add(peakValues);
                }
            }
            m.Peaks = peaks.ToArray();
            return m;
        }

        private WaveStream GetReader(string fileName)
        {
            if (fileName.EndsWith(".mp3", StringComparison.CurrentCultureIgnoreCase))
            {
                return new Mp3FileReader(fileName);
            }
            else if (fileName.EndsWith(".wav", StringComparison.CurrentCultureIgnoreCase))
            {
                return new WaveFileReader(fileName);
            }
            else
            {
                throw new ArgumentException("Unsupported file type");
            }
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
            points.Add(new Point(xOffset, yOffset)); xOffset += xStep; // extra zero point at beginning
            foreach (var m in magnitude)
            {
                points.Add(new Point(xOffset, yOffset + m * yMult));
                xOffset += xStep;
            }
            points.Add(new Point(xOffset, yOffset)); xOffset += xStep; // extra zero point at end
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
