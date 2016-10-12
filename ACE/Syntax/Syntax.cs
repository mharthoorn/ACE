using System.Collections.Generic;

namespace ACE
{
    public delegate void BuildToken(int Index, string snippet);
    public enum DefinitionType { Sequence, Switch, Alternate, Delimiter, RegularExpression, Literal }
    public enum Range { ZeroOrMore, OneOrMore, ZeroOrOne, One }

    public class Syntax
    {
        public List<Definition> definitions = new List<Definition>();
        public Definition ROOT = null; 
        public void DefineRoot(Definition root)
        {
            this.ROOT = root;
        }
        public bool Parse(Lexer lexer, Assembly assembly)
        {
            return this.ROOT.Parse(lexer, assembly);
        }
        public void Check()
        {
            ROOT.Check();
        }
    }
}
