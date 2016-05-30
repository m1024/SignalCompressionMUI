using System;
using System.Collections.Generic;
using System.Linq;
using SignalCompressionMUI.Models.Algorithms;
using SignalCompressionMUI.Models.Algorithms.Huffman;

namespace SignalCompressionMUI.Models
{
    public struct MyPointLight
    {
        public byte X { get; set; }
        public short Y { get; set; }

        public MyPointLight(byte x, short y)
        {
            X = x;
            Y = y;
        }
    }

    public struct MyPointLightArray
    {
        public byte[] X;
        public short[] Y;

        public MyPointLightArray(byte[] x, short[] y)
        {
            X = x;
            Y = y;
        }
    }

    public static class RDPModel
    {
        public static List<Statistic> Stat = new List<Statistic>();
        public static MyPoint[] PRez;
        public static short[] SequenceSmoothed;
        public static int MaxDeviationInd;
        public static int MinDeviationInd;
        private static byte[] _compressed;
        private static short[] _sequenceSourse;
        private static bool _genStatChanged;

        public static bool GenStatChanged
        {
            get { return _genStatChanged; }
            set
            {
                _genStatChanged = value;
                if (_genStatChanged) OnStatChanged?.Invoke();
            }
        }

        public static Statistic NothingStat { get; set; }
        public static Statistic RiseStat { get; set; }
        public static Statistic RleRiseStat { get; set; }
        public static Statistic RiseHuffStat { get; set; }

        public static short[] SequenceSourse
        {
            get { return _sequenceSourse; }
            set
            {
                _sequenceSourse = value;
                OnSourseParsingComplete?.Invoke();
            }
        }

        /// <summary>
        /// Для записи в файл
        /// </summary>
        public static byte[] Compressed
        {
            get { return _compressed; }
            set
            {
                _compressed = value;
                OnCompressComplete?.Invoke();
            }
        }

        /// <summary>
        /// То что считали из файла
        /// </summary>
        public static byte[] CompressedFromFile { get; set; }

        public delegate void ValueChangedHandler();
        public static event ValueChangedHandler OnCompressComplete;
        public static event ValueChangedHandler OnSourseParsingComplete;
        public static event ValueChangedHandler OnStatChanged;


        public static void Read(string path)
        {
            SequenceSourse = AccessoryFunc.ReadAndPrepare(path);
        }

        public static List<MyPointLight[]> DeltaEncode(MyPoint[] myPoints, int blockSize)
        {
            var lightBlocks = new List<MyPointLight[]>();
            var blocks = AccessoryFunc.DivideSequence(myPoints, blockSize);

            foreach (var block in blocks)
            {
                var lightBlock = new List<MyPointLight> {new MyPointLight(0, block.First().Y)}; //блоками преобразовывалось, у первого элемента x = 1
                var previous = block[0];

                for (int i = 1; i < block.Length; i++)
                {
                    var pl = new MyPointLight((byte)(block[i].X - previous.X), block[i].Y);
                    previous = block[i];
                    lightBlock.Add(pl);
                }
                lightBlocks.Add(lightBlock.ToArray());
            }

            return lightBlocks;
        }

        public static MyPoint[] DeltaDecode(List<MyPointLight[]> lightPoints)
        {
            var blocks = new List<MyPoint>();

            foreach (var lightBlock in lightPoints)
            {
                var previous = new MyPoint(0, lightBlock[0].Y);
                var block = new List<MyPoint> {previous};

                for (int i = 1; i < lightBlock.Length; i++)
                {
                    previous = new MyPoint(previous.X + lightBlock[i].X, lightBlock[i].Y);
                    block.Add(previous);
                }
                blocks.AddRange(block);
            }
            return blocks.ToArray();
        }

