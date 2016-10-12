using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ACE
{
    public class Lexer 
    {
        public string text;
        public List<string> skip = new List<string>();
        public Lexer()
        {
            //public string WhiteSpace = @"(\s)*";
            //public string LineComment = "//";
            skip.Add(@"(\s)*");
            skip.Add("//.*[\r\n]+");
            skip.Add("/\\*.*\\*/");

        }
        private bool SkipPattern(ref int at, string pattern)
        {
            pattern = @"\G" + pattern;
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = r.Match(text, at);
            if (m.Success) {
                at += m.Length;
            }
            return m.Success;
        }

        public int Skip(int at)
        {
            int i = at;
            do
            {
                at = i;
                foreach (string s in skip)
                {
                    SkipPattern(ref i, s);
                }
            }
            while (at < i);
            return at;
        }
        public bool Parse(int at, string pattern, ref Token token)
        {
            token.start = Skip(at);
            
            pattern = @"\G" + pattern;
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Match m = r.Match(text, token.start);
            if (m.Success)
            {
                token.end = token.start + m.Length;
                token.snippet = m.Value;
                return true;
            }
            else
            {
                return false;
            }
        }
        public string NextWordAt(int at)
        {
            at = this.Skip(at);
            string pattern = @"[^\s\n]*";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Match m = r.Match(text, at);
            if (m.Success)
            {
                return text.Substring(at, m.Length);
            }
            else
                return "";

        }
        public Coordinate PositionOf(int at) 
        {
            //x=colom, y=regel
            Coordinate p = new Coordinate(1, 1);
            for (int i = Math.Min(at, text.Length-1); i > 0; i--)
            {
                if (text[i] == '\n')
                {
                    p.y++;
                }
                else if (p.y == 1)
                {
                    p.x++;
                }
            }
            return p;
        }
    }
}
