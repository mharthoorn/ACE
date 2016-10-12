using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE
{
    public static class Tools
    {
            public static string Param(string name)
            {
                string[] args = Environment.GetCommandLineArgs();
                string[] kv = new string[2];
            
                foreach (string arg in args)
                {
                    kv = arg.Split('=');
                    if (kv[0].ToLower() == name.ToLower())
                    {
                        return kv[1];
                    }
                }
                throw new Exception(String.Format("Parameter [{0}] does not exist", name));
            }
    }
}
