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
        public static List<short[]> DivideSequence(int blockSize, short[] SequenceSourse)
        {
            var rez = new List<short[]>();
            for (int i = 0; i < SequenceSourse.Length; i += blockSize)
            {
                if (i + blockSize > SequenceSourse.Length)
                    blockSize = SequenceSourse.Length - i; //т.к. последний блок может быть короче
                var block = new short[blockSize];
                Array.Copy(SequenceSourse, i, block, 0, blockSize);
                rez.Add(block);
            }
            return rez;
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
    }
}
