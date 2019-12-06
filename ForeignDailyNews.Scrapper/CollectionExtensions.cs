using System.Collections.Generic;

namespace ForeignDailyNews.Scrapper
{
    public static class CollectionExtensions
    {
        public static void AddRange<TIn>(this HashSet<TIn> set, IEnumerable<TIn> items)
        {
            foreach(var item in items)
            {
                set.Add(item);
            }
        }
    }
}