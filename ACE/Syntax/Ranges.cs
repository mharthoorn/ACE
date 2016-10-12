namespace ACE
{

    public static class Ranges
    {
        public static Range FromString(string s)
        {
            switch (s)
            {
                case "*": return Range.ZeroOrMore;
                case "+": return Range.OneOrMore;
                case "?": return Range.ZeroOrOne;
                default: return Range.One;
            }
        }
        public static string ToString(Range q)
        {
            switch (q)
            {
                case Range.ZeroOrMore: return "*";
                case Range.OneOrMore: return "+";
                case Range.ZeroOrOne: return "?";
                case Range.One: return "";
                default: return "";
            }

        }
        public static bool InRange(int i, Range q)
        {
            switch (q)
            {
                case Range.ZeroOrMore: return (i >= 0);
                case Range.OneOrMore: return (i >= 1);
                case Range.ZeroOrOne: return (i >= 0 & i <= 1);
                case Range.One: return (i == 1);
                default: return false;
            }
        }
    }
}