using System;
using System.Collections.Generic;
using System.Linq;
using SignalCompressionMUI.Models.Algorithms;

namespace SignalCompressionMUI.Models
{
    class RDPModel
    {
        private static MyPoint[] _pRez;
        public static short[] SequenceSmoothed;
        public static short[] SequenceSourse;
        public static int MaxDeviationInd;
        public static int MinDeviationInd;

        public List<Statistic> Stat = new List<Statistic>();

        public void Read(string path)
        {
            SequenceSourse = AccessoryFunc.ReadAndPrepare(path);
        }

        /// <summary>
        /// Разбиение последовательности на части и ее преобразование с сохранением данных статистики
        /// </summary>
        public void ConvertRdp(string blockSizeStr, string epsilon)
        {
            AlgorithmRDP.Additions = 0;
            AlgorithmRDP.Multiplications = 0;

            MyPoint[] points = new MyPoint[SequenceSourse.Length];

            for (int i = 0; i < SequenceSourse.Length; i++)
            {
                points[i].X = i;
                points[i].Y = SequenceSourse[i];
            }

            int blockSize = int.Parse(blockSizeStr);
            List<MyPoint[]> smoothSequence = new List<MyPoint[]>();
            Stat = new List<Statistic>();

            int smooothSeqLength = 0;
            int j = 0;

            for (int i = 0; i < points.Length; i += blockSize)
            {
                System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch(); // создаем объект
                Statistic blockStat = new Statistic
                {
                    Additions = AlgorithmRDP.Additions,
                    Multiplications = AlgorithmRDP.Multiplications
                };
                AlgorithmRDP.MaxCalls = 0;
                AlgorithmRDP.Calls = 0;
                swatch.Start(); // старт

                smoothSequence.Add(AlgorithmRDP.DouglasPeucker(points, i,
                    (i + blockSize < points.Length) ? i + blockSize : points.Length - 1,
                    (epsilon == "") ? 1 : (float)Math.Pow(float.Parse(epsilon), 2)));

                smooothSeqLength += smoothSequence.ElementAt(j++).Length - 1;
                //-1 чтобы убрать последний элемент, т.к. он будет началом следующего блока

                swatch.Stop(); // стоп

                //сохраняем статситку
                blockStat.Time = swatch.Elapsed;
                blockStat.BlockRezultLength = smoothSequence.ElementAt(i / blockSize).Length;
                blockStat.BlockRezultSize = blockStat.BlockRezultLength * 3;
                blockStat.BlockSourseSize = blockSize * 2;
                blockStat.Additions = AlgorithmRDP.Additions - blockStat.Additions;
                blockStat.Multiplications = AlgorithmRDP.Multiplications - blockStat.Multiplications;
                blockStat.CompressionRatio =
                    (float)Math.Round(blockSize / (smoothSequence.ElementAt(i / blockSize).Length * 1.5), 3);
                blockStat.RecursiveCalls = AlgorithmRDP.MaxCalls;

                Stat.Add(blockStat);
            }

            _pRez = new MyPoint[smooothSeqLength + 1];
            j = 0;
            foreach (var block in smoothSequence)
            {
                for (int i = 0; i < block.Length - 1; i++)
                    _pRez[j++] = block[i];
            }
            _pRez[j] = smoothSequence.Last().Last();

            //textBoxKmin.Text = stat.OrderBy(u => u.СompressionRatio).First().СompressionRatio.ToString();
            //textBoxKmax.Text = stat.OrderByDescending(u => u.СompressionRatio).First().СompressionRatio.ToString();
            //textBoxRecursiveCalls.Text = stat.OrderByDescending(u => u.RecursiveCalls).First().RecursiveCalls.ToString();
        }

        /// <summary>
        /// Преобразование сжатой последовательности обратно в обычную
        /// </summary>
        public void DeconvertRdp()
        {
            SequenceSmoothed = new short[_pRez[_pRez.Length - 1].X + 1];

            SequenceSmoothed[0] = _pRez[0].Y;
            var n = 1;
            for (int i = 1; i < _pRez.Length && n < SequenceSmoothed.Length; i++)
            {
                if (_pRez[i].X - _pRez[i - 1].X > 1)
                {
                    var deltaX = _pRez[i].X - _pRez[i - 1].X;
                    var deltaY = _pRez[i].Y - _pRez[i - 1].Y;

                    for (int j = 1; j < (_pRez[i].X - _pRez[i - 1].X) && n < SequenceSmoothed.Length; j++)
                        SequenceSmoothed[n++] = (short)((double)deltaY / deltaX * j + _pRez[i - 1].Y);
                    SequenceSmoothed[n++] = _pRez[i].Y;
                }
                else
                {
                    SequenceSmoothed[n++] = _pRez[i].Y;
                }
            }
        }
    }
}
