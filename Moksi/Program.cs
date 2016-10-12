using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using ACE;

namespace Moksi
{
    class Program
    {
        static void SyntaxCheck()
        {

        }
        static void Build(string sourcefile, string targetfile)
        {
            try
            {
                MoksiEngine.Build(sourcefile, targetfile);
                Console.WriteLine("Compiled [{0}] to [{1}].", sourcefile, targetfile);
            }
            catch (CompilerError e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        static void Main(string[] args)
        {
            if (args.Count() > 0)
            {
                string file = args[0];
                string sourcefile = Path.ChangeExtension(file, ".mok");
                string targetfile = Path.ChangeExtension(file, ".exe");
                Build(sourcefile, targetfile);               
            }
            else
            {
                Console.WriteLine("Moksi 0.1 - Moksi .NET Compiler");
                Console.WriteLine("Use: moksi [filename(.mok)]");
            }
        }
    }
}
