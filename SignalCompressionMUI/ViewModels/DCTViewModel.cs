using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SignalCompressionMUI.Models;
using SignalCompressionMUI.Models.Algorithms;
using SignalCompressionMUI.Views;

namespace SignalCompressionMUI.ViewModels
{
    class DCTViewModel : INotifyPropertyChanged
    {
        #region Fields and properties

        private string _fileName;
        private int _blockSize = 1000;
        private int _coeffCount = 4;
        private int _coeffDC = 2;
        private List<Statistic> _statisticTable;
        private CompressType _compressionType;
        private bool _compressTypeNothing;
        private bool _compressTypeRise;
        private bool _compressTypeRiseRle;
        private bool _compressTypeRiseRleAcDc;
        private bool _compressTypeRiseAcDc;
        private Visibility _pBar;

        public enum CompressType : int { Nothing = 0, Rise = 1, RiseRle = 2, RiseRleAcDc = 3, RiseAcDc = 4 }

        public DCTModel Model = new DCTModel();

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

        public int CoeffCount
        {
            get { return _coeffCount;}
            set
            {
                if (value <= 8 && value >= 2)
                _coeffCount = value;
                OnPropertyChanged("CoeffCount");
            }
        }

        public int CoeffDC
        {
            get { return _coeffDC; }
            set
            {
                if (value >= 1 && CoeffCount-value >=1)
                _coeffDC = value;
                OnPropertyChanged("CoeffDC");
            }
        }

        public ObservableCollection<MyInt> CoeffCorrection { get; set; }

        public CompressType CompressionType { get; set; }

        public bool CompressTypeNothing
        {
            get { return _compressTypeNothing; }
            set
            {
                _compressTypeNothing = value;
                if (value) CompressionType = CompressType.Nothing;
                OnPropertyChanged("CompressTypeNothing");
            }
        }

        public bool CompressTypeRise
        {
            get { return _compressTypeRise; }
            set
            {
                _compressTypeRise = value;
                if (value) CompressionType = CompressType.Rise;
                OnPropertyChanged("CompressTypeRise");
            }
        }

        public bool CompressTypeRiseRle
        {
            get { return _compressTypeRiseRle; }
            set
            {
                _compressTypeRiseRle = value;
                if (value) CompressionType = CompressType.RiseRle;
                OnPropertyChanged("CompressTypeRiseRle");
            }
        }

        public bool CompressTypeRiseRleAcDc
        {
            get { return _compressTypeRiseRleAcDc; }
            set
            {
                _compressTypeRiseRleAcDc = value;
                if (value) CompressionType = CompressType.RiseRleAcDc;
                OnPropertyChanged("CompressTypeRiseRleAcDc");
            }
        }

