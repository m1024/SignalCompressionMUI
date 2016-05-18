using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows.Controls;
using SignalCompressionMUI.Models;
using SignalCompressionMUI.Models.Algorithms;
using SignalCompressionMUI.Models.Algorithms.Huffman;
using SignalCompressionMUI.Views;

namespace SignalCompressionMUI.ViewModels
{
    public enum СoeffCount
    {
        [Description("Все")]
        All,
        [Description("Три четверти")]
        ThreeQuarter,
        [Description("Половина")]
        Half,
        [Description("Четверть")]
        OneQuarter
    }
    public enum CompressType : int { Nothing = 0, Rise = 1, Rle = 2, RiseRle = 3, Huffman = 4, RiseHuffman = 5}

    class WaveletViewModel : INotifyPropertyChanged
    {
        #region Fields and properties

        private string _fileName;
        private int _blockSize = 1024;
        private СoeffCount _coeffCount;
        private int _rounding = 10;
        private int _depth = 2;
        private List<Statistic> _statisticTable;
        private Visibility _pBar;
        private WaveletType _wvType;
        private int _rleCount;
        private bool _compressTypeNothing;
        private bool _compressTypeRise;
        private bool _compressTypeRle;
        private bool _compressTypeRiseRle;
        private bool _compressTypeHuffman;
        private bool _compressTypeRiseHuffman;
        private bool _saveIsEnabled;
        private bool _convertIsEnabled;

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
        public bool SaveIsEnabled
        {
            get { return _saveIsEnabled; }
            set
            {
                _saveIsEnabled = value;
                OnPropertyChanged("SaveIsEnabled");
            }
        }

        public bool ConvertIsEnabled
        {
            get { return _convertIsEnabled; }
            set
            {
                _convertIsEnabled = value;
                OnPropertyChanged("ConvertIsEnabled");
            }
        }

