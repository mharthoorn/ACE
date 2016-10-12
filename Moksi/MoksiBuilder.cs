using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ACE;
using System.Xml;
using System.Reflection.Emit;
using System.Reflection;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Runtime.InteropServices;

namespace Moksi
{
    public class MoksiBuilder 
    {
        ACE.Builder builder;
        TypeBuilder mainclass;
        public Methods methods = new Methods();
        public Calls calls = new Calls();
        public ILGenerator IL
        {
            get
            {
                return methods.Current.Methodbuilder.GetILGenerator();
            }
        }        
        public MoksiBuilder(string filename) 
        {
             builder = new Builder(filename);
             mainclass = builder.Module.DefineType("MainClass", TypeAttributes.Class | TypeAttributes.Public);
        }
 
        Stack<Label> labels = new Stack<Label>();
        Stack<string> snippets = new Stack<string>();
        Stack<Type> ETS = new Stack<Type>(); // expression type stack
        int anonymouscounter = 0;
        public void Save()
        {
            builder.Save();
        }

        // XML tools
        FieldBuilder xml;
        Type TypeFromString(string s)
        {
            switch (s)
            {
                case "string":
                    return typeof(string);

                case "int":
                    return typeof(int);

                case "bool":
                    return typeof(bool);

                case "void":
                    return typeof(void);

                default:
                    throw new CompilerError(string.Format("Type [{0}] does not exist.", s));
            }
        }

