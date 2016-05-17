using System;
using System.Collections.Generic;
using System.Linq;
using SignalCompressionMUI.Models.Algorithms;

namespace SignalCompressionMUI.Models
{
    public class WaveletArray
    {
        public List<short> Sourse { get; set; } 
        public List<double> ClList { get; set; }
        public List<double> ChList { get; set; }
        public List<double> AllList { get; set; }
        public List<short> New { get; set; }
        public List<short> ClListRound { get; set; }
        public List<short> ChListRound { get; set; }
        public List<short> AllListRound { get; set; }   
        public WaveletType ConvertType { get; set; }

        public void Convert()
        {
            if (Sourse.Count % 2 == 1) throw new ArgumentException("Длина массива не кратна 2");

            List<double> cl, ch;
            AlgorithmWv.Type = ConvertType;
            AlgorithmWv.Convert(Sourse, out cl, out ch);
            ClList = cl;
            ChList = ch;
            ConcatLists();
        }

        public void Deconvert()
        {
            AlgorithmWv.Type = ConvertType;
            New = AlgorithmWv.Deconvert(AllList, AlgorithmWv.Delta);
        }

        public void DeconvertRounded()
        {
            AlgorithmWv.Type = ConvertType;
            New = AlgorithmWv.Deconvert(AllListRound, AlgorithmWv.Delta);
        }

        public void DeconvertRoundedHulf()
        {
            ChListRound = new short[ClList.Count].ToList();
            ConcatRoundedLists();
            New = AlgorithmWv.Deconvert(AllListRound, AlgorithmWv.Delta);
        }

        //округление последовательностей до short и больше
        public void Round(int num)
        {
            ClListRound = new List<short>();
            ChListRound = new List<short>();
            AllListRound = new List<short>();
            for (int i = 0; i < ClList.Count; i++)
            {
                var cl = (short) (Math.Round(ClList[i])); //(short) (Math.Round(ClList[i]/num)*num);
                var ch = (short) (Math.Round(ChList[i])); //(short) (Math.Round(ChList[i]/num)*num);
                ClListRound.Add(cl);
                ChListRound.Add(ch);
                AllListRound.Add(cl);
                AllListRound.Add(ch);
            }
        }

        /// <summary>
        /// Домножение на величину округления (при декодировании)
        /// </summary>
        //public void DeRound(int num)
        //{
        //    var ch = ChListRound.Select(c => (short) (c*num)).ToList();
        //    var cl = ClListRound.Select(c => (short) (c*num)).ToList();

        //    ChListRound = ch;
        //    ClListRound = cl;
        //}

        public void ConcatLists()
        {
            AllList = new List<double>();
            for (int i = 0; i < ClList.Count; i++)
            {
                AllList.Add(ClList[i]);
                AllList.Add(ChList[i]);
            }
        }

        public void ConcatRoundedLists()
        {
            AllListRound = new List<short>();
            for (int i = 0; i < ClListRound.Count; i++)
            {
                AllListRound.Add(ClListRound[i]);
                AllListRound.Add(ChListRound[i]);
            }
        }

        //public WaveletArray Left { get; set; }
        //public WaveletArray Right { get; set; }
        //public WaveletArray Parent { get; set; }
    }
}
