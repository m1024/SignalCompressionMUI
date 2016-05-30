using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using FirstFloor.ModernUI.Presentation;
using SignalCompressionMUI.Models;
using SignalCompressionMUI.Models.Algorithms;
using SignalCompressionMUI.Models.Algorithms.Spectrum;
using ZedGraph;

namespace SignalCompressionMUI.Views
{
    /// <summary>
    /// Interaction logic for ZedGraphSpectrum.xaml
    /// </summary>
    public partial class ZedGraphSpectrumView : UserControl
    {

        private static IPointList _spectrumSourse;
        private static IPointList _spectrumNew;

        public static IPointList SpectrumSourse
        {
            get { return _spectrumSourse; }
            set
            {
                _spectrumSourse = value;
                OnConvertionComplete?.Invoke();
            }
        }
        public static IPointList SpectrumNew
        {
            get { return _spectrumNew; }
            set
            {
                _spectrumNew = value;
                OnConvertionComplete?.Invoke();
            }
        }

        public ZedGraphSpectrumView()
        {
            InitializeComponent();
            DrawSpectrums();
            OnConvertionComplete += DrawSpectrums;
            ZedGraphModel.OnThemeChanged += SetStyle;
            SetStyle();
        }

        public delegate void ConvertionComplete();
        public static event ConvertionComplete OnConvertionComplete;

        public static IPointList ArrayToPointList(int[] points)
        {
            var pointsList = new PointPairList();
            var k = 5000f / points.Length;
            for (int i = 0; i < points.Length; i++)
                pointsList.Add(new PointPair(i * k, points[i]));
            return pointsList;
        }

        public static IPointList ArrayToPointList(short[] points)
        {
            var pointsList = new PointPairList();
            var k = 5000f / points.Length;
            for (int i = 0; i < points.Length; i++)
                pointsList.Add(new PointPair(i * k, points[i]));
            return pointsList;
        }

        public static IPointList ArrayToPointList(MyPoint[] points)
        {
            var pointsList = new PointPairList();
            foreach (var t in points)
                pointsList.Add(new PointPair(t.X, t.Y));
            return pointsList;
        }


        public static int[] CalculateSpectrum(IReadOnlyList<short> sequence)
        {
            if (sequence == null) return new int[0];

            var n = (int)Math.Log(sequence.Count, 2) + 1;
            var len = (int)Math.Pow(2, n);
            var sLen = sequence.Count;

            var complexSequence = new Complex[len];

            //длина должна быть кратна 2, лишнее надо заполнять нулями
            for (int i = 0; i < len; i++)
                complexSequence[i] = (i < sLen)
                    ? new Complex(sequence[i], 0)
                    : complexSequence[i] = new Complex(0, 0);

            var spectrumComplex = FFT.fft(complexSequence);

            var spectrum = new int[spectrumComplex.Length / 2];
            for (int i = 0; i < spectrumComplex.Length / 2; i++)
                spectrum[i] = (int)Math.Sqrt(spectrumComplex[i].Real * spectrumComplex[i].Real +
                                              spectrumComplex[i].Imaginary * spectrumComplex[i].Imaginary);
            return spectrum;
        }

        private void DrawSpectrums()
        {
            zedGraph.GraphPane.CurveList.Clear();
            //SetStyles();

            if (SpectrumNew != null)
                DrawGraph(SpectrumNew, Color.Blue, "Декодированный сигнал");
            if (SpectrumSourse != null)
                DrawGraph(SpectrumSourse, Color.Red, "Исходный сигнал");

            // Вызываем метод AxisChange (), чтобы обновить данные об осях. 
            zedGraph.AxisChange();

            // Обновляем график
            zedGraph.Invalidate();
        }

        private void SetStyles()
        {
            GraphPane pane = zedGraph.GraphPane;

            int labelsXfontSize = 10;
            int labelsYfontSize = 10;

            int titleXFontSize = 10;
            int titleYFontSize = 10;

            int legendFontSize = 10;

            int mainTitleFontSize = 10;

            // Установим размеры шрифтов для меток вдоль осей
            pane.XAxis.Scale.FontSpec.Size = labelsXfontSize;
            pane.YAxis.Scale.FontSpec.Size = labelsYfontSize;

            // Установим размеры шрифтов для подписей по осям
            pane.XAxis.Title.FontSpec.Size = titleXFontSize;
            pane.YAxis.Title.FontSpec.Size = titleYFontSize;

            // Установим размеры шрифта для легенды
            pane.Legend.FontSpec.Size = legendFontSize;

            // Установим размеры шрифта для общего заголовка
            pane.Title.FontSpec.Size = mainTitleFontSize;
            pane.Title.FontSpec.IsUnderline = true;

            pane.Title.IsVisible = false;

            pane.Border.IsVisible = false;
            pane.Chart.Border.Color = Color.Gray;
            pane.XAxis.Color = Color.Gray;
            pane.YAxis.Color = Color.Gray;

            // Установим цвет для сетки
            pane.XAxis.MajorGrid.Color = Color.Gray;
            pane.YAxis.MajorGrid.Color = Color.Gray;

            // Установим цвет для подписей рядом с осями
            pane.XAxis.Title.FontSpec.FontColor = Color.Black;
            pane.YAxis.Title.FontSpec.FontColor = Color.Black;

            // Установим цвет подписей под метками
            pane.XAxis.Scale.FontSpec.FontColor = Color.Black;
            pane.YAxis.Scale.FontSpec.FontColor = Color.Black;

            pane.XAxis.MajorTic.Color = Color.Gray;
            pane.XAxis.MinorTic.Color = Color.Gray;

            pane.YAxis.MajorTic.Color = Color.Gray;
            pane.YAxis.MinorTic.Color = Color.Gray;

            pane.Legend.Border.IsVisible = false;

            pane.XAxis.MajorGrid.IsVisible = true;
            pane.YAxis.MajorGrid.IsVisible = true;
        }

