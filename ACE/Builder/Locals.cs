using System.Collections.Generic;
using System.Reflection.Emit;

namespace ACE
{

    public class Locals
    {
        public Dictionary<string, LocalBuilder> items = new Dictionary<string, LocalBuilder>();
        public void Add(string name, LocalBuilder L)
        {
            try
            {
                items.Add(name, L);
            }
            catch
            {
                throw new CompilerError(string.Format("Local variable [{0}] is already defined.", name));
            }
        }
        public LocalBuilder Get(string name)
        {
            LocalBuilder L;
            if (items.TryGetValue(name, out L))
            {
                return L;
            }
            else
            {
                throw new CompilerError(string.Format("Local variable [{0}] does not exist.", name));

            }
        }
    }
}