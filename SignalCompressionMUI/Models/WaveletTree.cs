using System.Collections.Generic;
using SignalCompressionMUI.Models.Algorithms;

namespace SignalCompressionMUI.Models
{
    public class WaveletTree
    {
        public WaveletArray wvArray { get; set; }
        public List<List<short>> Converted { get; set; }
        public WaveletTree Left { get; set; }
        public WaveletTree Right { get; set; }
        public WaveletTree Parent { get; set; }

        public WaveletTree()
        {
        }

        public WaveletTree(WaveletArray vw)
        {
            wvArray = vw;
        }

        /// <summary>
        /// Создание дерева требуемой глубины (половина влево, половина вправо, и рекрсивное преобразование)
        /// </summary>
        /// <param name="depth">Глубина дерева</param>
        /// <param name="rounding">Округление</param>
        public void BuildTree(int depth, int rounding)
        {
            if (depth == 0) return;
            //исходный массив должен делиться на 2, столько раз, сколько глубина
            var left = new WaveletArray()
            {
                Sourse = wvArray.ClListRound,
                ConvertType = wvArray.ConvertType
            };
            left.Convert();
            left.Round(rounding);

            var right = new WaveletArray()
            {
                Sourse = wvArray.ChListRound,
                ConvertType = wvArray.ConvertType
            };
            right.Convert();
            right.Round(rounding);

            Left = new WaveletTree()
            {
                Parent = this,
                wvArray = left
            };
            Right = new WaveletTree()
            {
                Parent = this,
                wvArray = right
            };

            Left.BuildTree(depth - 1, rounding);
            Right.BuildTree(depth - 1, rounding);
        }

        /// <summary>
        /// Собирание элементов последнего уровня в корне
        /// </summary>
        public void ConvertedToRoot()
        {
            if (Left == null || Right == null)
            {
                Converted = new List<List<short>> { wvArray.ClListRound, wvArray.ChListRound };
            }
            else
            {
                Left.ConvertedToRoot();
                Right.ConvertedToRoot();

                Converted = new List<List<short>>();
                foreach (var list in Left.Converted)
                    Converted.Add(list);
                foreach (var list in Right.Converted)
                    Converted.Add(list);
            }
        }

        /// <summary>
        /// Построение дерева для декодирования
        /// </summary>
        /// <param name="converted"></param>
        /// <param name="depth"></param>
        public void BuidDeconvTree()
        {
            if (Converted.Count == 1) return;

            var left = new List<List<short>>();
            var right = new List<List<short>>();
            for (int i = 0; i < Converted.Count / 2; i++)
                left.Add(Converted[i]);
            for (int i = Converted.Count / 2; i < Converted.Count; i++)
                right.Add(Converted[i]);

            Left = new WaveletTree() { Converted = left, Parent = this };
            Right = new WaveletTree() { Converted = right, Parent = this };

            Left.BuidDeconvTree();
            Right.BuidDeconvTree();
        }

        /// <summary>
        /// Деконвертирование
        /// </summary>
        public void Deconvert(WaveletType type)
        {
            if (Left == null || Right == null) return;
            if (Left.Left == null || Left.Right == null || Right.Left == null || Right.Right == null)
            {
                wvArray = new WaveletArray { ClListRound = Left.Converted[0], ChListRound = Right.Converted[0], ConvertType = type };
                wvArray.ConcatRoundedLists();
                wvArray.DeconvertRounded();
            }
            else
            {
                Left.Deconvert(type);
                Right.Deconvert(type);

                wvArray = new WaveletArray
                {
                    ClListRound = Left.wvArray.New,
                    ChListRound = Right.wvArray.New,
                    ConvertType = type
                };
                wvArray.ConcatRoundedLists();
                wvArray.DeconvertRounded();
            }
        }
    }
}
