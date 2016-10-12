namespace ACE
{
    public class Token
    {
        public Token(int start, int end, string snippet)
        {
            this.start = start;
            this.end = end;
            this.snippet = snippet;
        }
        public Token()
        {
        }
        public int start, end;
        public string snippet;
    }
}