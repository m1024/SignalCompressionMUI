using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using SignalCompressionMUI.Models.Algorithms;
using SignalCompressionMUI.Models.Algorithms.Huffman;
using ZedGraph;

namespace SignalCompressionMUI.Models
{
    public static class DCTModel
    {
        public static short[] SequenceSmoothed;
        private static short[] _sequenceSourse;
        public static List<short[]> DcBlocks;
        public static List<short[]> AcBlocks;
        public static List<short[]> DcBlocksDecoded;
        public static List<short[]> AcBlocksDecoded;

        public static Statistic NothingStat { get; set; }
        public static Statistic RiseStat { get; set; }
        public static Statistic RiseRleStat { get; set; }
        public static Statistic RiseRleAcDcStat { get; set; }
        public static Statistic RiseAcDcStat { get; set; }
        public static Statistic HuffmanStat { get; set; }
        public static Statistic HuffmanRleAcDcStat { get; set; }

        public static short[] SequenceSourse
        {
            get { return _sequenceSourse; }
            set
            {
                _sequenceSourse = value;
                OnSourseParsingComplete?.Invoke();
            }
        }

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
        private static byte[] _compressed;

        /// <summary>
        /// То что считали из файла
        /// </summary>
        public static byte[] CompressedFromFile { get; set; }

        public static void Read(string path)
        {
            SequenceSourse = AccessoryFunc.ReadAndPrepare(path);
        }

        public static void StatChanged() => OnStatChanged?.Invoke();

        public delegate void ValueChangedHandler();
        public static event ValueChangedHandler OnCompressComplete;
        public static event ValueChangedHandler OnSourseParsingComplete;
        public static event ValueChangedHandler OnStatChanged;

        /// <summary>
        /// Обратное dct преобразование с разделением на dc и ac части
        /// </summary>
        private static short[] DctBlockDeconvertion(short[] block, int dcCoef, int coefCount)
        {
            var blockDc = new short[(int)(block.Length * (dcCoef / (float)coefCount))];
            var blockAc = new short[(int)(block.Length * ((coefCount - dcCoef) / (float)coefCount))];
            Array.Copy(block, blockDc, blockDc.Length);
            Array.Copy(block, blockDc.Length, blockAc, 0, blockAc.Length);

            var subBlocksDc = AccessoryFunc.DivideSequence(blockDc, dcCoef);
            var subBlocksAc = AccessoryFunc.DivideSequence(blockAc, coefCount - dcCoef);

            if (subBlocksDc.Count == 0) //если dc коэффициенты не были выделены
            {
                var deconvertSubBlocks = new List<short[]>();
                foreach (var subBlockAc in subBlocksAc)
                {
                    var subBlock = new short[8];
                    Array.Copy(subBlockAc, subBlock, subBlockAc.Length);
                    subBlock = AlgorithmDCT.InvertConvert(subBlock);
                    deconvertSubBlocks.Add(subBlock);
                }
                return AccessoryFunc.ConcatSequence(deconvertSubBlocks);
            }
            if (subBlocksAc.Count == 0) //если ас коэффициенты не были выделены
            {
                var deconvertSubBlocks = new List<short[]>();
                foreach (var subBlockDc in subBlocksDc)
                {
                    var subBlock = new short[8];
                    Array.Copy(subBlockDc, subBlock, subBlockDc.Length);
                    subBlock = AlgorithmDCT.InvertConvert(subBlock);
                    deconvertSubBlocks.Add(subBlock);
                }
                return AccessoryFunc.ConcatSequence(deconvertSubBlocks);
            }

            var deconvertedSubBlocks = new List<short[]>();
            if (subBlocksAc.Count != subBlocksDc.Count) throw new ArgumentException("Ac length != Dc length");

            for (int i = 0; i < subBlocksDc.Count; i++)
            {
                var subBlock = new short[8];
                Array.Copy(subBlocksDc[i], subBlock, subBlocksDc[i].Length);
                Array.Copy(subBlocksAc[i], 0, subBlock, subBlocksDc[i].Length, subBlocksAc[i].Length);
                deconvertedSubBlocks.Add(AlgorithmDCT.InvertConvert(subBlock));
            }

            return AccessoryFunc.ConcatSequence(deconvertedSubBlocks);
        }

