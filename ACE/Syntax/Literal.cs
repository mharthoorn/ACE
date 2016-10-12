using System.Text.RegularExpressions;

namespace ACE
{

    public class Literal : RegEx
    {
        public Literal(string text) : base(Regex.Escape(text))
        {
            this.Text = text;
        }
        public string Text;
        public override string description
        {
            get
            {
                return "\"" + Text + "\"";
            }
        }
    }    
}