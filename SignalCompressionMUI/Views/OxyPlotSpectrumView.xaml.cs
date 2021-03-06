﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using FirstFloor.ModernUI.Windows.Controls;
using OxyPlot;
using OxyPlot.Wpf;
using SignalCompressionMUI.ViewModels;

namespace SignalCompressionMUI.Views
{
    /// <summary>
    /// Interaction logic for OxyPlotSpectrumView.xaml
    /// </summary>
    public partial class OxyPlotSpectrumView : UserControl
    {
        private OxyPlotSpectrumViewModel _vm;

        public OxyPlotSpectrumView()
        {
            InitializeComponent();
            _vm = (OxyPlotSpectrumViewModel)Resources["Vm"];
        }

        private void ToPdf()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Signals",
                DefaultExt = ".pdf",
                Filter = "Pdf files (.pdf)|*.pdf"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                try
                {
                    using (var stream = File.Create(filename))
                    {
                        var pdfExporter = new PdfExporter { Width = 1100, Height = 600 };
                        pdfExporter.Export(Plot.ActualModel, stream);
                    }
                    ModernDialog.ShowMessage("Файл успешно сохранен", "Результат операции", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    ModernDialog.ShowMessage(ex.Message, "Результат операции", MessageBoxButton.OK);
                }
            }
        }

        private void ToPng()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Signals",
                DefaultExt = ".png",
                Filter = "Png files (.png)|*.png"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                try
                {
                    using (var stream = File.Create(filename))
                    {
                        var pngExporter = new PngExporter { Width = 1100, Height = 600, Background = OxyColors.LightGray };
                        pngExporter.Export(Plot.ActualModel, stream);
                    }
                    ModernDialog.ShowMessage("Файл успешно сохранен", "Результат операции", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    ModernDialog.ShowMessage(ex.Message, "Результат операции", MessageBoxButton.OK);
                }
            }
        }

        private void ToClipboard()
        {
            var pngExporter = new PngExporter { Width = (int)Plot.ActualWidth, Height = (int)Plot.ActualHeight, Background = OxyColors.LightGray };
            var bitmap = pngExporter.ExportToBitmap(Plot.ActualModel);
            Clipboard.SetImage(bitmap);
        }

        private void ToSvg()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Signals",
                DefaultExt = ".svg",
                Filter = "Svg files (.svg)|*.svg"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                try
                {
                    using (var stream = File.Create(filename))
                    {
                        var exporter = new OxyPlot.Wpf.SvgExporter { Width = 1100, Height = 600 };
                        exporter.Export(Plot.ActualModel, stream);
                    }
                    ModernDialog.ShowMessage("Файл успешно сохранен", "Результат операции", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    ModernDialog.ShowMessage(ex.Message, "Результат операции", MessageBoxButton.OK);
                }
            }
        }

        private void ToPdf_OnClick(object sender, RoutedEventArgs e)
        {
            ToPdf();
        }

        private void ToPng_OnClick(object sender, RoutedEventArgs e)
        {
            ToPng();
        }

        private void ToClipboard_OnClick(object sender, RoutedEventArgs e)
        {
            ToClipboard();
        }

        private void ToSvg_OnClick(object sender, RoutedEventArgs e)
        {
            ToSvg();
        }

        private void SourseGraph_OnClick(object sender, RoutedEventArgs e)
        {
            if (_vm == null) return;
            var s = (ModernToggleButton)sender;
            SourseLineSeries.ItemsSource = s.IsChecked == true ? _vm.SpectrumSourse : null;
        }

        private void NewGraph_OnClick(object sender, RoutedEventArgs e)
        {
            if (_vm == null) return;
            var s = (ModernToggleButton)sender;
            NewLineSeries.ItemsSource = s.IsChecked == true ? _vm.SpectrumNew : null;
        }
    }
}
