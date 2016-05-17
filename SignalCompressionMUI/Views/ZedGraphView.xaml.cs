using System.Windows.Controls;
using SignalCompressionMUI.Models.Algorithms;
using ZedGraph;
using Color = System.Drawing.Color;

namespace SignalCompressionMUI.Views
{
    public partial class ZedGraphView : UserControl
    {
        private static IPointList _curveSourse;
        private static IPointList _curveNew;

        public static IPointList CurveSourse
        {
            get { return _curveSourse; }
            set
            {
                _curveSourse = value;
                OnConvertionComplete?.Invoke();
            }
        }
        public static IPointList CurveNew
        {
            get { return _curveNew; }
            set
            {
                _curveNew = value;
                OnConvertionComplete?.Invoke();
            }
        }

        public ZedGraphView()
        {
            InitializeComponent();
            DrawCurves();
            OnConvertionComplete += DrawCurves;
        }

        public delegate void ConvertionComplete();
        public static event ConvertionComplete OnConvertionComplete;


        public static IPointList ListToPointList(short[] points)
        { 
            var pointsList = new PointPairList();
            for (int i=0; i<points.Length; i++)
                pointsList.Add(new PointPair(i, points[i]));
            return pointsList;
        }

        public static IPointList ListToPointList(MyPoint[] points)
        {
            var pointsList = new PointPairList();
            foreach (var t in points)
                pointsList.Add(new PointPair(t.X, t.Y));
            return pointsList;
        }

        private void DrawCurves()
        {
            zedGraph.GraphPane.CurveList.Clear();
            SetStyles();

            if (CurveNew != null)
                DrawGraph(CurveNew, Color.Blue);
            if (CurveSourse != null)
                DrawGraph(CurveSourse, Color.Red);

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

        /// <summary>
        /// Рисование одной кривой
        /// </summary>
        /// <param name="points">Точки для кривой</param>
        /// <param name="color">Цвет для кривой</param>
        private void DrawGraph(IPointList points, Color color)
        {
            zedGraph.GraphPane.AddCurve(points.Count + " точек", points, color, SymbolType.None);
        }
    }
}
