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
        public float CompressionRatio { get; set; }
        public int RecursiveCalls { get; set; }
    }
}
