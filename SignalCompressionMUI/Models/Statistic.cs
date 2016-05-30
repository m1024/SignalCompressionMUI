using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SignalCompressionMUI.Models
{
    public struct Statistic
    {
        public int Number { get; set; }
        public string Title { get; set; }
        public TimeSpan Time { get; set; }
        public int BlockSourseSize { get; set; }
        public int BlockRezultLength { get; set; }
        public int BlockRezultSize { get; set; }
        public ulong Additions { get; set; }
        public ulong Multiplications { get; set; }
        public float CompressionRatio => (float)BlockSourseSize/BlockRezultSize;
        public int RecursiveCalls { get; set; }
        public double Error { get; set; }

        public static Statistic operator +(Statistic s1, Statistic s2)
        {
            return new Statistic()
            {
                Time = s1.Time + s2.Time,
                Title = s1.Title,
                Number = s1.Number,
                BlockSourseSize = s1.BlockSourseSize + s2.BlockSourseSize,
                BlockRezultSize = s1.BlockRezultSize + s2.BlockRezultSize,
                BlockRezultLength = s1.BlockRezultLength + s2.BlockRezultLength,
                Additions = s1.Additions + s2.Additions,
                Multiplications = s1.Multiplications + s2.Multiplications,
                RecursiveCalls = s1.RecursiveCalls + s2.RecursiveCalls,
            };
        }

        public Statistic Clone() => new Statistic()
        {
            Number = Number,
            Additions = Additions,
            BlockRezultLength = BlockRezultLength,
            BlockRezultSize = BlockRezultSize,
            BlockSourseSize = BlockSourseSize,
            Error = Error,
            Multiplications = Multiplications,
            RecursiveCalls = RecursiveCalls,
            Time = Time,
            Title = Title
        };

        public static Statistic CalculateTotal(List<Statistic> stat)
        {
            var total = new Statistic();
            foreach (var s in stat)
            {
                total.Number = 0;
                total.Title = "Всего";
                total.Time += s.Time;
                total.BlockRezultSize += s.BlockRezultSize;
                total.BlockSourseSize += s.BlockSourseSize;
                total.RecursiveCalls += s.RecursiveCalls;
                total.Error += s.Error;
                total.Multiplications += s.Multiplications;
                total.Additions += s.Additions;
            }
            total.Error = Math.Round(total.Error/stat.Count, 5);
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
                double error = 0;
                long oldBlock = 0, newBlock = 0;
                int count = 0;
                for (int j = 0; j < blokSize; j++)
                {
                    if (j >= smoothBlocks[i].Length) continue;
                    //что делать если старое значение == 0?
                    if (sourseBlocks[i][j] == 0)
                    {
                        if (smoothBlocks[i][j] == 0)
                            count++;
                        continue;
                    }
                    error += Math.Abs((sourseBlocks[i][j] - smoothBlocks[i][j]) / sourseBlocks[i][j]);
                    count ++;
                    //oldBlock += Math.Abs(sourseBlocks[i][j]);
                    //newBlock += Math.Abs(smoothBlocks[i][j]);
                }
                //statArr[i].Error = ((double)(newBlock-oldBlock)/oldBlock)*100;
                statArr[i].Error = Math.Round((error/count) * 100, 5);
            }

            stat = statArr.ToList();
        }
    }
}
