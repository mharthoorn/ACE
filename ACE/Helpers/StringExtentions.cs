using System.Linq;

namespace ACE
{
    public static class StringExtentions
    {
        public static string Repeat(this string input, int count)
        {
            return string.Concat(Enumerable.Repeat(input, count));
        }
    }
}