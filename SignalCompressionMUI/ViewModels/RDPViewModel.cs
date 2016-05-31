using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FirstFloor.ModernUI.Windows.Controls;
using SignalCompressionMUI.Models;
using SignalCompressionMUI.Models.Algorithms.Spectrum;
using SignalCompressionMUI.Views;
using Excel = Microsoft.Office.Interop.Excel;

namespace SignalCompressionMUI.ViewModels
{
    class RDPViewModel : INotifyPropertyChanged
    {
        
        #region Fields and properties

        private string _fileName;
        private int _epsilon = 1;
        private int _blockSize = 1000;
        private List<Statistic> _statisticTable;
        private readonly BackgroundWorker _bwConvert;
        private readonly BackgroundWorker _bwOpenEnc;
        private readonly BackgroundWorker _bwExcel;
        private Visibility _statVisiblity;
        private Visibility _pBar;
        private Visibility _addAndMul;
        private CompressType _compressionType;
        private bool _compressTypeNothing;
        private bool _compressTypeRise;
        private bool _compressTypeHuffman;
        private bool _compressTypeRiseRle;
        private bool _compressTypeRiseHuffman;
        private bool _saveIsEnabled;
        private bool _convertIsEnabled;
        private bool _isStatExist;
        private bool _statColumnsFixed;
        private DataGridLength _colWidth;


        public Visibility AddAndMul
        {
            get { return _addAndMul; }
            set
            {
                _addAndMul = value;
                OnPropertyChanged("AddAndMul");
            }
        }

        public DataGridLength ColWidth
        {
            get { return _colWidth; }
            set
            {
                _colWidth = value;
                OnPropertyChanged("ColWidth");
            }
        }

