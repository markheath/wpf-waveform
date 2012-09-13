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
            this.radioButtonBezier.IsChecked = true;
            this.colorPickerFill.SelectedColor = Colors.Gray;
            this.colorPickerBottom.SelectedColor = Colors.LightGray;
            this.colorPickerOutline.SelectedColor = Colors.SlateGray;

            this.colorPickerFill.SelectedColorChanged += OnDrawingPropertyChanged;
            this.colorPickerOutline.SelectedColorChanged += OnDrawingPropertyChanged;
            this.radioButtonBezier.Click += OnDrawingPropertyChanged;
            this.radioButtonLinear.Click += OnDrawingPropertyChanged;
            this.radioButtonVertical.Click += OnDrawingPropertyChanged;
            this.checkBoxOutline.Click += OnDrawingPropertyChanged;
        }

        void OnDrawingPropertyChanged(object sender, RoutedEventArgs e)
        {
            if (this.mipMap != null)
            {
                GenerateWaveform();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All supported files|*.wav;*.mp3;*.ReaPeaks";
            bool? result = ofd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var generator = new WaveFormPointsGenerator();
                // 4100 = 10 points per second
                this.mipMap = generator.GetPeaks(ofd.FileName, 22050);
                GenerateWaveform();
            }
        }

        private MipMap mipMap;

        private void GenerateWaveform()
        {
            var generator = new WaveFormPointsGenerator();
            Brush strokeBrush = new SolidColorBrush(colorPickerOutline.SelectedColor);
            Brush fillBrush = new SolidColorBrush(colorPickerFill.SelectedColor);
            Brush bottomBrush = new SolidColorBrush(colorPickerBottom.SelectedColor);
            canvas.Children.Clear();
            var topPoints = mipMap.Peaks.Select(p => p.Channels[0].Max / 32768.0);
            var bottomPoints = mipMap.Peaks.Select(p => p.Channels[0].Min / 32768.0);

            if (radioButtonVertical.IsChecked.Value)
            {
                double xOffset = 0.5;
                var path = generator.GetAsVerticalLines(topPoints, bottomPoints, xOffset, 110, -100, strokeBrush);
                canvas.Children.Add(path);
            }
            else
            {
                if (!checkBoxOutline.IsChecked.Value) strokeBrush = null;
                double xStep = 1; // was 2
                double yMultTop = -100;
                double yMultBottom = -25;
                
                var topPath = radioButtonBezier.IsChecked.Value ? generator.GetBezierPath(topPoints, 0, xStep, 110, yMultTop, strokeBrush, fillBrush) : generator.GetLinearPath(topPoints, 0, xStep, 110, yMultTop, strokeBrush, fillBrush);
                var bottomPath = radioButtonBezier.IsChecked.Value ? generator.GetBezierPath(bottomPoints, 0, xStep, 110, yMultBottom, strokeBrush, bottomBrush) : generator.GetLinearPath(bottomPoints, 0, xStep, 110, yMultBottom, strokeBrush, bottomBrush);
                canvas.Children.Add(topPath);
                canvas.Children.Add(bottomPath);
            }
        }
    }
}
