using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SignalCompressionMUI.Models.Algorithms
{
    public class AlgorithmRise
    {
        /// <summary>
        /// Утеря знаков в исходном массиве!
        /// </summary>
        public static byte[] Encode(short[] input)
        {
            //выделяется отдельный бит для знака, так как это эффективнее чем увеличивать число до положительного
            //k запишем в начало выходного массива

            var max = input.Max() > -input.Min() ? input.Max() : -input.Min();
            var k = (short)(Math.Log(max) / Math.Log(2));
            if (k == 0) k = 1;

            var bitList = new List<BitArray>(); //список для чисел в сжатом битовом представлении
            var resultLen = 0;
            for (int ind = 0; ind < input.Length; ind++)
            {
                var positive = true;
                if (input[ind] < 0)
                {
                    input[ind] = (short)-input[ind];
                    positive = false;
                }

                var r = (short)(input[ind] % ((int)(Math.Pow(2, k))));
                var q = (short)((input[ind] - r) / ((int)(Math.Pow(2, k))));
                var num = new BitArray(q + 2 + k);

                int j = 0;
                byte[] rByte = BitConverter.GetBytes(r); //остаток в битовый массив, т.к. незначащие нули не нужны
                BitArray bitsArray = new BitArray(rByte); //в инвертированном виде, т.к. так хранится в памяти
                for (int i = 0; i < num.Length; i++)
                {
                    if (i < q) num[i] = false;
                    else if (i == q) num[i] = true;
                    else if (i == q + 1) num[i] = positive;
                    else num[i] = bitsArray[j++];
                }
                bitList.Add(num);
                resultLen += num.Length;
            }

            //неплохо бы как-нибудь оптимизировать
            var result = new BitArray(resultLen + 16);
            var kInBit = new BitArray(BitConverter.GetBytes(k));
            int j1 = 0;
            for (int i = 0; i < 16; i++)
                result[j1++] = kInBit[i];

            foreach (var num in bitList)
            {
                for (int i = 0; i < num.Length; i++)
                    result[j1++] = num[i];
            }
            return ConvertToByteArray(result);
        }

        public static byte[] Encode(byte[] input)
        {
            //выделяется отдельный бит для знака, так как это эффективнее чем увеличивать число до положительного
            //k запишем в начало выходного массива

            var max = input.Max() > -input.Min() ? input.Max() : -input.Min();
            var k = (short)(Math.Log(max) / Math.Log(2));
            if (k == 0) k = 1;

            var bitList = new List<BitArray>(); //список для чисел в сжатом битовом представлении
            var resultLen = 0;
            for (int ind = 0; ind < input.Length; ind++)
            {
                var positive = true;
                if (input[ind] < 0)
                {
                    input[ind] = (byte)-input[ind];
                    positive = false;
                }

                var r = (short)(input[ind] % ((int)(Math.Pow(2, k))));
                var q = (short)((input[ind] - r) / ((int)(Math.Pow(2, k))));
                var num = new BitArray(q + 2 + k);

                int j = 0;
                byte[] rByte = BitConverter.GetBytes(r); //остаток в битовый массив, т.к. незначащие нули не нужны
                BitArray bitsArray = new BitArray(rByte); //в инвертированном виде, т.к. так хранится в памяти
                for (int i = 0; i < num.Length; i++)
                {
                    if (i < q) num[i] = false;
                    else if (i == q) num[i] = true;
                    else if (i == q + 1) num[i] = positive;
                    else num[i] = bitsArray[j++];
                }
                bitList.Add(num);
                resultLen += num.Length;
            }

            //неплохо бы как-нибудь оптимизировать
            var result = new BitArray(resultLen + 16);
            var kInBit = new BitArray(BitConverter.GetBytes(k));
            int j1 = 0;
            for (int i = 0; i < 16; i++)
                result[j1++] = kInBit[i];

            foreach (var num in bitList)
            {
                for (int i = 0; i < num.Length; i++)
                    result[j1++] = num[i];
            }
            return ConvertToByteArray(result);
        }

        private static byte[] ConvertToByteArray(BitArray bits)
        {
            var len = bits.Count % 8 == 0 ? bits.Count / 8 : bits.Count / 8 + 1;
            var bytes = new byte[len];
            bits.CopyTo(bytes, 0);
            return bytes;
        }

        public static short[] Decode(byte[] data)
        {
            //извлечем k
            var k = BitConverter.ToInt16(data, 0);
            var data2 = new byte[data.Length - 2];
            Array.Copy(data, 2, data2, 0, data2.Length);
            var input = new BitArray(data2);

            var numList = new List<short>();
            int qDec = 0; //q декодированного числа
            bool before1 = true; //находимся до или после разделителя
            int indAfter = 0; //сколько считали после разделителя
            var residue = new BitArray(k); //остаток, в битах
            bool positive = true;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == false && before1)
                    qDec++;
                else if (input[i] && before1) //дошли до разделителя
                    before1 = false;
                else if (!before1 && indAfter == 0) //знаковый бит
                {
                    positive = input[i];
                    indAfter++;
                }
                else if (!before1 && indAfter <= k) //считываем остаток
                {
                    residue[indAfter++ - 1] = input[i];
                    if (indAfter == k + 1)
                    {
                        //записываем число
                        var residueByte = new BitArray(16); //остаток в 2 байта переведем
                        for (int g = 0; g < residue.Length; g++)
                            residueByte[g] = residue[g];
                        var bytes = new byte[2];
                        residueByte.CopyTo(bytes, 0);
                        short rDec = BitConverter.ToInt16(bytes, 0);

                        numList.Add(positive
                            ? (short)(qDec * Math.Pow(2, k) + rDec)
                            : (short)(-(qDec * Math.Pow(2, k) + rDec)));

                        before1 = true;
                        indAfter = 0;
                        qDec = 0;
                    }
                }
            }

            return numList.ToArray();
        }

        public static byte[] DecodeByte(byte[] data)
        {
            //извлечем k
            var k = BitConverter.ToInt16(data, 0);
            var data2 = new byte[data.Length - 2];
            Array.Copy(data, 2, data2, 0, data2.Length);
            var input = new BitArray(data2);

            var numList = new List<byte>();
            int qDec = 0; //q декодированного числа
            bool before1 = true; //находимся до или после разделителя
            int indAfter = 0; //сколько считали после разделителя
            var residue = new BitArray(k); //остаток, в битах
            bool positive = true;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == false && before1)
                    qDec++;
                else if (input[i] && before1) //дошли до разделителя
                    before1 = false;
                else if (!before1 && indAfter == 0) //знаковый бит
                {
                    positive = input[i];
                    indAfter++;
                }
                else if (!before1 && indAfter <= k) //считываем остаток
                {
                    residue[indAfter++ - 1] = input[i];
                    if (indAfter == k + 1)
                    {
                        //записываем число
                        var residueByte = new BitArray(16); //остаток в 2 байта переведем
                        for (int g = 0; g < residue.Length; g++)
                            residueByte[g] = residue[g];
                        var bytes = new byte[2];
                        residueByte.CopyTo(bytes, 0);
                        short rDec = BitConverter.ToInt16(bytes, 0);

                        numList.Add(positive
                            ? (byte)(qDec * Math.Pow(2, k) + rDec)
                            : (byte)(-(qDec * Math.Pow(2, k) + rDec)));

                        before1 = true;
                        indAfter = 0;
                        qDec = 0;
                    }
                }
            }

            return numList.ToArray();
        }
    }
}