        public bool StatColumnsFixed
        {
            get { return _statColumnsFixed; }
            set
            {
                _statColumnsFixed = value;                
                OnPropertyChanged("StatColumnsFixed");
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

        public CompressType CompressionType
        {
            get { return _compressionType; }
            set
            {
                _compressionType = value;
                AddAndMul = value == CompressType.Nothing ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged("CompressionType");
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

        public Visibility PBar
        {
            get { return _pBar; }
            set
            {
                _pBar = value;
                OnPropertyChanged("PBar");
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
                if (value >= 1)
                    _epsilon = value;
                OnPropertyChanged("Epsilon");
            }
        }

        public int BlockSize
        {
            get { return _blockSize;}
            set
            {
                if (value >= 1)
                    _blockSize = value;
                OnPropertyChanged("BlockSize");
            }
        }

        #endregion

        public RDPViewModel()
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

            PBar = Visibility.Hidden;
            OpenFileCommand = new RelayCommand(arg => OpenFile());
            ConvertCommand = new RelayCommand(arg => ConvertAssync());
            SaveCommand = new RelayCommand(arg => SaveFile());
            OpenInExcelCommand = new RelayCommand(arg => OpenInExcelAssync());
            ClearAllCommand = new RelayCommand(arg => ClearAll());
            //ConvertCommand = new RelayCommand(arg => Convert());

            CompressTypeNothing = true;
            RDPModel.OnCompressComplete += CompressedComplete;
            RDPModel.OnSourseParsingComplete += ParsingSourseComplete;

            ColWidth = 100;
        }

        #region Commands

        public ICommand OpenFileCommand { get; set; }

        public ICommand ConvertCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand OpenInExcelCommand { get; set; }

        public ICommand DoThis { get; set; }

        public ICommand ClearAllCommand { get; set; }

        #endregion

        #region Methods

        private void ParsingSourseComplete()
        {
            ConvertIsEnabled = RDPModel.SequenceSourse != null;
        }

        private void CompressedComplete()
        {
            SaveIsEnabled = RDPModel.Compressed != null;
        }

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
            PBar = Visibility.Hidden;
            RDPModel.StatChanged();
        }

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

        private void bwExcel_DoWork(object sender, DoWorkEventArgs e)
        {
            var input = (RDPViewModel)e.Argument;
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
            var input = (RDPViewModel)e.Argument;
            RDPModel.CompressedFromFile = AccessoryFunc.ReadFile(input.FileName);
            input.DecompressFile();
            e.Result = input;
        }

        private void bwOpenEnc_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                ModernDialog.ShowMessage(e.Error.Message, "Ошибка", MessageBoxButton.OK);
            PBar = Visibility.Hidden;
        }

        private void Convert()
        {
            RDPModel.Read(FileName);
            //RDPModel.ConvertRdp(BlockSize.ToString(), Epsilon.ToString());
            //Statistic = RDPModel.Stat;
            
            var stat = new List<Statistic>();
            var encRdp = RDPModel.ConvertRdp(out stat, RDPModel.SequenceSourse, BlockSize, Epsilon);
            var encDeltaCut = RDPModel.DeltaEncodeCut(encRdp);


            if (CompressTypeNothing)
            {
                #region Nothing

                var decDeltaCut = RDPModel.DeltaDecodedCut(encDeltaCut);
                var decRdp = RDPModel.DeconvertRdp(decDeltaCut);

                RDPModel.PRez = RDPModel.ConcatMyPoints(decDeltaCut);
                RDPModel.SequenceSmoothed = decRdp;

                var statAll = stat.Select(s => s.Clone()).ToList();

                Statistic.CalculateError(RDPModel.SequenceSourse, RDPModel.SequenceSmoothed, ref statAll,
                    BlockSize);
                statAll.Insert(0, Statistic.CalculateTotal(statAll));
                RDPModel.NothingStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }
            if (CompressTypeRise)
            {
                #region Rise

                stat = new List<Statistic>();
                encRdp = RDPModel.ConvertRdp(out stat, RDPModel.SequenceSourse, BlockSize, Epsilon);
                encDeltaCut = RDPModel.DeltaEncodeCut(encRdp);

                List<Statistic> statRise;
                var encRise = RDPModel.EncodeRise(encDeltaCut, out statRise);

                //сохранить в файл
                RDPModel.Compressed = AccessoryFunc.CreateForSavingRDP(encRise, CompressionType, Epsilon);

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


                Statistic.CalculateError(RDPModel.SequenceSourse, RDPModel.SequenceSmoothed, ref statAll,
                    BlockSize);
                statAll.Insert(0, Statistic.CalculateTotal(statAll));
                RDPModel.RiseStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }
            if (CompressTypeRiseRle)
            {
                #region RiseRle

                stat = new List<Statistic>();
                encRdp = RDPModel.ConvertRdp(out stat, RDPModel.SequenceSourse, BlockSize, Epsilon);
                encDeltaCut = RDPModel.DeltaEncodeCut(encRdp);

                List<Statistic> statRle;
                var encRle = RDPModel.EncodeRle(encDeltaCut, out statRle);
                List<Statistic> statRise;
                var encRise = RDPModel.EncodeRise(encRle, out statRise);

                //сохранить в файл
                RDPModel.Compressed = AccessoryFunc.CreateForSavingRDP(encRise, CompressionType, Epsilon);

                var statAll = new List<Statistic>();
                for (int i = 0; i < statRise.Count; i++)
                {
                    var all = stat[i] + statRise[i] + statRle[i];
                    all.BlockRezultSize = statRise[i].BlockRezultSize;
                    statAll.Add(all);
                }

                var decRise = RDPModel.DecodeRise(encRise);
                var decRle = RDPModel.DecodeRle(decRise);
                var decDeltaCut = RDPModel.DeltaDecodedCut(decRle);
                var decRdp = RDPModel.DeconvertRdp(decDeltaCut);

                RDPModel.PRez = RDPModel.ConcatMyPoints(decDeltaCut);
                RDPModel.SequenceSmoothed = decRdp;

                Models.Statistic.CalculateError(RDPModel.SequenceSourse, RDPModel.SequenceSmoothed, ref statAll,
                    BlockSize);
                statAll.Insert(0, Models.Statistic.CalculateTotal(statAll));
                RDPModel.RleRiseStat = statAll.First();
                StatisticTable = statAll;

                #endregion
            }
            if (CompressTypeRiseHuffman)
            {
                #region RiseHuff

                stat = new List<Statistic>();
                encRdp = RDPModel.ConvertRdp(out stat, RDPModel.SequenceSourse, BlockSize, Epsilon);
                encDeltaCut = RDPModel.DeltaEncodeCut(encRdp);

                List<Statistic> statHuff;
                var encHuffHalf = RDPModel.EncodeHuffmanHalf(encDeltaCut, out statHuff);
                List<Statistic> statRise;
                var encRise = RDPModel.EncodeRise(encHuffHalf, out statRise);

                //сохранить в файл
                RDPModel.Compressed = AccessoryFunc.CreateForSavingRDP(encRise, CompressionType, Epsilon);

                var statAll = new List<Statistic>();
                for (int i = 0; i < statRise.Count; i++)
                {
                    var all = stat[i] + statRise[i] + statHuff[i];
                    all.BlockRezultSize = statRise[i].BlockRezultSize;
                    statAll.Add(all);
                }

                var decRise = RDPModel.DecodeRise(encRise);
                var decHuffHalf = RDPModel.DecodeHuffmanHalf(decRise);
                var decDeltaCut = RDPModel.DeltaDecodedCut(decHuffHalf);
                var decRdp = RDPModel.DeconvertRdp(decDeltaCut);

                RDPModel.PRez = RDPModel.ConcatMyPoints(decDeltaCut);
                RDPModel.SequenceSmoothed = decRdp;


                Statistic.CalculateError(RDPModel.SequenceSourse, RDPModel.SequenceSmoothed, ref statAll,
                    BlockSize);
                statAll.Insert(0, Statistic.CalculateTotal(statAll));
                RDPModel.RiseHuffStat = statAll.First();
                StatisticTable = statAll;

                #endregion
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

        private void Do()
        {
            CompressType ct;
            int e;
            TryFindBest(1.5, 210, 20, BlockSize, out ct, out e);
        }

        /// <summary>
        /// Попытка найти наиболее оптимальный метод для заданных условий
        /// </summary>
        private void TryFindBest(double compressRatio, double time, double error, int blockSize, out CompressType type, out int epsilon)
        {
            epsilon = 1;
            double previousError = 0;
            bool first = true;

            while (true)
            {
                List<Statistic> stat;
                var encRdp = RDPModel.ConvertRdp(out stat, RDPModel.SequenceSourse, blockSize, epsilon++);
                var encDeltaCut = RDPModel.DeltaEncodeCut(encRdp);

                var decDeltaCut = RDPModel.DeltaDecodedCut(encDeltaCut);
                var decRdp = RDPModel.DeconvertRdp(decDeltaCut);

                RDPModel.PRez = RDPModel.ConcatMyPoints(decDeltaCut);
                RDPModel.SequenceSmoothed = decRdp;

                Statistic.CalculateError(RDPModel.SequenceSourse, RDPModel.SequenceSmoothed, ref stat,
                    BlockSize);
                var total = Statistic.CalculateTotal(stat);

                if (total.Error > error && first == false) break;

                previousError = total.Error;
                first = false;
            }

            type = CompressType.Rise;
            epsilon = 1;
        }

        private void DecompressFile()
        {
            CompressType type;
            int epsilon;
            var dec = AccessoryFunc.CreateFromSaving(RDPModel.CompressedFromFile, out type, out epsilon);
            Epsilon = epsilon;
            CompressionType = type;
            CompressTypeNothing = CompressionType == CompressType.Nothing;
            CompressTypeRise = CompressionType == CompressType.Rise;
            CompressTypeHuffman = CompressionType == CompressType.Huffman;
            CompressTypeRiseHuffman = CompressionType == CompressType.RiseHuffman;

            switch (type)
            {
                case CompressType.Rise:
                    {
                        var decRise = RDPModel.DecodeRise(dec);
                        var decDeltaCut = RDPModel.DeltaDecodedCut(decRise);
                        var decRdp = RDPModel.DeconvertRdp(decDeltaCut);

                        RDPModel.PRez = RDPModel.ConcatMyPoints(decDeltaCut);
                        RDPModel.SequenceSmoothed = decRdp;

                        break;
                    }
                case CompressType.RiseRle:
                    {
                        var decRise = RDPModel.DecodeRise(dec);
                        var decRle = RDPModel.DecodeRle(decRise);
                        var decDeltaCut = RDPModel.DeltaDecodedCut(decRle);
                        var decRdp = RDPModel.DeconvertRdp(decDeltaCut);

                        RDPModel.PRez = RDPModel.ConcatMyPoints(decDeltaCut);
                        RDPModel.SequenceSmoothed = decRdp;

                        break;
                    }
                case CompressType.RiseHuffman:
                    {
                        var decRise = RDPModel.DecodeRise(dec);
                        var decHuffHalf = RDPModel.DecodeHuffmanHalf(decRise);
                        var decDeltaCut = RDPModel.DeltaDecodedCut(decHuffHalf);
                        var decRdp = RDPModel.DeconvertRdp(decDeltaCut);

                        RDPModel.PRez = RDPModel.ConcatMyPoints(decDeltaCut);
                        RDPModel.SequenceSmoothed = decRdp;

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
                Filter = "Text documents (.txt)|*.txt|Emg compress files (.emgrdp)|*.emgrdp"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                var filename = dlg.FileName;
                FileName = filename;
            }

            //если открывают сжатый файл
            var ext = Path.GetExtension(FileName);
            RDPModel.Compressed = null;
            if (ext == ".emgrdp")
            {
                OpenAssync();
            }
            else if (ext == ".txt")
            {
                try
                {
                    RDPModel.Read(FileName);
                }
                catch (Exception ex)
                {
                    ModernDialog.ShowMessage(ex.Message, "Ошибка", MessageBoxButton.OK);
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
            sheet.Cells[2, 1] = "Статистика преобразования Рамера — Дугласа — Пекера";
            (sheet.Cells[2, 1] as Excel.Range).Font.Bold = true;
            (sheet.Cells[2, 1] as Excel.Range).Font.Size = 16;

            for (int i = 4; i < 7; i++)
            {
                sheet.Range[sheet.Cells[i, 1], sheet.Cells[i, 2]].Merge(Type.Missing);
                (sheet.Cells[i, 3] as Excel.Range).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            }

            sheet.Cells[4, 1] = "Длина блока";
            sheet.Cells[4, 3] = BlockSize;
            sheet.Cells[5, 1] = "Округление";
            sheet.Cells[5, 3] = Epsilon;
            sheet.Cells[6, 1] = "Метод сжатия";
            sheet.Cells[6, 3] = CompressionType.ToString();


            sheet.Range[sheet.Cells[8, 1], sheet.Cells[8, 8]].WrapText = true;
            sheet.Range[sheet.Cells[8, 1], sheet.Cells[8, 8]].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            sheet.Range[sheet.Cells[8, 1], sheet.Cells[8, 8]].Font.Bold = true;
            sheet.Cells[8, 1] = "Номер";
            sheet.Cells[8, 2] = "Время (Ticks)";
            sheet.Cells[8, 3] = "Размер исходного блока (bytes)";
            sheet.Cells[8, 4] = "Размер нового блока (bytes)";
            sheet.Cells[8, 5] = "Коэффициент сжатия";
            sheet.Cells[8, 6] = "Средняя погрешность, %";
            if (CompressionType == CompressType.Nothing)
            {
                sheet.Cells[8, 7] = "Сложений";
                sheet.Cells[8, 8] = "Умножений";
            }

            for (int i = 0; i < StatisticTable.Count; i++)
            {
                sheet.Range[sheet.Cells[i + 9, 1], sheet.Cells[i + 9, 8]].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                sheet.Cells[i + 9, 1] = StatisticTable[i].Title;
                sheet.Cells[i + 9, 2] = StatisticTable[i].Time.Ticks;
                sheet.Cells[i + 9, 3] = StatisticTable[i].BlockSourseSize;
                sheet.Cells[i + 9, 4] = StatisticTable[i].BlockRezultSize;
                sheet.Cells[i + 9, 5] = StatisticTable[i].CompressionRatio;
                sheet.Cells[i + 9, 6] = StatisticTable[i].Error;
                if (CompressionType == CompressType.Nothing)
                {
                    sheet.Cells[i + 9, 7] = StatisticTable[i].Additions;
                    sheet.Cells[i + 9, 8] = StatisticTable[i].Multiplications;
                }
            }

            excelApp.Visible = true;
        }

        private static void SaveFile()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "Compressed signal",
                DefaultExt = ".emgrdp",
                Filter = "Emg compress files (.emgrdp)|*.emgrdp"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                try
                {
                    AccessoryFunc.WriteFile(filename, RDPModel.Compressed);
                    ModernDialog.ShowMessage("Файл успешно сохранен", "Результат операции", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    ModernDialog.ShowMessage(ex.Message, "Результат операции", MessageBoxButton.OK);
                }
            }
        }

        private void ClearAll()
        {
            RDPModel.SequenceSourse = null;
            RDPModel.SequenceSmoothed = null;
            RDPModel.Compressed = null;
            RDPModel.CompressedFromFile = null;
            RDPModel.NothingStat = new Statistic();
            RDPModel.RiseStat = new Statistic();
            RDPModel.RleRiseStat = new Statistic();
            RDPModel.RiseHuffStat = new Statistic();
            StatisticTable = null;
            FileName = "";
            RDPModel.StatChanged();

            var sourseSpectrum = Spectrum.CalculateSpectrum(RDPModel.SequenceSourse);
            var newSpectrum = Spectrum.CalculateSpectrum(RDPModel.SequenceSmoothed);

            OxyPlotModel.SequenceSourse = RDPModel.SequenceSourse ?? new short[1];
            OxyPlotModel.SequenceNew = RDPModel.SequenceSmoothed ?? new short[1];
            OxyPlotSpectrumModel.SpectrumSourse = sourseSpectrum;
            OxyPlotSpectrumModel.SpectrumNew = newSpectrum;

            ZedGraphView.CurveSourse = ZedGraphView.ListToPointList(RDPModel.SequenceSourse);
            ZedGraphView.CurveNew = ZedGraphView.ListToPointList(RDPModel.SequenceSmoothed);
            ZedGraphSpectrumView.SpectrumSourse = ZedGraphSpectrumView.ArrayToPointList(sourseSpectrum);
            ZedGraphSpectrumView.SpectrumNew = ZedGraphSpectrumView.ArrayToPointList(newSpectrum);
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
