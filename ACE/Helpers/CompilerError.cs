using System;

namespace ACE
{
    public class CompilerError : Exception
    {
        public CompilerError(string message)
            : base(message)
        {

        }
    }
}