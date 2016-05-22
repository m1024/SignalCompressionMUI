using System.Collections.Generic;
using System.Linq;
using SignalCompressionMUI.ViewModels;
using SignalCompressionMUI.Models.Algorithms;
using SignalCompressionMUI.Models.Algorithms.Huffman;

namespace SignalCompressionMUI.Models
{
    public static class WaveletModel
    {
        public static short[] SequenceSmoothed { get; set; }
        private static short[] _sequenceSourse;

        public static short[] SequenceSourse
        {
            get { return _sequenceSourse; }
            set
            {
                _sequenceSourse = value;
                OnSourseParsingComplete?.Invoke();
            }
        }
        public static List<List<short>> Converted;
        public static List<List<List<short>>> ConvertedBlocks { get; set; }
        private static List<short[]> _deconvertedBlocks;
        private static string _path;
        private static byte[] _compressed;

        /// <summary>
        /// Для записи в файл
        /// </summary>
        public static byte[] Compressed
        {
            get { return _compressed; }
            set
            {
                _compressed = value;
                OnCompressComplete?.Invoke();
            }
        }

        /// <summary>
        /// То что считали из файла
        /// </summary>
        public static byte[] CompressedFromFile { get; set; }

        public delegate void Complete();
        public static event Complete OnCompressComplete;
        public static event Complete OnSourseParsingComplete;

        public static void Read(string path)
        {
            _path = path;
            SequenceSourse = AccessoryFunc.ReadAndPrepare(path);
        }

        public static List<Statistic> Convert(WaveletType wvType, СoeffCount coeffCount, int rounding, int blockSize, int depth)
        {
            //округлим чтобы на блоки нацело делилось
            Read(_path);
            var len = SequenceSourse.Length/blockSize*blockSize;
            var ss = SequenceSourse.ToList();
            ss.RemoveRange(len, SequenceSourse.Length - len);
            SequenceSourse = ss.ToArray();

            var stat = ConvertAll(blockSize, wvType, rounding, depth);

            //возможно нужно уничтожить часть коэффициентов
            switch (coeffCount)
            {
                case СoeffCount.All:
                    break;

                case СoeffCount.Half:
                    stat = CutPart(stat, 2, 1);
                    break;

                case СoeffCount.ThreeQuarter:
                    stat = CutPart(stat, 4, 3);
                    break;

                case СoeffCount.OneQuarter:
                    stat = CutPart(stat, 4, 1);
                    break;
            }

            return stat;
        }

        public static void Deconvert(WaveletType wvType, СoeffCount coeffCount, int rounding)
        {
            switch (coeffCount)
            {
                case СoeffCount.All:
                    break;

                case СoeffCount.Half:
                    AddHalf();
                    break;

                case СoeffCount.ThreeQuarter:
                    AddPart(4,1);
                    break;

                case СoeffCount.OneQuarter:
                    AddPart(4, 3);
                    break;
            }

            DeConvertAll(wvType, rounding);
            SequenceSmoothed = AccessoryFunc.ConcatSequence(_deconvertedBlocks);
        }


        public static List<List<byte[]>> EncodeHuffman(List<List<List<short>>> data, out List<Statistic> stat)
        {
            stat = new List<Statistic>();
            var enc = new List<List<byte[]>>();
            //var htree = new HuffmanTree();

            foreach (var block in data)
            {
                #region замеры
                var blockstat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                var enclen = 0;
                #endregion

                var blockEnc = new List<byte[]>();
                foreach (var subblock in block)
                {
                    var byteSubblock = AccessoryFunc.ShortsToBytes(subblock.ToArray());
                    var subblockEnc = AlgorithmDynHuff.Encode(byteSubblock);
                    blockEnc.Add(subblockEnc);
                    enclen += subblockEnc.Length;
                }
                enc.Add(blockEnc);

                #region замеры
                swatch.Stop();
                blockstat.BlockRezultSize = enclen;
                blockstat.Time = swatch.Elapsed;
                stat.Add(blockstat);
                #endregion
            }
            return enc;
        }

