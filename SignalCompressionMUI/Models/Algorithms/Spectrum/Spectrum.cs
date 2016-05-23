using System;
using System.Collections.Generic;

namespace SignalCompressionMUI.Models.Algorithms.Spectrum
{
    public static class Spectrum
    {
        /// <summary>
        /// Вычисление спектра
        /// </summary>
        /// <param name="sequence">Дискретный сигнал</param>
        /// <returns>Амплитудный спектр сигнала</returns>
        public static int[] CalculateSpectrum(IReadOnlyList<short> sequence)
        {
            if (sequence == null) return new int[0];

            var n = (int)Math.Log(sequence.Count, 2) + 1;
            var len = (int)Math.Pow(2, n);
            var sLen = sequence.Count;

            var complexSequence = new Complex[len];

            //длина должна быть кратна 2, лишнее надо заполнять нулями
            for (int i = 0; i < len; i++)
                complexSequence[i] = (i < sLen)
                    ? new Complex(sequence[i], 0)
                    : complexSequence[i] = new Complex(0, 0);

            var spectrumComplex = FFT.fft(complexSequence);

            var spectrum = new int[spectrumComplex.Length / 2];
            for (int i = 0; i < spectrumComplex.Length / 2; i++)
                spectrum[i] = (int)Math.Sqrt(spectrumComplex[i].Real * spectrumComplex[i].Real +
                                              spectrumComplex[i].Imaginary * spectrumComplex[i].Imaginary);
            return spectrum;
        }
    }
}
