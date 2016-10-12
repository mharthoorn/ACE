using System.Diagnostics;

namespace ACE
{
    public static class Debug
    {
        static int indent = 0;
        public static void Start(string kind, Definition D, Assembly A)
        {

            string spaces = "|  ".Repeat(indent);
            Trace.WriteLine(spaces + "+ " + string.Format("[START] {0}. {1}: {2} [{3}] ({4})", A.AT, kind, D.Name, A.GOOD ? "GOOD" : "bad", A.MAX));
            indent++;
        }
        public static void Stop(string kind, Definition D, Assembly A)
        {
            indent--;
            string spaces = "|  ".Repeat(indent);
            Trace.WriteLine(spaces + "- " + string.Format("[-STOP] {0}. {1}: {2} [{3}] ({4})", A.AT, kind, D.Name, A.GOOD ? "GOOD" : "bad", A.MAX));
            
            
        }
    }
}