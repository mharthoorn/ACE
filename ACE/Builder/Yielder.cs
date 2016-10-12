using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ACE
{

    public class Yielder : Callable
    {
        private TypeBuilder type;
        private MethodBuilder method;
        public Yielder(TypeBuilder owner, string name) : base(owner, name) { }
        private void DefineFields()
        {

        }
        private void Define()
        {
            type = Owner.DefineNestedType(Name);
            DefineFields();
            method = type.DefineMethod("Run", MethodAttributes.Public | MethodAttributes.Static, typeof(void), new Type[] {typeof(int)});
        }
    }
}