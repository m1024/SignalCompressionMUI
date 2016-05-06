namespace SignalCompressionMUI.Models.Algorithms
{
    public struct MyPoint
    {
        public long X { get; set; }
        public short Y { get; set; }
    }

    public static class AlgorithmRDP
    {
        public static ulong Multiplications { get; set; }
        public static ulong Additions { get; set; }
        public static int Calls { get; set; }
        public static int MaxCalls { get; set; }

        /// <summary>
        /// Функция рассчета расстояния от точки до прямой, задаваемой двумя точками
        /// </summary>
        private static void PerpendicularDistance(MyPoint point, MyPoint start, MyPoint end, out long numerator,
            out long denominator)
        {
            Multiplications += 5;
            Additions += 6;

            var deltaX = end.X - start.X;
            var deltaY = end.Y - start.Y;
            var num = deltaX*(start.Y - point.Y) - (start.X - point.X)*deltaY;
            numerator = num*num;
            denominator = deltaX*deltaX + deltaY*deltaY;
        }

        /// <summary>
        /// Алгоритм Рамера — Дугласа — Пекера, для уменьшения точек кривой
        /// </summary>
        /// <param name="pointList">Массив точек, задающий кривую</param>
        /// <param name="startInd">Индекс начала кривой</param>
        /// <param name="endInd">Индекс конца кривой</param>
        /// <param name="epsilon">Точность, возведенная в квадрат</param>
        /// <returns></returns>
        public static MyPoint[] DouglasPeucker(MyPoint[] pointList, int startInd, int endInd, float epsilon)
        {
            //Находим точку с максимальным расстоянием от прямой между первой и последней точками набора
            long numeratorMax = 0, denominatorMax = 1;
            var index = 0;
            for (int i = 1; i < endInd - startInd; i++)
            {
                Multiplications += 2;
                Additions += 2;

                long numerator, denominator;
                PerpendicularDistance(pointList[i + startInd], pointList[startInd], pointList[endInd],
                    out numerator, out denominator);

                if ((numerator*denominatorMax) > (denominator*numeratorMax))
                {
                    Additions++;
                    index = i + startInd;
                    numeratorMax = numerator;
                    denominatorMax = denominator;
                }
            }

            Multiplications++;
            //Если максимальная дистанция больше, чем epsilon, то рекурсивно вызываем её на участках
            if (numeratorMax >= epsilon*denominatorMax)
            {
                Calls++;
                if (Calls > MaxCalls)
                    MaxCalls = Calls;
                MyPoint[] recResults1 = DouglasPeucker(pointList, startInd, index, epsilon);
                Calls--;

                Calls++;
                if (Calls > MaxCalls)
                    MaxCalls = Calls;
                MyPoint[] recResults2 = DouglasPeucker(pointList, index, endInd, epsilon);
                Calls--;

                Additions += 2;
                MyPoint[] result = new MyPoint[recResults1.Length + recResults2.Length - 1];

                for (int i = 0; i < recResults1.Length; i++)
                    result[i] = recResults1[i];

                for (int i = 1; i < recResults2.Length; i++)
                {
                    result[i + recResults1.Length - 1] = recResults2[i];
                    Additions += 2;
                }

                return result;
            }
            else
            {
                //проверка чтобы потом при разностном кодировании X, не выйти за пределы 1 байта
                Additions++;
                if (pointList[endInd].X - pointList[startInd].X > 255)
                {
                    Additions += 2;

                    int length = 0;
                    var deltaX = pointList[endInd].X - pointList[startInd].X;
                    int k = 0;
                    while (length < deltaX)
                    {
                        length += 255;
                        k++;
                        Additions++;
                    }
                    //ну или с делением:
                    //int k = (pointList[endInd].X - pointList[startInd].X)/255 + 1;

                    MyPoint[] resultR = new MyPoint[k + 1];
                    for (int i = 0; i < k; i++)
                    {
                        resultR[i] = pointList[startInd + i*255];
                        Additions++;
                        Multiplications++;
                    }
                    resultR[k] = pointList[endInd];
                    return resultR;
                }
                else
                {
                    MyPoint[] resultR = new MyPoint[2];
                    resultR[0] = pointList[startInd];
                    resultR[1] = pointList[endInd];
                    return resultR;
                }
            }
        }
    }
}
