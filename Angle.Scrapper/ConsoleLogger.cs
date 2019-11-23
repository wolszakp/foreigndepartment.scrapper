using System;
using AngleSharp.Js;

namespace Angle.Scrapper
{
    public class ConsoleLogger : IConsoleLogger
    {
        public void Log(object[] values)
        {
            foreach (var item in values)
            {
                Console.WriteLine(item.ToString());
            }
        }
    }
}