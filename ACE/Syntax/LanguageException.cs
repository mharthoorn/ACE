using System;

namespace ACE
{

    class LanguageException : Exception
    {
        public LanguageException(string message) : base("Definition Error: " + message) { }
    }
}