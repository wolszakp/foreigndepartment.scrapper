using System;

namespace Scrapper.Runner
{
    public static class ByteSizeExtensions
    {
        public enum SizeUnits
        {
            Byte, KB, MB, GB, TB, PB, EB, ZB, YB
        }

        public static string ToSize(this long value, SizeUnits unit)
        {
            return (value / Math.Pow(1024, (long)unit)).ToString("0.00");
        }
    }
}