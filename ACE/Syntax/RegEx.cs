namespace ACE
{

    public class RegEx : IElement
    {
        protected string pattern;
        public RegEx(string pattern)
        {
            this.pattern = pattern;
        }
        public bool Parse(Definition D, Lexer lexer, Assembly assembly)
        {
            Debug.Start("Literal '"+pattern+"'", D, assembly);
            Node node = new Node(D, this);
            node.token = new Token();
            if (lexer.Parse(assembly.AT, pattern, ref node.token))
            {
                
                assembly.Add(node);
                Debug.Stop("Literal", D, assembly);
                return true;
            }
            else 
            {
                assembly.Fail(node);
                Debug.Stop("Literal", D, assembly);
                return false;
            }
        }
        public virtual string description
        {
            get
            {
                return "/" + pattern + "/";
            }
        }
    }
}