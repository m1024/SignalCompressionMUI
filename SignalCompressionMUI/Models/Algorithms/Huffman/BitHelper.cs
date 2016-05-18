using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalCompressionMUI.Models.Algorithms.Huffman
{
    public static class BitHelper
    {
        //position - с нуля
        public static byte GetBit(byte data, byte position)
        {
            //сдвинуть влево и вправо чтобы избавиться от лишних значащих битов
            return (byte)((byte)(data << position) >> 7);
        }

        public static byte[] ByteToBits(byte data)
        {
            var result = new byte[8];
            for (int i = 0; i < 8; i++)
                result[i] = GetBit(data, (byte)i);
            return result;
        }

        public static List<bool> ByteToBoolBits(byte data)
        {
            var result = new List<bool>();
            for (int i = 0; i < 8; i++)
                result.Add(GetBit(data, (byte)i) == 1);
            return result;
        }


        public static byte[] BoolsToBytes(List<bool> data)
        {
            var len = data.Count / 8;
            var lastlen = data.Count - len * 8;

            var bytes = new List<byte> { (byte)lastlen };
            for (int i = 0; i < len * 8; i += 8)
            {
                var b = BitsToByte(data.GetRange(i, 8));
                bytes.Add(b);
            }

            //сдвиг остатка
            byte last = 0;
            for (int i = 0; i < data.Count - len * 8; i++)
            {
                last = (byte)(last | (data[len * 8 + i] ? 1 : 0) << (7 - i));
            }
            if (data.Count - len > 0) bytes.Add(last);

            return bytes.ToArray();
        }

        public static List<bool> BytesToBools(byte[] data)
        {
            var lastlen = data.First();
            var bools = new List<bool>();
            for (int i = 1; i < data.Length; i++)
            {
                bools.AddRange(ByteToBoolBits(data[i]));
            }
            bools.RemoveRange(bools.Count - (8 - lastlen), (8 - lastlen));
            return bools;
        }

        public static byte BitsToByte(List<bool> data)
        {
            if (data.Count != 8) throw new ArgumentException("длина байта должна быть 8");
            var b = (byte)
                    (((data[0] ? 1 : 0) << 7) | ((data[1] ? 1 : 0) << 6) | ((data[2] ? 1 : 0) << 5) | ((data[3] ? 1 : 0) << 4) |
                     ((data[4] ? 1 : 0) << 3) | ((data[5] ? 1 : 0) << 2) | ((data[6] ? 1 : 0) << 1) | (data[7] ? 1 : 0));
            return b;
        }

        public static byte[] BitsToBytes(byte[] data)
        {
            var result = new List<byte>();
            var len = (data.Length / 8) * 8;
            for (int i = 0; i < len; i++)
            {
                var b = (byte)(data[i] << 7 | data[++i] << 6 | data[++i] << 5 | data[++i] << 4 |
                                data[++i] << 3 | data[++i] << 2 | data[++i] << 1 | data[++i]);
                result.Add(b);
            }

            //сдвиг остатка
            byte last = 0;
            for (int i = 0; i < data.Length - len; i++)
            {
                last = (byte)(last | data[i] << 7 - i);
            }
            if (data.Length - len > 0) result.Add(last);

            return result.ToArray();
        }
    }
}
