using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace ACE
{

    public class Call
    {
        public List<string> prenameparts = new List<string>();
        public string Name;
        public List<LocalBuilder> parameters = new List<LocalBuilder>();
        public Call()
        {
            prenameparts.Clear();
            parameters.Clear();
        }
        public string PreName
        {
            get
            {
                return string.Join(".", prenameparts);
            }
        }
        public string FullName
        {
            get
            {
                return PreName + "." + Name;

            }
        }
        public Type[] paramtypes
        {
            get
            {
                return (from n in parameters select n.LocalType).ToArray();
            }

        }
    }
}