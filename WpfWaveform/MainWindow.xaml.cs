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
using Microsoft.Win32;

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
            this.checkBoxBezier.IsChecked = true;
            this.colorPickerFill.SelectedColor = Colors.LightGray;
            this.colorPickerOutline.SelectedColor = Colors.SlateGray;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All supported files|*.wav;*.mp3;*.ReaPeaks";
            bool? result = ofd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                GenerateWaveform(ofd.FileName);
            }
        }

        private void GenerateWaveform(string fileName)
        {
            var generator = new WaveFormPointsGenerator();
            var mipMap = generator.GetPeaks(fileName, 4100);
            Brush strokeBrush = (checkBoxOutline.IsChecked.Value) ? new SolidColorBrush(colorPickerOutline.SelectedColor) : null;
            Brush fillBrush = new SolidColorBrush(colorPickerFill.SelectedColor);
            canvas.Children.Clear();

            var topPoints = mipMap.Peaks.Select(p => p.Channels[0].Max / 32768.0);
            var bottomPoints = mipMap.Peaks.Select(p => p.Channels[0].Min / 32768.0);

            var topPath = checkBoxBezier.IsChecked.Value ? generator.GetBezierPath(topPoints, 0, 2, 110, -100, strokeBrush, fillBrush) : generator.GetLinearPath(topPoints, 0, 2, 110, -100, strokeBrush, fillBrush);
            var bottomPath = checkBoxBezier.IsChecked.Value ? generator.GetBezierPath(bottomPoints, 0, 2, 110, -100, strokeBrush, fillBrush) : generator.GetLinearPath(bottomPoints, 0, 2, 110, -100, strokeBrush, fillBrush);
            canvas.Children.Add(topPath);
            canvas.Children.Add(bottomPath);
        }


    }
}
