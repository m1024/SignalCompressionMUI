using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalCompressionMUI.Models
{
    public struct Statistic
    {
        public TimeSpan Time { get; set; }
        public int BlockSourseSize { get; set; }
        public int BlockRezultLength { get; set; }
        public int BlockRezultSize { get; set; }
        public ulong Additions { get; set; }
        public ulong Multiplications { get; set; }
        public float CompressionRatio => (float)BlockSourseSize/BlockRezultSize;
        public int RecursiveCalls { get; set; }
        public long Error { get; set; }

        public static Statistic operator +(Statistic s1, Statistic s2)
        {
            return new Statistic()
            {
                Time = s1.Time + s2.Time,
                BlockSourseSize = s1.BlockSourseSize + s2.BlockSourseSize,
                BlockRezultSize = s1.BlockRezultSize + s2.BlockRezultSize,
                BlockRezultLength = s1.BlockRezultLength + s2.BlockRezultLength,
                Additions = s1.Additions + s2.Additions,
                Multiplications = s1.Multiplications + s2.Multiplications,
                RecursiveCalls = s1.RecursiveCalls + s2.RecursiveCalls,
            };
        }

        public static Statistic CalculateTotal(List<Statistic> stat)
        {
            var total = new Statistic();
            foreach (var s in stat)
            {
                total.Time += s.Time;
                total.BlockRezultSize += s.BlockRezultSize;
                total.BlockSourseSize += s.BlockSourseSize;
                total.RecursiveCalls += s.RecursiveCalls;
                total.Error += s.Error;
            }
            return total;
        }

        public static void CalculateError(short[] sourse, short[] smooth, ref List<Statistic> stat, int blokSize)
        {
            var sourseBlocks = AccessoryFunc.DivideSequence(sourse, blokSize);
            var smoothBlocks = AccessoryFunc.DivideSequence(smooth, blokSize);

            //выбираем наименьшую длину
            int len = sourseBlocks.Count < smoothBlocks.Count ? sourseBlocks.Count : smoothBlocks.Count;
            len = stat.Count < len ? stat.Count : len;

            var statArr = stat.ToArray();

            for (int i = 0; i < len; i++)
            {
                long error = 0;
                for (int j = 0; j < blokSize; j++)
                {
                    error += Math.Abs(sourseBlocks[i][j] - smoothBlocks[i][j]);
                }
                statArr[i].Error = error;
            }

            stat = statArr.ToList();
        }
    }
}
