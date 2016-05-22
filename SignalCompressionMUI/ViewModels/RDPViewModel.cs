using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows.Controls;
using SignalCompressionMUI.Models;
using SignalCompressionMUI.Models.Algorithms.Spectrum;
using SignalCompressionMUI.Views;

namespace SignalCompressionMUI.ViewModels
{
    class RDPViewModel : INotifyPropertyChanged
    {
        
        #region Fields and properties

        private string _fileName;
        private int _epsilon = 1;
        private int _blockSize = 1000;
        private List<Statistic> _statistic;
        private readonly BackgroundWorker _bwConvert;
        private Visibility _pBar;
        private bool _compressTypeNothing;
        private bool _compressTypeRise;
        private bool _compressTypeHuffman;


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
        public bool CompressTypeHuffman
        {
            get { return _compressTypeHuffman; }
            set
            {
                _compressTypeHuffman = value;
                if (value) CompressionType = CompressType.Huffman;
                OnPropertyChanged("CompressTypeHuffman");
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

        public List<Statistic> Statistic
        {
            get { return _statistic; }
            set
            {
                _statistic = value;
                OnPropertyChanged("Statistic");
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

        public int Epsilon
        {
            get { return _epsilon; }
            set
            {
                _epsilon = value;
                OnPropertyChanged("Epsilon");
            }
        }

        public int BlockSize
        {
            get { return _blockSize;}
            set
            {
                _blockSize = value;
                OnPropertyChanged("BlockSize");
            }
        }

        #endregion

        public RDPViewModel()
        {
            _bwConvert = new BackgroundWorker();
            _bwConvert.DoWork += bwConvert_DoWork;
            _bwConvert.RunWorkerCompleted += bwConvert_RunWorkerCompleted;

            PBar = Visibility.Hidden;
            OpenFileCommand = new RelayCommand(arg => OpenFile());
            //ConvertCommand = new RelayCommand(arg => ConvertAssync());
            ConvertCommand = new RelayCommand(arg => Convert());
        }

        #region Commands

        public ICommand OpenFileCommand { get; set; }

        public ICommand ConvertCommand { get; set; }

        #endregion

        #region Methods


        private void bwConvert_DoWork(object sender, DoWorkEventArgs e)
        {
            var input = (RDPViewModel)e.Argument;
            input.Convert();
            e.Result = input;
        }

        private void bwConvert_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                ModernDialog.ShowMessage(e.Error.Message, "Ошибка", MessageBoxButton.OK);
            else
                PBar = Visibility.Hidden;
        }

        private void ConvertAssync()
        {
            PBar = Visibility.Visible;
            if (!_bwConvert.IsBusy)
                _bwConvert.RunWorkerAsync(this);
        }

        private void Convert()
        {
            RDPModel.Read(FileName);
            //RDPModel.ConvertRdp(BlockSize.ToString(), Epsilon.ToString());
            //Statistic = RDPModel.Stat;
            
            var stat = new List<Statistic>();
            var encRdp = RDPModel.ConvertRdp(out stat, RDPModel.SequenceSourse, BlockSize, Epsilon);
            var encDeltaCut = RDPModel.DeltaEncodeCut(encRdp);

            switch (CompressionType)
            {
                case CompressType.Nothing:
                {
                    var decDeltaCut = RDPModel.DeltaDecodedCut(encDeltaCut);
                    var decRdp = RDPModel.DeconvertRdp(decDeltaCut);

                    RDPModel.PRez = RDPModel.ConcatMyPoints(decDeltaCut);
                    RDPModel.SequenceSmoothed = decRdp;

                    Models.Statistic.CalculateError(RDPModel.SequenceSourse, RDPModel.SequenceSmoothed, ref stat,
                        BlockSize);
                    stat.Insert(0, Models.Statistic.CalculateTotal(stat));
                    Statistic = stat;
                    break;
                }
                case CompressType.Rise:
                {
                    List<Statistic> statRise;
                    var encRise = RDPModel.EncodeRise(encDeltaCut, out statRise);

                    var statAll = new List<Statistic>();
                    for (int i = 0; i < statRise.Count; i++)
                    {
                        var all = stat[i] + statRise[i];
                        all.BlockRezultSize = statRise[i].BlockRezultSize;
                        statAll.Add(all);
                    }


                    var decRise = RDPModel.DecodeRise(encRise);
                    var decDeltaCut = RDPModel.DeltaDecodedCut(decRise);
                    var decRdp = RDPModel.DeconvertRdp(decDeltaCut);

                    RDPModel.PRez = RDPModel.ConcatMyPoints(decDeltaCut);
                    RDPModel.SequenceSmoothed = decRdp;


                    Models.Statistic.CalculateError(RDPModel.SequenceSourse, RDPModel.SequenceSmoothed, ref statAll, BlockSize);
                    statAll.Insert(0, Models.Statistic.CalculateTotal(statAll));
                    Statistic = statAll;

                    break;
                }
            }

            var sourseSpectrum = Spectrum.CalculateSpectrum(RDPModel.SequenceSourse);
            var newSpectrum = Spectrum.CalculateSpectrum(RDPModel.SequenceSmoothed);

            OxyPlotModel.SequenceSourse = RDPModel.SequenceSourse;
            OxyPlotModel.SequenceNew = RDPModel.SequenceSmoothed;
            OxyPlotSpectrumModel.SpectrumSourse = sourseSpectrum;
            OxyPlotSpectrumModel.SpectrumNew = newSpectrum;

            ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(RDPModel.SequenceSourse);
            ZedGraphView.CurveNew = ZedGraphView.ListToPointList(RDPModel.PRez);
            ZedGraphSpectrumView.SpectrumSourse = ZedGraphSpectrumView.ArrayToPointList(sourseSpectrum);
            ZedGraphSpectrumView.SpectrumNew = ZedGraphSpectrumView.ArrayToPointList(newSpectrum);
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
