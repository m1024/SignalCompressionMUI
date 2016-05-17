namespace SignalCompressionMUI.Models.Algorithms.Spectrum
{
    public struct Complex
    {
        public double Real;
        public double Imaginary;

        public Complex(double r, double i)
        {
            Real = r;
            Imaginary = i;
        }

        public static Complex operator +(Complex c1, Complex c2)
        {
            return new Complex(c1.Real + c2.Real, c1.Imaginary + c2.Imaginary);
        }

        public static Complex operator -(Complex c1, Complex c2)
        {
            return new Complex(c1.Real - c2.Real, c1.Imaginary - c2.Imaginary);
        }

        public static Complex operator *(Complex c1, Complex c2)
        {
            return new Complex(c1.Real * c2.Real - c1.Imaginary * c2.Imaginary,
                c1.Real * c2.Imaginary + c2.Real * c1.Imaginary);
        }
    }
}
