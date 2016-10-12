using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ACE
{

    public class Builder
    {
        public Builder(string filename)
        {
            this.filename = filename;
            AssemblyName assemblyname = new AssemblyName(filename);
            Assembly = domain.DefineDynamicAssembly(assemblyname, AssemblyBuilderAccess.Save);
            Module = Assembly.DefineDynamicModule("Module", filename);
        }
        string filename;
        AppDomain domain = AppDomain.CurrentDomain;
        public AssemblyBuilder Assembly;
        public ModuleBuilder Module;
        
        public void Save()
        {
            Assembly.Save(filename);
        }
    }
}
