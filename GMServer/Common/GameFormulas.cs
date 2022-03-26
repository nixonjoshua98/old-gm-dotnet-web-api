using System;

namespace GMServer.Common
{
    public static class GameFormulas
    {
        public static double SumNonIntegerPowerSeq(int start, int total, float exponent)
        {
            // https://math.stackexchange.com/questions/82588/is-there-a-formula-for-sums-of-consecutive-powers-where-the-powers-are-non-inte

            double Predicate(int startValue)
            {
                double x = Math.Pow(startValue, exponent + 1) / (exponent + 1);
                double y = Math.Pow(startValue, exponent) / 2;
                double z = Math.Sqrt(Math.Pow(startValue, exponent - 1));

                return x + y + z;
            }

            return Predicate(start + total) - Predicate(start);
        }
    }
}