        /// <summary>
        /// Прямое dct преобразование для одного блока с разбиением на группы коэффициентов
        /// </summary>
        /// <param name="block">Блок данных для преобразования</param>
        /// <param name="dcCoef">Число dc коэффициентов</param>
        /// <returns>Преобразованный блок (dc в начале, ac в конце)</returns>
        private static short[] DctBlockConvertion(short[] block, int dcCoef)
        {
            //нужно первые два коэффициента записывать отдельно
            var subBlocks = AccessoryFunc.DivideSequence(block, 8);
            var convertedSubBlocksDc = new List<short[]>();
            var convertedSubBlocksAc = new List<short[]>();

            foreach (var convertedSubBlock in subBlocks.Select(AlgorithmDCT.Convert))
            {
                if (convertedSubBlock == null) continue;
                var dc = new short[dcCoef];
                var ac = new short[convertedSubBlock.Length - dcCoef];
                Array.Copy(convertedSubBlock, dc, dcCoef);
                Array.Copy(convertedSubBlock, dcCoef, ac, 0, convertedSubBlock.Length - dcCoef);
                convertedSubBlocksDc.Add(dc);
                convertedSubBlocksAc.Add(ac);
            }

            //объединить, dc записав вначале
            var convertedBlockDc = AccessoryFunc.ConcatSequence(convertedSubBlocksDc);
            var convertedBlockAc = AccessoryFunc.ConcatSequence(convertedSubBlocksAc);
            var convertedBlock = new short[convertedBlockDc.Length + convertedBlockAc.Length];
            Array.Copy(convertedBlockDc, convertedBlock, convertedBlockDc.Length);
            Array.Copy(convertedBlockAc, 0, convertedBlock, convertedBlockDc.Length, convertedBlockAc.Length);

            return convertedBlock;
        }

        /// <summary>
        /// Dct преобразование
        /// </summary>
        /// <param name="blockSizeStr">Размер блока</param>
        /// <param name="koefCount">Число коэффициентов которые надо сохранить</param>
        /// <param name="koefVector">Вектор коэффициентов</param>
        /// <param name="dcCoef">8 коэффициентов dct преобразования делятся на две группы DC и AC. Параметр показывает сколько DC.</param>
        /// <returns></returns>
        public static List<short[]> ConvertDct(int blockSize, int koefCount, double[] koefVector, out List<Statistic> stat, int dcCoef)
        {
            SequenceSourse = SequenceSourse.ToList().GetRange(0, (int)(SequenceSourse.Length / blockSize) * blockSize).ToArray();

            //размер блока должен быть кратен 8
            if (blockSize % 8 != 0)
                throw new ArgumentException("Block size must divide on 8");

            stat = new List<Statistic>();

            //задание параметров преобразования
            AlgorithmDCT.KoefCount = koefCount;
            AlgorithmDCT.VectorKoef = koefVector;

            var blocks = AccessoryFunc.DivideSequence(SequenceSourse, blockSize);
            var convertedBlocks = new List<short[]>();

            foreach (var block in blocks)
            {
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();

                //деление на маленькие блоки по 8, их преобразование и склеивание обратно
                var convertedBlock = DctBlockConvertion(block, dcCoef);
                convertedBlocks.Add(convertedBlock);

                //остановка замера времени и подсчет статистики
                swatch.Stop();
                var blockStat = new Statistic
                {
                    BlockSourseSize = blockSize * 2,
                    BlockRezultSize = convertedBlock.Length * 2,
                    BlockRezultLength = blockSize,
                    Time = swatch.Elapsed,
                    Number = stat.Count,
                    Title = stat.Count.ToString()
                };
                //blockStat.CompressionRatio = blockStat.BlockSourseSize / (float)blockStat.BlockRezultSize;
                stat.Add(blockStat);
            }

            //результат преобразования
            return convertedBlocks;
        }

