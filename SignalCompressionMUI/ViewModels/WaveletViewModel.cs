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
        private Visibility _statVisiblity;
        private bool _isStatExist;
        private BackgroundWorker _worker;
        private BackgroundWorker _worker2;

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
            StatisticTable = null;
            _worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += worker_DoWork;
           // _worker.ProgressChanged += worker_ProgressChanged;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            _worker2 = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _worker2.DoWork += worker2_DoWork;
            _worker2.RunWorkerCompleted += worker2_RunWorkerCompleted;

            WaveletModel.OnCompressComplete += CompressedComplete;
            WaveletModel.OnSourseParsingComplete += ParsingSourseComplete;
            OpenFileCommand = new RelayCommand(arg => OpenFile());
            //ConvertCommand = new RelayCommand(arg => Convert());
            ConvertCommand = new RelayCommand(arg => ConvertAssync());
            SaveCommand = new RelayCommand(arg => SaveFile());
            OpenInExcelCommand = new RelayCommand(arg => OpenInExcel());
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

        private void ConvertAssync()
        {
            PBar = Visibility.Visible;
            if (!_worker.IsBusy)
                _worker.RunWorkerAsync(this);
        }

        private void OpenAssync()
        {
            PBar = Visibility.Visible;
            if (!_worker2.IsBusy)
                _worker2.RunWorkerAsync(this);
        }

        private void worker2_DoWork(object sender, DoWorkEventArgs e)
        {
            var input = (WaveletViewModel)e.Argument;
            WaveletModel.CompressedFromFile = AccessoryFunc.ReadFile(input.FileName);
            input.DecompressFile();
            e.Result = input;
        }

        private void worker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                // Ошибка была сгенерирована обработчиком события DoWork
                ModernDialog.ShowMessage(e.Error.Message, "Ошибка", MessageBoxButton.OK);
            else
                PBar = Visibility.Hidden;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var input = (WaveletViewModel)e.Argument;
            input.Convert();
            e.Result = input;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                // Ошибка была сгенерирована обработчиком события DoWork
                ModernDialog.ShowMessage(e.Error.Message, "Ошибка", MessageBoxButton.OK);
            else
                PBar = Visibility.Hidden;
        }

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

                    Statistic.CalculateError(WaveletModel.SequenceSourse, WaveletModel.SequenceSmoothed, ref stat, BlockSize);
                    stat.Insert(0, Statistic.CalculateTotal(stat));
                    StatisticTable = stat;
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
                case CompressType.RiseHuffman:
                {
                    var decHuff = WaveletModel.DecodeHuffman(dec);
                    var decRise = WaveletModel.DecodeRleShort(decHuff, RleCount);
                    WaveletModel.ConvertedBlocks = decRise;
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

        private void OpenInExcel()
        {
            var excelApp = new Microsoft.Office.Interop.Excel.Application();
            excelApp.Application.Workbooks.Add(Type.Missing);
            excelApp.Columns.ColumnWidth = 15;

            var sheet = (Excel.Worksheet)excelApp.ActiveSheet;

            sheet.Range[sheet.Cells[2, 1], sheet.Cells[2, 5]].Merge(Type.Missing);
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
            //for (int i = 1; i < 7; i++)
            //{
            //    (sheet.Cells[12, i] as Excel.Range).WrapText = true;
            //    (sheet.Cells[12, i] as Excel.Range).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            //}
            sheet.Range[sheet.Cells[12, 1], sheet.Cells[12, 6]].Font.Bold = true;
            sheet.Cells[12, 1] = "Номер";
            sheet.Cells[12, 2] = "Время (Ticks)";
            sheet.Cells[12, 3] = "Размер исходного блока (bytes)";
            sheet.Cells[12, 4] = "Размер нового блока (bytes)";
            sheet.Cells[12, 5] = "Коэффициент сжатия";
            sheet.Cells[12, 6] = "Суммарная погрешность";

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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
