﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace SignalCompressionMUI.Views
{
    /// <summary>
    /// Interaction logic for BasicPage1.xaml
    /// </summary>
    public partial class OxyPlotChartView : UserControl
    {
        public OxyPlotChartView()
        {
            this.InitializeComponent();

            // Create some data
            //this.Items = new Collection<Item>
            //                {
            //                    new Item {Label = "Apples", Value1 = 37, Value2 = 12, Value3 = 19},
            //                    new Item {Label = "Pears", Value1 = 7, Value2 = 21, Value3 = 9},
            //                    new Item {Label = "Bananas", Value1 = 23, Value2 = 2, Value3 = 29}
            //                };

            //// Create the plot model
            //var tmp = new PlotModel { Title = "Bar series", LegendPlacement = LegendPlacement.Outside, LegendPosition = LegendPosition.RightTop, LegendOrientation = LegendOrientation.Vertical };

            //// Add the axes, note that MinimumPadding and AbsoluteMinimum should be set on the value axis.
            //tmp.Axes.Add(new CategoryAxis { Position = AxisPosition.Left, ItemsSource = this.Items, LabelField = "Label" });
            //tmp.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MinimumPadding = 0, AbsoluteMinimum = 0 });

            //// Add the series, note that the BarSeries are using the same ItemsSource as the CategoryAxis.
            //tmp.Series.Add(new BarSeries { Title = "2009", ItemsSource = this.Items, ValueField = "Value1" });
            //tmp.Series.Add(new BarSeries { Title = "2010", ItemsSource = this.Items, ValueField = "Value2" });
            //tmp.Series.Add(new BarSeries { Title = "2011", ItemsSource = this.Items, ValueField = "Value3" });

           // this.Model1 = tmp;

            //this.DataContext = this;
        }

        //public Collection<Item> Items { get; set; }
    }
    //public class Item
    //{
    //    public string Label { get; set; }
    //    public double Value1 { get; set; }
    //    public double Value2 { get; set; }
    //    public double Value3 { get; set; }
    //}
}