        public static List<List<byte[]>> EncodeHuffman(List<List<byte[]>> data, out List<Statistic> stat)
        {
            stat = new List<Statistic>();
            var enc = new List<List<byte[]>>();
            //var htree = new HuffmanTree();

            foreach (var block in data)
            {
                #region замеры
                var blockstat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                var enclen = 0;
                #endregion

                var blockEnc = new List<byte[]>();
                foreach (var subblock in block)
                {
                    var subblockEnc = AlgorithmDynHuff.Encode(subblock);
                    blockEnc.Add(subblockEnc);
                    enclen += subblockEnc.Length;
                }
                enc.Add(blockEnc);

                #region замеры
                swatch.Stop();
                blockstat.BlockRezultSize = enclen;
                blockstat.Time = swatch.Elapsed;
                stat.Add(blockstat);
                #endregion
            }
            return enc;
        }

        public static List<List<byte[]>> DecodeHuffmanByte(List<List<byte[]>> data)
        {
            var dec = new List<List<byte[]>>();

            foreach (var block in data)
            {
                var blockDec = new List<byte[]>();
                foreach (var subblock in block)
                {
                    var subblockDec = AlgorithmDynHuff.Decode(subblock);
                    blockDec.Add(subblockDec);
                }
                dec.Add(blockDec);
            }
            return dec;
        }

        public static List<List<List<short>>> DecodeHuffman(List<List<byte[]>> data)
        {
            var dec = new List<List<List<short>>>();

            foreach (var block in data)
            {
                var blockDec = new List<List<short>>();
                foreach (var subblock in block)
                {
                    var subblockDec = AlgorithmDynHuff.Decode(subblock);
                    var shortSubBl = AccessoryFunc.BytesToShorts(subblockDec);
                    blockDec.Add(shortSubBl.ToList());
                }
                dec.Add(blockDec);
            }
            return dec;
        }

        public static List<List<List<short>>> EncodeRleShort(List<List<List<short>>> data, out List<Statistic> stat)
        {
            stat = new List<Statistic>();
            var enc = new List<List<List<short>>>();

            foreach (var block in data)
            {
                #region замеры
                var blockstat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                var enclen = 0;
                #endregion

                var blockEnc = new List<List<short>>();
                foreach (var subblock in block)
                {
                    var subblockEnc = AlgorithmRLE.Encode(subblock.ToArray()).ToList();
                    blockEnc.Add(subblockEnc);
                    enclen += subblockEnc.Count;
                }
                enc.Add(blockEnc);

                #region замеры
                swatch.Stop();
                blockstat.BlockRezultSize = enclen*2;
                blockstat.Time = swatch.Elapsed;
                stat.Add(blockstat);
                #endregion
            }
            return enc;
        }

        public static List<List<List<short>>> EncodeRleShort(List<List<List<short>>> data, out List<Statistic> stat, int rleCount)
        {
            stat = new List<Statistic>();
            var enc = new List<List<List<short>>>();

            foreach (var block in data)
            {
                #region замеры
                var blockstat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                var enclen = 0;
                #endregion

                var blockEnc = new List<List<short>>();
                for (int i=0; i<block.Count; i++)
                {
                    var subblockEnc = i > rleCount ? AlgorithmRLE.Encode(block[i].ToArray()).ToList() : block[i];
                    blockEnc.Add(subblockEnc);
                    enclen += subblockEnc.Count;
                }
                enc.Add(blockEnc);

                #region замеры
                swatch.Stop();
                blockstat.BlockRezultSize = enclen * 2;
                blockstat.Time = swatch.Elapsed;
                stat.Add(blockstat);
                #endregion
            }
            return enc;
        }


        public static List<List<List<short>>> DecodeRleShort(List<List<List<short>>> data, int rleCount)
        {
            var dec = new List<List<List<short>>>();

            foreach (var block in data)
            {
                var blockEnc = new List<List<short>>();
                for (int i=0; i<block.Count; i++)
                {
                    var subblockEnc = i > rleCount ? AlgorithmRLE.Decode(block[i].ToArray()).ToList() : block[i];
                    blockEnc.Add(subblockEnc);
                }
                dec.Add(blockEnc);
            }
            return dec;
        }

