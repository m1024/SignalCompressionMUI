using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows.Controls;
using SignalCompressionMUI.Models;
using SignalCompressionMUI.Models.Algorithms;
using SignalCompressionMUI.Views;

namespace SignalCompressionMUI.ViewModels
{
    public enum СoeffCount
    {
        [Description("Все")]
        All,
        [Description("Половина")]
        Half,
        [Description("Четверть")]
        Quart
    }
    class WaveletViewModel : INotifyPropertyChanged
    {
        #region Fields and properties

        private string _fileName;
        private int _blockSize = 1000;
        private СoeffCount _coeffCount;
        private int _rounding = 10;
        private int _depth = 2;
        private List<Statistic> _statisticTable;
        private Visibility _pBar;
        private WaveletType _wvType;

        public СoeffCount CoeffCount
        {
            get { return _coeffCount; }
            set
            {
                _coeffCount = value;
                OnPropertyChanged("CoeffCount");
            }
        }

        public WaveletType WvType
        {
            get { return _wvType; }
            set
            {
                _wvType = value;
                OnPropertyChanged("WvType");
            }
        }

        public int Rounding
        {
            get { return _rounding; }
            set
            {
                if (value >= 1)
                    _rounding = value;
                OnPropertyChanged("Rounding");
            }
        }

        public int Depth
        {
            get { return _depth; }
            set
            {
                if (CoeffCount == СoeffCount.Quart && value < 1) return;
                if (value >= 0)
                    _depth = value;
                OnPropertyChanged("Depth");
            }
        }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        public int BlockSize
        {
            get { return _blockSize; }
            set
            {
                if (value >= 8 && value % 8 == 0)
                    _blockSize = value;
                OnPropertyChanged("BlockSize");
            }
        }

        public List<Statistic> StatisticTable
        {
            get { return _statisticTable; }
            set
            {
                _statisticTable = value;
                OnPropertyChanged("StatisticTable");
            }
        }

        public Visibility PBar
        {
            get { return _pBar; }
            set
            {
                _pBar = value;
                OnPropertyChanged("PBar");
            }
        }

        #endregion

        public WaveletViewModel()
        {
            OpenFileCommand = new RelayCommand(arg => OpenFile());
            ConvertCommand = new RelayCommand(arg => Convert());
            PBar = Visibility.Hidden;
            CoeffCount = СoeffCount.All;
        }

        #region Commands

        public ICommand OpenFileCommand { get; set; }

        public ICommand ConvertCommand { get; set; }

        #endregion

        #region Methods

        private void Convert()
        {
            try
            {
                WaveletModel.Read(FileName);
            }
            catch (Exception ex)
            {
                ModernDialog.ShowMessage(ex.Message, "Ошибка", MessageBoxButton.OK);
                return;
            }

            //AlgorithmWv.Type = WaveletType.Haar;
            //List<double> sl;
            //List<double> sh;
            //List<double> outList;
            //AlgorithmWv.Convert(new List<short>() {(short) 1, (short) 2, (short) 3, (short) 4}, out sl, out sh);
            //AlgorithmWv.Convert(new List<short>() { (short)1, (short)2, (short)3, (short)4}, out outList);
            //List<short> deconv = AlgorithmWv.Deconvert(outList, AlgorithmWv.Delta);


            ////------------------------------------------- Tree
            //short[] block = new short[BlockSize];
            //Array.Copy(WaveletModel.SequenceSourse, block, BlockSize);

            //WaveletArray wvArray = new WaveletArray
            //{
            //    Sourse = block.ToList(),
            //    ConvertType = WvType
            //};
            //wvArray.Convert();
            //wvArray.Round(Rounding);

            //var wvTree = new WaveletTree(wvArray);
            //wvTree.BuildTree(Depth, Rounding);
            //wvTree.ConvertedToRoot();

            ////округление
            //var rounded = WaveletModel.RoundListList(wvTree.Converted, Rounding);
            //var derounded = WaveletModel.DeRoundListList(rounded, Rounding);


            //var deconvWvTree = new WaveletTree { Converted = derounded };
            //deconvWvTree.BuidDeconvTree();
            //deconvWvTree.Deconvert(WvType);

            //ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(block);
            //ZedGraphView.CurveNew = ZedGraphView.ListToPointList(deconvWvTree.wvArray.New.ToArray());


            //ZedGraphSpectrumView.SpectrumSourse =
            //    ZedGraphSpectrumView.ArrayToPointList(ZedGraphSpectrumView.CalculateSpectrum(block));
            //ZedGraphSpectrumView.SpectrumNew =
            //    ZedGraphSpectrumView.ArrayToPointList(ZedGraphSpectrumView.CalculateSpectrum(deconvWvTree.wvArray.New.ToArray()));
            ////-----------------------------------------------------



            var stat = WaveletModel.Convert(WvType, CoeffCount, Rounding, BlockSize, Depth);
            WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);

            ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(WaveletModel.SequenceSourse);
            ZedGraphView.CurveNew = ZedGraphView.ListToPointList(WaveletModel.SequenceSmoothed);


            ZedGraphSpectrumView.SpectrumSourse =
                ZedGraphSpectrumView.ArrayToPointList(ZedGraphSpectrumView.CalculateSpectrum(WaveletModel.SequenceSourse));
            ZedGraphSpectrumView.SpectrumNew =
                ZedGraphSpectrumView.ArrayToPointList(ZedGraphSpectrumView.CalculateSpectrum(WaveletModel.SequenceSmoothed));

            StatisticTable = stat;


            ////---------------------------------------- not tree
            //WaveletArray wvArray = new WaveletArray
            //{
            //    Sourse = WaveletModel.SequenceSourse.ToList(),
            //    ConvertType = WvType
            //};
            //wvArray.Convert();
            //wvArray.Round(Rounding);
            //wvArray.DeconvertRoundedHulf();

            //ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(WaveletModel.SequenceSourse);
            //ZedGraphView.CurveNew = ZedGraphView.ListToPointList(wvArray.New.ToArray());

            //ZedGraphSpectrumView.SpectrumSourse =
            //    ZedGraphSpectrumView.ArrayToPointList(ZedGraphSpectrumView.CalculateSpectrum(WaveletModel.SequenceSourse));
            //ZedGraphSpectrumView.SpectrumNew =
            //    ZedGraphSpectrumView.ArrayToPointList(ZedGraphSpectrumView.CalculateSpectrum(wvArray.New.ToArray()));
            ////------------------------------------
        }

        private void OpenFile()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                // Open document
                var filename = dlg.FileName;
                FileName = filename;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
