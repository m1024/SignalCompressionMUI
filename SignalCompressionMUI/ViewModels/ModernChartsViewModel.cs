using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
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
            if (!double.IsNaN(RDPModel.NothingStat.CompressionRatio))
            _compressRatio.Add(new ChartStat() {Name = "RDP", Count = RoundTo3(RDPModel.NothingStat.CompressionRatio)});
            if (!double.IsNaN(RDPModel.RiseStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "RDP_Rise", Count = RoundTo3(RDPModel.RiseStat.CompressionRatio) });
            if (!double.IsNaN(RDPModel.RleRiseStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "RDP_RleRise", Count = RoundTo3(RDPModel.RleRiseStat.CompressionRatio) });
            if (!double.IsNaN(RDPModel.RiseHuffStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "RDP_HuffRise", Count = RoundTo3(RDPModel.RiseHuffStat.CompressionRatio) });
            if (!double.IsNaN(WaveletModel.NothingStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "WV", Count = RoundTo3(WaveletModel.NothingStat.CompressionRatio) });
            if (!double.IsNaN(WaveletModel.RiseStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "WV_Rise", Count = RoundTo3(WaveletModel.RiseStat.CompressionRatio) });
            if (!double.IsNaN(WaveletModel.RleStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "WV_Rle", Count = RoundTo3(WaveletModel.RleStat.CompressionRatio) });
            if (!double.IsNaN(WaveletModel.RleRiseStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "WV_RleRise", Count = RoundTo3(WaveletModel.RleRiseStat.CompressionRatio) });
            if (!double.IsNaN(WaveletModel.HuffStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "WV_Huff", Count = RoundTo3(WaveletModel.HuffStat.CompressionRatio) });
            if (!double.IsNaN(WaveletModel.RleHuffStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "WV_RleHuff", Count = RoundTo3(WaveletModel.RleHuffStat.CompressionRatio) });
            if (!double.IsNaN(DCTModel.NothingStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "DCT", Count = RoundTo3(DCTModel.NothingStat.CompressionRatio) });
            if (!double.IsNaN(DCTModel.RiseRleStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "DCT_RiseRle", Count = RoundTo3(DCTModel.RiseRleStat.CompressionRatio) });
            if (!double.IsNaN(DCTModel.RiseStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "DCT_Rise", Count = RoundTo3(DCTModel.RiseStat.CompressionRatio) });
            if (!double.IsNaN(DCTModel.RiseAcDcStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "DCT_RiseAcDc", Count = RoundTo3(DCTModel.RiseAcDcStat.CompressionRatio) });
            if (!double.IsNaN(DCTModel.RiseRleAcDcStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "DCT_RiseRleAcDc", Count = RoundTo3(DCTModel.RiseRleAcDcStat.CompressionRatio) });
            if (!double.IsNaN(DCTModel.HuffmanStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "DCT_Huff", Count = RoundTo3(DCTModel.HuffmanStat.CompressionRatio) });
            if (!double.IsNaN(DCTModel.HuffmanRleAcDcStat.CompressionRatio))
                _compressRatio.Add(new ChartStat() { Name = "DCT_HuffRleAcDc", Count = RoundTo3(DCTModel.HuffmanRleAcDcStat.CompressionRatio) });

            _errors.Clear();
            if (RDPModel.NothingStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "RDP", Count = RoundTo3(RDPModel.NothingStat.Error) });
            if (RDPModel.RiseStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "RDP_Rise", Count = RoundTo3(RDPModel.RiseStat.Error) });
            if (RDPModel.RleRiseStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "RDP_RleRise", Count = RoundTo3(RDPModel.RleRiseStat.Error) });
            if (RDPModel.RiseHuffStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "RDP_HuffRise", Count = RoundTo3(RDPModel.RiseHuffStat.Error) });
            if (WaveletModel.NothingStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "WV", Count = RoundTo3(WaveletModel.NothingStat.Error) });
            if (WaveletModel.RiseStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "WV_Rise", Count = RoundTo3(WaveletModel.RiseStat.Error) });
            if (WaveletModel.RleStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "WV_Rle", Count = RoundTo3(WaveletModel.RleStat.Error) });
            if (WaveletModel.RleRiseStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "WV_RleRise", Count = RoundTo3(WaveletModel.RleRiseStat.Error) });
            if (WaveletModel.HuffStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "WV_Huff", Count = RoundTo3(WaveletModel.HuffStat.Error) });
            if (WaveletModel.RleHuffStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "WV_RleHuff", Count = RoundTo3(WaveletModel.RleHuffStat.Error) });
            if (DCTModel.NothingStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "DCT", Count = RoundTo3(DCTModel.NothingStat.Error) });
            if (DCTModel.RiseRleStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "DCT_RiseRle", Count = RoundTo3(DCTModel.RiseRleStat.Error) });
            if (DCTModel.RiseStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "DCT_Rise", Count = RoundTo3(DCTModel.RiseStat.Error) });
            if (DCTModel.RiseAcDcStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "DCT_RiseAcDc", Count = RoundTo3(DCTModel.RiseAcDcStat.Error) });
            if (DCTModel.RiseRleAcDcStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "DCT_RiseRleAcDc", Count = RoundTo3(DCTModel.RiseRleAcDcStat.Error) });
            if (DCTModel.HuffmanStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "DCT_Huff", Count = RoundTo3(DCTModel.HuffmanStat.Error) });
            if (DCTModel.HuffmanRleAcDcStat.Error != 0)
                _errors.Add(new ChartStat() { Name = "DCT_HuffRleAcDc", Count = RoundTo3(DCTModel.HuffmanRleAcDcStat.Error) });

            _time.Clear();
            if (RDPModel.NothingStat.Time.TotalMilliseconds != 0)
            _time.Add(new ChartStat() { Name = "RDP", Time = RDPModel.NothingStat.Time.TotalMilliseconds });
            if (RDPModel.RiseStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "RDP_Rise", Time = RDPModel.RiseStat.Time.TotalMilliseconds });
            if (RDPModel.RleRiseStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "RDP_RleRise", Time = RDPModel.RleRiseStat.Time.TotalMilliseconds });
            if (RDPModel.RiseHuffStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "RDP_HuffRise", Time = RDPModel.RiseHuffStat.Time.TotalMilliseconds });
            if (WaveletModel.NothingStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "WV", Time = WaveletModel.NothingStat.Time.TotalMilliseconds });
            if (WaveletModel.RiseStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "WV_Rise", Time = WaveletModel.RiseStat.Time.TotalMilliseconds });
            if (WaveletModel.RleStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "WV_Rle", Time = WaveletModel.RleStat.Time.TotalMilliseconds });
            if (WaveletModel.RleRiseStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "WV_RleRise", Time = WaveletModel.RleRiseStat.Time.TotalMilliseconds });
            if (WaveletModel.HuffStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "WV_Huff", Time = WaveletModel.HuffStat.Time.TotalMilliseconds });
            if (WaveletModel.RleHuffStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "WV_RleHuff", Time = WaveletModel.RleHuffStat.Time.TotalMilliseconds });
            if (DCTModel.NothingStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "DCT", Time = DCTModel.NothingStat.Time.TotalMilliseconds });
            if (DCTModel.RiseRleStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "DCT_RiseRle", Time = DCTModel.RiseRleStat.Time.TotalMilliseconds });
            if (DCTModel.RiseStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "DCT_Rise", Time = DCTModel.RiseStat.Time.TotalMilliseconds });
            if (DCTModel.RiseAcDcStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "DCT_RiseAcDc", Time = DCTModel.RiseAcDcStat.Time.TotalMilliseconds });
            if (DCTModel.RiseRleAcDcStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "DCT_RiseRleAcDc", Time = DCTModel.RiseRleAcDcStat.Time.TotalMilliseconds });
            if (DCTModel.HuffmanStat.Time.TotalMilliseconds != 0)
                _time.Add(new ChartStat() { Name = "DCT_Huff", Time = DCTModel.HuffmanStat.Time.TotalMilliseconds });
            if (DCTModel.HuffmanRleAcDcStat.Time.TotalMilliseconds != 0)
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