        public static List<List<List<short>>> DecodeRleShort(List<List<List<short>>> data)
        {
            var dec = new List<List<List<short>>>();

            foreach (var block in data)
            {
                var blockEnc = new List<List<short>>();
                foreach (var subblock in block)
                {
                    var subblockEnc = AlgorithmRLE.Decode(subblock.ToArray()).ToList();
                    blockEnc.Add(subblockEnc);
                }
                dec.Add(blockEnc);
            }
            return dec;
        }

        public static List<List<byte[]>> EncodeRle(List<List<List<short>>> data, out List<Statistic> stat)
        {
            stat = new List<Statistic>();
            var enc = new List<List<byte[]>>();

            foreach (var block in data)
            {
                #region замеры
                var blockstat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                var enclen = 0;
                #endregion

                var blockEnc = new List<byte[]>();
                foreach (var subblock in block)
                {
                    var subblockEnc = AlgorithmRLE.Encode(AlgorithmRLE.ShortToByte(subblock.ToArray()));
                    blockEnc.Add(subblockEnc);
                    enclen += subblockEnc.Length;
                }
                enc.Add(blockEnc);

                #region замеры
                swatch.Stop();
                blockstat.BlockRezultSize = enclen;
                blockstat.Time = swatch.Elapsed;
                stat.Add(blockstat);
                #endregion
            }
            return enc;
        }

        public static List<List<List<short>>> DecodeRle(List<List<byte[]>> data)
        {
            var dec = new List<List<List<short>>>();

            foreach (var block in data)
            {
                var blockEnc = new List<List<short>>();
                foreach (var subblock in block)
                {
                    var subblockEnc = AlgorithmRLE.ByteToShort(AlgorithmRLE.Decode(subblock)).ToList();
                    blockEnc.Add(subblockEnc);
                }
                dec.Add(blockEnc);
            }
            return dec;
        }

        public static List<List<byte[]>> EncodeRiseByte(List<List<byte[]>> data, out List<Statistic> stat)
        {
            stat = new List<Statistic>();
            var enc = new List<List<byte[]>>();

            foreach (var block in data)
            {
                #region замеры
                var blockstat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                var enclen = 0;
                #endregion

                var blockEnc = new List<byte[]>();
                foreach (var subblock in block)
                {
                    var subblockEnc = AlgorithmRise.Encode(subblock);
                    blockEnc.Add(subblockEnc);
                    enclen += subblockEnc.Length;
                }
                enc.Add(blockEnc);

                #region замеры
                swatch.Stop();
                blockstat.BlockRezultSize = enclen;
                blockstat.Time = swatch.Elapsed;
                stat.Add(blockstat);
                #endregion
            }
            return enc;
        }

        public static List<List<byte[]>> EncodeRise(List<List<List<short>>> data, out List<Statistic> stat)
        {
            stat = new List<Statistic>();
            var enc = new List<List<byte[]>>();

            foreach (var block in data)
            {
                #region замеры
                var blockstat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                var enclen = 0;
                #endregion

                var blockEnc = new List<byte[]>();
                foreach (var subblock in block)
                {
                    var subblockEnc = AlgorithmRise.Encode(subblock.ToArray());
                    blockEnc.Add(subblockEnc);
                    enclen += subblockEnc.Length;
                }
                enc.Add(blockEnc);

                #region замеры
                swatch.Stop();
                blockstat.BlockRezultSize = enclen;
                blockstat.Time = swatch.Elapsed;
                stat.Add(blockstat);
                #endregion
            }
            return enc;
        }

        public static List<List<byte[]>> DecodeRiseByte(List<List<byte[]>> enc)
        {
            var dec = new List<List<byte[]>>();
            foreach (var block in enc)
            {
                var decBlock = new List<byte[]>();
                foreach (var subblock in block)
                {
                    var decSubblock = AlgorithmRise.DecodeByte(subblock);
                    decBlock.Add(decSubblock);
                }
                dec.Add(decBlock);
            }
            return dec;
        }

        public static List<List<List<short>>> DecodeRise(List<List<byte[]>> enc)
        {
            var dec = new List<List<List<short>>>();
            foreach (var block in enc)
            {
                var decBlock = new List<List<short>>();
                foreach (var subblock in block)
                {
                    var decSubblock = AlgorithmRise.Decode(subblock);
                    decBlock.Add(decSubblock.ToList());
                }
                dec.Add(decBlock);
            }
            return dec;
        }