        /// <summary>
        /// С разделением на X и Y
        /// </summary>
        public static List<MyPointLightArray> DeltaEncodeCut(List<MyPoint[]> myPoints)
        {
            var lightBlocks = new List<MyPointLightArray>();

            foreach (var block in myPoints)
            {
                var xcoord = new List<byte> {0};
                var ycoord = new List<short> {block.First().Y};

                var previous = block[0];

                for (int i = 1; i < block.Length; i++)
                {
                    xcoord.Add((byte)(block[i].X - previous.X));
                    ycoord.Add(block[i].Y);
                    previous = block[i];
                }
                
                var points = new MyPointLightArray(xcoord.ToArray(), ycoord.ToArray());
                lightBlocks.Add(points);
            }
            return lightBlocks;
        }

        public static MyPoint[] ConcatMyPoints(List<MyPoint[]> data)
        {
            var res = new List<MyPoint>();
            long firstX = 0;
            foreach (var block in data)
            {
                for (int i = 0; i < block.Length; i++)
                {
                    res.Add(new MyPoint(block[i].X + firstX, block[i].Y));
                }
                firstX = res.Last().X+1;
            }
            return res.ToArray();
        }

        public static List<MyPoint[]> DeltaDecodedCut(List<MyPointLightArray> data)
        {
            var blocks = new List<MyPoint[]>();

            foreach (var blocklight in data)
            {
                var xcoord = blocklight.X;
                var ycoord = blocklight.Y;
                var previous = new MyPoint(0, ycoord[0]);
                var block = new List<MyPoint>() {previous};

                for (int i = 0; i < xcoord.Length; i++)
                {
                    previous = new MyPoint(previous.X + xcoord[i], ycoord[i]);
                    block.Add(previous);
                }

                blocks.Add(block.ToArray());
            }
            return blocks;
        }

        public static MyPoint[] ToMyPoints(short[] data) => data.Select((t, i) => new MyPoint(i, t)).ToArray();

        public static short[] ToShorts(MyPoint[] data)
        {
            var previous = data[0];
            var res = new List<short> {previous.Y};

            for (int i = 1; i < data.Length; i++)
            {
                var current = data[i];
                if (current.X - previous.X > 1)
                {
                    var deltaX = current.X - previous.X;
                    var deltaY = current.Y - previous.Y;

                    for (int j = 1; j < deltaX; j++)
                        res.Add((short) ((double) deltaY/deltaX*j + previous.Y));
                    res.Add(current.Y);
                }
                else
                {
                    res.Add(current.Y);
                }
                previous = current;
            }

            return res.ToArray();
        }

        public static short[] DeconvertRdp(List<MyPoint[]> data)
        {
            var sequence = new List<short>();
            foreach (var block in data)
            {
                var b = ToShorts(block);
                b = ToShorts(block).ToList().GetRange(1, b.Length - 1).ToArray();
                sequence.AddRange(b);
            }
            return sequence.ToArray();
        } 

        public static List<MyPoint[]> ConvertRdp(out List<Statistic> stat, short[] data, int blockSize, int epsilon)
        {
            var blocksShort = AccessoryFunc.DivideSequence(data, blockSize);
            var blocks = blocksShort.Select(ToMyPoints).ToList();
            if (blocks.Last().Length != blockSize)
                blocks.RemoveAt(blocks.Count - 1); //убрать последний
            SequenceSourse = SequenceSourse.ToList().GetRange(0, (int) (SequenceSourse.Length/blockSize)*blockSize).ToArray(); //и в исходной
            var newBlocks = new List<MyPoint[]>();

            stat = new List<Statistic>();

            foreach (var block in blocks)
            {
                var swatch = new System.Diagnostics.Stopwatch();
                var st = new Statistic();

                AlgorithmRDP.Additions = 0;
                AlgorithmRDP.Multiplications = 0;
                AlgorithmRDP.MaxCalls = 0;
                AlgorithmRDP.Calls = 0;

                swatch.Start(); // старт

                var newBlock = AlgorithmRDP.DouglasPeucker(block, 0, block.Length-1, epsilon);

                swatch.Stop();
                st.Number = stat.Count;
                st.Title = st.Number.ToString();
                st.Time = swatch.Elapsed;
                st.BlockRezultLength = newBlock.Length;
                st.BlockRezultSize = newBlock.Length*3;
                st.Additions = AlgorithmRDP.Additions;
                st.Multiplications = AlgorithmRDP.Multiplications;
                st.RecursiveCalls = AlgorithmRDP.MaxCalls;
                st.BlockSourseSize = blockSize*2;
                stat.Add(st);

                newBlocks.Add(newBlock); //.GetRange(1,newBlock.Length-1).ToArray()
            }

            return newBlocks;
        }

