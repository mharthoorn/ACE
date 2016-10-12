namespace ACE
{

    public class Reference : IElement
    {
        public Reference(Definition definition)
        {
            this.Definition = definition;
        }
        public Definition Definition;
        public bool Parse(Definition D, Lexer lexer, Assembly assembly)
        {
            if (Definition.Parse(lexer, assembly))
            {
                assembly.New(D, this);
                return true;
            }
            else
            {
                return false;
            }
        }
        public string description
        {
            get
            {
                return Definition.Name;
            }
        }
    }
}