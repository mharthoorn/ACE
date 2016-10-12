using System;
using System.Reflection.Emit;

namespace ACE
{

    public class Callable
    {
        protected TypeBuilder Owner;
        public string Name;
        public Locals Locals = new Locals();
        public Parameters Parameters = new Parameters();
        public Type ReturnType = typeof(void);
        public Callable(TypeBuilder owner, string name)
        {
            this.Owner = owner;
            this.Name = name;
        }
    }
}