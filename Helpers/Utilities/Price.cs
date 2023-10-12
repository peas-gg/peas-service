using System;
namespace PEAS.Helpers.Utilities
{
    public class Price
    {
        public const int FreePrice = 0;
        public const int MinPrice = 10_00;
        public const int MaxPrice = 5000_00;

        public static string Format(long value)
        {
            double valueToDouble = value * 0.01;
            return String.Format("{0:0.00}", valueToDouble);
        }
    }
}