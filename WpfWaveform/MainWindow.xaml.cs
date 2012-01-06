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
            var generator = new WaveFormPointsGenerator();
            var peaks = generator.GetPeaks(@"E:\Audio\Music\Coldplay\X&Y\04-Fix You.mp3", 200);

            canvas.Children.Add(generator.GetBezierPath(peaks.Select(p => p.LeftMax), 0, 2, 110, -100, Brushes.Brown, Brushes.Beige));
            canvas.Children.Add(generator.GetBezierPath(peaks.Select(p => p.LeftMin), 0, 2, 110, -100, Brushes.Brown, Brushes.Beige));
        }

    }
}
