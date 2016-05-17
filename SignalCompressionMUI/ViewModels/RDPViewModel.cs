using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SignalCompressionMUI.Models;
using SignalCompressionMUI.Views;

namespace SignalCompressionMUI.ViewModels
{
    class RDPViewModel : INotifyPropertyChanged
    {
        
        #region Fields and properties

        public RDPModel Model = new RDPModel();

        private string _fileName;
        private int _epsilon = 1;
        private int _blockSize = 1000;
        private List<Statistic> _statistic;

        private Visibility _pBar;

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
            PBar = Visibility.Hidden;
            OpenFileCommand = new RelayCommand(arg => OpenFile());
        }


        #region Commands

        /// <summary>
        /// Get or set ClickCommand.
        /// </summary>
        public ICommand OpenFileCommand { get; set; }


        private AsyncDelegateCommand _сonvertCommand;
        public ICommand ConvertCommand => _сonvertCommand ?? (_сonvertCommand = new AsyncDelegateCommand(LongConvert));

        private async Task LongConvert(object o)
        {
            PBar = Visibility.Visible;
            await Task.Delay(10000);
            Model.Read(FileName);
            Model.ConvertRdp(BlockSize.ToString(), Epsilon.ToString());
            Statistic = Model.Stat;
            ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(RDPModel.SequenceSourse);
            ZedGraphView.CurveNew = ZedGraphView.ListToPointList(RDPModel.PRez);

            ZedGraphSpectrumView.SpectrumSourse =
                ZedGraphSpectrumView.ArrayToPointList(ZedGraphSpectrumView.CalculateSpectrum(RDPModel.SequenceSourse));
            Model.DeconvertRdp();
            ZedGraphSpectrumView.SpectrumNew =
                ZedGraphSpectrumView.ArrayToPointList(ZedGraphSpectrumView.CalculateSpectrum(RDPModel.SequenceSmoothed));

            PBar = Visibility.Hidden;
        }

        #endregion

        #region Methods

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
