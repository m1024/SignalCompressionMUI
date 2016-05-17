using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace SignalCompressionMUI.Models.Algorithms
{
    public enum WaveletType
    {
        [Description("Хаар (D2)")]
        Haar,
        [Description("Добеши D4")]
        D4,
        [Description("Добеши D6")]
        D6,
        [Description("Добеши D8")]
        D8,
        [Description("Добеши D10")]
        D10
    }

    public static class AlgorithmWv
    {
        //использовать нормированные или не нормированные?
        #region Ортогональные нормированные коэффициенты

        private static readonly double[] Haar = {1.0, 1.0};

        private static readonly double[] D4 = {0.6830127, 1.1830127, 0.3169873, -0.1830127};

        private static readonly double[] D6 = {0.47046721, 1.14111692, 0.650365, -0.19093442, -0.12083221, 0.0498175};

        private static readonly double[] D8 = {0.32580343, 1.01094572, 0.8922014, -0.03957503, -0.26450717, 0.0436163, 0.0465036, -0.01498699};

        private static readonly double[] D10 = {0.22641898, 0.85394354, 1.02432694, 0.19576696, -0.34265671, -0.04560113, 0.10970265, -0.00882680, -0.01779187, 4.71742793e-3};
        #endregion

        public static int Delta => CH.Count - 2;

        //public enum WaveletType : int { Haar = 0, D4 = 1, D6 = 2, D8 = 3, D10 = 4}

        private static WaveletType _type;

        public static WaveletType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                GetCl();
                GetCh();
                SetIrregular();
                //GetReverse();
            }
        }

        private static void SetIrregular()
        {
            var chArray = CH.ToArray();
            var clArray = CL.ToArray();
            for (int i = 0; i < chArray.Length; i++)
            {
                chArray[i] /= Math.Sqrt(2);
                clArray[i] /= Math.Sqrt(2);
            }
            CH = chArray.ToList();
            CL = clArray.ToList();

            GetReverse();
        }

        //низкочастотный фильтр
        private static List<double> CL { get; set; }
        
        //высокочастотный фильтр
        private static List<double> CH { get; set; } 

        private static List<double> ReverseCL { get; set; }
        private static List<double> ReverseCH { get; set; } 

        //высокочастотный фильтр, в зависимости от настроек
        private static void GetCl()
        {
            switch (Type)
            {
                case WaveletType.Haar:
                    CL = Haar.ToList();
                    break;
                case WaveletType.D4:
                    CL = D4.ToList();
                    break;
                case WaveletType.D6:
                    CL = D6.ToList();
                    break;
                case WaveletType.D8:
                    CL = D8.ToList();
                    break;
                case WaveletType.D10:
                    CL = D10.ToList();
                    break;
            }
        }

        //генерация второй строчки - высокочастотного фильтра (коэф. в обратном порядке с чередованием знаков)
        private static void GetCh()
        {
            CL.Reverse();
            CH = new List<double>();
            for (var i=0; i<CL.Count; i++)
                CH.Add(Math.Pow(-1, i) * CL[i]);
            CL.Reverse();
        }

        private static void GetReverse()
        {
            CL.Reverse();
            CH.Reverse();
            ReverseCL = new List<double>();
            ReverseCH = new List<double>();

            //формируем первый вектор
            for (int i = 1; i < CH.Count; i+=2)
            {
                ReverseCL.Add(CL[i]);
                ReverseCL.Add(CH[i]);
            }

            //формируем второй вектор
            for (int i = 0; i < CH.Count; i += 2)
            {
                ReverseCH.Add(CL[i]);
                ReverseCH.Add(CH[i]);
            }
            CL.Reverse();
            CH.Reverse();
        }

        //чтобы по  индексу -1 можно было получать доступ к последнему элементу (-2 предпоследний, и т.д.)
        private static int GetIndex(int index, int length)
        {
            if (index >= 0) return index%length;
            var ind = (length + index)%length;
            if (ind > 0) return ind;
            return GetIndex(ind, length);
        }

        public static void Convert(List<short> data, out List<double> slList, out List<double> shList, int delta = 0)
        {
            slList = new List<double>();
            shList = new List<double>();

            //перебор исходного массива, через один
            for (int i = 0; i < data.Count; i += 2)
            {
                double sL = 0;
                double sH = 0;

                for (int j = 0; j < CH.Count; j++)
                {
                    sL += data[GetIndex(i + j - delta, data.Count)]*CL[j];
                    sH += data[GetIndex(i + j - delta, data.Count)] *CH[j];
                }

                slList.Add(sL);
                shList.Add(sH);
            }
        }

        public static void Convert(List<short> data, out List<double> outList, int delta = 0)
        {
            outList = new List<double>();

            //перебор исходного массива, через один
            for (int i = 0; i < data.Count; i += 2)
            {
                double sL = 0;
                double sH = 0;

                for (int j = 0; j < CH.Count; j++)
                {
                    sL += data[GetIndex(i + j - delta, data.Count)] * CL[j];
                    sH += data[GetIndex(i + j - delta, data.Count)] * CH[j];
                }

                outList.Add(sL);
                outList.Add(sH);
            }
        }

        public static List<short> Deconvert(List<double> data, int delta = 0)
        {
            var outList = new List<short>();

            //перебор исходного массива, через один
            for (int i = 0; i < data.Count; i += 2)
            {
                double sL = 0;
                double sH = 0;

                for (int j = 0; j < CH.Count; j++)
                {
                    sL += data[GetIndex(i + j - delta, data.Count)] * ReverseCL[j];
                    sH += data[GetIndex(i + j - delta, data.Count)] * ReverseCH[j];
                }

                outList.Add((short)Math.Round(sL));
                outList.Add((short)Math.Round(sH));
            }
            return outList;
        }

        public static List<short> Deconvert(List<short> data, int delta = 0)
        {
            var outList = new List<short>();

            //перебор исходного массива, через один
            for (int i = 0; i < data.Count; i += 2)
            {
                double sL = 0;
                double sH = 0;

                for (int j = 0; j < CH.Count; j++)
                {
                    sL += data[GetIndex(i + j - delta, data.Count)] * ReverseCL[j];
                    sH += data[GetIndex(i + j - delta, data.Count)] * ReverseCH[j];
                }

                outList.Add((short)Math.Round(sL));
                outList.Add((short)Math.Round(sH));
            }
            return outList;
        }


        #region матрицы
        private static readonly double[][] KoefMatrix = new[]
        {
            new double[]
            {
                (1 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (3 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (3 - Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (1 - Math.Sqrt(3))/(4*Math.Sqrt(2))
            },
            new double[]
            {
                (1 - Math.Sqrt(3))/(4*Math.Sqrt(2)),
                -(3 - Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (3 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                -(1 + Math.Sqrt(3))/(4*Math.Sqrt(2))
            }
        };

        private static readonly double[][] IkoefMatrix = new[]
        {
            new double[]
            {
                (3 - Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (3 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (1 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (1 - Math.Sqrt(3))/(4*Math.Sqrt(2))
            },
            new double[]
            {
                (1 - Math.Sqrt(3))/(4*Math.Sqrt(2)),
                -(1 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                (3 + Math.Sqrt(3))/(4*Math.Sqrt(2)),
                -(3 - Math.Sqrt(3))/(4*Math.Sqrt(2)),
            }
        };
        #endregion

        public static List<double> Encode(short[] data, int delta, out List<double> sL, out List<double> sH)
        {
            var rez = new List<double>();
            sL = new List<double>();
            sH = new List<double>();

            for (int i = 0; i < data.Length; i += 2)
            {
                double[] vectorKoef = new double[2]; //0 - СL, 1 - CH 
                for (int j = 0; j < 4; j++)
                {
                    vectorKoef[0] += data[(i + j - delta) % data.Length] * KoefMatrix[0][j];
                    vectorKoef[1] += data[(i + j - delta) % data.Length] * KoefMatrix[1][j];
                }
                sL.Add(vectorKoef[0]);
                sH.Add(vectorKoef[1]);
                rez.Add(vectorKoef[0]);
                rez.Add(vectorKoef[1]);
            }
            return rez;
        }

        public static List<double> Decode(double[] data, int delta, out List<double> sL, out List<double> sH)
        {
            var rez = new List<double>();
            sL = new List<double>();
            sH = new List<double>();

            for (int i = 0; i < data.Length; i += 2)
            {
                double[] vectorKoef = new double[2]; //0 - СL, 1 - CH 
                for (int j = 0; j < 4; j++)
                {
                    vectorKoef[0] += data[(i + j + delta) % data.Length] * IkoefMatrix[0][j];
                    vectorKoef[1] += data[(i + j + delta) % data.Length] * IkoefMatrix[1][j];
                }
                sL.Add(vectorKoef[0]);
                sH.Add(vectorKoef[1]);
                rez.Add(vectorKoef[0]);
                rez.Add(vectorKoef[1]);
            }
            return rez;
        }
    }
}