        public int RleCount
        {
            get { return _rleCount; }
            set
            {
                _rleCount = value;
                OnPropertyChanged("RleCount");
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
                if ((CoeffCount == СoeffCount.OneQuarter || CoeffCount == СoeffCount.ThreeQuarter) && value < 1) return;
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
        public bool CompressTypeRle
        {
            get { return _compressTypeRle; }
            set
            {
                _compressTypeRle = value;
                if (value) CompressionType = CompressType.Rle;
                OnPropertyChanged("CompressTypeRle");
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
        public bool CompressTypeRiseHuffman
        {
            get { return _compressTypeRiseHuffman; }
            set
            {
                _compressTypeRiseHuffman = value;
                if (value) CompressionType = CompressType.RiseHuffman;
                OnPropertyChanged("CompressTypeRiseHuffman");
            }
        }

        #endregion

        public WaveletViewModel()
        {
            WaveletModel.OnCompressComplete += CompressedComplete;
            WaveletModel.OnSourseParsingComplete += ParsingSourseComplete;
            OpenFileCommand = new RelayCommand(arg => OpenFile());
            ConvertCommand = new RelayCommand(arg => Convert());
            SaveCommand = new RelayCommand(arg => SaveFile());
            CompressTypeNothing = true;
            PBar = Visibility.Hidden;
            CoeffCount = СoeffCount.All;
        }

        #region Commands

        public ICommand OpenFileCommand { get; set; }

        public ICommand ConvertCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        #endregion

        #region Methods

        private void ParsingSourseComplete()
        {
            ConvertIsEnabled = WaveletModel.SequenceSourse != null;
        }

        private void CompressedComplete()
        {
            SaveIsEnabled = WaveletModel.Compressed != null;
        }

        private void Convert()
        {
            var stat = WaveletModel.Convert(WvType, CoeffCount, Rounding, BlockSize, Depth);

            switch (CompressionType)
            {
                case CompressType.Nothing:
                {
                    StatisticTable = stat;
                    WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);
                    break;
                }
                case CompressType.Rise:  
                {
                    List<Statistic> statRise;
                    var encoded = WaveletModel.EncodeRise(WaveletModel.ConvertedBlocks, out statRise);

                    //сохранить
                    WaveletModel.Compressed = AccessoryFunc.CreateForSaving(encoded, CompressionType);

                    var statAll = new List<Statistic>();
                    for (int i = 0; i < statRise.Count; i++)
                    {
                        var all = stat[i] + statRise[i];
                        all.BlockRezultSize = statRise[i].BlockRezultSize;
                        statAll.Add(all);
                    }
                    var decoded = WaveletModel.DecodeRise(encoded);
                    WaveletModel.ConvertedBlocks = decoded;
                    WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);

                    Statistic.CalculateError(WaveletModel.SequenceSourse, WaveletModel.SequenceSmoothed, ref statAll, BlockSize);
                    statAll.Insert(0, Statistic.CalculateTotal(statAll));
                    StatisticTable = statAll;

                    break;
                }
                case CompressType.Rle:
                {
                    List<Statistic> statRle;
                    var encoded = WaveletModel.EncodeRle(WaveletModel.ConvertedBlocks, out statRle);

                    //сохранить
                    WaveletModel.Compressed = AccessoryFunc.CreateForSaving(encoded, CompressionType);

                    var statAll = new List<Statistic>();
                    for (int i = 0; i < statRle.Count; i++)
                    {
                        var all = stat[i] + statRle[i];
                        all.BlockRezultSize = statRle[i].BlockRezultSize;
                        statAll.Add(all);
                    }
                    var decoded = WaveletModel.DecodeRle(encoded);

                    WaveletModel.ConvertedBlocks = decoded;
                    WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);

                    Statistic.CalculateError(WaveletModel.SequenceSourse, WaveletModel.SequenceSmoothed, ref statAll,
                        BlockSize);
                    statAll.Insert(0, Statistic.CalculateTotal(statAll));
                    StatisticTable = statAll;

                    break;
                }
                case CompressType.RiseRle:
                {
                    List<Statistic> statRise;
                    List<Statistic> statRle;
                    var encRle = WaveletModel.EncodeRleShort(WaveletModel.ConvertedBlocks, out statRle, RleCount);
                    var encoded = WaveletModel.EncodeRise(encRle, out statRise);

                    //сохранить
                    WaveletModel.Compressed = AccessoryFunc.CreateForSaving(encoded, CompressionType);

                    var statAll = new List<Statistic>();
                    for (int i = 0; i < statRise.Count; i++)
                    {
                        var all = stat[i] + statRise[i] + statRle[i];
                        all.BlockRezultSize = statRise[i].BlockRezultSize;
                        statAll.Add(all);
                    }
                    var decoded = WaveletModel.DecodeRise(encoded);
                    var decRle = WaveletModel.DecodeRleShort(decoded, RleCount);

                    WaveletModel.ConvertedBlocks = decRle;
                    WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);

                    Statistic.CalculateError(WaveletModel.SequenceSourse, WaveletModel.SequenceSmoothed, ref statAll,
                        BlockSize);
                    statAll.Insert(0, Statistic.CalculateTotal(statAll));
                    StatisticTable = statAll;

                    break;
                }
                case CompressType.Huffman:
                {
                    List<Statistic> statHuff;
                    var encoded = WaveletModel.EncodeHuffman(WaveletModel.ConvertedBlocks, out statHuff);
                    
                    //сохранить
                    WaveletModel.Compressed = AccessoryFunc.CreateForSaving(encoded, CompressionType);

                    var statAll = new List<Statistic>();
                    for (int i = 0; i < statHuff.Count; i++)
                    {
                        var all = stat[i] + statHuff[i];
                        all.BlockRezultSize = statHuff[i].BlockRezultSize;
                        statAll.Add(all);
                    }
                    var decoded = WaveletModel.DecodeHuffman(encoded);
                    WaveletModel.ConvertedBlocks = decoded;
                    WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);

                    Statistic.CalculateError(WaveletModel.SequenceSourse, WaveletModel.SequenceSmoothed, ref statAll,
                        BlockSize);
                    statAll.Insert(0, Statistic.CalculateTotal(statAll));
                    StatisticTable = statAll;

                    break;
                }
                case CompressType.RiseHuffman:
                {
                    List<Statistic> statRle;
                    List<Statistic> statHuff;
                    var encRise = WaveletModel.EncodeRleShort(WaveletModel.ConvertedBlocks, out statRle, RleCount);
                    var encHuff = WaveletModel.EncodeHuffman(encRise, out statHuff);
                    
                    //сохранить
                    WaveletModel.Compressed = AccessoryFunc.CreateForSaving(encHuff, CompressionType);

                    var statAll = new List<Statistic>();
                    for (int i = 0; i < statRle.Count; i++)
                    {
                        var all = stat[i] + statRle[i] + statHuff[i];
                        all.BlockRezultSize = statHuff[i].BlockRezultSize;
                        statAll.Add(all);
                    }

                    var decHuff = WaveletModel.DecodeHuffman(encHuff);
                    var decRise = WaveletModel.DecodeRleShort(decHuff, RleCount);

                    WaveletModel.ConvertedBlocks = decRise;
                    WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);

                    Statistic.CalculateError(WaveletModel.SequenceSourse, WaveletModel.SequenceSmoothed, ref statAll,
                        BlockSize);
                    statAll.Insert(0, Statistic.CalculateTotal(statAll));
                    StatisticTable = statAll;

                    break;
                }
            }

            ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(WaveletModel.SequenceSourse);
            ZedGraphView.CurveNew = ZedGraphView.ListToPointList(WaveletModel.SequenceSmoothed);