        /// <summary>
        /// Преобразование из коэффициентов DCT в SequenceSmoothed
        /// </summary>
        public static short[] DeconvertDct(List<short[]> convertedSequence, int blockSizeStr, int dcCoef)
        {
            // длина блока возможно короче, т.к. не все коэффициенты сохранены
            //var blocks = DivideSequence(convertedSequence, (int)(int.Parse(blockSizeStr) * AlgorithmDCT.KoefCount / (float)8 ));
            var deconvertedBlocks =
                convertedSequence.Select(block => DctBlockDeconvertion(block, dcCoef, AlgorithmDCT.KoefCount)).ToList();

            return AccessoryFunc.ConcatSequence(deconvertedBlocks);
        }

        /// <summary>
        /// Деление на две (обычно неравные части), содержащие ас и dc коэф. соответственно
        /// </summary>
        /// <param name="blocks"></param>
        /// <param name="countAll"></param>
        /// <param name="countDc">Число dc коэффициентов</param>
        public static List<List<short[]>> DivideOnDcAc(List<short[]> blocks, int blockSize, int countAll = 4, int countDc = 1)
        {
            var dcBlocks = new List<short[]>();
            var acBlocks = new List<short[]>();
            var countAc = countAll - countDc;

            foreach (var b in blocks)
            {
                var dcMiniBlocks = new List<short[]>();
                var acMiniBlocks = new List<short[]>();

                for (int i = 0; i < b.Length; i += countAll)
                {
                    var dc = new short[countDc];
                    var ac = new short[countAll - countDc];

                    Array.Copy(b,i,dc,0,countDc);
                    Array.Copy(b,i+countDc,ac,0,countAc);

                    dcMiniBlocks.Add(dc);
                    acMiniBlocks.Add(ac);
                }

                dcBlocks.Add(AccessoryFunc.ConcatSequence(dcMiniBlocks));
                acBlocks.Add(AccessoryFunc.ConcatSequence(acMiniBlocks));
            }

            DcBlocks = dcBlocks;
            AcBlocks = acBlocks;
            return new List<List<short[]>> { dcBlocks, acBlocks };
        }


        public static List<short[]> ConcatDcAc(List<short[]> DC, List<short[]> AC, int countAll = 4, int countDc = 1)
        {
            var countAc = countAll - countDc;

            if (DC.Count != AC.Count)
                throw new ArgumentException("Ac count != Dc count");

            var concatList = new List<short[]>();

            for (int k = 0; k < AC.Count; k++)
            {
                var dcCoeffs = DC[k];
                var acCoeffs = AC[k];

                var dcCoeffList = new List<short[]>();
                var acCoeffList = new List<short[]>();

                for (int i = 0; i < dcCoeffs.Length; i += countDc)
                {
                    var dc = new short[countDc];
                    Array.Copy(dcCoeffs, i, dc, 0, countDc);
                    dcCoeffList.Add(dc);
                }

                for (int j = 0; j < acCoeffs.Length; j += countAc)
                {
                    var ac = new short[countAc];
                    Array.Copy(acCoeffs, j, ac, 0, countAc);
                    acCoeffList.Add(ac);
                }

                var blockAcDc = new short[countAll*acCoeffList.Count];
                for (int i = 0, j = 0; i < acCoeffList.Count; i++, j+=countAll)
                {
                    var all = new short[countAll]; //ac+dc
                    Array.Copy(dcCoeffList[i], 0, all, 0, countDc);
                    Array.Copy(acCoeffList[i], 0, all, countDc, countAc);

                    Array.Copy(all, 0, blockAcDc, j, countAll);
                }

                concatList.Add(blockAcDc);
            }
            return concatList;
        }