        private static List<Statistic> CutPart(List<Statistic> stat, int denominator, int numerator)
        {
            var statNew = new List<Statistic>();
            int i = 0;
            foreach (var block in ConvertedBlocks)
            {
                block.RemoveRange((block.Count / denominator) * numerator, (block.Count / denominator)*(denominator-numerator));

                var st = stat[i++];
                st.BlockRezultSize = (st.BlockRezultSize / denominator) * numerator;
                statNew.Add(st);
            }

            return statNew;
        }

        private static void AddPart(int denominator, int numerator)
        {
            foreach (var block in ConvertedBlocks)
            {
                var len = block.Count/(denominator - numerator)*numerator;
                for (int i = 0; i < len; i++)
                {
                    var m = new short[block[0].Count];
                    block.Add(m.ToList());
                }
            }
        }

        private static void AddHalf()
        {
            foreach (var block in ConvertedBlocks)
            {
                var h = block.Select(t => new short[block[0].Count]).Select(m => m.ToList()).ToList();
                block.AddRange(h);
            }
        }

        private static List<Statistic> ConvertAll(int blockSize, WaveletType wvType, int rounding, int depth)
        {
            var statistic = new List<Statistic>();

            //дробим на блоки
            var blocks = AccessoryFunc.DivideSequence(SequenceSourse, blockSize);
            //последний неровный может быть, ну его нафиг
            //blocks.RemoveAt(blocks.Count-1);

            ConvertedBlocks = new List<List<List<short>>>();

            foreach (var block in blocks)
            {
                #region замеры
                var blockstat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                #endregion

                var convBlock = ConvertBlock(block, wvType, rounding, depth);
                ConvertedBlocks.Add(convBlock);

                #region замеры
                swatch.Stop();
                blockstat.Number = statistic.Count;
                blockstat.Title = statistic.Count.ToString();
                blockstat.BlockSourseSize = blockSize*2;
                blockstat.BlockRezultSize = convBlock.Count*convBlock[0].Count*2;
                blockstat.Time = swatch.Elapsed;
                statistic.Add(blockstat);
                #endregion
            }

            return statistic;
        }

        private static void DeConvertAll(WaveletType wvType, int rounding)
        {
            _deconvertedBlocks = new List<short[]>();

            foreach (var block in ConvertedBlocks)
            {
                var deconvBlock = DeconvertBlock(block, wvType, rounding);
                _deconvertedBlocks.Add(deconvBlock);
            }
        }


        private static List<List<short>> ConvertBlock(short[] block, WaveletType wvType, int rounding, int depth)
        {
            WaveletArray wvArray = new WaveletArray
            {
                Sourse = block.ToList(),
                ConvertType = wvType
            };
            wvArray.Convert();
            wvArray.Round(rounding);

            var wvTree = new WaveletTree(wvArray);
            wvTree.BuildTree(depth, rounding);
            wvTree.ConvertedToRoot();

            //округление
            var rounded = RoundListList(wvTree.Converted, rounding);
            return rounded;
        }

        private static short[] DeconvertBlock(List<List<short>> rounded, WaveletType wvType, int rounding)
        {
            //обратное округление
            var derounded = DeRoundListList(rounded, rounding);

            var deconvWvTree = new WaveletTree { Converted = derounded };
            deconvWvTree.BuidDeconvTree();
            deconvWvTree.Deconvert(wvType);

            return deconvWvTree.wvArray.New.ToArray();
        }

        public static List<List<short>> RoundListList(List<List<short>> s, int num) 
            => s.Select(l => RoundList(l, num)).ToList();

        public static List<List<short>> DeRoundListList(List<List<short>> s, int num)
            => s.Select(l => DeRoundList(l, num)).ToList();

        private static List<short> RoundList(List<short> s, int num)
        {
            var res = new short[s.Count];
            for (int i = 0; i < s.Count; i++)
                res[i] = (short)(s[i]/num);
            return res.ToList();
        }

        private static List<short> DeRoundList(List<short> s, int num)
        {
            var res = new short[s.Count];
            for (int i = 0; i < s.Count; i++)
                res[i] = (short)(s[i] * num);
            return res.ToList();
        }
    }
}
