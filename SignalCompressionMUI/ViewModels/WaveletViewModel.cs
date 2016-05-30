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
using SignalCompressionMUI.Models.Algorithms.Spectrum;
using SignalCompressionMUI.Views;
using Excel = Microsoft.Office.Interop.Excel;

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
    public enum CompressType : int { Nothing = 0, Rise = 1, Rle = 2, RiseRle = 3, Huffman = 4, RleHuffman = 5, RiseRleAcDc = 6, RiseAcDc = 7, HuffmanRleAcDc = 8, RiseHuffman }

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
        private bool _compressTypeRleHuffman;
        private bool _saveIsEnabled;
        private bool _convertIsEnabled;
        private Visibility _statVisiblity;
        private bool _isStatExist;
        private readonly BackgroundWorker _bwConvert;
        private readonly BackgroundWorker _bwOpenEnc;
        private readonly BackgroundWorker _bwExcel;

        public bool IsStatExist
        {
            get { return _isStatExist; }
            set
            {
                _isStatExist = value;
                OnPropertyChanged("IsStatExist");
            }
        }

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

        public Visibility StatVisibility
        {
            get { return _statVisiblity;}
            set
            {
                _statVisiblity = value;
                OnPropertyChanged("StatVisibility");
            }
        }

        public List<Statistic> StatisticTable
        {
            get { return _statisticTable; }
            set
            {
                _statisticTable = value;
                if (_statisticTable == null)
                {
                    StatVisibility = Visibility.Hidden;
                    IsStatExist = false;
                }
                else
                {
                    StatVisibility = Visibility.Visible;
                    IsStatExist = true;
                }
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
        public bool CompressTypeRleHuffman
        {
            get { return _compressTypeRleHuffman; }
            set
            {
                _compressTypeRleHuffman = value;
                if (value) CompressionType = CompressType.RleHuffman;
                OnPropertyChanged("CompressTypeRleHuffman");
            }
        }

        #endregion

        public WaveletViewModel()
        {
            StatisticTable = null;
            _bwConvert = new BackgroundWorker();
            _bwConvert.DoWork += bwConvert_DoWork;
            _bwConvert.RunWorkerCompleted += bwConvert_RunWorkerCompleted;

            _bwOpenEnc = new BackgroundWorker();
            _bwOpenEnc.DoWork += bwOpenEnc_DoWork;
            _bwOpenEnc.RunWorkerCompleted += bwOpenEnc_RunWorkerCompleted;

            _bwExcel = new BackgroundWorker();
            _bwExcel.DoWork += bwExcel_DoWork;
            _bwExcel.RunWorkerCompleted += bwExcel_RunWorkerCompleted;

            WaveletModel.OnCompressComplete += CompressedComplete;
            WaveletModel.OnSourseParsingComplete += ParsingSourseComplete;

            OpenFileCommand = new RelayCommand(arg => OpenFile());
            ConvertCommand = new RelayCommand(arg => ConvertAssync());
            SaveCommand = new RelayCommand(arg => SaveFile());
            OpenInExcelCommand = new RelayCommand(arg => OpenInExcelAssync());

            CompressTypeNothing = true;
            PBar = Visibility.Hidden;
            CoeffCount = СoeffCount.All;
        }

        #region Commands

        public ICommand OpenFileCommand { get; set; }

        public ICommand ConvertCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand OpenInExcelCommand { get; set; }

        #endregion

        #region Methods

        #region Background workers

        private void ConvertAssync()
        {
            PBar = Visibility.Visible;
            if (!_bwConvert.IsBusy)
                _bwConvert.RunWorkerAsync(this);
        }

        private void OpenAssync()
        {
            PBar = Visibility.Visible;
            if (!_bwOpenEnc.IsBusy)
                _bwOpenEnc.RunWorkerAsync(this);
        }

        private void OpenInExcelAssync()
        {
            PBar = Visibility.Visible;
            if (!_bwExcel.IsBusy)
                _bwExcel.RunWorkerAsync(this);
        }

        private void bwOpenEnc_DoWork(object sender, DoWorkEventArgs e)
        {
            var input = (WaveletViewModel)e.Argument;
            WaveletModel.CompressedFromFile = AccessoryFunc.ReadFile(input.FileName);
            input.DecompressFile();
            e.Result = input;
        }

        private void bwOpenEnc_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                ModernDialog.ShowMessage(e.Error.Message, "Ошибка", MessageBoxButton.OK);
            PBar = Visibility.Hidden;
        }

        private void bwConvert_DoWork(object sender, DoWorkEventArgs e)
        {
            var input = (WaveletViewModel)e.Argument;
            input.Convert();
            e.Result = input;
        }

        private void bwConvert_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                ModernDialog.ShowMessage(e.Error.Message, "Ошибка", MessageBoxButton.OK);
            PBar = Visibility.Hidden;
            WaveletModel.GenStatChanged = true;
        }

        private void bwExcel_DoWork(object sender, DoWorkEventArgs e)
        {
            var input = (WaveletViewModel)e.Argument;
            input.OpenInExcel();
            e.Result = input;
        }

        private void bwExcel_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                ModernDialog.ShowMessage(e.Error.Message, "Ошибка", MessageBoxButton.OK);
            PBar = Visibility.Hidden;
        }

        #endregion

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

            if (CompressTypeNothing)
            {
                #region Nothing

                WaveletModel.Compressed = null;

                WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);

                Statistic.CalculateError(WaveletModel.SequenceSourse, WaveletModel.SequenceSmoothed, ref stat,
                    BlockSize);
                stat.Insert(0, Statistic.CalculateTotal(stat));
                WaveletModel.NothingStat = stat.First();
                StatisticTable = stat;

                #endregion
            }
            if (CompressTypeRise)
            {
                #region Rise

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

                Statistic.CalculateError(WaveletModel.SequenceSourse, WaveletModel.SequenceSmoothed, ref statAll,
                    BlockSize);
                statAll.Insert(0, Statistic.CalculateTotal(statAll));
                WaveletModel.RiseStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }
            if (CompressTypeRle)
            {
                #region Rle

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
                WaveletModel.RleStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }
            if (CompressTypeRiseRle)
            {
                #region RiseRle

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
                WaveletModel.RleRiseStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }
            if (CompressTypeHuffman)
            {
                #region Huffman

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
                WaveletModel.HuffStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }
            if (CompressTypeRleHuffman)
            {
                #region RleHuff

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
                WaveletModel.RleHuffStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }

            #region OldSwitch
            //switch (CompressionType)
            //{
            //    case CompressType.Nothing:
            //    {
            //        #region Nothing
            //        WaveletModel.Compressed = null;

            //        WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);

            //        Statistic.CalculateError(WaveletModel.SequenceSourse, WaveletModel.SequenceSmoothed, ref stat,
            //            BlockSize);
            //        stat.Insert(0, Statistic.CalculateTotal(stat));
            //        WaveletModel.NothingStat = stat.First();
            //        StatisticTable = stat;
            //        break;

            //        #endregion
            //    }
            //    case CompressType.Rise:
            //    {
            //        #region Rise

            //        List<Statistic> statRise;
            //        var encoded = WaveletModel.EncodeRise(WaveletModel.ConvertedBlocks, out statRise);

            //        //сохранить
            //        WaveletModel.Compressed = AccessoryFunc.CreateForSaving(encoded, CompressionType);

            //        var statAll = new List<Statistic>();
            //        for (int i = 0; i < statRise.Count; i++)
            //        {
            //            var all = stat[i] + statRise[i];
            //            all.BlockRezultSize = statRise[i].BlockRezultSize;
            //            statAll.Add(all);
            //        }
            //        var decoded = WaveletModel.DecodeRise(encoded);
            //        WaveletModel.ConvertedBlocks = decoded;
            //        WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);

            //        Statistic.CalculateError(WaveletModel.SequenceSourse, WaveletModel.SequenceSmoothed, ref statAll,
            //            BlockSize);
            //        statAll.Insert(0, Statistic.CalculateTotal(statAll));
            //        WaveletModel.RiseStat = statAll.First();
            //        StatisticTable = statAll;

            //        break;

            //        #endregion
            //    }
            //    case CompressType.Rle:
            //    {
            //        #region Rle

            //        List<Statistic> statRle;
            //        var encoded = WaveletModel.EncodeRle(WaveletModel.ConvertedBlocks, out statRle);

            //        //сохранить
            //        WaveletModel.Compressed = AccessoryFunc.CreateForSaving(encoded, CompressionType);

            //        var statAll = new List<Statistic>();
            //        for (int i = 0; i < statRle.Count; i++)
            //        {
            //            var all = stat[i] + statRle[i];
            //            all.BlockRezultSize = statRle[i].BlockRezultSize;
            //            statAll.Add(all);
            //        }
            //        var decoded = WaveletModel.DecodeRle(encoded);

            //        WaveletModel.ConvertedBlocks = decoded;
            //        WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);

            //        Statistic.CalculateError(WaveletModel.SequenceSourse, WaveletModel.SequenceSmoothed, ref statAll,
            //            BlockSize);
            //        statAll.Insert(0, Statistic.CalculateTotal(statAll));
            //        WaveletModel.RleStat = statAll.First();
            //        StatisticTable = statAll;

            //        break;

            //        #endregion
            //    }
            //    case CompressType.RiseRle:
            //    {
            //        #region RiseRle

            //        List<Statistic> statRise;
            //        List<Statistic> statRle;
            //        var encRle = WaveletModel.EncodeRleShort(WaveletModel.ConvertedBlocks, out statRle, RleCount);
            //        var encoded = WaveletModel.EncodeRise(encRle, out statRise);

            //        //сохранить
            //        WaveletModel.Compressed = AccessoryFunc.CreateForSaving(encoded, CompressionType);

            //        var statAll = new List<Statistic>();
            //        for (int i = 0; i < statRise.Count; i++)
            //        {
            //            var all = stat[i] + statRise[i] + statRle[i];
            //            all.BlockRezultSize = statRise[i].BlockRezultSize;
            //            statAll.Add(all);
            //        }
            //        var decoded = WaveletModel.DecodeRise(encoded);
            //        var decRle = WaveletModel.DecodeRleShort(decoded, RleCount);

            //        WaveletModel.ConvertedBlocks = decRle;
            //        WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);

            //        Statistic.CalculateError(WaveletModel.SequenceSourse, WaveletModel.SequenceSmoothed, ref statAll,
            //            BlockSize);
            //        statAll.Insert(0, Statistic.CalculateTotal(statAll));
            //        WaveletModel.RleRiseStat = statAll.First();
            //        StatisticTable = statAll;

            //        break;

            //        #endregion
            //    }
            //    case CompressType.Huffman:
            //    {
            //        #region Huffman

            //        List<Statistic> statHuff;
            //        var encoded = WaveletModel.EncodeHuffman(WaveletModel.ConvertedBlocks, out statHuff);

            //        //сохранить
            //        WaveletModel.Compressed = AccessoryFunc.CreateForSaving(encoded, CompressionType);

            //        var statAll = new List<Statistic>();
            //        for (int i = 0; i < statHuff.Count; i++)
            //        {
            //            var all = stat[i] + statHuff[i];
            //            all.BlockRezultSize = statHuff[i].BlockRezultSize;
            //            statAll.Add(all);
            //        }
            //        var decoded = WaveletModel.DecodeHuffman(encoded);
            //        WaveletModel.ConvertedBlocks = decoded;
            //        WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);

            //        Statistic.CalculateError(WaveletModel.SequenceSourse, WaveletModel.SequenceSmoothed, ref statAll,
            //            BlockSize);
            //        statAll.Insert(0, Statistic.CalculateTotal(statAll));
            //        WaveletModel.HuffStat = statAll.First();
            //        StatisticTable = statAll;

            //        break;

            //        #endregion
            //    }
            //    case CompressType.RiseHuffman:
            //    {
            //        #region RiseHuff

            //        List<Statistic> statRle;
            //        List<Statistic> statHuff;
            //        var encRise = WaveletModel.EncodeRleShort(WaveletModel.ConvertedBlocks, out statRle, RleCount);
            //        var encHuff = WaveletModel.EncodeHuffman(encRise, out statHuff);

            //        //сохранить
            //        WaveletModel.Compressed = AccessoryFunc.CreateForSaving(encHuff, CompressionType);

            //        var statAll = new List<Statistic>();
            //        for (int i = 0; i < statRle.Count; i++)
            //        {
            //            var all = stat[i] + statRle[i] + statHuff[i];
            //            all.BlockRezultSize = statHuff[i].BlockRezultSize;
            //            statAll.Add(all);
            //        }

            //        var decHuff = WaveletModel.DecodeHuffman(encHuff);
            //        var decRise = WaveletModel.DecodeRleShort(decHuff, RleCount);

            //        WaveletModel.ConvertedBlocks = decRise;
            //        WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);

            //        Statistic.CalculateError(WaveletModel.SequenceSourse, WaveletModel.SequenceSmoothed, ref statAll,
            //            BlockSize);
            //        statAll.Insert(0, Statistic.CalculateTotal(statAll));
            //        WaveletModel.RleHuffStat = statAll.First();
            //        StatisticTable = statAll;

            //        break;

            //        #endregion
            //    }
            //}
            #endregion

            var sourseSpectrum = Spectrum.CalculateSpectrum(WaveletModel.SequenceSourse);
            var newSpectrum = Spectrum.CalculateSpectrum(WaveletModel.SequenceSmoothed);

            OxyPlotModel.SequenceSourse = WaveletModel.SequenceSourse;
            OxyPlotModel.SequenceNew = WaveletModel.SequenceSmoothed;
            OxyPlotSpectrumModel.SpectrumSourse = sourseSpectrum;
            OxyPlotSpectrumModel.SpectrumNew = newSpectrum;

            ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(WaveletModel.SequenceSourse);
            ZedGraphView.CurveNew = ZedGraphView.ListToPointList(WaveletModel.SequenceSmoothed);
            ZedGraphSpectrumView.SpectrumSourse = ZedGraphSpectrumView.ArrayToPointList(sourseSpectrum);
            ZedGraphSpectrumView.SpectrumNew = ZedGraphSpectrumView.ArrayToPointList(newSpectrum);
        }

        private void OpenFile()
        {
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
                OpenAssync();
                //WaveletModel.CompressedFromFile = AccessoryFunc.ReadFile(FileName);
                //DecompressFile();
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
                case CompressType.Rle:
                {
                    var decoded = WaveletModel.DecodeRle(dec);
                    WaveletModel.ConvertedBlocks = decoded;
                    WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);
                    break;
                }
                case CompressType.RiseRle:
                {
                    var decoded = WaveletModel.DecodeRise(dec);
                    var decRle = WaveletModel.DecodeRleShort(decoded, RleCount);

                    WaveletModel.ConvertedBlocks = decRle;
                    WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);
                    break;
                }
                case CompressType.Huffman:
                {
                    var decoded = WaveletModel.DecodeHuffman(dec);
                    WaveletModel.ConvertedBlocks = decoded;
                    WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);
                    break;
                }
                case CompressType.RleHuffman:
                {
                    var decHuff = WaveletModel.DecodeHuffman(dec);
                    var decRise = WaveletModel.DecodeRleShort(decHuff, RleCount);
                    WaveletModel.ConvertedBlocks = decRise;
                    WaveletModel.Deconvert(_wvType, CoeffCount, Rounding);
                    break;
                }
            }

            var sourseSpectrum = Spectrum.CalculateSpectrum(WaveletModel.SequenceSourse);
            var newSpectrum = Spectrum.CalculateSpectrum(WaveletModel.SequenceSmoothed);

            OxyPlotModel.SequenceSourse = WaveletModel.SequenceSourse;
            OxyPlotModel.SequenceNew = WaveletModel.SequenceSmoothed;
            OxyPlotSpectrumModel.SpectrumSourse = sourseSpectrum;
            OxyPlotSpectrumModel.SpectrumNew = newSpectrum;

            ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(WaveletModel.SequenceSourse);
            ZedGraphView.CurveNew = ZedGraphView.ListToPointList(WaveletModel.SequenceSmoothed);
            ZedGraphSpectrumView.SpectrumSourse = ZedGraphSpectrumView.ArrayToPointList(sourseSpectrum);
            ZedGraphSpectrumView.SpectrumNew = ZedGraphSpectrumView.ArrayToPointList(newSpectrum);
        }

        private static void SaveFile()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Compressed signal",
                DefaultExt = ".emgwv",
                Filter = "Emg compress files (.emgwv)|*.emgwv"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
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

        private void OpenInExcel()
        {
            var excelApp = new Excel.Application();
            excelApp.Application.Workbooks.Add(Type.Missing);
            excelApp.Columns.ColumnWidth = 15;

            var sheet = (Excel.Worksheet)excelApp.ActiveSheet;

            sheet.Range[sheet.Cells[2, 1], sheet.Cells[2, 6]].Merge(Type.Missing);
            sheet.Cells[2, 1] = "Статистика вейвлет преобразования";
            (sheet.Cells[2, 1] as Excel.Range).Font.Bold = true;
            (sheet.Cells[2, 1] as Excel.Range).Font.Size = 16;

            for (int i = 4; i < 11; i++)
            {
                sheet.Range[sheet.Cells[i, 1], sheet.Cells[i, 2]].Merge(Type.Missing);
                (sheet.Cells[i, 3] as Excel.Range).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            }

            sheet.Cells[4, 1] = "Длина блока";
            sheet.Cells[4, 3] = BlockSize;
            sheet.Cells[5, 1] = "Глубина рекурсии вейвлета";
            sheet.Cells[5, 3] = Depth;
            sheet.Cells[6, 1] = "Округление";
            sheet.Cells[6, 3] = Rounding;
            sheet.Cells[7, 1] = "Коэф. не кодированных rle";
            sheet.Cells[7, 3] = RleCount;
            sheet.Cells[8, 1] = "Оставить коэффициентов";
            sheet.Cells[8, 3] = CoeffCount.ToString();
            sheet.Cells[9, 1] = "Тип вейвлета";
            sheet.Cells[9, 3] = WvType.ToString();
            sheet.Cells[10, 1] = "Метод сжатия";
            sheet.Cells[10, 3] = CompressionType.ToString();


            sheet.Range[sheet.Cells[12, 1], sheet.Cells[12, 6]].WrapText = true;
            sheet.Range[sheet.Cells[12, 1], sheet.Cells[12, 6]].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

            sheet.Range[sheet.Cells[12, 1], sheet.Cells[12, 6]].Font.Bold = true;
            sheet.Cells[12, 1] = "Номер";
            sheet.Cells[12, 2] = "Время (Ticks)";
            sheet.Cells[12, 3] = "Размер исходного блока (bytes)";
            sheet.Cells[12, 4] = "Размер нового блока (bytes)";
            sheet.Cells[12, 5] = "Коэффициент сжатия";
            sheet.Cells[12, 6] = "Средняя погрешность, %";

            for (int i = 0; i < StatisticTable.Count; i++)
            {
                sheet.Range[sheet.Cells[i+13, 1], sheet.Cells[i+13, 6]].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.Cells[i + 13, 1] = StatisticTable[i].Title;
                sheet.Cells[i + 13, 2] = StatisticTable[i].Time.Ticks;
                sheet.Cells[i + 13, 3] = StatisticTable[i].BlockSourseSize;
                sheet.Cells[i + 13, 4] = StatisticTable[i].BlockRezultSize;
                sheet.Cells[i + 13, 5] = StatisticTable[i].CompressionRatio;
                sheet.Cells[i + 13, 6] = StatisticTable[i].Error;
            }

            excelApp.Visible = true;
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