using System.Diagnostics;

namespace ACE
{
    public class Node
    {
        public Node(Definition D, IElement E, Token T)
        {
            this.definition = D;
            this.element = E;
            this.token = T;
        }
        public Node(Definition D, IElement E)
        {
            this.definition = D;
            this.element = E;
        }
        public Node(Definition D)
        {
            this.definition = D;
        }
        public Definition definition = null;
        public IElement element = null;
        public Token token = null;
       
        public int index
        {
            get
            {
                return (element != null) ? definition.elements.IndexOf(element) + 1 : 0;
            }
        }
        public int start
        {
            get
            {
                if (token != null)
                {
                    return token.start;
                }
                return 0;
            }
        }
        public int end
        {
            get
            {
                if (token != null)
                {
                    return token.end;
                }
                return 0;
            }
        }
        public string snippet
        {
            get
            {
                return (token != null) ? token.snippet : null;
            }
        }
        public BuildToken build
        {
            get
            {
                return definition.build;
            }
        }
       
        public void Dump()
        {

            int idx = (element != null) ? definition.elements.IndexOf(element)+1 : 0;
            string s = string.Format("{0}[{1}]", definition.Name, idx);
            s = s.PadRight(15, '.');
            if (element != null)
            {
                s += string.Format("{0}", element.description);
                if (token != null)
                {
                    s = s.PadRight(35, '.');
                    s += string.Format("'{1}' ({0})", token.start, token.snippet);
                }
            }
            Trace.WriteLine(s);
        }
    }
}