using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows.Controls;
using SignalCompressionMUI.Models;

namespace SignalCompressionMUI.ViewModels
{
    class RDPViewModel : INotifyPropertyChanged
    {
        
        #region Fields and properties

        public RDPModel Model = new RDPModel();

        private string _fileName;
        private int _epsilon = 1;
        private int _blockSize = 1000;
        private ObservableCollection<Statistic> _statistic;

        public ObservableCollection<Statistic> Statistic
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
            OpenFileCommand = new Command(arg => OpenFile());
            ConvertCommand = new Command(arg => Convert());
        }


        #region Commands

        /// <summary>
        /// Get or set ClickCommand.
        /// </summary>
        public ICommand OpenFileCommand { get; set; }

        public ICommand ConvertCommand { get; set; }

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

        private void Convert()
        {
            Model.Read(FileName);
            Model.ConvertRdp(BlockSize.ToString(), Epsilon.ToString());

            var list = Model.Stat;
            var oc = new ObservableCollection<Statistic>();
            foreach (var item in list)
                oc.Add(item);

            Statistic = oc;

            ModernDialog.ShowMessage("Преобразование успешно завершено", "", MessageBoxButton.OK);
            
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
