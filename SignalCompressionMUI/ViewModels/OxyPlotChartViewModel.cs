using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignalCompressionMUI.Models;
using SignalCompressionMUI.Views;

namespace SignalCompressionMUI.ViewModels
{
    class OxyPlotChartViewModel : INotifyPropertyChanged
    {


        public ObservableCollection<ItemNew> CompressRatio { get; set; }

        public OxyPlotChartViewModel()
        {
            CompressRatio = new ObservableCollection<ItemNew>();
            SetChartsStat();
            RDPModel.OnStatChanged += SetChartsStat;
        }

        private void SetChartsStat()
        {
            CompressRatio.Clear();
            CompressRatio.Add(new ItemNew()
            {
                Label = "RDP",
                Value1 = RoundTo3(RDPModel.NothingStat.CompressionRatio),
                Value2 = RoundTo3(RDPModel.RiseStat.CompressionRatio),
                Value3 = RoundTo3(RDPModel.RleRiseStat.CompressionRatio),
                Value4 = RoundTo3(RDPModel.RiseHuffStat.CompressionRatio)
            });
        }

        private double RoundTo3(double d) => d != d ? 0 : Math.Round(d, 3);

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class ItemNew
    {
        public string Label { get; set; }
        public double Value1 { get; set; }
        public double Value2 { get; set; }
        public double Value3 { get; set; }
        public double Value4 { get; set; }
    }
}
