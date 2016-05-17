using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalCompressionMUI.Models.Algorithms;

namespace SignalCompressionUnitTest
{
    [TestClass]
    public class AlgorithmRleTest
    { 
        [TestMethod]
        public void EncodeSimpleByteTest()
        {
            byte[] array = { 3, 2, 3, 6, 5, 5, 1, 2, 3, 4, 4, 4, 8, 9 };

            var encoded = AlgorithmRLE.EncodeSimple(array);
            var decoded = AlgorithmRLE.DecodeSimple(encoded);

            CollectionAssert.AreEqual(array, decoded);
        }

        [TestMethod]
        public void EncodeSimpleShortTest()
        {
            short[] array = { 3, 3, 3, 6, 5, 5 };
            short[] array2 = { 3, 3, 1, 6, 2, 5 };

            var encoded = AlgorithmRLE.EncodeSimple(array);
            var decoded = AlgorithmRLE.DecodeSimple(encoded);

            CollectionAssert.AreEqual(array, decoded);
        }

        [TestMethod]
        public void EncodeTest()
        {
            byte[] array = { 3, 2, 3, 6, 5, 5, 1, 2, 3, 4, 4, 4, 8, 9 };

            var encoded = AlgorithmRLE.Encode(array);
            var decoded = AlgorithmRLE.Decode(encoded);

            CollectionAssert.AreEqual(array, decoded);
        }

        [TestMethod]
        public void EncodeTest1()
        {
            byte[] array = { 3, 3, 3, 6, 5, 5, 1, 2, 3, 4, 4, 4, 4, 4 };

            var encoded = AlgorithmRLE.Encode(array);
            var decoded = AlgorithmRLE.Decode(encoded);

            CollectionAssert.AreEqual(array, decoded);
        }

        [TestMethod]
        public void EncodeTest2()
        {
            byte[] array = new byte[300];
            array[0] = 1;
            array[1] = 3;

            var encoded = AlgorithmRLE.Encode(array);
            var decoded = AlgorithmRLE.Decode(encoded);

            CollectionAssert.AreEqual(array, decoded);
        }

        [TestMethod]
        public void EncodeTest3()
        {
            byte[] array = new byte[300];
            for (int i = 0; i < 300; i++)
                array[i] = (byte)i;

            var encoded = AlgorithmRLE.Encode(array);
            var decoded = AlgorithmRLE.Decode(encoded);

            CollectionAssert.AreEqual(array, decoded);
        }

        [TestMethod]
        public void EncodeTest4()
        {
            byte[] array = new byte[300];

            var encoded = AlgorithmRLE.Encode(array);
            var decoded = AlgorithmRLE.Decode(encoded);

            CollectionAssert.AreEqual(array, decoded);
        }

        [TestMethod]
        public void EncodeTest5()
        {
            short[] array = { 3, 2, 3, 6, 5, 5, 1, 2, 3, 4, 4, 4, 8, 9 };

            var encoded = AlgorithmRLE.Encode(array);
            var decoded = AlgorithmRLE.Decode(encoded);

            CollectionAssert.AreEqual(array, decoded);
        }
    }
}