        private void SetStyle()
        {
            var pane = zedGraph.GraphPane;

            var labelsXfontSize = 8;
            var labelsYfontSize = 8;
            var titleXFontSize = 8;
            var titleYFontSize = 8;
            var legendFontSize = 8;
            var mainTitleFontSize = 8;

            pane.XAxis.Title.Text = "Частота, Гц";
            pane.YAxis.Title.Text = "";

            // Установим размеры шрифтов для меток вдоль осей
            pane.XAxis.Scale.FontSpec.Size = labelsXfontSize;
            pane.YAxis.Scale.FontSpec.Size = labelsYfontSize;

            // Установим размеры шрифтов для подписей по осям
            pane.XAxis.Title.FontSpec.Size = titleXFontSize;
            pane.YAxis.Title.FontSpec.Size = titleYFontSize;

            // Установим размеры шрифта для легенды
            pane.Legend.FontSpec.Size = legendFontSize;

            // Установим размеры шрифта для общего заголовка
            pane.Title.FontSpec.Size = mainTitleFontSize;
            pane.Title.FontSpec.IsUnderline = true;

            pane.Title.IsVisible = false;
            pane.Border.IsVisible = false;

            if (ZedGraphModel.ThemeType == AppearanceManager.DarkThemeSource)
            {
                pane.Fill.Color = System.Drawing.ColorTranslator.FromHtml("#FF252526");
                pane.Chart.Fill.Type = FillType.Solid;
                pane.Chart.Fill.Color = System.Drawing.ColorTranslator.FromHtml("#FF252526");
                pane.Legend.Fill.Type = FillType.Solid;
                pane.Legend.Fill.Color = System.Drawing.ColorTranslator.FromHtml("#FF252526");

                pane.Border.IsVisible = false;
                pane.Chart.Border.Color = Color.Gray;
                pane.XAxis.Color = Color.Gray;
                pane.YAxis.Color = Color.Gray;

                // Установим цвет для сетки
                pane.XAxis.MajorGrid.Color = Color.Gray;
                pane.YAxis.MajorGrid.Color = Color.Gray;

                // Установим цвет для подписей рядом с осями
                pane.XAxis.Title.FontSpec.FontColor = Color.LightGray;
                pane.YAxis.Title.FontSpec.FontColor = Color.LightGray;
                pane.Legend.FontSpec.FontColor = Color.LightGray;

                // Установим цвет подписей под метками
                pane.XAxis.Scale.FontSpec.FontColor = Color.LightGray;
                pane.YAxis.Scale.FontSpec.FontColor = Color.LightGray;

                pane.XAxis.MajorTic.Color = Color.Gray;
                pane.XAxis.MinorTic.Color = Color.Gray;

                pane.YAxis.MajorTic.Color = Color.Gray;
                pane.YAxis.MinorTic.Color = Color.Gray;
            }
            else
            {
                pane.Fill.Color = Color.White;
                pane.Chart.Fill.Type = FillType.Solid;
                pane.Chart.Fill.Color = Color.White;
                pane.Legend.Fill.Type = FillType.Solid;
                pane.Legend.Fill.Color = Color.White;

                pane.Border.IsVisible = false;
                pane.Chart.Border.Color = Color.Gray;
                pane.XAxis.Color = Color.Gray;
                pane.YAxis.Color = Color.Gray;

                // Установим цвет для сетки
                pane.XAxis.MajorGrid.Color = Color.Gray;
                pane.YAxis.MajorGrid.Color = Color.Gray;

                // Установим цвет для подписей рядом с осями
                pane.XAxis.Title.FontSpec.FontColor = Color.Black;
                pane.YAxis.Title.FontSpec.FontColor = Color.Black;

                // Установим цвет подписей под метками
                pane.XAxis.Scale.FontSpec.FontColor = Color.Black;
                pane.YAxis.Scale.FontSpec.FontColor = Color.Black;
                pane.Legend.FontSpec.FontColor = Color.Black;

                pane.XAxis.MajorTic.Color = Color.Gray;
                pane.XAxis.MinorTic.Color = Color.Gray;

                pane.YAxis.MajorTic.Color = Color.Gray;
                pane.YAxis.MinorTic.Color = Color.Gray;
            }

            pane.Legend.Border.IsVisible = false;
            pane.XAxis.MajorGrid.IsVisible = true;
            pane.YAxis.MajorGrid.IsVisible = true;
        }

        /// <summary>
        /// Рисование одной кривой
        /// </summary>
        /// <param name="points">Точки для кривой</param>
        /// <param name="color">Цвет для кривой</param>
        private void DrawGraph(IPointList points, Color color, string title)
        {
            zedGraph.GraphPane.AddCurve(title, points, color, SymbolType.None);
        }
    }
}
