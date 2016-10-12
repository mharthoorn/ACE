namespace ACE
{

    public static class Parsing
    {
        public delegate bool Parser(Definition D, Lexer lexer, Assembly nodes);
        public static bool Sequence(Definition D, Lexer lexer, Assembly assembly) 
        {
            Debug.Start("Sequence", D, assembly);
            bool OK = true;
            assembly.New(D);
            foreach (IElement E in D.elements)
            {
                OK = E.Parse(D, lexer, assembly);
                if (!OK) break;
            }
            Debug.Stop("Sequence", D, assembly);
           
            return assembly.GOOD;
        }
        public static bool Switch(Definition D, Lexer lexer, Assembly assembly)
        {
            Debug.Start("Switch", D, assembly);
            Branches B = new Branches(assembly.AT);
            foreach (IElement E in D.elements)
            {
                Assembly A = B.Branche();
                E.Parse(D, lexer, A);
            }
            
            assembly.Consume(B);
            Debug.Stop("Switch", D, assembly);
            return assembly.GOOD;
        }
        public static bool Range(Definition D, Lexer lexer, Assembly assembly)
        {
            Debug.Start("Range", D, assembly);
            int count = 0;
            bool OK = true;
            while (OK && Ranges.InRange(count+1, D.range))
            {
                Assembly A = new Assembly(assembly.AT);
                OK = Sequence(D, lexer, A);
                if (OK) count++;
                assembly.Consume(A);
            }
            Debug.Stop("Range", D, assembly);
            assembly.GOOD = (Ranges.InRange(count, D.range));
            return assembly.GOOD;
        }
        public static bool Interlace(Definition D, Lexer lexer, Assembly assembly)
        {
            Debug.Start("Interlace", D, assembly);
            int i = 0; 
            bool OK;
            Assembly A = new Assembly(assembly.AT);
            A.New(D);
            do
            {
                OK = D.elements[i].Parse(D, lexer, A);
                if (OK & (i==0))
                {
                    assembly.Consume(A);
                    A.Reset();
                }
                i = (i + 1) % 2;
            }
            while (OK);
            Debug.Stop("Interlace", D, assembly);
            return assembly.GOOD; 
        }
    }
}