        public static List<MyPointLightArray> EncodeHuffmanHalf(List<MyPointLightArray> data, out List<Statistic> stat)
        {
            var enc = new List<MyPointLightArray>();
            stat = new List<Statistic>();

            foreach (var block in data)
            {
                #region замеры
                var blockstat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                #endregion

                enc.Add(new MyPointLightArray(AlgorithmDynHuff.Encode(block.X), block.Y));

                #region замеры
                swatch.Stop();
                blockstat.BlockRezultSize = enc.Last().X.Length + enc.Last().Y.Length;
                blockstat.Time = swatch.Elapsed;
                stat.Add(blockstat);
                #endregion
            }

            return enc;
        }

        public static List<MyPointLightArray> DecodeHuffmanHalf(List<MyPointLightArray> data)
        {
            var dec = new List<MyPointLightArray>();

            foreach (var block in data)
            {
                dec.Add(new MyPointLightArray(AlgorithmDynHuff.Decode(block.X), block.Y));
            }

            return dec;
        }

        public static List<MyPointLightArray> EncodeRle(List<MyPointLightArray> data, out List<Statistic> stat)
        {
            var enc = new List<MyPointLightArray>();
            stat = new List<Statistic>();

            foreach (var block in data)
            {
                #region замеры
                var blockstat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                #endregion

                enc.Add(new MyPointLightArray(AlgorithmRLE.Encode(block.X), block.Y));

                #region замеры
                swatch.Stop();
                blockstat.BlockRezultSize = enc.Last().X.Length + enc.Last().Y.Length;
                blockstat.Time = swatch.Elapsed;
                stat.Add(blockstat);
                #endregion
            }

            return enc;
        }

        public static List<MyPointLightArray> DecodeRle(List<MyPointLightArray> data)
        {
            var dec = new List<MyPointLightArray>();

            foreach (var block in data)
            {
                dec.Add(new MyPointLightArray(AlgorithmRLE.Decode(block.X), block.Y));
            }

            return dec;
        } 

        public static List<List<byte[]>> EncodeRise(List<MyPointLightArray> data, out List<Statistic> stat)
        {
            var enc = new List<List<byte[]>>();
            stat = new List<Statistic>();

            foreach (var block in data)
            {
                #region замеры
                var blockstat = new Statistic();
                var swatch = new System.Diagnostics.Stopwatch();
                swatch.Start();
                #endregion

                var blockEncX = AlgorithmRise.Encode(block.X);
                var blockEncY = AlgorithmRise.Encode(block.Y);
                var blockEnc = new List<byte[]>() {blockEncX, blockEncY};
                enc.Add(blockEnc);

                #region замеры
                swatch.Stop();
                blockstat.BlockRezultSize = blockEncX.Length + blockEncY.Length;
                blockstat.Time = swatch.Elapsed;
                stat.Add(blockstat);
                #endregion
            }

            return enc;
        }

        public static List<MyPointLightArray> DecodeRise(List<List<byte[]>> data)
        {
            var dec = new List<MyPointLightArray>();

            foreach (var block in data)
            {
                var blockDecX = AlgorithmRise.DecodeByte(block.First());
                var blockDecY = AlgorithmRise.Decode(block.Last());
                var blockDec = new MyPointLightArray(blockDecX, blockDecY);
                dec.Add(blockDec);
            }

            return dec;
        } 

