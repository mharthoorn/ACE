# ACE
ACE. An Abstract Compiler Engine with which you can define a language with little overhead. 

*My father always says: if you want to learn a language build a compiler in it.*

### Language defininitions
You can define a language in code in a BNF'ish format. 
```csharp
            method
                .Sequence(decltype, identifier, "(", paramdecls, ")", braces)
                .Build(build.Method);

            paramdecls
                .Interlace(paramdecl, ",");

            paramdecl
                .Sequence(decltype, identifier)
                .Build(build.ParamDecl);

            repeat
                .Sequence("repeat", "(", expression, ")", block)
                .Build(build.Repeat);
```
It works with full look ahead. So it's not the fastest compiler. But it allows for virtually unlimited language complexity.

### Components
It has a lexer, parser, AST, a .NET binary producer and a .NET type checking system for expression parsing. 

### Builder
To build binaries it has a basic system that works recursive and is event driven.

### Example language
An language project "Moksi" is included as example.

