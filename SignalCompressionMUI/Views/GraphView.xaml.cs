using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SignalCompressionMUI.Models;

namespace SignalCompressionMUI.Views
{
    /// <summary>
    /// Interaction logic for GraphView.xaml
    /// </summary>
    public partial class GraphView : UserControl
    {
        public static readonly DependencyProperty Accelerometer1Property =
            DependencyProperty.Register("Accelerometer1Points",
                typeof(PointCollection), typeof(GraphView));

        public PointCollection Accelerometer1Points
        {
            get { return (PointCollection)GetValue(Accelerometer1Property); }
            set { SetValue(Accelerometer1Property, value); }
        }

        public GraphView()
        {
            InitializeComponent();
            DataContext = this;
            var rnd = new Random();
            Accelerometer1Points = GetRndPointCollection(rnd);
        }

        private static PointCollection GetRndPointCollection(Random rnd)
        {
            var pCol = new PointCollection();
            //for (int i = 0; i < 3000; i += 10)
            //    pCol.Add(new Point(i, rnd.Next(0, 300)));
            for (int i = 0; i < RDPModel.PRez.Length/100; i++)
            {
                pCol.Add(new Point(RDPModel.PRez[i].X, RDPModel.PRez[i].Y));
            }
            return pCol;
        }


        private Point MouseDownPoint { get; set; }

        public static readonly DependencyProperty Polyline1TranslateProperty =
            DependencyProperty.Register("Polyline1Translate",
                typeof(Point), typeof(GraphView));

        public Point Polyline1Translate
        {
            get { return (Point)GetValue(Polyline1TranslateProperty); }
            set { SetValue(Polyline1TranslateProperty, value); }
        }

        private void Canvas1_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseDownPoint = new Point(e.GetPosition(Polyline1).X, e.GetPosition(Polyline1).Y);
        }

        private void Canvas1_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            var mouseUpPoint = new Point(e.GetPosition(Polyline1).X, e.GetPosition(Polyline1).Y);
            Polyline1Translate = new Point(Polyline1Translate.X + (mouseUpPoint.X - MouseDownPoint.X) / 10,
                Polyline1Translate.Y + (mouseUpPoint.Y - MouseDownPoint.Y) / 10);
        }

        private void Canvas1_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //Polyline1Scale.CenterX = e.GetPosition(Polyline1).X;
            //Polyline1Scale.CenterY = e.GetPosition(Polyline1).Y;

            var scaleValue = e.Delta / 1000f;
            if (Polyline1Scale.ScaleX + scaleValue < 0.1) return;
            Polyline1Scale.ScaleX += scaleValue;
            Polyline1Scale.ScaleY += scaleValue;
        }


        private void Scale()
        {
            var delta = Accelerometer1Points.Max(p => p.Y) - Accelerometer1Points.Min(p => p.Y);
            var scale = Canvas1.ActualHeight / delta;
            Polyline1Scale.ScaleX = scale;
            Polyline1Scale.ScaleY = scale;
            Polyline1Translate = new Point(0, 0);
        }

        private void Graph_OnLoaded(object sender, RoutedEventArgs e)
        {
            Scale();
        }
    }
}