        public void StoreVariable(string name)
        {
            LocalBuilder L;
            if (methods.Current.Locals.items.TryGetValue(name, out L))
            {
                IL.Emit(OpCodes.Stloc, L);
            }
            else
            {
                Type t; int idx;
                if (methods.Current.Parameters.Get(name, out t, out idx))
                {
                    ETS.Push(t);
                    IL.Emit(OpCodes.Starg, idx);
                }
                else
                {
                    throw new CompilerError(string.Format("Variable [{0}] does not exist.", name));
                }
            }

        }
        public void LoadVariable(string name)
        {
            LocalBuilder L;
            if (methods.Current.Locals.items.TryGetValue(name, out L))
            {
                ETS.Push(L.LocalType);
                IL.Emit(OpCodes.Ldloc, L);
            }
            else
            {
                Type t; int idx;
                if (methods.Current.Parameters.Get(name, out t, out idx))
                {
                    ETS.Push(t);
                    IL.Emit(OpCodes.Ldarg, idx);
                }
                else
                {
                    throw new CompilerError(string.Format("Variable [{0}] does not exist.", name));
                }
            }
        }
        public bool EqualParameters(Type[] caller, Type[] callee)
        {
            if (caller.Length != callee.Length)
                return false;

            for (int i = 0; i < caller.Count(); i++)
            {
                if (caller[i] != callee[i])
                {
                    if (callee[i] != typeof(object))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        protected bool FindLocalMethod(Call M, out MethodInfo methodinfo, out Type[] paramtypes)
        {
            Type[] caller = M.paramtypes;
            Type[] callee;
            foreach (Method m in methods)
            {
                if (m.Name == M.Name)
                {
                    callee = m.Parameters.Types;
                    if (EqualParameters(caller, callee))
                    {
                        methodinfo = m.Methodbuilder;
                        paramtypes = callee;
                        return true;
                    }
                }
            }
            methodinfo = null;
            paramtypes = null;
            return false;
        }
        protected bool FindMethod(Call M, out MethodInfo methodinfo, out Type[] paramtypes)
        {
            methodinfo = null;
            paramtypes = null;
            Type type = Type.GetType(M.PreName);
            if (type == null)
            {
                type = Type.GetType("System." + M.PreName);
            }
            if (type == null)
            {
                type = typeof(ACE.Tools);
            }
            if (type != null)
            {
                methodinfo = type.GetMethod(M.Name, M.paramtypes);
                if (methodinfo != null)
                {
                    paramtypes = (from p in methodinfo.GetParameters() select p.ParameterType).ToArray();
                    return true;
                }
                return false;
            }
            return false;
        }
        protected Type BuildCall(Call M)
        {
            Type[] paramtypes;
            MethodInfo methodinfo;
            bool found = FindLocalMethod(M, out methodinfo, out paramtypes);
            if (!found)
            {
                found = FindMethod(M, out methodinfo, out paramtypes);

            }
            if (found)
            {
                for (int i = 0; i < paramtypes.Length; i++)
                {
                    IL.Emit(OpCodes.Ldloc, M.parameters[i]);

                    Type sourcetype = M.paramtypes[i];
                    Type targettype = paramtypes[i];
                    if (targettype == typeof(object))
                    {
                        if (sourcetype != typeof(object))
                        {
                            IL.Emit(OpCodes.Box, sourcetype);
                        }
                    }
                }

                IL.Emit(OpCodes.Call, methodinfo);
                return methodinfo.ReturnType;
            }
            else
            {
                throw new CompilerError(string.Format("No method [{0}] was found with these parameters. ", M.FullName));
            }
        }
        
        Label TryCatchBlock;
        public void BuildCmdLineMethod()
        {
            Method m = methods.Push(new Method(mainclass, "cmdline"));
            m.ReturnType = typeof(string); // ReturnType
            m.Parameters.Add(typeof(string), "name");
            m.Define();
            LocalBuilder loc_args = IL.DeclareLocal(typeof(String[]));
            LocalBuilder loc_nv = IL.DeclareLocal(typeof(String[]));
            LocalBuilder loc_idx = IL.DeclareLocal(typeof(int));
            LocalBuilder loc_char = IL.DeclareLocal(typeof(char[]));
            LocalBuilder loc_equal = IL.DeclareLocal(typeof(bool));
            LocalBuilder loc_value = IL.DeclareLocal(typeof(string));
            Label RetEmpty = IL.DefineLabel();
            Label RetFound = IL.DefineLabel();

            IL.Emit(OpCodes.Nop);

            // 
            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Callvirt, typeof(string).GetMethod("ToLower"));

            IL.Emit(OpCodes.Call, typeof(Environment).GetMethod("GetCmdLineArgs"));
            IL.Emit(OpCodes.Stloc, loc_args);

            IL.Emit(OpCodes.Ldc_I4_0);
            IL.Emit(OpCodes.Stloc, loc_idx);

            IL.Emit(OpCodes.Stloc, loc_idx);
            IL.Emit(OpCodes.Ldloc, loc_args);
            IL.Emit(OpCodes.Ldelem_Ref);
            IL.Emit(OpCodes.Stloc, loc_nv);

            IL.Emit(OpCodes.Ldc_I4_1);
            IL.Emit(OpCodes.Newarr, typeof(Char));
            IL.Emit(OpCodes.Stloc, loc_char);

            IL.Emit(OpCodes.Ldloc, loc_char);
            IL.Emit(OpCodes.Ldc_I4_0);
            IL.Emit(OpCodes.Ldc_I4_S, '=');
            IL.Emit(OpCodes.Stelem_I2);

            IL.Emit(OpCodes.Ldloc, loc_char);
            IL.Emit(OpCodes.Callvirt, typeof(String).GetMethod("Split", new Type[] { typeof(char[]) }));
            IL.Emit(OpCodes.Stloc, loc_nv);

            IL.Emit(OpCodes.Ldloc, loc_nv);
            IL.Emit(OpCodes.Ldc_I4_0);
            IL.Emit(OpCodes.Ldelem_Ref);
            IL.Emit(OpCodes.Callvirt, typeof(string).GetMethod("ToLower"));
            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Call, typeof(string).GetMethod("Compare", new Type[] { typeof(string), typeof(string) }));

            IL.Emit(OpCodes.Ldc_I4, 0);
            IL.Emit(OpCodes.Ceq);
            IL.Emit(OpCodes.Stloc, loc_equal);
            IL.Emit(OpCodes.Ldloc, loc_equal);
            IL.Emit(OpCodes.Brtrue, RetFound);

            IL.Emit(OpCodes.Ldstr, "");
            IL.Emit(OpCodes.Ret);

            IL.MarkLabel(RetFound);


            IL.Emit(OpCodes.Ret);
            methods.Pop();
        }
        public void Program(int i, string snippet)
        {
            switch (i)
            {
                case 0: // start
                    //BuildCmdLineMethod();
                    Method method = methods.Push(new Method(mainclass, "Main"));
                    method.Define();
                    builder.Assembly.SetEntryPoint(method.Methodbuilder);

                    xml = mainclass.DefineField("xmlwriter", typeof(XmlTextWriter), FieldAttributes.Static | FieldAttributes.Public);
                    ConstructorInfo xml_ctor = typeof(XmlTextWriter).GetConstructor(new Type[] { typeof(TextWriter) });

                    IL.Emit(OpCodes.Call, typeof(Encoding).GetProperty("UTF8").GetGetMethod());
                    IL.Emit(OpCodes.Call, typeof(Console).GetProperty("OutputEncoding").GetSetMethod());

                    IL.Emit(OpCodes.Call, typeof(Console).GetProperty("Out").GetGetMethod());
                    IL.Emit(OpCodes.Newobj, xml_ctor);
                    IL.Emit(OpCodes.Stsfld, xml);

                    Type t = Enum.GetUnderlyingType(typeof(Formatting));
                    int value = (int)Convert.ChangeType(Formatting.Indented, t);

                    IL.Emit(OpCodes.Ldsfld, xml);
                    IL.Emit(OpCodes.Ldc_I4, Convert.ToInt32(Formatting.Indented));
                    IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetProperty("Formatting").GetSetMethod());

                    IL.Emit(OpCodes.Nop);
                    IL.Emit(OpCodes.Nop);
                    IL.Emit(OpCodes.Nop);

                    //xml document header.
                    //IL.Emit(OpCodes.Ldsfld, xml);
                    //IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("WriteStartDocument", Type.EmptyTypes));

                    TryCatchBlock = IL.BeginExceptionBlock();
                    break;

                case 1: // end
                    IL.Emit(OpCodes.Nop);
                    IL.Emit(OpCodes.Nop);
                    IL.Emit(OpCodes.Nop);

                    EmitCloseConnections();


                    IL.Emit(OpCodes.Ldsfld, xml);
                    IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("Close", Type.EmptyTypes));
                    IL.Emit(OpCodes.Call, typeof(SqlConnection).GetMethod("ClearAllPools", Type.EmptyTypes)); // anders duurt het even voor dat de app afsluit.

                    // Opvangen van een eventuele Exception
                    IL.BeginCatchBlock(typeof(Exception));
                    IL.Emit(OpCodes.Nop);
                    LocalBuilder exception = IL.DeclareLocal(typeof(Exception));
                    IL.Emit(OpCodes.Stloc, exception);
                    IL.Emit(OpCodes.Ldstr, "Caught: {0}\n");
                    IL.Emit(OpCodes.Ldloc, exception);
                    IL.Emit(OpCodes.Call, typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object) }));
                    IL.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
                    IL.EndExceptionBlock();

                    IL.Emit(OpCodes.Nop); // om zeker te zijn dat de leave exception block niet ins blauwe hineins springt.
                    IL.Emit(OpCodes.Ret);

                    mainclass.CreateType();
                    break;
            }
        }
        public void XmlWrite()
        {
            Type type = ETS.Pop();
            if (type != typeof(void))
            {
                EmitConvertToString(type);
                LocalBuilder L = IL.DeclareLocal(typeof(string));
                IL.Emit(OpCodes.Stloc, L);
                IL.Emit(OpCodes.Ldsfld, xml);
                IL.Emit(OpCodes.Ldloc, L);
                IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("WriteString", new Type[] { typeof(string) }));
            }
        }
        public void EmitWrite(string s = null)
        {
            if (s != null)
            {
                IL.Emit(OpCodes.Ldstr, s);
            }
            IL.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", new Type[] { typeof(string) }));
        }

        List<FieldBuilder> Connections = new List<FieldBuilder>();
        Stack<FieldBuilder> Readers = new Stack<FieldBuilder>();
        
        public void EmitCreateSqlConnection(string name) // verwacht connection string op de stack;
        {
            FieldBuilder c = mainclass.DefineField(name, typeof(SqlConnection), FieldAttributes.Static | FieldAttributes.Public);
            Connections.Add(c);

            ConstructorInfo ctor = typeof(SqlConnection).GetConstructor(new Type[] { typeof(string) });
            IL.Emit(OpCodes.Newobj, ctor);
            IL.Emit(OpCodes.Stsfld, c);

            IL.Emit(OpCodes.Ldsfld, c);
            IL.Emit(OpCodes.Call, typeof(SqlConnection).GetMethod("Open", Type.EmptyTypes));
        }
        public void EmitCloseConnections()
        {
            foreach (FieldBuilder b in Connections)
            {
                IL.Emit(OpCodes.Ldsfld, b);
                IL.Emit(OpCodes.Call, typeof(SqlConnection).GetMethod("Close", Type.EmptyTypes));

            }
        }

        int cmdidx = 0;
        public void EmitOpenSqlReader() // stack expects: 1 string (sqlstatement)
        {
            cmdidx++;
            string name = "command" + cmdidx.ToString();
            FieldBuilder field_SqlCommand = mainclass.DefineField(name, typeof(SqlCommand), FieldAttributes.Static | FieldAttributes.Public);

            IL.Emit(OpCodes.Ldsfld, Connections.Last());
            ConstructorInfo ctor = typeof(SqlCommand).GetConstructor(new Type[] { typeof(string), typeof(SqlConnection) });
            IL.Emit(OpCodes.Newobj, ctor);
            IL.Emit(OpCodes.Stsfld, field_SqlCommand);

            IL.Emit(OpCodes.Ldsfld, field_SqlCommand);
            IL.Emit(OpCodes.Call, typeof(SqlCommand).GetMethod("ExecuteReader", Type.EmptyTypes));

            name = "reader" + cmdidx.ToString();
            FieldBuilder field_SqlDataReader = mainclass.DefineField(name, typeof(SqlDataReader), FieldAttributes.Static | FieldAttributes.Public);
            IL.Emit(OpCodes.Stsfld, field_SqlDataReader);
            Readers.Push(field_SqlDataReader);
        }
        public void EmitCloseSqlReader()
        {
            FieldBuilder field_SqlDataReader = Readers.Pop();
            IL.Emit(OpCodes.Ldsfld, field_SqlDataReader);
            IL.Emit(OpCodes.Call, typeof(SqlDataReader).GetMethod("Close", Type.EmptyTypes));
        }
        public void Literal(int i, string snippet)
        {
            switch (i)
            {
                case 1: //pre: stack: <string>
                    LocalBuilder s = IL.DeclareLocal(typeof(string));
                    IL.Emit(OpCodes.Stloc, s);
                    IL.Emit(OpCodes.Ldsfld, xml);
                    IL.Emit(OpCodes.Ldloc, s);
                    IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("WriteString", new Type[] { typeof(string) }));
                    break;
            }
        }

        string conname;
        public void Connect(int i, string snippet)
        {
            switch (i) // connect ( <name> ) <string> ;
            {
                case 3:
                    conname = snippet;
                    break;
                case 5:
                    ETS.Pop();
                    EmitCreateSqlConnection(conname);
                    break;
            }
        }
        public void ForSql(int i, string snippet)
        {
            switch (i) // for <expression> do <block>
            {
                case 2:
                    EmitOpenSqlReader();

                    Label A = IL.DefineLabel();
                    IL.MarkLabel(A);
                    labels.Push(A);

                    IL.Emit(OpCodes.Ldsfld, Readers.Peek());
                    IL.Emit(OpCodes.Call, typeof(SqlDataReader).GetMethod("Read", Type.EmptyTypes)); // returns: boolean

                    Label B = IL.DefineLabel();
                    IL.Emit(OpCodes.Brfalse, B);
                    labels.Push(B);
                    break;

                case 4:
                    IL.Emit(OpCodes.Nop); // zodat de break niet naar zichzelf kan wijzen bij een leeg block.
                    Label B5 = labels.Pop();
                    Label A5 = labels.Pop();
                    IL.Emit(OpCodes.Br, A5);
                    IL.MarkLabel(B5);
                    EmitCloseSqlReader();
                    break;
            }
        }
        public void DbValue(int i, string snippet)
        {
            switch (i) // [ <name> ]
            {
                case 2:
                    if (Readers.Count > 0)
                    {
                        IL.Emit(OpCodes.Ldsfld, Readers.Peek());
                        IL.Emit(OpCodes.Ldstr, snippet);
                        IL.Emit(OpCodes.Call, typeof(IDataRecord).GetProperty("Item", new Type[] { typeof(string) }).GetGetMethod());
                        //EST.Push(typeof(object));
                        //IL.Emit(OpCodes.Unbox, typeof(string));
                        //EST.Push(typeof(string));
                        EmitConvertToString(typeof(object));
                        ETS.Push(typeof(string));

                    }
                    else
                    {
                        throw new CompilerError(string.Format("There is no query for field [{0}].", snippet));
                    }
                    break;
            }
        }

        Type paramtype;
        internal void ParamDecl(int i, string snippet)
        {
            switch (i)
            {
                case 1:
                    paramtype = TypeFromString(snippet);
                    break;

                case 2:
                    methods.Current.Parameters.Add(paramtype, snippet);
                    break;
            }
        }
        public void Method(int i, string snippet)
        {
            switch (i) // <type> <name> ( <params> ) <braces>
            {
                case 1:
                    snippets.Push(snippet);
                    break;

                case 2:
                    methods.Push(new Method(mainclass, snippet));
                    methods.Current.ReturnType = TypeFromString(snippets.Pop()); // ReturnType
                    break;

                case 5:
                    methods.Current.Define();
                    break;

                case 6:
                    if (methods.Current.ReturnType == typeof(void))
                    {
                        IL.Emit(OpCodes.Ret);
                    }
                    methods.Pop();
                    break;

            }
        }

        public void Wrap(int i, string snippet)
        {
            switch(i) // wrap <identifier> { atoms }
            {
                case 2:
                    Method m = methods.Push(new Method(mainclass, snippet));
                    m.ReturnType = typeof(void);
                    m.Parameters.Add(typeof(Action), "yieldaction");
                    m.Define();
                    break;

                case 5:
                    IL.Emit(OpCodes.Ret);
                    methods.Pop();
                    break;
            }
        }
        public void WrapCall(int i, string snippet)
        {
            Method m;
            switch (i) // <identifier> { <atoms> }
            {
                case 1:
                    calls.New();
                    calls.Current.Name = snippet;
                    break;
                case 2:
                    // Build anonymous function
                    string name = methods.Current.Name + "_wrapbody_" + (anonymouscounter++).ToString();
                    m = methods.Push(new Method(mainclass, name));
                    break;

                case 4:
                    // Finish anonymous function
                    IL.Emit(OpCodes.Ret);
                    m = methods.Current;
                    methods.Pop();
                    
                    // Pass anonymous function as parameter
                    LocalBuilder L = IL.DeclareLocal(typeof(Action));

                    IL.Emit(OpCodes.Ldnull);
                    IL.Emit(OpCodes.Ldftn, m.Methodbuilder);
                    IL.Emit(OpCodes.Newobj, typeof(Action).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
                    IL.Emit(OpCodes.Stloc, L);
                    
                    calls.Current.parameters.Add(L);
                    Type t = BuildCall(calls.Current);
                    calls.Pop();
                    break;
            }
        }
        public void Yield(int i, string snippet)
        {
            switch (i)
            {
                case 1:
                    // execute first argument (type Action - the wrap content) of current function 
                    IL.Emit(OpCodes.Ldarg_0);
                    IL.Emit(OpCodes.Callvirt, typeof(Action).GetMethod("Invoke"));
                    break;
            }
        }

        public void ReturnStatement(int i, string snippet)
        {
            switch (i) // return <expression>
            {
                case 2:
                    Type r = methods.Current.ReturnType;
                    Type t = ETS.Pop();
                    if (r == t)
                    {
                        // expects: expression value on stack, 
                        IL.Emit(OpCodes.Ret);
                    }
                    else
                    {
                        throw new CompilerError(string.Format("Expression is of the wrong type."));
                    }
                    break;
            }
        }
        public void Echo(int i, string snippet)
        {
            switch (i) // echo <string>
            {
                case 1:
                    XmlWrite();
                    break;
            }
        }
        public void Write(int i, string snippet)
        {
            // write ( <local> )
            switch (i)
            {
                case 3: // after local var is pushed
                    IL.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", new Type[] { typeof(string) }));
                    break;
            }
        }
        public void AsmDecl(int i, string snippet)
        {
            switch (i) // <name> .
            {
                case 1:
                    calls.Current.prenameparts.Add(snippet);
                    break;
            }
        }

        public void Param(int i, string snippet)
        {
            switch (i)
            {
                case 1:
                    Type paramtype = ETS.Pop(); // resulting of expression Stack Type
                    LocalBuilder L = IL.DeclareLocal(paramtype);
                    IL.Emit(OpCodes.Stloc, L);
                    calls.Current.parameters.Add(L);
                    break;
            }
        }
        public void Call(int i, string snippet)
        {
            switch (i) // <asmdecl*> <name> ( <params> )
            {
                case 0:
                    calls.New();
                    break;
                case 2:
                    calls.Current.Name = snippet;
                    break;
                case 5:
                    Type t = BuildCall(calls.Current);
                    ETS.Push(t);
                    calls.Pop();
                    break;
            }
        }

        public void Quotedstring(int i, string snippet)
        {
            switch (i)
            {
                case 1:
                    IL.Emit(OpCodes.Ldstr, snippet);
                    break;
            }
        }
        public void DeclareDefault(int i, string snippet)
        {
            switch (i) // <name>
            {
                case 1:
                    Type decltype = ETS.Peek();
                    LocalBuilder L = IL.DeclareLocal(decltype);
                    methods.Current.Locals.Add(snippet, L);
                    break;
            }
        }
        public void DeclareAssign(int i, string snippet)
        {
            switch (i) // <name> <declop> <expression>
            {
                case 1:
                    snippets.Push(snippet); // save the name
                    break;

                case 3: // expects: expression result on stack
                    Type exprtype = ETS.Pop();
                    Type decltype = ETS.Peek();
                    string declname = snippets.Pop();
                    if (exprtype == decltype)
                    {
                        LocalBuilder L = IL.DeclareLocal(decltype);
                        methods.Current.Locals.Add(declname, L);
                        IL.Emit(OpCodes.Stloc, L);
                    }
                    else
                    {
                        throw new CompilerError(string.Format("Declaration [{0}] is of different type the expression. ", declname));
                    }
                    break;


            }
        }
        public void Declarations(int i, string snippet)
        {
            switch (i) //<decltype> <declares>
            {
                case 1:
                    Type type = TypeFromString(snippet);
                    ETS.Push(type);
                    break;

                case 2:
                    ETS.Pop(); // omdat alle elementen in <declares> een Peek() hebben gedaan.
                    break;
            }
        }

        public void Repeat(int i, string snippet)
        {
            // repeat ( <expression> ) <block> 
            switch (i)
            {
                case 3: // stack expects: <expression>
                    Type t = ETS.Pop();
                    if (t == typeof(int))
                    {

                        Label LA = IL.DefineLabel();
                        labels.Push(LA);
                        IL.MarkLabel(LA);
                    }
                    else
                    {
                        throw new CompilerError(string.Format("Repeat expects an integer counter."));
                    }
                    break;
                case 5: // block-einde
                    IL.Emit(OpCodes.Ldc_I4, 1);
                    IL.Emit(OpCodes.Sub);
                    IL.Emit(OpCodes.Dup);
                    Label LB = labels.Pop();
                    IL.Emit(OpCodes.Brtrue, LB);
                    IL.Emit(OpCodes.Pop); // gooi de counter van de stack.
                    break;
            }
        }
        public void Tagset(int i, string snippet)
        {
            //  "<", identifier, tagattributes, ">", atoms, "<", "/", identifier, ">"
            switch (i)
            {
                case 2:
                    snippets.Push(snippet);
                    IL.Emit(OpCodes.Ldsfld, xml);
                    IL.Emit(OpCodes.Ldstr, snippet);
                    IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("WriteStartElement", new Type[] { typeof(string) }));
                    break;

                    //Tags.Push(new TagBuilder(snippet));

                case 4:
                    // WriteStartTag(Tag);
                    break;

                case 8:
                    string start = snippets.Pop();
                    if (snippet == start)
                    {
                        IL.Emit(OpCodes.Ldsfld, xml);
                        IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("WriteEndElement", Type.EmptyTypes));
                    }
                    else
                    {
                        throw new CompilerError(string.Format("Tags do not match: '<{0}> and </{1}>.", start, snippet));
                    }
                    break;
            }
        }

        public void TagStruct(int i, string snippet)
        {
            switch (i) //<tagname> <tagspecs> <tagstructattributes> <braces>
            {
                case 1:
                    IL.Emit(OpCodes.Ldsfld, xml);
                    IL.Emit(OpCodes.Ldstr, snippet);
                    IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("WriteStartElement", new Type[] { typeof(string) }));
                    break;
                case 4:
                    //Tags.Pop();
                    IL.Emit(OpCodes.Ldsfld, xml);
                    IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("WriteEndElement", Type.EmptyTypes));

                    break;
            }
        }
        public void TagDefine(int i, string snippet)
        {
            switch (i)  //<tagname> <tagspecs> <tagstructattributes> = <expression>
            {
                case 1:
                    IL.Emit(OpCodes.Ldsfld, xml);
                    IL.Emit(OpCodes.Ldstr, snippet);
                    IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("WriteStartElement", new Type[] { typeof(string) }));
                    break;

                case 3:
                    break;

                case 5:
                    Type type = ETS.Pop();
                    if (type != typeof(void))
                    {
                        EmitConvertToString(type);
                        LocalBuilder L = IL.DeclareLocal(typeof(string));
                        IL.Emit(OpCodes.Stloc, L);
                        IL.Emit(OpCodes.Ldsfld, xml);
                        IL.Emit(OpCodes.Ldloc, L);
                        IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("WriteString", new Type[] { typeof(string) }));
                    }
                    IL.Emit(OpCodes.Ldsfld, xml);
                    IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("WriteEndElement", Type.EmptyTypes));

                    break;
            }
        }

        public void TagId(int i, string snippet)
        {
            switch (i) // # <name>
            {
                case 2:
                    IL.Emit(OpCodes.Ldsfld, xml);
                    IL.Emit(OpCodes.Ldstr, "id");
                    IL.Emit(OpCodes.Ldstr, snippet);
                    IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("WriteAttributeString", new Type[] { typeof(string), typeof(string) }));
                    break;
            }
        }
        public void TagClass(int i, string snippet)
        {
            switch (i) // . <name>
            {
                case 2:
                    ///Tag.classes.Add(snippet);
                    IL.Emit(OpCodes.Ldsfld, xml);
                    IL.Emit(OpCodes.Ldstr, "class");
                    IL.Emit(OpCodes.Ldstr, snippet);
                    IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("WriteAttributeString", new Type[] { typeof(string), typeof(string) }));
                    break;
            }
        }
        public void TagAttribute(int i, string snippet)
        {
            switch (i) // <name> = <expression>
            {
                case 1:
                    IL.Emit(OpCodes.Ldsfld, xml);
                    IL.Emit(OpCodes.Ldstr, snippet);
                    break;
                case 3: //expects expression result on stack.
                    IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("WriteAttributeString", new Type[] { typeof(string), typeof(string) }));
                    break;
            }

        }

        public void If(int i, string snippet)
        {
            // if ( <bool> ) <block> (else)
            switch (i)
            {
                case 3: //<bool> //expects: 1 boolean op de stack
                    Label LA3 = IL.DefineLabel();
                    labels.Push(LA3);
                    IL.Emit(OpCodes.Brfalse, LA3);
                    break;
                case 5:
                    Label LA5 = labels.Pop();
                    Label LB5 = IL.DefineLabel();
                    labels.Push(LB5);
                    IL.Emit(OpCodes.Br, LB5);


                    IL.MarkLabel(LA5);
                    break;

                case 6:
                    IL.Emit(OpCodes.Nop);
                    Label LB6 = labels.Pop();
                    IL.MarkLabel(LB6);

                    break;
            }
        }
        public void While(int i, string snippet)
        {
            switch (i) // while ( <condition> ) <block> 
            {
                case 2:
                    Label A = IL.DefineLabel();
                    IL.MarkLabel(A);
                    labels.Push(A);
                    break;

                case 3: // assumes: bool true/false on the stack
                    Label B = IL.DefineLabel();
                    IL.Emit(OpCodes.Brfalse, B);
                    labels.Push(B);
                    break;

                case 5:
                    Label B5 = labels.Pop();
                    Label A5 = labels.Pop();
                    IL.Emit(OpCodes.Br, A5);
                    IL.MarkLabel(B5);
                    break;
            }
        }
        public void Bool(int i, string snippet)
        {
            switch (i)
            {
                case 1:
                    if (snippet.ToLower() == "true")
                    {
                        IL.Emit(OpCodes.Ldc_I4_1);
                    }
                    else if (snippet.ToLower() == "false")
                    {
                        IL.Emit(OpCodes.Ldc_I4_0);
                    }
                    break;
            }
        }

        public void StringOperation(string operation)
        {
            switch (operation)
            {
                case "+":
                    IL.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }));
                    ETS.Push(typeof(string));
                    break;

                case "==":
                    IL.Emit(OpCodes.Call, typeof(string).GetMethod("Compare", new Type[] { typeof(string), typeof(string) }));
                    IL.Emit(OpCodes.Ldc_I4_0);
                    IL.Emit(OpCodes.Ceq);
                    ETS.Push(typeof(bool));
                    break;
                case "!=":
                    IL.Emit(OpCodes.Call, typeof(string).GetMethod("Compare", new Type[] { typeof(string), typeof(string) }));
                    IL.Emit(OpCodes.Ldc_I4_0);
                    IL.Emit(OpCodes.Ceq);
                    IL.Emit(OpCodes.Ldc_I4_0);
                    IL.Emit(OpCodes.Ceq);
                    ETS.Push(typeof(bool));
                    break;

                default:
                    throw new CompilerError(string.Format("Operator [{0}] invalid for strings", operation));
            }
        }
        public void IntegerOperation(string operation)
        {
            switch (operation)
            {
                case "+":
                    IL.Emit(OpCodes.Add);
                    ETS.Push(typeof(int));
                    break;
                case "-":
                    IL.Emit(OpCodes.Sub);
                    ETS.Push(typeof(int));
                    break;
                case "*":
                    IL.Emit(OpCodes.Mul);
                    ETS.Push(typeof(int));
                    break;
                case "/":
                    IL.Emit(OpCodes.Div);
                    ETS.Push(typeof(int));
                    break;

                case "==":
                    IL.Emit(OpCodes.Ceq);
                    ETS.Push(typeof(bool));
                    break;

                case "!=":
                    IL.Emit(OpCodes.Ceq);
                    IL.Emit(OpCodes.Ldc_I4_0);
                    IL.Emit(OpCodes.Ceq);
                    ETS.Push(typeof(bool));
                    break;

                case "=<":
                    IL.Emit(OpCodes.Clt);
                    ETS.Push(typeof(bool));
                    break;


                default:
                    throw new CompilerError(string.Format("Operator [{0}] invalid for integers", operation));
            }
        }
        public void BoolOperation(string operation)
        {
            switch (operation)
            {
                case "==":
                    IL.Emit(OpCodes.Ceq);
                    ETS.Push(typeof(bool));
                    break;

                case "!=":
                    IL.Emit(OpCodes.Ceq);
                    IL.Emit(OpCodes.Ldc_I4_0);
                    IL.Emit(OpCodes.Ceq);
                    ETS.Push(typeof(bool));
                    break;

                case "||":
                    IL.Emit(OpCodes.Or);
                    ETS.Push(typeof(bool));
                    break;

                case "&&":
                    IL.Emit(OpCodes.And);
                    ETS.Push(typeof(bool));
                    break;

                default:
                    throw new CompilerError(string.Format("Operator [{0}] invalid for integers", operation));
            }
        }
        public void Operate(string operation, Type type)
        {
            if (type == typeof(string))
                StringOperation(operation);

            else if (type == typeof(int))
                IntegerOperation(operation);

            else if (type == typeof(bool))
                BoolOperation(operation);

        }
        public void Operation(int i, string snippet)
        {
            switch (i) // <expression> <operator> <expression>  
            {
                case 2:
                    snippets.Push(snippet);
                    break;
                case 3:
                    string operation = snippets.Pop();
                    Type type = ETS.Pop();
                    Type type2 = ETS.Pop();
                    if (type == type2)
                    {
                        Operate(operation, type);
                    }
                    else
                    {
                        throw new CompilerError(string.Format("Operator [{0}] invalid for different types", operation));
                    }

                    break;
            }
        }

        public void UniOperand(int i, string snippet)
        {
            switch (i) // <operator> <operand>
            {
                case 2:
                    IL.Emit(OpCodes.Not);
                    break;
            }
        }

        public void PreIncrement(int i, string snippet)
        {
            switch (i)
            {
                case 1:
                    snippets.Push(snippet); //operand
                    break;

                case 2:
                    string operand = snippets.Pop(), name = snippet;

                    IL.Emit(OpCodes.Ldc_I4_1);
                    if (operand == "++") IL.Emit(OpCodes.Add);
                    if (operand == "--") IL.Emit(OpCodes.Sub);
                    IL.Emit(OpCodes.Dup);
                    StoreVariable(name);
                    break;
            }
        }
        public void PostIncrement(int i, string snippet)
        {
            switch (i) // <variable> <incrementor>
            {
                case 1:
                    snippets.Push(snippet); //variable
                    break;

                case 2: // expects variabele on stack 

                    string name = snippets.Pop(), operand = snippet;
                    IL.Emit(OpCodes.Dup);
                    IL.Emit(OpCodes.Ldc_I4_1);

                    if (operand == "++") IL.Emit(OpCodes.Add);
                    if (operand == "--") IL.Emit(OpCodes.Sub);
                    StoreVariable(name);
                    break;
            }
        }
        public void IncrementStatement(int i, string snippet)
        {
            switch (i) // <increment>
            {
                case 1:
                    IL.Emit(OpCodes.Pop); // om het expressie resultaat van de stack te halen.
                    break;
            }
        }

        public void IniRef(int i, string snippet)
        {
            switch (i) // "@", "[", identifier, "]", identifier)
            {
                case 3:
                    snippets.Push(snippet);
                    break;

                case 5:
                    string section = snippets.Pop();
                    string name = snippet;

                    break;

            }
        }

        string cmdlineparam;
        internal void CmdLine(int i, string snippet)
        {
            switch (i) // @ ( expression , expression )
            {
                case 1:
                    cmdlineparam = snippet;
                    break;

                case 4:

                    break;
            }
        }

        public void ExpressionVariable(int i, string snippet)
        {
            switch (i)
            {
                case 1:
                    LoadVariable(snippet);
                    break;
            }
        }
        private string cmp;
        public void EmitConvertToString(Type from)
        {
            if (from != typeof(string))
            {
                MethodInfo m = typeof(Convert).GetMethod("ToString", new Type[] { from });
                IL.Emit(OpCodes.Call, m);
            }
        }
        public void Direct(int i, string snippet)
        {
            switch (i)
            {
                case 1:
                    Type type = ETS.Pop();
                    if (type != typeof(void))
                    {
                        EmitConvertToString(type);
                        LocalBuilder L = IL.DeclareLocal(typeof(string));
                        IL.Emit(OpCodes.Stloc, L);
                        IL.Emit(OpCodes.Ldsfld, xml);
                        IL.Emit(OpCodes.Ldloc, L);
                        IL.Emit(OpCodes.Call, typeof(XmlTextWriter).GetMethod("WriteString", new Type[] { typeof(string) }));
                    }
                    break;
            }
        }
        public void NumEq(int i, string snippet)
        {
            switch (i) // <value> <cmp> <value>
            {
                case 2:
                    cmp = snippet;
                    break;
                case 3:
                    switch (cmp)
                    {
                        case "==":
                            IL.Emit(OpCodes.Ceq);
                            break;
                        case "!=":
                            IL.Emit(OpCodes.Ceq);
                            IL.Emit(OpCodes.Ldc_I4, 0);
                            IL.Emit(OpCodes.Ceq);
                            break;

                        case "<":
                            IL.Emit(OpCodes.Clt);
                            break;

                        case ">":
                            IL.Emit(OpCodes.Cgt);
                            break;

                        case "<=":
                            IL.Emit(OpCodes.Cgt); // less or equal == not greater
                            IL.Emit(OpCodes.Ldc_I4, 0);
                            IL.Emit(OpCodes.Ceq);
                            break;

                        case ">=":
                            IL.Emit(OpCodes.Clt);
                            IL.Emit(OpCodes.Ldc_I4, 0);
                            IL.Emit(OpCodes.Ceq);
                            break;

                    }
                    break;
            }
        }
        public void StrEq(int i, string snippet)
        {
            switch (i) // <value> <cmp> <value>
            {
                case 2:
                    cmp = snippet;
                    break;
                case 3:
                    switch (cmp)
                    {
                        case "==":
                            IL.Emit(OpCodes.Call, typeof(string).GetMethod("Compare", new Type[] { typeof(string), typeof(string) }));
                            // returns 0 if equal
                            IL.Emit(OpCodes.Ldc_I4, 0);
                            IL.Emit(OpCodes.Ceq);
                            break;

                        case "!=":
                            IL.Emit(OpCodes.Call, typeof(string).GetMethod("Compare", new Type[] { typeof(string), typeof(string) }));
                            // returns 0 if equal
                            IL.Emit(OpCodes.Ldc_I4, 1);
                            IL.Emit(OpCodes.Ceq);
                            break;

                        case "<":
                            IL.Emit(OpCodes.Call, typeof(string).GetMethod("Compare", new Type[] { typeof(string), typeof(string) }));
                            // returns 0 if equal
                            IL.Emit(OpCodes.Ldc_I4, 0);
                            IL.Emit(OpCodes.Clt);
                            break;

                        case ">":
                            IL.Emit(OpCodes.Call, typeof(string).GetMethod("Compare", new Type[] { typeof(string), typeof(string) }));
                            // returns 0 if equal
                            IL.Emit(OpCodes.Ldc_I4, 0);
                            IL.Emit(OpCodes.Cgt);
                            break;

                        case "<=":
                            IL.Emit(OpCodes.Call, typeof(string).GetMethod("Compare", new Type[] { typeof(string), typeof(string) }));
                            // returns 0 if equal
                            IL.Emit(OpCodes.Ldc_I4, 0);
                            IL.Emit(OpCodes.Cgt); // less or equal == not greater
                            IL.Emit(OpCodes.Ldc_I4, 0);
                            IL.Emit(OpCodes.Ceq);
                            break;

                        case ">=":
                            IL.Emit(OpCodes.Call, typeof(string).GetMethod("Compare", new Type[] { typeof(string), typeof(string) }));
                            // returns 0 if equal
                            IL.Emit(OpCodes.Ldc_I4, 0);
                            IL.Emit(OpCodes.Clt); // less or equal == not greater
                            IL.Emit(OpCodes.Ldc_I4, 0);
                            IL.Emit(OpCodes.Ceq);
                            break;

                    }
                    break;
            }
        }
        public void Assignment(int i, string snippet)
        {
            switch (i) // set <name> = <expression>
            {
                case 2:
                    snippets.Push(snippet);
                    break;

                case 4:
                    string s = snippets.Pop();
                    LocalBuilder L = methods.Current.Locals.Get(s);
                    Type exprtype = ETS.Pop();
                    if (exprtype == L.LocalType)
                    {
                        IL.Emit(OpCodes.Stloc, L);
                    }
                    else
                    {
                        throw new CompilerError(string.Format("The assignment for [{0}] is of the wrong type.", s));
                    }
                    break;
            }
        }
        public void IntVal(int i, string snippet)
        {
            switch (i)
            {
                case 1:
                    ETS.Push(typeof(int));
                    int value = Convert.ToInt32(snippet);
                    IL.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }
        public void Strings(int i, string snippet)
        {
            switch (i) // <strelem> +
            {
                case 0:
                    IL.Emit(OpCodes.Ldstr, ""); // voor concatenation
                    break;

                case 1:
                    IL.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }));
                    break;
            }

        }
        public void StrVal(int i, string snippet)
        {
            switch (i)
            {
                case 2:
                    ETS.Push(typeof(string));
                    IL.Emit(OpCodes.Ldstr, snippet);
                    break;
            }
        }
        public void Increment(int i, string snippet)
        {
            switch (i) // <name> ++
            {
                case 1:
                    LocalBuilder L = methods.Current.Locals.Get(snippet);
                    IL.Emit(OpCodes.Ldloc, L);
                    IL.Emit(OpCodes.Ldc_I4, 1);
                    IL.Emit(OpCodes.Add);
                    IL.Emit(OpCodes.Stloc, L);

                    break;

            }
        }
    }
}
