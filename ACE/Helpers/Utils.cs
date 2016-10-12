using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACE
{
    static class Utils
    {
        public static string glue(string[] a, string gluestr)
        {
            string s = "";

            foreach (string e in a)
            {
                if (s != "") { s += gluestr; }
                s = s + e;
            }

            return s;
        }

    }
}
