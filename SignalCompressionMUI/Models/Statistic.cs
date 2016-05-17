using System;

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
    }
}
