using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using ACE;


namespace Moksi
{
    static class MoksiEngine
    {
        private static string ReadFile(string filename)
        {
            StreamReader streamReader = new StreamReader(filename);
            string text = streamReader.ReadToEnd();
            streamReader.Close();
            return text;
        }
        public static void Build(string sourcefile, string targetfile)
        {
            MoksiBuilder build = new MoksiBuilder(targetfile);
            MoksiSyntax syntax = new MoksiSyntax(build);
            //syntax.Check();

            Compiler compiler = new Compiler(syntax);

            string code = ReadFile(sourcefile);
            compiler.TryCompile(code);
            build.Save();
        }
    }
}
