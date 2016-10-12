using System.Reflection;
using System.Reflection.Emit;

namespace ACE
{
    public class Method : Callable
    {
        public MethodBuilder Methodbuilder;
        public Method(TypeBuilder owner, string name) : base(owner, name) { }
        public void Define()
        {
            Methodbuilder = Owner.DefineMethod(Name, MethodAttributes.Public | MethodAttributes.Static, ReturnType, Parameters.Types);
        }
    }
}