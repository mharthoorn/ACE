using System.Collections.Generic;

namespace ACE
{

    public class Calls : Stack<Call>
    {
        public void New()
        {
            this.Push(new Call());
        }
        public Call Current
        {
            get
            {
                return this.Peek();
            }
        }
    }
}