            ZedGraphSpectrumView.SpectrumSourse =
                ZedGraphSpectrumView.ArrayToPointList(ZedGraphSpectrumView.CalculateSpectrum(WaveletModel.SequenceSourse));
            ZedGraphSpectrumView.SpectrumNew =
                ZedGraphSpectrumView.ArrayToPointList(ZedGraphSpectrumView.CalculateSpectrum(WaveletModel.SequenceSmoothed));
        }

        private void OpenFile()
        {
            //var dialog = new System.Windows.Forms.FolderBrowserDialog();
            //System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt|Emg compress files (.emgwv)|*.emgwv"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                // Open document
                var filename = dlg.FileName;
                FileName = filename;
            }

            //если открывают сжатый файл
            var ext = Path.GetExtension(FileName);
            WaveletModel.Compressed = null;
            if (ext == ".emgwv")
            {
                WaveletModel.CompressedFromFile = AccessoryFunc.ReadFile(FileName);
                DecompressFile();
            }
            else if (ext == ".txt")
            {
                try
                {
                    WaveletModel.Read(FileName);
                }
                catch (Exception ex)
                {
                    ModernDialog.ShowMessage(ex.Message, "Ошибка", MessageBoxButton.OK);
                }
            }
        }

        private void DecompressFile()
        {
            //надо все это в модель переместить
            CompressType type;
            var dec = AccessoryFunc.CreateFromSaving(WaveletModel.CompressedFromFile, out type);

            switch (type)
            {
                case  CompressType.Rise:
                {
                    var decoded = WaveletModel.DecodeRise(dec);
                    WaveletModel.ConvertedBlocks = decoded;
                    WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);
                    break;
                }
            }

            ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(WaveletModel.SequenceSourse);
            ZedGraphView.CurveNew = ZedGraphView.ListToPointList(WaveletModel.SequenceSmoothed);

            ZedGraphSpectrumView.SpectrumSourse =
                ZedGraphSpectrumView.ArrayToPointList(ZedGraphSpectrumView.CalculateSpectrum(WaveletModel.SequenceSourse));
            ZedGraphSpectrumView.SpectrumNew =
                ZedGraphSpectrumView.ArrayToPointList(ZedGraphSpectrumView.CalculateSpectrum(WaveletModel.SequenceSmoothed));
        }

        private void SaveFile()
        {
            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Compressed signal"; // Default file name
            dlg.DefaultExt = ".emgwv"; // Default file extension
            dlg.Filter = "Emg compress files (.emgwv)|*.emgwv"; // Filter files by extension

            // Show save file dialog box
            var result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                try
                {
                    AccessoryFunc.WriteFile(filename, WaveletModel.Compressed);
                    ModernDialog.ShowMessage("Файл успешно сохранен", "Результат операции", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    ModernDialog.ShowMessage(ex.Message, "Результат операции", MessageBoxButton.OK);
                }
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