        public static List<Statistic> EncodeRiseNew(List<short[]> data, out List<byte[]> encodedBlocks)
        {
            encodedBlocks = new List<byte[]>();
            var statList = new List<Statistic>();

            foreach (var block in data)
            {
                var stat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();

                var encodedBlock = AlgorithmRise.Encode(block);
                encodedBlocks.Add(encodedBlock);

                //остановка замера времени и подсчет статистики
                swatch.Stop();
                stat.Time = swatch.Elapsed;
                stat.BlockRezultSize = encodedBlock.Length;
                stat.BlockSourseSize = block.Length*2;

                statList.Add(stat);
            }
            return statList;
        }

        public static List<Statistic> EncodeHuffman(List<short[]> data, out List<byte[]> encodedBlocks)
        {
            encodedBlocks = new List<byte[]>();
            var statList = new List<Statistic>();

            foreach (var block in data)
            {
                var stat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();

                var encodedBlock = AlgorithmDynHuff.Encode(AccessoryFunc.ShortsToBytes(block));
                encodedBlocks.Add(encodedBlock);

                //остановка замера времени и подсчет статистики
                swatch.Stop();
                stat.Time = swatch.Elapsed;
                stat.BlockRezultSize = encodedBlock.Length;
                stat.BlockSourseSize = block.Length * 2;

                statList.Add(stat);
            }
            return statList;
        }

        public static List<short[]> DecodeHuffman(List<byte[]> encoded) => encoded.Select(block => AccessoryFunc.BytesToShorts(AlgorithmDynHuff.Decode(block))).ToList();
 

        public static List<short[]> DecodeRiseNew(List<byte[]> encoded) => encoded.Select(AlgorithmRise.Decode).ToList();

        public static List<Statistic> EncodeRleNew(List<short[]> data, out List<short[]> encodedBlocks)
        {
            encodedBlocks = new List<short[]>();
            var statList = new List<Statistic>();

            foreach (var block in data)
            {
                var stat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();

                var encodedBlock = AlgorithmRLE.Encode(block);
                encodedBlocks.Add(encodedBlock);

                //остановка замера времени и подсчет статистики
                swatch.Stop();
                stat.Time = swatch.Elapsed;
                stat.BlockRezultSize = encodedBlock.Length;
                stat.BlockSourseSize = block.Length * 2;

                statList.Add(stat);
            }
            return statList;
        }

        public static List<short[]> DecodeRleNew(List<short[]> encoded) => encoded.Select(AlgorithmRLE.Decode).ToList();

        public static List<Statistic> EncodeRleNew(List<byte[]> data, out List<byte[]> encodedBlocks)
        {
            encodedBlocks = new List<byte[]>();
            var statList = new List<Statistic>();

            foreach (var block in data)
            {
                var stat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();

                var encodedBlock = AlgorithmRLE.Encode(block);
                encodedBlocks.Add(encodedBlock);

                //остановка замера времени и подсчет статистики
                swatch.Stop();
                stat.Time = swatch.Elapsed;
                stat.BlockRezultSize = encodedBlock.Length;
                stat.BlockSourseSize = block.Length * 2;

                statList.Add(stat);
            }
            return statList;
        }

        public static List<byte[]> DecodeRleNew(List<byte[]> encoded) => encoded.Select(AlgorithmRLE.Decode).ToList();



        #region Алгоритм Райса

