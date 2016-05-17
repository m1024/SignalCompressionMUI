using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalCompressionMUI.Models.Algorithms;

namespace SignalCompressionUnitTest
{
    [TestClass]
    public class AlgorithmDctTest
    {
        [TestMethod]
        public void EncodeTest()
        {
            short[] input = { 12, 10, 8, 10, 12, 10, 8, 11 };
            AlgorithmDCT.VectorKoef = new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

            short[] expected = { 29, 1, 0, 2, 3, -2, 0, 0 };
            var result = AlgorithmDCT.Convert(input);

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void DecodeTest()
        {
            short[] input = { 29, 1, 0, 2, 3, -2, 0, 0 };
            AlgorithmDCT.VectorKoef = new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

            short[] expected = { 12, 10, 8, 10, 13, 10, 8, 11 };
            var result = AlgorithmDCT.InvertConvert(input);

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void EncodeDecodeTest()
        {
            short[] input = { 12, 10, 8, 10, 12, 10, 8, 11 };
            AlgorithmDCT.VectorKoef = new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

            short[] expected = { 12, 10, 8, 10, 13, 10, 8, 11 };
            var result = AlgorithmDCT.InvertConvert(AlgorithmDCT.Convert(input));

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void EncodeDecodeTest2()
        {
            short[] input = { 12, 10, 8, 10, 12, 10, 8, 11 };
            AlgorithmDCT.VectorKoef = new double[] { 0.01, 0.01, 0.01, 0.01, 0.01, 0.01, 0.01, 0.01 };

            short[] expected = { 12, 10, 8, 10, 12, 10, 8, 11 };
            var result = AlgorithmDCT.InvertConvert(AlgorithmDCT.Convert(input));

            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void EncodeNullTest()
        {
            short[] input = { 12, 10, 8, 10, 12, 10, 8, 11, 10 }; //не кратно 8
            AlgorithmDCT.VectorKoef = new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };
            ;
            var result = AlgorithmDCT.Convert(input);

            CollectionAssert.AreEqual(null, result);
        }

        [TestMethod]
        public void DecodeNullTest()
        {
            short[] input = { 29, 1, 0, 2, 3, -2, 0, 0, 3, -2, 0, 0 }; //не кратно 8
            AlgorithmDCT.VectorKoef = new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

            var result = AlgorithmDCT.InvertConvert(input);

            CollectionAssert.AreEqual(null, result);
        }


        [TestMethod]
        public void VectorKoefMore8Test()
        {
            short[] input = { 12, 10, 8, 10, 12, 10, 8, 11 };
            AlgorithmDCT.VectorKoef = new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

            var result = AlgorithmDCT.Convert(input);

            CollectionAssert.AreEqual(null, result);
        }

        [TestMethod]
        public void VectorKoefLess8Test()
        {
            short[] input = { 12, 10, 8, 10, 12, 10, 8, 11 };
            AlgorithmDCT.VectorKoef = new double[] { 1.0, 1.0, 1.0, 1.0 };

            var result = AlgorithmDCT.Convert(input);

            CollectionAssert.AreEqual(null, result);
        }
    }
}
