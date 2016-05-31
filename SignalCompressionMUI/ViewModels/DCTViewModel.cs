using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows.Controls;
using SignalCompressionMUI.Models;
using SignalCompressionMUI.Models.Algorithms.Spectrum;
using SignalCompressionMUI.Views;
using Excel = Microsoft.Office.Interop.Excel;

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
        private bool _compressTypeNothing;
        private bool _compressTypeRise;
        private bool _compressTypeRiseRle;
        private bool _compressTypeRiseRleAcDc;
        private bool _compressTypeRiseAcDc;
        private bool _compressTypeHuffman;
        private bool _compressTypeHuffmanRleAcDc;
        private bool _saveIsEnabled;
        private bool _convertIsEnabled;
        private bool _isStatExist;
        private Visibility _pBar;
        private Visibility _statVisiblity;
        private readonly BackgroundWorker _bwConvert;
        private readonly BackgroundWorker _bwOpenEnc;
        private readonly BackgroundWorker _bwExcel;
        private ObservableCollection<MyInt> _coeffCorrection;

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

        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        public Visibility StatVisibility
        {
            get { return _statVisiblity; }
            set
            {
                _statVisiblity = value;
                OnPropertyChanged("StatVisibility");
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

        public bool IsStatExist
        {
            get { return _isStatExist; }
            set
            {
                _isStatExist = value;
                OnPropertyChanged("IsStatExist");
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

        public ObservableCollection<MyInt> CoeffCorrection
        {
            get { return _coeffCorrection; }
            set
            {
                _coeffCorrection = value;
                OnPropertyChanged("CoeffCorrection");
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
        public bool CompressTypeHuffmanRleAcDc
        {
            get { return _compressTypeHuffmanRleAcDc; }
            set
            {
                _compressTypeHuffmanRleAcDc = value;
                if (value) CompressionType = CompressType.HuffmanRleAcDc;
                OnPropertyChanged("CompressTypeHuffmanRleAcDc");
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
            StatisticTable = null;
            _bwConvert = new BackgroundWorker();
            _bwConvert.DoWork += bwConvert_DoWork;
            _bwConvert.RunWorkerCompleted += bwConvert_RunWorkerCompleted;

            _bwExcel = new BackgroundWorker();
            _bwExcel.DoWork += bwExcel_DoWork;
            _bwExcel.RunWorkerCompleted += bwExcel_RunWorkerCompleted;

            _bwOpenEnc = new BackgroundWorker();
            _bwOpenEnc.DoWork += bwOpenEnc_DoWork;
            _bwOpenEnc.RunWorkerCompleted += bwOpenEnc_RunWorkerCompleted;

            OpenFileCommand = new RelayCommand(arg => OpenFile());
            ConvertCommand = new RelayCommand(arg => ConvertAssync());
            OpenInExcelCommand = new RelayCommand(arg => OpenInExcelAssync());
            SaveCommand = new RelayCommand(arg => SaveFile());
            ClearAllCommand = new RelayCommand(arg => ClearAll());

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

            DCTModel.OnCompressComplete += CompressedComplete;
            DCTModel.OnSourseParsingComplete += ParsingSourseComplete;
        }

        #region Commands

        public ICommand OpenFileCommand { get; set; }

        public ICommand ConvertCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand OpenInExcelCommand { get; set; }

        public ICommand ClearAllCommand { get; set; }

        #endregion

        #region Methods

        #region Background workers

        private void ConvertAssync()
        {
            PBar = Visibility.Visible;
            if (!_bwConvert.IsBusy)
                _bwConvert.RunWorkerAsync(this);
        }

        private void OpenInExcelAssync()
        {
            PBar = Visibility.Visible;
            if (!_bwExcel.IsBusy)
                _bwExcel.RunWorkerAsync(this);
        }

        private void OpenAssync()
        {
            PBar = Visibility.Visible;
            if (!_bwOpenEnc.IsBusy)
                _bwOpenEnc.RunWorkerAsync(this);
        }

        private void bwConvert_DoWork(object sender, DoWorkEventArgs e)
        {
            var input = (DCTViewModel)e.Argument;
            input.Convert();
            e.Result = input;
        }

        private void bwConvert_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                ModernDialog.ShowMessage(e.Error.Message, "Ошибка", MessageBoxButton.OK);
            PBar = Visibility.Hidden;
            DCTModel.StatChanged();
        }

        private void bwExcel_DoWork(object sender, DoWorkEventArgs e)
        {
            var input = (DCTViewModel)e.Argument;
            input.OpenInExcel();
            e.Result = input;
        }

        private void bwExcel_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                ModernDialog.ShowMessage(e.Error.Message, "Ошибка", MessageBoxButton.OK);
            PBar = Visibility.Hidden;
        }

        private void bwOpenEnc_DoWork(object sender, DoWorkEventArgs e)
        {
            var input = (DCTViewModel)e.Argument;
            DCTModel.CompressedFromFile = AccessoryFunc.ReadFile(input.FileName);
            input.DecompressFile();
            e.Result = input;
        }

        private void bwOpenEnc_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                ModernDialog.ShowMessage(e.Error.Message, "Ошибка", MessageBoxButton.OK);
            PBar = Visibility.Hidden;
        }

        #endregion

        private void ClearAll()
        {
            DCTModel.SequenceSourse = null;
            DCTModel.SequenceSmoothed = null;
            DCTModel.AcBlocks = null;
            DCTModel.DcBlocks = null;
            DCTModel.AcBlocksDecoded = null;
            DCTModel.DcBlocksDecoded = null;
            DCTModel.Compressed = null;
            DCTModel.CompressedFromFile = null;
            DCTModel.NothingStat = new Statistic();
            DCTModel.RiseAcDcStat = new Statistic();
            DCTModel.HuffmanRleAcDcStat = new Statistic();
            DCTModel.HuffmanStat = new Statistic();
            DCTModel.RiseRleStat = new Statistic();;
            DCTModel.RiseRleAcDcStat = new Statistic();
            DCTModel.RiseStat = new Statistic();
            StatisticTable = null;
            FileName = "";
            DCTModel.StatChanged();

            var sourseSpectrum = Spectrum.CalculateSpectrum(DCTModel.SequenceSourse);
            var newSpectrum = Spectrum.CalculateSpectrum(DCTModel.SequenceSmoothed);

            OxyPlotModel.SequenceSourse = DCTModel.SequenceSourse ?? new short[1];
            OxyPlotModel.SequenceNew = DCTModel.SequenceSmoothed ?? new short[1];
            OxyPlotSpectrumModel.SpectrumSourse = sourseSpectrum;
            OxyPlotSpectrumModel.SpectrumNew = newSpectrum;

            ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(DCTModel.SequenceSourse);
            ZedGraphView.CurveNew = ZedGraphView.ListToPointList(DCTModel.SequenceSmoothed);
            ZedGraphSpectrumView.SpectrumSourse = ZedGraphSpectrumView.ArrayToPointList(sourseSpectrum);
            ZedGraphSpectrumView.SpectrumNew = ZedGraphSpectrumView.ArrayToPointList(newSpectrum);
        }

        private void Convert()
        {
            DCTModel.Read(FileName);

            List<Statistic> stat;
            var encDct = DCTModel.ConvertDct(BlockSize, CoeffCount, MyIntsToDoubles(CoeffCorrection), out stat, CoeffDC);

            if (CompressTypeNothing)
            {
                #region Nothing

                DCTModel.SequenceSmoothed = DCTModel.DeconvertDct(encDct, BlockSize, CoeffDC);

                var statAll = stat.Select(s => s.Clone()).ToList();

                Statistic.CalculateError(DCTModel.SequenceSourse, DCTModel.SequenceSmoothed, ref statAll, BlockSize);
                statAll.Insert(0, Statistic.CalculateTotal(statAll));
                DCTModel.NothingStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }
            if (CompressTypeRise)
            {
                #region Rise
                encDct = DCTModel.ConvertDct(BlockSize, CoeffCount, MyIntsToDoubles(CoeffCorrection), out stat, CoeffDC);

                List<byte[]> encRise;
                var statRise = DCTModel.EncodeRiseNew(encDct, out encRise);

                //сохранить в файл
                DCTModel.Compressed = AccessoryFunc.CreateForSavingDCT(new List<List<byte[]>>() { encRise }, CompressionType, CoeffCount, CoeffDC, MyIntsToDoubles(CoeffCorrection));
                
                var statAll = new List<Statistic>();
                for (int i = 0; i < statRise.Count; i++)
                {
                    var all = stat[i] + statRise[i];
                    all.BlockSourseSize = stat[i].BlockSourseSize;
                    statAll.Add(all);
                }

                encDct = DCTModel.DecodeRiseNew(encRise);
                DCTModel.SequenceSmoothed = DCTModel.DeconvertDct(encDct, BlockSize, CoeffDC);


                Statistic.CalculateError(DCTModel.SequenceSourse, DCTModel.SequenceSmoothed, ref statAll, BlockSize);
                statAll.Insert(0, Statistic.CalculateTotal(statAll));
                DCTModel.RiseStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }
            if (CompressTypeRiseAcDc)
            {
                #region RiseAcDc
                encDct = DCTModel.ConvertDct(BlockSize, CoeffCount, MyIntsToDoubles(CoeffCorrection), out stat, CoeffDC);

                DCTModel.DivideOnDcAc(encDct, BlockSize, CoeffCount, CoeffDC);
                List<byte[]> encodedDc;
                var statDc = DCTModel.EncodeRiseNew(DCTModel.DcBlocks, out encodedDc);
                List<byte[]> encodedAc;
                var statAc = DCTModel.EncodeRiseNew(DCTModel.AcBlocks, out encodedAc);

                //сохранить в файл
                DCTModel.Compressed = AccessoryFunc.CreateForSavingDCT(new List<List<byte[]>>() { encodedDc, encodedAc }, CompressionType, CoeffCount, CoeffDC, MyIntsToDoubles(CoeffCorrection));

                var statAll = new List<Statistic>();
                for (int i = 0; i < statAc.Count; i++)
                {
                    var all = stat[i] + statDc[i] + statAc[i];
                    all.BlockSourseSize = stat[i].BlockSourseSize;
                    statAll.Add(all);
                }

                DCTModel.DcBlocksDecoded = DCTModel.DecodeRiseNew(encodedDc);
                DCTModel.AcBlocksDecoded = DCTModel.DecodeRiseNew(encodedAc);
                encDct = DCTModel.ConcatDcAc(DCTModel.DcBlocksDecoded, DCTModel.AcBlocksDecoded, CoeffCount, CoeffDC);
                DCTModel.SequenceSmoothed = DCTModel.DeconvertDct(encDct, BlockSize, CoeffDC);


                Statistic.CalculateError(DCTModel.SequenceSourse, DCTModel.SequenceSmoothed, ref statAll, BlockSize);
                statAll.Insert(0, Statistic.CalculateTotal(statAll));
                DCTModel.RiseAcDcStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }
            if (CompressTypeRiseRle)
            {
                #region RiseRle
                encDct = DCTModel.ConvertDct(BlockSize, CoeffCount, MyIntsToDoubles(CoeffCorrection), out stat, CoeffDC);

                List<short[]> encRle;
                var statRle = DCTModel.EncodeRleNew(encDct, out encRle);

                List<byte[]> encRise;
                var statRise = DCTModel.EncodeRiseNew(encRle, out encRise);

                //сохранить в файл
                DCTModel.Compressed = AccessoryFunc.CreateForSavingDCT(new List<List<byte[]>>() { encRise }, CompressionType, CoeffCount, CoeffDC, MyIntsToDoubles(CoeffCorrection));

                var statAll = new List<Statistic>();
                for (int i = 0; i < statRise.Count; i++)
                {
                    var all = stat[i] + statRise[i] + statRle[i];

                    all.BlockRezultSize = statRise[i].BlockRezultSize;
                    all.BlockSourseSize = stat[i].BlockSourseSize;
                    statAll.Add(all);
                }

                var decRise = DCTModel.DecodeRiseNew(encRise);
                var decRle = DCTModel.DecodeRleNew(decRise);

                DCTModel.SequenceSmoothed = DCTModel.DeconvertDct(decRle, BlockSize, CoeffDC);


                Statistic.CalculateError(DCTModel.SequenceSourse, DCTModel.SequenceSmoothed, ref statAll, BlockSize);
                statAll.Insert(0, Statistic.CalculateTotal(statAll));
                DCTModel.RiseRleStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }
            if (CompressTypeRiseRleAcDc)
            {
                #region RiseRleAcDc
                encDct = DCTModel.ConvertDct(BlockSize, CoeffCount, MyIntsToDoubles(CoeffCorrection), out stat, CoeffDC);

                DCTModel.DivideOnDcAc(encDct, BlockSize, CoeffCount, CoeffDC);
                List<byte[]> encDcRise;
                var statDcRise = DCTModel.EncodeRiseNew(DCTModel.DcBlocks, out encDcRise);

                List<short[]> encAcRle;
                var statAcRle = DCTModel.EncodeRleNew(DCTModel.AcBlocks, out encAcRle);
                List<byte[]> encAcRiseRle;
                var statAcRleRise = DCTModel.EncodeRiseNew(encAcRle, out encAcRiseRle);

                //сохранить в файл
                DCTModel.Compressed = AccessoryFunc.CreateForSavingDCT(new List<List<byte[]>>() { encDcRise, encAcRiseRle }, CompressionType, CoeffCount, CoeffDC, MyIntsToDoubles(CoeffCorrection));

                var statAll = new List<Statistic>();
                for (int i = 0; i < statDcRise.Count; i++)
                {
                    var all = stat[i] + statDcRise[i] + statAcRle[i] + statAcRleRise[i];
                    all.BlockSourseSize = stat[i].BlockSourseSize;
                    all.BlockRezultSize = statDcRise[i].BlockRezultSize + statAcRleRise[i].BlockRezultSize;
                    statAll.Add(all);
                }

                var decAcRiseRle = DCTModel.DecodeRiseNew(encAcRiseRle);
                var decAcRle = DCTModel.DecodeRleNew(decAcRiseRle);
                var decDc = DCTModel.DecodeRiseNew(encDcRise);
                var encDcAc = DCTModel.ConcatDcAc(decDc, decAcRle, CoeffCount, CoeffDC);

                DCTModel.SequenceSmoothed = DCTModel.DeconvertDct(encDcAc, BlockSize, CoeffDC);


                Statistic.CalculateError(DCTModel.SequenceSourse, DCTModel.SequenceSmoothed, ref statAll, BlockSize);
                statAll.Insert(0, Statistic.CalculateTotal(statAll));
                DCTModel.RiseRleAcDcStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }
            if (CompressTypeHuffman)
            {
                #region Huff
                encDct = DCTModel.ConvertDct(BlockSize, CoeffCount, MyIntsToDoubles(CoeffCorrection), out stat, CoeffDC);

                List<byte[]> encHuff;
                var statHuff = DCTModel.EncodeHuffman(encDct, out encHuff);

                //сохранить в файл
                DCTModel.Compressed = AccessoryFunc.CreateForSavingDCT(new List<List<byte[]>>() { encHuff }, CompressionType, CoeffCount, CoeffDC, MyIntsToDoubles(CoeffCorrection));

                var statAll = new List<Statistic>();
                for (int i = 0; i < statHuff.Count; i++)
                {
                    var all = stat[i] + statHuff[i];
                    all.BlockSourseSize = stat[i].BlockSourseSize;
                    statAll.Add(all);
                }

                var decHuff = DCTModel.DecodeHuffman(encHuff);
                DCTModel.SequenceSmoothed = DCTModel.DeconvertDct(decHuff, BlockSize, CoeffDC);

                Statistic.CalculateError(DCTModel.SequenceSourse, DCTModel.SequenceSmoothed, ref statAll, BlockSize);
                statAll.Insert(0, Statistic.CalculateTotal(statAll));
                DCTModel.HuffmanStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }
            if (CompressTypeHuffmanRleAcDc)
            {
                #region HuffRleAcDc
                encDct = DCTModel.ConvertDct(BlockSize, CoeffCount, MyIntsToDoubles(CoeffCorrection), out stat, CoeffDC);

                DCTModel.DivideOnDcAc(encDct, BlockSize, CoeffCount, CoeffDC);
                List<byte[]> encHuffDc;
                var statHuffDc = DCTModel.EncodeHuffman(DCTModel.DcBlocks, out encHuffDc);

                List<short[]> encAcRle;
                var statAcRle = DCTModel.EncodeRleNew(DCTModel.AcBlocks, out encAcRle);
                List<byte[]> encHuffAc;
                var statHuffAc = DCTModel.EncodeHuffman(encAcRle, out encHuffAc);

                //сохранить в файл
                DCTModel.Compressed = AccessoryFunc.CreateForSavingDCT(new List<List<byte[]>>() { encHuffDc, encHuffAc }, CompressionType, CoeffCount, CoeffDC, MyIntsToDoubles(CoeffCorrection));

                var statAll = new List<Statistic>();
                for (int i = 0; i < statHuffAc.Count; i++)
                {
                    var all = stat[i] + statHuffDc[i] + statHuffAc[i] + statAcRle[i];
                    all.BlockRezultSize = statHuffAc[i].BlockRezultSize + statHuffDc[i].BlockRezultSize;
                    all.BlockSourseSize = stat[i].BlockSourseSize;
                    statAll.Add(all);
                }

                DCTModel.DcBlocksDecoded = DCTModel.DecodeHuffman(encHuffDc);
                DCTModel.AcBlocksDecoded = DCTModel.DecodeRleNew(DCTModel.DecodeHuffman(encHuffAc));
                var decHuff = DCTModel.ConcatDcAc(DCTModel.DcBlocksDecoded, DCTModel.AcBlocksDecoded, CoeffCount, CoeffDC);
                DCTModel.SequenceSmoothed = DCTModel.DeconvertDct(decHuff, BlockSize, CoeffDC);


                Statistic.CalculateError(DCTModel.SequenceSourse, DCTModel.SequenceSmoothed, ref statAll, BlockSize);
                statAll.Insert(0, Statistic.CalculateTotal(statAll));
                DCTModel.HuffmanRleAcDcStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }

            var sourseSpectrum = Spectrum.CalculateSpectrum(DCTModel.SequenceSourse);
            var newSpectrum = Spectrum.CalculateSpectrum(DCTModel.SequenceSmoothed);

            OxyPlotModel.SequenceSourse = DCTModel.SequenceSourse;
            OxyPlotModel.SequenceNew = DCTModel.SequenceSmoothed;
            OxyPlotSpectrumModel.SpectrumSourse = sourseSpectrum;
            OxyPlotSpectrumModel.SpectrumNew = newSpectrum;

            ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(DCTModel.SequenceSourse);
            ZedGraphView.CurveNew = ZedGraphView.ListToPointList(DCTModel.SequenceSmoothed);
            ZedGraphSpectrumView.SpectrumSourse = ZedGraphSpectrumView.ArrayToPointList(sourseSpectrum);
            ZedGraphSpectrumView.SpectrumNew = ZedGraphSpectrumView.ArrayToPointList(newSpectrum);
        }

        private void ParsingSourseComplete()
        {
            ConvertIsEnabled = DCTModel.SequenceSourse != null;
        }

        private void CompressedComplete()
        {
            SaveIsEnabled = DCTModel.Compressed != null;
        }

        private void OpenFile()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt|Emg compress files (.emgdct)|*.emgdct"
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
            DCTModel.Compressed = null;
            if (ext == ".emgdct")
            {
                OpenAssync();
            }
            else if (ext == ".txt")
            {
                try
                {
                    DCTModel.Read(FileName);
                }
                catch (Exception ex)
                {
                    ModernDialog.ShowMessage(ex.Message, "Ошибка", MessageBoxButton.OK);
                }
            }
        }

        private static void SaveFile()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Compressed signal",
                DefaultExt = ".emgdct",
                Filter = "Emg compress files (.emgdct)|*.emgdct"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                try
                {
                    AccessoryFunc.WriteFile(filename, DCTModel.Compressed);
                    ModernDialog.ShowMessage("Файл успешно сохранен", "Результат операции", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    ModernDialog.ShowMessage(ex.Message, "Результат операции", MessageBoxButton.OK);
                }
            }
        }

        private static double[] MyIntsToDoubles(IEnumerable<MyInt> myInts) => myInts.Select(t => (double)t.Value).ToArray();

        private static ObservableCollection<MyInt> DoublesToMyInt(double[] doubles)
        {
            var obsColl = new ObservableCollection<MyInt>();
            foreach (var d in doubles)
            {
                obsColl.Add(ToMyInt(d));
            }
            return obsColl;
        }

        private static MyInt ToMyInt(double d) => new MyInt((int) d);

        #endregion

        private void OpenInExcel()
        {
            var excelApp = new Excel.Application();
            excelApp.Application.Workbooks.Add(Type.Missing);
            excelApp.Columns.ColumnWidth = 15;

            var sheet = (Excel.Worksheet)excelApp.ActiveSheet;

            sheet.Range[sheet.Cells[2, 1], sheet.Cells[2, 6]].Merge(Type.Missing);
            sheet.Cells[2, 1] = "Статистика дискретного косинусного преобразования";
            (sheet.Cells[2, 1] as Excel.Range).Font.Bold = true;
            (sheet.Cells[2, 1] as Excel.Range).Font.Size = 16;

            for (int i = 4; i < 9; i++)
            {
                sheet.Range[sheet.Cells[i, 1], sheet.Cells[i, 2]].Merge(Type.Missing);
                (sheet.Cells[i, 3] as Excel.Range).HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
            }

            sheet.Cells[4, 1] = "Длина блока";
            sheet.Cells[4, 3] = BlockSize;
            sheet.Cells[5, 1] = "Оставить коэффициентов";
            sheet.Cells[5, 3] = CoeffCount.ToString();
            sheet.Cells[6, 1] = "Из них DC коэффициентов";
            sheet.Cells[6, 3] = CoeffDC.ToString();
            sheet.Cells[7, 1] = "Вектор коэффициентов:";
            var coeffs = MyIntsToDoubles(CoeffCorrection);
            sheet.Range[sheet.Cells[7, 3], sheet.Cells[7, 4]].Merge(Type.Missing);
            sheet.Cells[7, 3] = coeffs.Aggregate("", (current, t) => current + (t + "; "));
            sheet.Cells[8, 1] = "Метод сжатия";
            sheet.Cells[8, 3] = CompressionType.ToString();


            sheet.Range[sheet.Cells[10, 1], sheet.Cells[10, 6]].WrapText = true;
            sheet.Range[sheet.Cells[10, 1], sheet.Cells[10, 6]].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

            sheet.Range[sheet.Cells[10, 1], sheet.Cells[10, 6]].Font.Bold = true;
            sheet.Cells[10, 1] = "Номер";
            sheet.Cells[10, 2] = "Время (Ticks)";
            sheet.Cells[10, 3] = "Размер исходного блока (bytes)";
            sheet.Cells[10, 4] = "Размер нового блока (bytes)";
            sheet.Cells[10, 5] = "Коэффициент сжатия";
            sheet.Cells[10, 6] = "Средняя погрешность, %";

            for (int i = 0; i < StatisticTable.Count; i++)
            {
                sheet.Range[sheet.Cells[i + 11, 1], sheet.Cells[i + 11, 6]].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.Cells[i + 11, 1] = StatisticTable[i].Title;
                sheet.Cells[i + 11, 2] = StatisticTable[i].Time.Ticks;
                sheet.Cells[i + 11, 3] = StatisticTable[i].BlockSourseSize;
                sheet.Cells[i + 11, 4] = StatisticTable[i].BlockRezultSize;
                sheet.Cells[i + 11, 5] = StatisticTable[i].CompressionRatio;
                sheet.Cells[i + 11, 6] = StatisticTable[i].Error;
            }

            excelApp.Visible = true;
        }

        private void DecompressFile()
        {
            CompressType type;
            int coeffCount, dcCount;
            double[] vectCorr;
            var dec = AccessoryFunc.CreateFromSavingDCT(DCTModel.CompressedFromFile, out type, out coeffCount, out dcCount, out vectCorr);
            CoeffCount = coeffCount;
            CoeffDC = dcCount;
            CoeffCorrection = DoublesToMyInt(vectCorr);
            CompressionType = type;
            CompressTypeNothing = CompressionType == CompressType.Nothing;
            CompressTypeRise = CompressionType == CompressType.Rise;
            CompressTypeRiseRle = CompressionType == CompressType.RiseRle;
            CompressTypeRiseAcDc = CompressionType == CompressType.RiseAcDc;
            CompressTypeRiseRleAcDc = CompressionType == CompressType.RiseRleAcDc;
            CompressTypeHuffman = CompressionType == CompressType.Huffman;
            CompressTypeHuffmanRleAcDc = CompressionType == CompressType.HuffmanRleAcDc;

            switch (CompressionType)
            {
                case CompressType.Rise:
                    {
                        var encDct = DCTModel.DecodeRiseNew(dec.First());
                        DCTModel.SequenceSmoothed = DCTModel.DeconvertDct(encDct, BlockSize, CoeffDC);

                        break;
                    }
                case CompressType.RiseRle:
                    {
                        var decRise = DCTModel.DecodeRiseNew(dec.First());
                        var decRle = DCTModel.DecodeRleNew(decRise);

                        DCTModel.SequenceSmoothed = DCTModel.DeconvertDct(decRle, BlockSize, CoeffDC);

                        break;
                    }
                case CompressType.RiseRleAcDc:
                {
                    var encDcRise = dec.First();
                    var encAcRiseRle = dec.Last();

                    var decAcRiseRle = DCTModel.DecodeRiseNew(encAcRiseRle);
                    var decAcRle = DCTModel.DecodeRleNew(decAcRiseRle);
                    var decDc = DCTModel.DecodeRiseNew(encDcRise);
                    var encDcAc = DCTModel.ConcatDcAc(decDc, decAcRle, CoeffCount, CoeffDC);

                    DCTModel.SequenceSmoothed = DCTModel.DeconvertDct(encDcAc, BlockSize, CoeffDC);

                    break;
                }
                case CompressType.RiseAcDc:
                {
                    var encDc = dec.First();
                    var encAc = dec.Last();

                    DCTModel.DcBlocksDecoded = DCTModel.DecodeRiseNew(encDc);
                    DCTModel.AcBlocksDecoded = DCTModel.DecodeRiseNew(encAc);
                    var encDct = DCTModel.ConcatDcAc(DCTModel.DcBlocksDecoded, DCTModel.AcBlocksDecoded, CoeffCount,
                        CoeffDC);
                    DCTModel.SequenceSmoothed = DCTModel.DeconvertDct(encDct, BlockSize, CoeffDC);

                    break;
                }
                case CompressType.Huffman:
                {
                    var decHuff = DCTModel.DecodeHuffman(dec.First());
                    DCTModel.SequenceSmoothed = DCTModel.DeconvertDct(decHuff, BlockSize, CoeffDC);

                    break;
                }
                case CompressType.HuffmanRleAcDc:
                {
                    var encHuffDc = dec.First();
                    var encHuffAc = dec.Last();

                    DCTModel.DcBlocksDecoded = DCTModel.DecodeHuffman(encHuffDc);
                    DCTModel.AcBlocksDecoded = DCTModel.DecodeRleNew(DCTModel.DecodeHuffman(encHuffAc));
                    var decHuff = DCTModel.ConcatDcAc(DCTModel.DcBlocksDecoded, DCTModel.AcBlocksDecoded, CoeffCount,
                        CoeffDC);
                    DCTModel.SequenceSmoothed = DCTModel.DeconvertDct(decHuff, BlockSize, CoeffDC);
                    break;
                }
            }

            var sourseSpectrum = Spectrum.CalculateSpectrum(DCTModel.SequenceSourse);
            var newSpectrum = Spectrum.CalculateSpectrum(DCTModel.SequenceSmoothed);

            OxyPlotModel.SequenceSourse = DCTModel.SequenceSourse ?? new short[1];
            OxyPlotModel.SequenceNew = DCTModel.SequenceSmoothed;
            OxyPlotSpectrumModel.SpectrumSourse = sourseSpectrum;
            OxyPlotSpectrumModel.SpectrumNew = newSpectrum;

            ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(DCTModel.SequenceSourse);
            ZedGraphView.CurveNew = ZedGraphView.ListToPointList(DCTModel.SequenceSmoothed);
            ZedGraphSpectrumView.SpectrumSourse = ZedGraphSpectrumView.ArrayToPointList(sourseSpectrum);
            ZedGraphSpectrumView.SpectrumNew = ZedGraphSpectrumView.ArrayToPointList(newSpectrum);
        }

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
