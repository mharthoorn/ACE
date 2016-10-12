using System.Collections.Generic;
using System.Linq;

namespace ACE
{

    public class Definition 
    {
        public Definition(string name, BuildToken build = null)
        {
            this.Name = name;
            this.build = build;
            parser = Parsing.Sequence;
        }
        public string Name { get; set; }
        public string Expectation { get; private set; }
        public BuildToken build;
        public Definition Build(BuildToken build)
        {
            this.build = build;
            return this;
        }
        public List<IElement> elements = new List<IElement>();
        public Range range;
        public Parsing.Parser parser; 
        public virtual bool Parse(Lexer lexer, Assembly assembly)
        {
            return parser(this, lexer, assembly);
        }
        public string Dump()
        {
            string s = "";
            foreach (IElement e in elements)
            {
                if (s != "") s += " ";
                s += e.ToString();
            }
            return base.ToString() + " ::= " + s;
        }
        private bool ischecked = false;
        public void Check()
        {
            if (ischecked) return;
            ischecked = true;

            if (elements.Count() == 0)
                throw new CompilerError(string.Format("Error. Definition [{0}] has no elements.", Name));
            
            foreach (IElement E in elements)
            {
                if (E is Reference)
                    (E as Reference).Definition.Check();
            }
            
        }
        
        public Definition Define(Parsing.Parser parser, params object[] items)
        {
            this.parser = parser;
            foreach (object item in items)
            {
                if (item is string) elements.Add(new Literal((string)item));
                if (item is Definition) elements.Add(new Reference((Definition)item));
            }
            return this;
        }
        public Definition Sequence(params object[] items)
        {
            return Define(Parsing.Sequence, items);
        }
        public Definition ZeroOrMore(params object[] items)
        {
            Definition D = Define(Parsing.Range, items);
            D.range = Range.ZeroOrMore;
            return D;
        }
        public Definition OneOrMore(params object[] items)
        {
            Definition D = Define(Parsing.Range, items);
            D.range = Range.OneOrMore;
            return D;
        }
        public Definition Optional(params object[] items)
        {
            Definition D = Define(Parsing.Range, items);
            D.range = Range.ZeroOrOne;
            return D;
        }
        public Definition Switch(params object[] items)
        {
            return Define(Parsing.Switch, items);
        }
        public Definition Interlace(params object[] items)
        {
            return Define(Parsing.Interlace, items);
        }
        public Definition RegEx(string pattern)
        {
            elements.Add(new RegEx(pattern));
            return this;
        }
        public Definition Expect(string s)
        {
            this.Expectation = s;
            return this;
        }
    }
}