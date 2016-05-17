using System;

namespace SignalCompressionMUI.Models.Algorithms
{
    public class AlgorithmDCT
    {
        private static readonly double[][] CosMatrix =
        {
            new[] {1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0},
            new[] {0.981, 0.831, 0.556, 0.195, -0.195, -0.556, -0.831, -0.981},
            new[] {0.924, 0.383, -0.383, -0.924, -0.924, -0.383, 0.383, 0.924},
            new[] {0.831, -0.195, -0.981, -0.556, 0.556, 0.981, 0.195, -0.831},
            new[] {0.707, -0.707, -0.707, 0.707, 0.707, -0.707, -0.707, 0.707},
            new[] {0.556, -0.981, 0.195, 0.831, -0.831, -0.195, 0.981, -0.556},
            new[] {0.383, -0.924, 0.924, -0.383, -0.383, 0.924, -0.924, 0.383},
            new[] {0.195, -0.556, 0.831, -0.981, 0.981, -0.831, 0.556, -0.195},
        };

        private static readonly double[] Lambda = { 1 / (2 * Math.Pow(2, 0.5)), 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5 };

        /// <summary>
        /// умножение матрицы на вектор
        /// </summary>
        /// <param name="vector">вектор, длина 8</param>
        /// <returns></returns>
        private static double[] СosMatrixOnVector(double[] vector)
        {
            double[] rezult = new double[8];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    rezult[i] += CosMatrix[i][j] * vector[j];
            return rezult;
        }

        private static double[] OnLambda(double[] vector)
        {
            for (int i = 0; i < vector.Length; i++)
                vector[i] *= Lambda[i];
            return vector;
        }

        private static double[] СosMatrixTranspOnVector(double[] vector)
        {
            double[] rezult = new double[8];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    rezult[i] += CosMatrix[j][i] * vector[j];
            return rezult;
        }

        private static double[] VectorOnVector(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length) return null;
            double[] result = new double[vector1.Length];
            for (int i = 0; i < vector1.Length; i++)
            {
                result[i] = vector1[i] * vector2[i];
            }
            return result;
        }

        private static double[] VectorDivideOnVector(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length) return null;
            double[] result = new double[vector1.Length];
            for (int i = 0; i < vector1.Length; i++)
            {
                result[i] = vector1[i] / vector2[i];
            }
            return result;
        }

        //private static double[] vectorKoef = new[] {100.0, 70.0, 25.0, 30.0, 35.0, 45.0, 60.0, 80.0};
        //private static double[] vectorKoef = new[] { 3.0, 2.0, 2.0, 3.0, 4.0, 5.0, 7.0, 10.0, };
        public static double[] VectorKoef = new[] { 12.0, 8.0, 12.0, 20.0, 40.0, 50.0, 40.0, 50.0, };
        public static int KoefCount = 8;

        /// <summary>
        /// DCT преобразование
        /// </summary>
        /// <param name="koefCount">Число коэффициентов которые надо оставить (max = 8)</param>
        /// <param name="vector">Входной вектор, длины 8</param>
        public static short[] Convert(short[] vector)
        {
            if (vector.Length != 8) return null;

            double[] dvector = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++)
                dvector[i] = vector[i];

            double[] Rez = СosMatrixOnVector(dvector);
            Rez = OnLambda(Rez);

            short[] sRez = new short[KoefCount];
            Rez = VectorDivideOnVector(Rez, VectorKoef);
            for (int i = 0; i < KoefCount; i++)
                sRez[i] = (short)Math.Round(Rez[i]);

            return sRez;
        }

        public static short[] InvertConvert(short[] vector)
        {
            if (vector.Length != 8) return null;

            double[] dvector = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++)
                dvector[i] = vector[i];

            dvector = VectorOnVector(dvector, VectorKoef);
            double[] Rez = OnLambda(dvector);
            Rez = СosMatrixTranspOnVector(Rez);

            short[] sRez = new short[Rez.Length];
            for (int i = 0; i < vector.Length; i++)
                sRez[i] = (short)Math.Round(Rez[i]);

            return sRez;
        }
    }
}
