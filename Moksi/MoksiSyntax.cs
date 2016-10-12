using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ACE;

namespace Moksi
{
    class MoksiSyntax : Syntax
    {
        private MoksiBuilder build;
        public MoksiSyntax(MoksiBuilder builder)
        {
            this.build = builder;
            Define();
        }
        public void Define()
        {
            Definition program = new Definition("program");
            Definition atoms = new Definition("atoms");
            Definition atom = new Definition("atom");
            Definition structure = new Definition("structure");
            Definition fullstatement = new Definition("statement;");
            Definition direct = new Definition("direct");
            Definition method = new Definition("method");
            Definition wrap = new Definition("wrap");
            Definition yield = new Definition("yield");
            Definition paramdecls = new Definition("paramdecls");
            Definition paramdecl = new Definition("paramdecl");
            Definition repeat = new Definition("repeat");
            Definition While = new Definition("while");
            Definition ReturnStatement = new Definition("return");
            Definition tagset = new Definition("tagset");
            Definition If = new Definition("if");
            Definition Else = new Definition("else");
            //Definition tagstruct = new Definition("tagstruct");
            Definition tagdefine = new Definition("tagdefine");
            Definition tagspecs = new Definition("tagspecs");
            Definition statement = new Definition("statement");
            Definition connect = new Definition("connect");
            Definition forsql = new Definition("forsql");
            Definition block = new Definition("block");
            Definition optionalbraces = new Definition("braces?");
            Definition braces = new Definition("braces");
            Definition call = new Definition("call");
            Definition wrapcall = new Definition("Wrap call");
            Definition declarations = new Definition("declarations");
            Definition declares = new Definition("declares");
            Definition declaration = new Definition("declaration");
            Definition declaredefault = new Definition("declaredefault");
            Definition declareassign = new Definition("declareassign");
            Definition decltype = new Definition("decltype");
            Definition assignment = new Definition("assignment");
            Definition assignop = new Definition("assignop");
            Definition expression = new Definition("expression");
            Definition operation = new Definition("operation");
            Definition operand = new Definition("operand");
            Definition UniOperand = new Definition("UniOperand");
            Definition UniOperator = new Definition("UniOperator");
            Definition enclosedexpression = new Definition("enclosed-expr");
            Definition iniref = new Definition("ini");
            Definition cmdline = new Definition("cmdline");
            Definition bin_operator = new Definition("exop");
            Definition value = new Definition("value");
            Definition dbvalue = new Definition("dbvalue");
            Definition Variable = new Definition("var");
            Definition IncrementStatement = new Definition("Inc statement");
            Definition Increment = new Definition("Increment");
            Definition PreIncrement = new Definition("PreIncrement");
            Definition PostIncrement = new Definition("PostIncrement");
            Definition Incrementor = new Definition("Incrementor");
            Definition echo = new Definition("echo");
            Definition write = new Definition("write");
            Definition asmdecl = new Definition("asmdecl");
            Definition parameters = new Definition("params");
            Definition param = new Definition("param");
            Definition tagstructattributes = new Definition("tag-struct-attributes");
            Definition tagattributes = new Definition("tagattributes");
            Definition tagattribute = new Definition("tagattribute");
            Definition tagname = new Definition("tagname");
            Definition tagid = new Definition("tagid");
            Definition tagclass = new Definition("tagclass");
            Definition strings = new Definition("strings");
            Definition strelem = new Definition("strelem");
            Definition condition = new Definition("condition");
            Definition Bool = new Definition("bool");
            Definition equation = new Definition("equation");
            Definition numeq = new Definition("numeq");
            Definition numexp = new Definition("numexp");
            Definition streq = new Definition("streq");
            Definition strexp = new Definition("strexp");
            Definition cmp = new Definition("cmp");
            Definition strcmp = new Definition("strcmp");
            Definition True = new Definition("true");
            Definition False = new Definition("false");
            Definition intval = new Definition("intval");
            Definition strval = new Definition("strval");
            Definition number = new Definition("number");
            Definition identifier = new Definition("name");
            Definition StringLiteral = new Definition("String");
            Definition StringContent = new Definition("Stringcontent");
            Definition TypeInt = new Definition("integer");

            this.ROOT = program;

            program
                .Sequence(atoms)
                .Build(build.Program);

            atoms
                .ZeroOrMore(atom);

            atom
                .Switch(method, wrap, structure, fullstatement, echo);

            structure
                .Switch(repeat, While, If, wrapcall, /* tagstruct,*/ tagset, connect, forsql);

            fullstatement
                .Sequence(statement, ";");

            direct
                .Sequence(expression)
                .Build(build.Direct);

            method
                .Sequence(decltype, identifier, "(", paramdecls, ")", braces)
                .Build(build.Method);

            wrap
                .Sequence("wrap", identifier, "{", atoms, "}")
                .Build(build.Wrap);

            yield
                .Sequence("yield")
                .Build(build.Yield);


            paramdecls
                .Interlace(paramdecl, ",");

            paramdecl
                .Sequence(decltype, identifier)
                .Build(build.ParamDecl);

            repeat
                .Sequence("repeat", "(", expression, ")", block)
                .Build(build.Repeat);

            While
                .Sequence("while", "(", expression, ")", block)
                .Build(build.While);

            ReturnStatement
                .Sequence("return", expression)
                .Build(build.ReturnStatement);

            tagset
                .Sequence("<", identifier, tagattributes, ">", atoms, "<", "/", identifier, ">")
                .Build(build.Tagset);

            tagattributes
                .Interlace(tagattribute, ",");

            tagattribute
               .Sequence(identifier, "=", expression)
               .Build(build.TagAttribute);

            If
                .Sequence("if", "(", expression, ")", block, Else)
                .Build(build.If);

            Else
                .Optional("else", block);


            /*
            tagstruct
                .Sequence(tagname, tagspecs, tagstructattributes, optionalbraces)
                .Build(build.TagStruct);
            */

            tagdefine
                .Sequence(tagname, tagspecs, tagstructattributes, "=", expression)
                .Build(build.TagDefine);

            tagspecs
                .Sequence(tagid, tagclass);

            tagid
                .Optional("#", identifier)
                .Build(build.TagId);

            tagclass
                .ZeroOrMore(".", identifier)
                .Build(build.TagClass);

            tagstructattributes
                .Optional("(", tagattributes, ")");

            statement
                .Switch(call, declarations, assignment, ReturnStatement, IncrementStatement, tagdefine, yield);

            connect
                .Sequence("db", "(", identifier, ")", expression, ";")
                .Build(build.Connect);

            forsql
                .Sequence("for", expression, "do", block)
                .Build(build.ForSql);

            block
                .Switch(braces, tagset);

            optionalbraces
                .Switch(braces, ";");

            braces
                .Sequence("{", atoms, "}");

            call
                .Sequence(asmdecl, identifier, "(", parameters, ")")
                .Build(build.Call);

            wrapcall
                .Sequence(identifier, "{", atoms, "}")
                .Build(build.WrapCall);

            declarations
                .Sequence(decltype, declares)
                .Build(build.Declarations);

            declares
                .Interlace(declaration, ",");

            declaration
                .Switch(declareassign, declaredefault);

            declaredefault
                .Sequence(identifier)
                .Build(build.DeclareDefault);

            declareassign
                .Sequence(identifier, assignop, expression)
                .Build(build.DeclareAssign);

            decltype
                .Switch("void", "string", "int");

            assignment
                .Sequence("set", identifier, assignop, expression)
                .Build(build.Assignment);

            assignop
                .Switch("=", "+=", "-=");

            expression
                .Switch(operation, operand);

            operation
                .Sequence(operand, bin_operator, expression)
                .Build(build.Operation);

            operand
                .Switch(call, Variable, value, dbvalue, enclosedexpression, iniref, Increment, UniOperand);

            UniOperand
                .Sequence(UniOperator, operand)
                .Build(build.UniOperand);

            UniOperator
                .Switch("!", "not");

            enclosedexpression
                .Sequence("(", expression, ")");

            iniref
                .Sequence("@", "[", identifier, "]", identifier)
                .Build(build.IniRef)
                .Expect("ini file property reference");

            cmdline
                .Sequence("@", "(", expression, ", ", expression, ")")
                .Build(build.CmdLine);

            bin_operator
                .Switch("+", "-", "*", "/", "==", "!=", "=<", "||", "&&");

            value
                .Switch(strval, intval);

            dbvalue
                .Sequence("[", identifier, "]")
                .Build(build.DbValue);

            Variable
                .Sequence(identifier)
                .Build(build.ExpressionVariable);

            IncrementStatement
                .Sequence(Increment)
                .Build(build.IncrementStatement);

            Increment
                .Switch(PreIncrement, PostIncrement);

            PreIncrement
                .Sequence(Incrementor, Variable)
                .Build(build.PreIncrement);

            PostIncrement
                .Sequence(Variable, Incrementor)
                .Build(build.PostIncrement);

            Incrementor
                .Switch("++", "--");

            echo
                .Sequence(expression)
                .Build(build.Echo);

            write
                .Sequence("write", "(", strings, ")")
                .Build(build.Write);

            asmdecl
                .ZeroOrMore(identifier, ".")
                .Build(build.AsmDecl);

            parameters
                .Interlace(param, ",");

            param
                .Sequence(expression)
                .Build(build.Param);

            tagname
                .Switch("body", "html", "div", "span", "table", "tr", "td", "a", "h1", "h2", "h3", "h4", "h5", "h6", "b", "br", "ul", "li", "meta", "p");

            strings
                .Interlace(strelem, "+")
                .Build(build.Strings);

            Bool
                .Switch(equation, True, False);

            equation
                .Switch(numeq, streq)
                .Build(build.NumEq);

            numeq
                .Sequence(numexp, cmp, numexp);

            numexp
                .Switch(intval, Variable);

            streq
                .Sequence(strexp, strcmp, strexp)
                .Build(build.StrEq);

            strexp
                .Switch(strval, Variable);

            True
                .Sequence("true")
                .Build(build.Bool);

            False
                .Sequence("false")
                .Build(build.Bool);

            intval
                .Sequence(number)
                .Build(build.IntVal);

            strval
                .Sequence("\"", StringContent, "\"")
                .Build(build.StrVal);

            StringContent
                .RegEx(@"[^""]*");

            number
                .RegEx("\\d+");

            identifier
                .RegEx("[A-Za-z_][A-Za-z0-9_]*")
                .Expect("an identifier");
        }
    }
}
