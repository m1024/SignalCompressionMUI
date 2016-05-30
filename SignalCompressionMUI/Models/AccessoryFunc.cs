using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media.Animation;
using SignalCompressionMUI.Models.Algorithms;
using SignalCompressionMUI.ViewModels;

namespace SignalCompressionMUI.Models
{
    public static class AccessoryFunc
    {
        /// <summary>
        /// Чтение из файла
        /// </summary>
        public static short[] ReadAndPrepare(string fileName)
        {
            string file = File.ReadAllText(fileName);
            var numbers = file.Split(new char[] { '\n', '\r' }).ToList();
            var numDoublesList = new List<double>();

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            foreach (var number in numbers)
            {
                double d;
                if (Double.TryParse(number, out d))
                    numDoublesList.Add(d);
            }

            double maxValue = numDoublesList.Max();
            double minValue = numDoublesList.Min();
            double k = 8192 / (maxValue - minValue);

            short[] numbersInt = new short[numDoublesList.Count];
            for (int i = 0; i < numbersInt.Length; i++)
                numbersInt[i] = (System.Convert.ToInt16(numDoublesList.ElementAt(i) * k));

            //textBoxSourseSize.Text = String.Format("{0:0,0}", numbersInt.Length * 2);

            return numbersInt;
        }

        /// <summary>
        /// Запись в файл
        /// </summary>
        public static void WriteFile(string fileName, byte[] data)
        {
            File.WriteAllBytes(fileName, data);
        }

        public static byte[] ReadFile(string fileName)
        {
            return File.ReadAllBytes(fileName);
        }

        /// <summary>
        /// Разбиение последовательности на блоки
        /// </summary>
        public static List<short[]> DivideSequence(short[] sequense, int blockSize)
        {
            var rez = new List<short[]>();
            for (int i = 0; i < sequense.Length; i += blockSize)
            {
                if (i + blockSize > sequense.Length)
                    blockSize = sequense.Length - i; //т.к. последний блок может быть короче
                var block = new short[blockSize];
                Array.Copy(sequense, i, block, 0, blockSize);
                rez.Add(block);
            }
            return rez;
        }

        /// <summary>
        /// Разбиение последовательности на блоки
        /// </summary>
        public static List<MyPoint[]> DivideSequence(MyPoint[] sequense, int blockSize)
        {
            var rez = new List<MyPoint[]>();
            for (int i = 0; i < sequense.Length; i += blockSize)
            {
                if (i + blockSize > sequense.Length)
                    blockSize = sequense.Length - i; //т.к. последний блок может быть короче
                var block = new MyPoint[blockSize];
                Array.Copy(sequense, i, block, 0, blockSize);
                rez.Add(block);
            }
            return rez;
        }

        /// <summary>
        /// Слияние блоков
        /// </summary>
        public static short[] ConcatSequence(List<short[]> sequence)
        {
            if (sequence.Count <= 1) return sequence.FirstOrDefault();
            if (sequence.Last() == null) sequence.RemoveAt(sequence.Count - 1); //в некоторых случаях такое возможно
            int length = sequence.First().Length * (sequence.Count - 1) + sequence.Last().Length;
            var rez = new short[length];
            int i = 0;
            foreach (var block in sequence)
            {
                Array.Copy(block, 0, rez, i, block.Length);
                i += block.Length;
            }
            return rez;
        }

        public static byte[] ConcatSequence(List<byte[]> sequence)
        {
            if (sequence.Count <= 1) return sequence.FirstOrDefault();
            if (sequence.Last() == null) sequence.RemoveAt(sequence.Count - 1); //в некоторых случаях такое возможно
            int length = sequence.First().Length * (sequence.Count - 1) + sequence.Last().Length;
            var rez = new byte[length];
            int i = 0;
            foreach (var block in sequence)
            {
                Array.Copy(block, 0, rez, i, block.Length);
                i += block.Length;
            }
            return rez;
        }

        public static byte[] ConcatSequence(List<List<byte[]>> sequence)
        {
            var res = new List<byte>();
            foreach (var b in sequence.Select(ConcatSequence))
                res.AddRange(b);
            return res.ToArray();
        }

