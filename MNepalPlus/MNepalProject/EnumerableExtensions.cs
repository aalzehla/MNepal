using System.Collections;


namespace MNepalProject
{
    static class EnumerableExtensions
    {
        public static int Count(this IEnumerable source)
        {
            int res = 0;

            foreach (var item in source)
                res++;

            return res;
        }

        public static bool single(this IEnumerable source)
        {
            int res = 0;
            if (source == null)
            {
                return (res == 1);
            }
            foreach (var item in source)
                res++;

            return (res == 1);
        }

        public static object lastOne(this IEnumerable source)
        {
            int res = 0;
            var lastItem = new object();

            foreach (var item in source)
            {
                lastItem = item;
                res++;
            }
            return lastItem;
        }


    }
}