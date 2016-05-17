using System;
using System.Collections.Generic;
using System.Linq;
using SignalCompressionMUI.ViewModels;
using SignalCompressionMUI.Models.Algorithms;

namespace SignalCompressionMUI.Models
{
    public static class WaveletModel
    {
        public static short[] SequenceSmoothed;
        public static short[] SequenceSourse;
        public static List<List<short>> Converted;
        private static List<List<List<short>>> _convertedBlocks;
        private static List<short[]> _deconvertedBlocks; 

        public static void Read(string path)
        {
            SequenceSourse = AccessoryFunc.ReadAndPrepare(path);
        }

        public static List<Statistic> Convert(WaveletType wvType, СoeffCount coeffCount, int rounding, int blockSize, int depth)
        {
            var stat = ConvertAll(blockSize, wvType, rounding, depth);

            //возможно нужно уничтожить часть коэффициентов
            switch (coeffCount)
            {
                case СoeffCount.All:
                    break;

                case СoeffCount.Half:
                    stat = CutHalf(stat);
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
            }

            DeConvertAll(wvType, rounding);
            SequenceSmoothed = AccessoryFunc.ConcatSequence(_deconvertedBlocks);
        }


        private static List<Statistic> CutHalf(List<Statistic> stat)
        {
            var statNew = new List<Statistic>();
            int i = 0;
            foreach (var block in _convertedBlocks)
            {
                block.RemoveRange(block.Count/2, block.Count/2);

                var st = stat[i++];
                st.BlockRezultSize = st.BlockRezultSize/2;
                statNew.Add(st);
            }

            return statNew;
        }

        private static void AddHalf()
        {
            foreach (var block in _convertedBlocks)
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
            _convertedBlocks = new List<List<List<short>>>();

            foreach (var block in blocks)
            {
                #region замеры
                var blockstat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                #endregion

                var convBlock = ConvertBlock(block, wvType, rounding, depth);
                _convertedBlocks.Add(convBlock);

                #region замеры
                swatch.Stop();
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

            foreach (var block in _convertedBlocks)
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
