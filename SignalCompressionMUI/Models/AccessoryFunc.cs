using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

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
