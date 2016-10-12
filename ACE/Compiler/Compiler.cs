using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection.Emit;

namespace ACE
{
    public class Compiler
    {
        Lexer lexer;
        Assembly assembly;
        Syntax syntax;
        public string message;
        public Compiler(Syntax syntax)
        {
            this.syntax = syntax;
            lexer = new Lexer();
            assembly = new Assembly(0);
        }
        public void Parse()
        {
            bool OK = syntax.ROOT.Parse(lexer, assembly);
            assembly.Dump();
            if (OK)
            {
                int at = lexer.Skip(assembly.AT);
                if (at < assembly.MAX)
                {
                    OK = false;
                }
                else
                    if (at < lexer.text.Length)
                    {
                        throw new CompilerError(string.Format("Unexpected token at end of file: {0}.", lexer.NextWordAt(at)));
                    }
            }
            if (!OK)
            {
                Node N = assembly.stray;
                if (N != null)
                {
                    string expect = N.definition.Expectation ?? N.element.description;

                    string s = lexer.NextWordAt(assembly.MAX);
                    throw new CompilerError(string.Format("Syntax error. Expected {0}. Found '{1}'. At {2}", expect, s, N.token.start));
                }
                else
                {


                }
            }
        }
        public void TryCompile(string code)
        {
            this.lexer.text = code;
            Parse();
            assembly.Build();
        }
        public bool Compile(string code)
        {
            try
            {
                TryCompile(code);
                message = "Syntax correct.";
                return true;
            }
            catch (Exception e)
            {
                message = e.ToString();
                return false;
            }
        }
    }
}

