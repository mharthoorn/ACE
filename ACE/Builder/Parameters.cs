using System;
using System.Collections.Generic;
using System.Linq;

namespace ACE
{

    public class Parameters
    {
        public Dictionary<string, Type> Definitions = new Dictionary<string, Type>();
        public void Add(Type type, string name)
        {
            Definitions.Add(name, type);
        }
        public Type[] Types
        {
            get
            {
                return (from d in Definitions select d.Value).ToArray();
            }
        }
        public bool Get(string name, out Type type, out int index)
        {
            for (int i = 0; i < Definitions.Count(); i++)
            {
                KeyValuePair<string, Type> KV = Definitions.ElementAt(i);
                if (KV.Key == name)
                {
                    type = KV.Value;
                    index = i;
                    return true;
                }

            }
            type = null;
            index = -1;
            return false;

        }
    }
}