namespace ACE
{
    public interface IElement 
    {
        bool Parse(Definition D, Lexer lexer, Assembly nodes);
        string description { get; }
    }
}