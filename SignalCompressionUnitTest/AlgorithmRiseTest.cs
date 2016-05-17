using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalCompressionMUI.Models.Algorithms;

namespace SignalCompressionUnitTest
{
    [TestClass]
    public class AlgorithmRiseTest
    {
        [TestMethod]
        public void PositiveNumbersTest()
        {
            short[] array = { 3, 34, 1032, 882 };

            var encoded = AlgorithmRise.Encode(array);
            var decoded = AlgorithmRise.Decode(encoded);

            CollectionAssert.AreEqual(array, decoded);
        }

        [TestMethod]
        public void NegativeNumbersTest()
        {
            short[] array = { -3, -34, -1032, -882 };
            short[] array2 = new short[array.Length];
            Array.Copy(array, 0, array2, 0, array2.Length);

            var encoded = AlgorithmRise.Encode(array);
            var decoded = AlgorithmRise.Decode(encoded);

            CollectionAssert.AreEqual(array2, decoded);
        }

        [TestMethod]
        public void BothNumbersTest()
        {
            short[] array = { -3, 34, -1032, 882 };
            short[] array2 = new short[array.Length];
            Array.Copy(array, 0, array2, 0, array2.Length);

            var encoded = AlgorithmRise.Encode(array);
            var decoded = AlgorithmRise.Decode(encoded);

            CollectionAssert.AreEqual(array2, decoded);
        }
    }
}
