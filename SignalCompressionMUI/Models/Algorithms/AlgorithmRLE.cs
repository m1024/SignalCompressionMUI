using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalCompressionMUI.Models.Algorithms
{
    public static class AlgorithmRLE
    {
        #region Преобразование byte<->short

        public static byte[] ShortToByte(short[] data)
        {
            var dataList = new List<byte>();
            foreach (var m in data.Select(BitConverter.GetBytes))
            {
                dataList.Add(m[0]);
                dataList.Add(m[1]);
            }
            return dataList.ToArray();
        }

        public static short[] ByteToShort(byte[] data)
        {
            var dataList = new List<short>();
            for (int i = 0; i < data.Length; i += 2)
            {
                var m = new byte[2];
                Array.Copy(data, i, m, 0, i + 2 > data.Length ? 1 : 2);
                dataList.Add(BitConverter.ToInt16(m, 0));

            }
            return dataList.ToArray();
        }

        #endregion

        #region Простое кодирование/декодирование

        public static byte[] EncodeSimple(byte[] data)
        {
            byte previous = data[0];
            var encoded = new List<byte>();
            byte count = 0;
            for (int i = 1; i < data.Length; i++)
            {
                if (previous != data[i] || count == 255 || i == data.Length - 1)
                {
                    encoded.Add((byte)(count + 1));
                    encoded.Add(previous);
                    previous = data[i];
                    count = 0;
                }
                else
                {
                    count++;
                }
            }
            encoded.Add((byte)(count + 1));
            encoded.Add(previous);
            return encoded.ToArray();
        }

        public static short[] EncodeSimple(short[] data)
        {
            short previous = data[0];
            var encoded = new List<short>();
            short count = 0;
            for (int i = 1; i < data.Length; i++)
            {
                if (previous != data[i] || count == short.MaxValue || i == data.Length - 1)
                {
                    encoded.Add((short)(count + 1));
                    encoded.Add(previous);
                    previous = data[i];
                    count = 0;
                }
                else
                {
                    count++;
                }
            }
            encoded.Add((short)(count + 1));
            encoded.Add(previous);
            return encoded.ToArray();
        }


        public static byte[] DecodeSimple(byte[] data)
        {
            var decoded = new List<byte>();
            for (int i = 0; i < data.Length; i += 2)
            {
                for (int j = 0; j < data[i]; j++)
                {
                    decoded.Add(data[i + 1]);
                }
            }
            return decoded.ToArray();
        }

        public static short[] DecodeSimple(short[] data)
        {
            var decoded = new List<short>();
            for (int i = 0; i < data.Length; i += 2)
            {
                for (int j = 0; j < data[i]; j++)
                {
                    decoded.Add(data[i + 1]);
                }
            }
            return decoded.ToArray();
        }

        #endregion

        #region Кодирование/декодирование с выделением бита под признак

        public static byte[] Encode(byte[] data)
        {
            byte previous = data[0];
            var encoded = new List<byte>();
            byte countSame = 0; //число повторяющихся, пишется на 1 меньше чем на самом деле, т.к. 0 не может быть
            byte countDiff = 0; //число неповторяющихся
            var index = 0;
            for (int i = 1; i < data.Length; i++)
            {
                if (previous != data[i] || countSame == 127)
                {
                    //окончание цикла одинаковых значений или достижение максимальной длины одинаковых
                    if (countSame > 0 || countSame == 127)
                    {
                        //кроме того, надо записать сколько было разных, старший бит = 1, 
                        //показывает что поcледовательность различных чисел
                        if (countDiff > 0)
                        {
                            encoded.Insert(index, (byte)(countDiff | 128));
                            countDiff = 0;
                        }

                        encoded.Add(countSame);
                        encoded.Add(previous);
                        previous = data[i];
                        countSame = 0;
                    }
                    //первый раз разные элементы, либо конец блока, надо запомнить индекс, записать потом
                    else if (countDiff == 0 || countDiff == 126)
                    {
                        //закончился блок разных элементов, перед этим был тоже блок разных, надо записать маркировочный байт
                        if (countDiff == 126)
                        {
                            encoded.Insert(index, (byte)(countDiff | 128));
                            countDiff = 0;
                        }

                        index = encoded.Count;
                        encoded.Add(previous);
                        previous = data[i];
                        countDiff++;
                    }
                    //просто идут разные элементы
                    else
                    {
                        encoded.Add(previous);
                        previous = data[i];
                        countDiff++;
                    }
                }
                else
                {
                    countSame++;
                }
            }

            //обработка последнего элемента (только не хватает обработки если последний элемент и есть 128й)
            if (countSame > 0)
            {
                if (countDiff > 0)
                    encoded.Insert(index, (byte)(countDiff | 128));

                encoded.Add(countSame);
                encoded.Add(previous);
            }
            else if (countDiff > 0)
            {
                encoded.Insert(index, (byte)(++countDiff | 128));
                encoded.Add(previous);
            }

            return encoded.ToArray();
        }

        public static List<short[]> Encode(List<short[]> data) => data.Select(Encode).ToList();

        public static short[] Encode(short[] data)
        {
            short previous = data[0];
            var encoded = new List<short>();
            short countSame = 0; //число повторяющихся, пишется на 1 меньше чем на самом деле, т.к. 0 не может быть
            short countDiff = 0; //число неповторяющихся
            var index = 0;
            for (int i = 1; i < data.Length; i++)
            {
                if (previous != data[i] || countSame == 16380)
                {
                    //окончание цикла одинаковых значений или достижение максимальной длины одинаковых
                    if (countSame > 0 || countSame == 16380)
                    {
                        //кроме того, надо записать сколько было разных, старший бит = 1, 
                        //показывает что поcледовательность различных чисел
                        if (countDiff > 0)
                        {
                            encoded.Insert(index, (short)(countDiff << 1 | 1)); //младший бит используем
                            countDiff = 0;
                        }

                        encoded.Add((short)(countSame << 1));
                        encoded.Add(previous);
                        previous = data[i];
                        countSame = 0;
                    }
                    //первый раз разные элементы, либо конец блока, надо запомнить индекс, записать потом
                    else if (countDiff == 0 || countDiff == 16380)
                    {
                        //закончился блок разных элементов, перед этим был тоже блок разных, надо записать маркировочный байт
                        if (countDiff == 126)
                        {
                            encoded.Insert(index, (short)(countDiff << 1 | 1));
                            countDiff = 0;
                        }

                        index = encoded.Count;
                        encoded.Add(previous);
                        previous = data[i];
                        countDiff++;
                    }
                    //просто идут разные элементы
                    else
                    {
                        encoded.Add(previous);
                        previous = data[i];
                        countDiff++;
                    }
                }
                else
                {
                    countSame++;
                }
            }

            //обработка последнего элемента (только не хватает обработки если последний элемент и есть 128й)
            if (countSame > 0)
            {
                if (countDiff > 0)
                    encoded.Insert(index, (short)(countDiff << 1 | 1));

                encoded.Add((short)(countSame << 1));
                encoded.Add(previous);
            }
            else if (countDiff > 0)
            {
                encoded.Insert(index, (short)(++countDiff << 1 | 1));
                encoded.Add(previous);
            }
            else //послдений, после последовательности одинаковых
            {
                encoded.Add((short)3); //то же что (1 << 1 | 1) - последовательность разных, длиной 1
                encoded.Add(previous);
            }

            return encoded.ToArray();
        }

        public static byte[] Decode(byte[] data)
        {
            var decoded = new List<byte>();
            for (int i = 0; i < data.Length; i++)
            {
                //тип блока - старший бит
                var blockType = (byte)(data[i] >> 7);
                if (blockType == 0) //одинаковые закодированы
                {
                    i++;
                    for (int j = 0; i < data.Length && j < data[i - 1] + 1; j++)
                        decoded.Add(data[i]);
                }
                else //разные, причем известна длина 
                {
                    var len = (byte)((byte)(data[i] << 1) >> 1);   //длина
                    for (int j = 0; i < data.Length - 1 && j < len; j++)
                        decoded.Add(data[++i]);
                }
            }
            return decoded.ToArray();
        }

        public static short[] Decode(short[] data)
        {
            var decoded = new List<short>();
            for (int i = 0; i < data.Length; i++)
            {
                //тип блока - младший бит
                var blockType = (short)((short)(data[i] << 15) >> 15);
                if (blockType == 0) //одинаковые закодированы
                {
                    i++;
                    for (int j = 0; i < data.Length && j < (short)(data[i - 1] >> 1) + 1; j++)
                        decoded.Add(data[i]);
                }
                else //разные, причем известна длина 
                {
                    var len = (short)(data[i] >> 1);   //длина
                    for (int j = 0; i < data.Length - 1 && j < len; j++)
                        decoded.Add(data[++i]);
                }
            }
            return decoded.ToArray();
        }

        #endregion
    }
}