        public bool CompressTypeRiseAcDc
        {
            get { return _compressTypeRiseAcDc;  }
            set
            {
                _compressTypeRiseAcDc = value;
                if (value) CompressionType = CompressType.RiseAcDc;
                OnPropertyChanged("CompressTypeRiseAcDc");
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

        public DCTViewModel()
        {
            OpenFileCommand = new RelayCommand(arg => OpenFile());
            ConvertCommand = new RelayCommand(arg => Convert());
            CompressTypeNothing = true;
            CoeffCorrection = new ObservableCollection<MyInt>
            {
                new MyInt(128),
                new MyInt(64),
                new MyInt(64),
                new MyInt(64),
                new MyInt(32),
                new MyInt(32),
                new MyInt(16),
                new MyInt(16)
            };
            PBar = Visibility.Hidden;
        }

        #region Commands

        public ICommand OpenFileCommand { get; set; }

        private AsyncDelegateCommand _сonvertCommand;
        //public ICommand ConvertCommand => _сonvertCommand ?? (_сonvertCommand = new AsyncDelegateCommand(LongConvert));

        public ICommand ConvertCommand { get; set; }

        private async Task LongConvert(object o)
        {
            PBar = Visibility.Visible;
            await Task.Delay(10000);

            Model.Read(FileName);

            List<short[]> converted;
            var stat = Model.ConvertDct(BlockSize.ToString(), CoeffCount.ToString(), MyIntsToDoubles(CoeffCorrection),
                out converted, CoeffDC.ToString());

            switch (CompressionType)
            {
                case CompressType.Nothing:
                {
                    StatisticTable = stat;
                    break;
                }
                case CompressType.Rise:
                {
                    break;
                }
                case CompressType.RiseAcDc:
                {
                    break;
                }
                case CompressType.RiseRle:
                {
                    break;
                }
                case CompressType.RiseRleAcDc:
                {
                    break;
                }
            }

            //if (DcpNothing.IsChecked == true)
            //{
            //    StatisticTable.ItemsSource = stat;
            //}
            //else if (DcpRiseAcDc.IsChecked == true)
            //{
            //    List<byte[]> encoded;
            //    stat = GeneralCalculations.EncodeRise(converted, out encoded, DcpDcCoef.Text, stat, 0);

            //    StatisticTable.ItemsSource = stat;

            //    GeneralCalculations.DecodeRise(encoded, out converted, 0);
            //}
            //else if (DcpRiseRleAcDc.IsChecked == true)
            //{
            //    List<byte[]> encoded;
            //    stat = GeneralCalculations.EncodeRise(converted, out encoded, DcpDcCoef.Text, stat, 1);

            //    StatisticTable.ItemsSource = stat;

            //    GeneralCalculations.DecodeRise(encoded, out converted, 1);
            //}

            //var simpleencRle = AlgorithmRle.EncodeSimple(converted);
            //var encodedRle = AlgorithmRle.Encode(converted);

            Model.DeconvertDct(converted, BlockSize.ToString(), CoeffDC.ToString());


            ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(DCTModel.SequenceSourse);
            ZedGraphView.CurveNew = ZedGraphView.ListToPointList(DCTModel.SequenceSmoothed);






            PBar = Visibility.Hidden;
        }

        #endregion

        #region Methods

        private void Convert()
        {
            Model.Read(FileName);

            List<short[]> converted;
            var stat = Model.ConvertDct(BlockSize.ToString(), CoeffCount.ToString(), MyIntsToDoubles(CoeffCorrection),
                out converted, CoeffDC.ToString());

            switch (CompressionType)
            {
                case CompressType.Nothing:
                    {
                        StatisticTable = stat;
                        break;
                    }
                case CompressType.Rise:
                    {
                        break;
                    }
                case CompressType.RiseAcDc:
                    {
                        Model.DivideOnDcAc(converted, BlockSize, CoeffCount, CoeffDC);
                        List<byte[]> encodedDc;
                        var statDc = Model.EncodeRiseNew(DCTModel.DcBlocks, out encodedDc);
                        List<byte[]> encodedAc;
                        var statAc = Model.EncodeRiseNew(DCTModel.AcBlocks, out encodedAc);

                        var statAll = new List<Statistic>();
                        for (int i = 0; i < statAc.Count; i++)
                        {
                            var all = statDc[i] + statAc[i];
                            all.Time += stat[i].Time;
                            all.BlockSourseSize = stat[i].BlockSourseSize;
                            statAll.Add(all);
                        }
                        StatisticTable = statAll;

                        DCTModel.DcBlocksDecoded = Model.DecodeRiseNew(encodedDc);
                        DCTModel.AcBlocksDecoded = Model.DecodeRiseNew(encodedAc);
                        converted = Model.ConcatDcAc(DCTModel.DcBlocksDecoded, DCTModel.AcBlocksDecoded, CoeffCount, CoeffDC);

                        //List<byte[]> encoded;
                        //stat = Model.EncodeRise(converted, out encoded, CoeffDC.ToString(), stat, 0);
                        //StatisticTable = stat;
                        //Model.DecodeRise(encoded, out converted, 0);
                        break;
                    }
                case CompressType.RiseRle:
                    {
                        break;
                    }
                case CompressType.RiseRleAcDc:
                    {
                        Model.DivideOnDcAc(converted, BlockSize, CoeffCount, CoeffDC);
                        List<byte[]> encodedDcRise;
                        var statDcRise = Model.EncodeRiseNew(DCTModel.DcBlocks, out encodedDcRise);


                        // еще rle можно применять побайтово или на short! (реализовано) сейчас по 2 байта - не надо наверное так/ 
                        //мда, только хуже.. нет, норм, но лучше short

                        //List<byte[]> encodedAcRle;
                        //var statAcRle = Model.EncodeRleNew(AccessoryFunc.ShortsToBytes(DCTModel.AcBlocks), out encodedAcRle);

                        //List<byte[]> encodedAcRiseRle;
                        //var statAcRleRise = Model.EncodeRiseNew(AccessoryFunc.BytesToShorts(encodedAcRle), out encodedAcRiseRle);
                        List<short[]> encodedAcRle;
                        var statAcRle = Model.EncodeRleNew(DCTModel.AcBlocks, out encodedAcRle);

                        var decoded = Model.DecodeRleNew(encodedAcRle);

                        List<byte[]> encodedAcRiseRle;
                        var statAcRleRise = Model.EncodeRiseNew(encodedAcRle, out encodedAcRiseRle);

                        var statAll = new List<Statistic>();
                        for (int i = 0; i < statDcRise.Count; i++)
                        {
                            var all = statDcRise[i] + statAcRle[i] + statAcRleRise[i] + stat[i];
                            all.BlockSourseSize = stat[i].BlockSourseSize;
                            all.BlockRezultSize = statDcRise[i].BlockRezultSize + statAcRleRise[i].BlockRezultSize;
                            statAll.Add(all);
                        }
                        StatisticTable = statAll;


                        DCTModel.DcBlocksDecoded = Model.DecodeRiseNew(encodedDcRise);

                       // DCTModel.AcBlocksDecoded = Model.DecodeRleNew(Model.DecodeRiseNew(encodedAcRiseRle));
                        //converted = Model.ConcatDcAc(DCTModel.DcBlocksDecoded, DCTModel.AcBlocksDecoded, CoeffCount, CoeffDC);


                        //List<byte[]> encoded;
                        //stat = Model.EncodeRise(converted, out encoded, CoeffDC.ToString(), stat, 0);
                        //StatisticTable = stat;
                        //Model.DecodeRise(encoded, out converted, 1);

                        break;
                    }
            }

            Model.DeconvertDct(converted, BlockSize.ToString(), CoeffDC.ToString());

            ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(DCTModel.SequenceSourse);
            ZedGraphView.CurveNew = ZedGraphView.ListToPointList(DCTModel.SequenceSmoothed);

            ZedGraphSpectrumView.SpectrumSourse =
                ZedGraphSpectrumView.ArrayToPointList(ZedGraphSpectrumView.CalculateSpectrum(DCTModel.SequenceSourse));
            ZedGraphSpectrumView.SpectrumNew =
                ZedGraphSpectrumView.ArrayToPointList(ZedGraphSpectrumView.CalculateSpectrum(DCTModel.SequenceSmoothed));
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

        private double[] MyIntsToDoubles(IEnumerable<MyInt> myInts)
        {
            return myInts.Select(t => (double)t.Value).ToArray();
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


    public class MyInt : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _value;
        public int Value
        {
            get { return _value; }
            set { _value = value; OnPropertyChanged("Value"); }
        }

        public MyInt(int n)
        {
            Value = n;
        }

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