        public static byte[] CreateForSavingDCT(List<List<byte[]>> data, CompressType type, int leaveCoef, int dcCount, double[] vector)
        {
            var outbytes = new List<byte>();

            outbytes.Add((byte)type); //тип сжатия
            outbytes.Add((byte)leaveCoef);
            outbytes.Add((byte)dcCount);
            outbytes.Add((byte)vector.Length);
            foreach (var t in vector)
                outbytes.AddRange(BitConverter.GetBytes((short)t));
            outbytes.Add((byte) data.Count);

            foreach (var block in data)
            {
                var subblocksCount = BitConverter.GetBytes((short)block.Count);
                outbytes.AddRange(subblocksCount);

                foreach (var subblock in block)
                {
                    var numsCount = BitConverter.GetBytes((short)subblock.Length);
                    outbytes.AddRange(numsCount);
                    outbytes.AddRange(subblock);
                }
            }

            return outbytes.ToArray();
        }

        public static List<List<byte[]>> CreateFromSavingDCT(byte[] data, out CompressType type, out int leaveCoef, out int dcCount, out double[] vector)
        {
            int index = 0;
            type = (CompressType)data[index++];
            leaveCoef = data[index++];
            dcCount = data[index++];
            var len = data[index++];
            vector = new double[len];
            for (int i = 0; i < len; i++)
            {
                vector[i] = BitConverter.ToInt16(data, index);
                index += 2;
            }
            var blocksCount = data[index++];

            var sequence = new List<List<byte[]>>();
            for (int i = 0; i < blocksCount; i++)
            {
                var block = new List<byte[]>();
                var subblocksCount = BitConverter.ToInt16(data, index);
                index += 2;

                for (int j = 0; j < subblocksCount; j++)
                {
                    var numsCount = BitConverter.ToInt16(data, index);
                    index += 2;

                    var subblock = new byte[numsCount];
                    Array.Copy(data, index, subblock, 0, numsCount);
                    index += numsCount;

                    block.Add(subblock);
                }
                sequence.Add(block);
            }

            return sequence;
        }

        public static byte[] CreateForSaving(List<List<byte[]>> sequence, CompressType type)
        {
            var outbytes = new List<byte>();

            //еще параметры которые использовались при конвертации надо добавить

            outbytes.Add((byte)type); //тип сжатия
            //в начало каждого списка и массива надо добавить его длину, причем в два байта и тип сжатия который использовался
            var blocksCount = BitConverter.GetBytes((short)sequence.Count);
            outbytes.AddRange(blocksCount);

            foreach (var block in sequence)
            {
                var subblocksCount = BitConverter.GetBytes((short) block.Count);
                outbytes.AddRange(subblocksCount);

                foreach (var subblock in block)
                {
                    var numsCount = BitConverter.GetBytes((short) subblock.Length);
                    outbytes.AddRange(numsCount);
                    outbytes.AddRange(subblock);
                }
            }

            return outbytes.ToArray();
        }

        public static List<List<byte[]>> CreateFromSaving(byte[] data, out CompressType type)
        {
            type = (CompressType) data[0];
            var blocksCount = BitConverter.ToInt16(data, 1);
            int index = 3;

            var sequence = new List<List<byte[]>>();
            for (int i = 0; i < blocksCount; i++)
            {
                var block = new List<byte[]>();
                var subblocksCount = BitConverter.ToInt16(data, index);
                index += 2;

                for (int j = 0; j < subblocksCount; j++)
                {
                    var numsCount = BitConverter.ToInt16(data, index);
                    index += 2;

                    var subblock = new byte[numsCount];
                    Array.Copy(data, index, subblock, 0, numsCount);
                    index += numsCount;

                    block.Add(subblock);
                }

                sequence.Add(block);
            }

            return sequence;
        } 

        public static List<byte[]> ShortsToBytes(List<short[]> data) => data.Select(ShortsToBytes).ToList();

        public static List<short[]> BytesToShorts(List<byte[]> data) => data.Select(BytesToShorts).ToList(); 

        public static byte[] ShortsToBytes(short[] data)
        {
            var bytes = new byte[data.Length*2];
            for (int i = 0, j=0; i < data.Length; i++, j+=2)
                FromShort(data[i], out bytes[j], out bytes[j+1]);
            return bytes;
        }

        public static short[] BytesToShorts(byte[] data)
        {
            var shorts = new short[data.Length/2];
            for (int i = 0, j = 0; i < data.Length - 1; i += 2, j++)
                shorts[j] = ToShort(data[i], data[i + 1]);
            return shorts;
        }

        private static short ToShort(short byte1, short byte2)
        {
            return (short)((byte2 << 8) + byte1);
        }

        private static void FromShort(short number, out byte byte1, out byte byte2)
        {
            byte2 = (byte)(number >> 8);
            byte1 = (byte)(number & 255);
        }
    }
}