        /// <summary>
        /// Алгоритм блочного кодирования алгоритмом Райса
        /// </summary>
        /// <param name="data">Данные предназначенные для кодирования</param>
        /// <param name="encodedBlocks">Закодированные данные (результат)</param>
        /// <param name="dcCoef">Число DC коэффициентов</param>
        /// <param name="statistic">Статистика</param>
        /// <param name="secondEncoding">
        /// Тип следующего уровня кодирования: 0 - без кодирования, 1 - кодирование dc алгоритмом rle
        /// </param>
        /// <returns></returns>
        public static List<Statistic> EncodeRise(List<short[]> data, out List<byte[]> encodedBlocks,
            string dcCoef, List<Statistic> statistic, int secondEncoding)
        {
            //var blocks = DivideSequence(data, (int)(int.Parse(blockSizeStr) * AlgorithmDCT.KoefCount / (float)8));
            var blocks = data;
            encodedBlocks = new List<byte[]>();
            var stat = statistic.ToArray();
            int i = 0;

            //т.к. алгоритм для работы с dct, dc надо кодировать отдельно от ас
            foreach (var block in blocks)
            {
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();

                var blockDc = new short[(int)(block.Length * int.Parse(dcCoef) / (float)AlgorithmDCT.KoefCount)];
                var blockAc = new short[(int)(block.Length * (AlgorithmDCT.KoefCount - int.Parse(dcCoef)) / (float)AlgorithmDCT.KoefCount)];
                Array.Copy(block, blockDc, blockDc.Length);
                Array.Copy(block, blockDc.Length, blockAc, 0, blockAc.Length);

                var encodedDc = new byte[0];
                var encodedAc = new byte[0];

                //выбор второго алгоритма
                if (secondEncoding == 0)
                {
                    encodedDc = AlgorithmRise.Encode(blockDc);
                    encodedAc = AlgorithmRise.Encode(blockAc);
                }
                else if (secondEncoding == 1)
                {
                    encodedDc = AlgorithmRise.Encode(blockDc);
                    encodedAc = AlgorithmRise.Encode(AlgorithmRLE.Encode(blockAc));
                }

                var dcLen = BitConverter.GetBytes((short)encodedDc.Length);
                var acLen = BitConverter.GetBytes((short)encodedAc.Length);

                var encodedBlock = new byte[encodedDc.Length + encodedAc.Length + dcLen.Length + acLen.Length];
                Array.Copy(dcLen, encodedBlock, dcLen.Length);
                Array.Copy(encodedDc, 0, encodedBlock, dcLen.Length, encodedDc.Length);
                Array.Copy(acLen, 0, encodedBlock, dcLen.Length + encodedDc.Length, acLen.Length);
                Array.Copy(encodedAc, 0, encodedBlock, dcLen.Length + encodedDc.Length + acLen.Length, encodedAc.Length);

                encodedBlocks.Add(encodedBlock);
                //encodedBlocks.Add(AlgorithmRise.Encode(block));

                //остановка замера времени и подсчет статистики
                swatch.Stop();
                stat[i].Time += swatch.Elapsed;
                stat[i].BlockRezultSize = encodedBlock.Length;
                //stat[i].CompressionRatio = stat[i].BlockSourseSize / (float)stat[i].BlockRezultSize;
                i++;
            }

            return stat.ToList();
        }

        public static void DecodeRise(List<byte[]> encodedBlocks, out List<short[]> decodedData, int secondEncoding)
        {
            var decodedBlocks = new List<short[]>();
            foreach (var encBlock in encodedBlocks)
            {
                var dcLen = BitConverter.ToInt16(encBlock, 0);
                var encodedDc = new byte[dcLen];
                Array.Copy(encBlock, 2, encodedDc, 0, dcLen);

                var acLen = BitConverter.ToInt16(encBlock, 2 + dcLen);
                var encodedAc = new byte[acLen];
                Array.Copy(encBlock, 4 + dcLen, encodedAc, 0, acLen);

                var decodedDc = new short[0];
                var decodedAc = new short[0];

                //выбор декодирования
                if (secondEncoding == 0)
                {
                    decodedDc = AlgorithmRise.Decode(encodedDc);
                    decodedAc = AlgorithmRise.Decode(encodedAc);
                }
                else if (secondEncoding == 1)
                {
                    decodedDc = AlgorithmRise.Decode(encodedDc);
                    decodedAc = AlgorithmRise.Decode(AlgorithmRLE.Decode(encodedAc));
                }

                var decodedBlock = new short[decodedDc.Length + decodedAc.Length];
                Array.Copy(decodedDc, decodedBlock, decodedDc.Length);
                Array.Copy(decodedAc, 0, decodedBlock, decodedDc.Length, decodedAc.Length);

                decodedBlocks.Add(decodedBlock);
            }
            decodedData = decodedBlocks;
        }

        #endregion
    }
}