        /// <summary>
        /// Разбиение последовательности на части и ее преобразование с сохранением данных статистики
        /// </summary>
        public static void ConvertRdp(string blockSizeStr, string epsilon)
        {
            AlgorithmRDP.Additions = 0;
            AlgorithmRDP.Multiplications = 0;

            MyPoint[] points = new MyPoint[SequenceSourse.Length];

            for (int i = 0; i < SequenceSourse.Length; i++)
            {
                points[i].X = i;
                points[i].Y = SequenceSourse[i];
            }

            int blockSize = int.Parse(blockSizeStr);
            List<MyPoint[]> smoothSequence = new List<MyPoint[]>();
            Stat = new List<Statistic>();

            int smooothSeqLength = 0;
            int j = 0;

            for (int i = 0; i < points.Length; i += blockSize)
            {
                System.Diagnostics.Stopwatch swatch = new System.Diagnostics.Stopwatch(); // создаем объект
                Statistic blockStat = new Statistic
                {
                    Additions = AlgorithmRDP.Additions,
                    Multiplications = AlgorithmRDP.Multiplications
                };
                AlgorithmRDP.MaxCalls = 0;
                AlgorithmRDP.Calls = 0;
                swatch.Start(); // старт

                smoothSequence.Add(AlgorithmRDP.DouglasPeucker(points, i,
                    (i + blockSize < points.Length) ? i + blockSize : points.Length - 1,
                    (epsilon == "") ? 1 : (float)Math.Pow(float.Parse(epsilon), 2)));

                smooothSeqLength += smoothSequence.ElementAt(j++).Length - 1;
                //-1 чтобы убрать последний элемент, т.к. он будет началом следующего блока

                swatch.Stop(); // стоп

                //сохраняем статситку
                blockStat.Time = swatch.Elapsed;
                blockStat.BlockRezultLength = smoothSequence.ElementAt(i / blockSize).Length;
                blockStat.BlockRezultSize = blockStat.BlockRezultLength * 3;
                blockStat.BlockSourseSize = blockSize * 2;
                blockStat.Additions = AlgorithmRDP.Additions - blockStat.Additions;
                blockStat.Multiplications = AlgorithmRDP.Multiplications - blockStat.Multiplications;
                //blockStat.CompressionRatio =
                //    (float)Math.Round(blockSize / (smoothSequence.ElementAt(i / blockSize).Length * 1.5), 3);
                blockStat.RecursiveCalls = AlgorithmRDP.MaxCalls;

                Stat.Add(blockStat);
            }

            PRez = new MyPoint[smooothSeqLength + 1];
            j = 0;
            foreach (var block in smoothSequence)
            {
                for (int i = 0; i < block.Length - 1; i++)
                    PRez[j++] = block[i];
            }
            PRez[j] = smoothSequence.Last().Last();

            //textBoxKmin.Text = stat.OrderBy(u => u.СompressionRatio).First().СompressionRatio.ToString();
            //textBoxKmax.Text = stat.OrderByDescending(u => u.СompressionRatio).First().СompressionRatio.ToString();
            //textBoxRecursiveCalls.Text = stat.OrderByDescending(u => u.RecursiveCalls).First().RecursiveCalls.ToString();
        }

        /// <summary>
        /// Преобразование сжатой последовательности обратно в обычную
        /// </summary>
        public static void DeconvertRdp()
        {
            SequenceSmoothed = new short[PRez[PRez.Length - 1].X + 1];

            SequenceSmoothed[0] = PRez[0].Y;
            var n = 1;
            for (int i = 1; i < PRez.Length && n < SequenceSmoothed.Length; i++)
            {
                if (PRez[i].X - PRez[i - 1].X > 1)
                {
                    var deltaX = PRez[i].X - PRez[i - 1].X;
                    var deltaY = PRez[i].Y - PRez[i - 1].Y;

                    for (int j = 1; j < (PRez[i].X - PRez[i - 1].X) && n < SequenceSmoothed.Length; j++)
                        SequenceSmoothed[n++] = (short)((double)deltaY / deltaX * j + PRez[i - 1].Y);
                    SequenceSmoothed[n++] = PRez[i].Y;
                }
                else
                {
                    SequenceSmoothed[n++] = PRez[i].Y;
                }
            }
        }
    }
}
