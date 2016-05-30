using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignalCompressionMUI.Models;

namespace SignalCompressionMUI.ViewModels
{
    class ModernChartsViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<ChartStat> _time = new ObservableCollection<ChartStat>();
        public ObservableCollection<ChartStat> Time
        {
            get
            {
                return _time;
            }
        }

        private readonly ObservableCollection<ChartStat> _errors = new ObservableCollection<ChartStat>();
        public ObservableCollection<ChartStat> Errors
        {
            get
            {
                return _errors;
            }
        }

        private readonly ObservableCollection<ChartStat> _compressRatio = new ObservableCollection<ChartStat>();
        public ObservableCollection<ChartStat> CompressRatio
        {
            get
            {
                return _compressRatio;
            }
        }

        public ModernChartsViewModel()
        {
            RDPModel.OnStatChanged += SetChartsStat;
            WaveletModel.OnStatChanged += SetChartsStat;
            DCTModel.OnStatChanged += SetChartsStat;
            SetChartsStat();
        }

        private void SetChartsStat()
        {
            _compressRatio.Clear();
            _compressRatio.Add(new ChartStat() {Name = "RDP", Count = RoundTo3(RDPModel.NothingStat.CompressionRatio)});
            _compressRatio.Add(new ChartStat() { Name = "RDP_Rise", Count = RoundTo3(RDPModel.RiseStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "RDP_RleRise", Count = RoundTo3(RDPModel.RleRiseStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "RDP_HuffRise", Count = RoundTo3(RDPModel.RiseHuffStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "WV", Count = RoundTo3(WaveletModel.NothingStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "WV_Rise", Count = RoundTo3(WaveletModel.RiseStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "WV_Rle", Count = RoundTo3(WaveletModel.RleStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "WV_RleRise", Count = RoundTo3(WaveletModel.RleRiseStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "WV_Huff", Count = RoundTo3(WaveletModel.HuffStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "WV_RleHuff", Count = RoundTo3(WaveletModel.RleHuffStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "DCT", Count = RoundTo3(DCTModel.NothingStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "DCT_RiseRle", Count = RoundTo3(DCTModel.RiseRleStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "DCT_Rise", Count = RoundTo3(DCTModel.RiseStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "DCT_RiseAcDc", Count = RoundTo3(DCTModel.RiseAcDcStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "DCT_RiseRleAcDc", Count = RoundTo3(DCTModel.RiseRleAcDcStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "DCT_Huff", Count = RoundTo3(DCTModel.HuffmanStat.CompressionRatio) });
            _compressRatio.Add(new ChartStat() { Name = "DCT_HuffRleAcDc", Count = RoundTo3(DCTModel.HuffmanRleAcDcStat.CompressionRatio) });

            _errors.Clear();
            _errors.Add(new ChartStat() { Name = "RDP", Count = RoundTo3(RDPModel.NothingStat.Error) });
            _errors.Add(new ChartStat() { Name = "RDP_Rise", Count = RoundTo3(RDPModel.RiseStat.Error) });
            _errors.Add(new ChartStat() { Name = "RDP_RleRise", Count = RoundTo3(RDPModel.RleRiseStat.Error) });
            _errors.Add(new ChartStat() { Name = "RDP_HuffRise", Count = RoundTo3(RDPModel.RiseHuffStat.Error) });
            _errors.Add(new ChartStat() { Name = "WV", Count = RoundTo3(WaveletModel.NothingStat.Error) });
            _errors.Add(new ChartStat() { Name = "WV_Rise", Count = RoundTo3(WaveletModel.RiseStat.Error) });
            _errors.Add(new ChartStat() { Name = "WV_Rle", Count = RoundTo3(WaveletModel.RleStat.Error) });
            _errors.Add(new ChartStat() { Name = "WV_RleRise", Count = RoundTo3(WaveletModel.RleRiseStat.Error) });
            _errors.Add(new ChartStat() { Name = "WV_Huff", Count = RoundTo3(WaveletModel.HuffStat.Error) });
            _errors.Add(new ChartStat() { Name = "WV_RleHuff", Count = RoundTo3(WaveletModel.RleHuffStat.Error) });
            _errors.Add(new ChartStat() { Name = "DCT", Count = RoundTo3(DCTModel.NothingStat.Error) });
            _errors.Add(new ChartStat() { Name = "DCT_RiseRle", Count = RoundTo3(DCTModel.RiseRleStat.Error) });
            _errors.Add(new ChartStat() { Name = "DCT_Rise", Count = RoundTo3(DCTModel.RiseStat.Error) });
            _errors.Add(new ChartStat() { Name = "DCT_RiseAcDc", Count = RoundTo3(DCTModel.RiseAcDcStat.Error) });
            _errors.Add(new ChartStat() { Name = "DCT_RiseRleAcDc", Count = RoundTo3(DCTModel.RiseRleAcDcStat.Error) });
            _errors.Add(new ChartStat() { Name = "DCT_Huff", Count = RoundTo3(DCTModel.HuffmanStat.Error) });
            _errors.Add(new ChartStat() { Name = "DCT_HuffRleAcDc", Count = RoundTo3(DCTModel.HuffmanRleAcDcStat.Error) });

            _time.Clear();
            _time.Add(new ChartStat() { Name = "RDP", Time = RDPModel.NothingStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "RDP_Rise", Time = RDPModel.RiseStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "RDP_RleRise", Time = RDPModel.RleRiseStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "RDP_HuffRise", Time = RDPModel.RiseHuffStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "WV", Time = WaveletModel.NothingStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "WV_Rise", Time = WaveletModel.RiseStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "WV_Rle", Time = WaveletModel.RleStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "WV_RleRise", Time = WaveletModel.RleRiseStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "WV_Huff", Time = WaveletModel.HuffStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "WV_RleHuff", Time = WaveletModel.RleHuffStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "DCT", Time = DCTModel.NothingStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "DCT_RiseRle", Time = DCTModel.RiseRleStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "DCT_Rise", Time = DCTModel.RiseStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "DCT_RiseAcDc", Time = DCTModel.RiseAcDcStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "DCT_RiseRleAcDc", Time = DCTModel.RiseRleAcDcStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "DCT_Huff", Time = DCTModel.HuffmanStat.Time.TotalMilliseconds });
            _time.Add(new ChartStat() { Name = "DCT_HuffRleAcDc", Time = DCTModel.HuffmanRleAcDcStat.Time.TotalMilliseconds });
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
}
