namespace ACE
{
    public class Assembly : Nodes
    {
        public Assembly(int at)
        {
            this.start = at;
        }
        private int start;
        private bool good = true;
        public bool GOOD
        {
            get { return good; }
            set { good = value; }
        }
        public bool BAD
        {
            get { return !good; }
            set { good = !value; }
        }
        public int AT
        {
            get
            {
                int at = start;
                foreach (Node N in this)
                {
                    if (N.token != null)
                    {
                        at = N.token.end;
                    }
                }
                return at;
            }
        }
        public void Reset()
        {
            this.start = this.AT;
            this.Clear();
            good = true;

        }

        public Node stray;
        public void Stray(Node node)
        {
            if (node != null)
            {
                if ((stray == null) || (stray.start < node.start))
                    stray = node;
            }
        }
        public void Fail(Node node)
        {
            Stray(node);
            BAD = true; 
        }
        public void Build()
        {
            int i;
            string snippet = "";
            Token T = null;
            foreach (Node N in this)
            {
                i = N.index;

                if (N.token != null)
                    T = N.token;

                if (N.snippet != null)
                    snippet = N.snippet;

                if (N.build != null)
                {
                    try
                    {
                        N.build(i, snippet);
                    }
                    catch (CompilerError E)
                    {
                        string s = E.Message;
                        if (T != null)
                        {
                            throw new CompilerError(string.Format("At character {0}, " + s, T.start));
                        }
                        else throw;
                    }
                }
                
            }
        }
        public int MAX
        {
            get
            {
                if (stray != null)
                    return stray.start;
                else
                    return 0;
            }
        }
        public void Consume(Assembly A)
        {
            this.GOOD = A.GOOD;
            Stray(A.stray);
            if (GOOD)
            {
                this.AddRange(A);
                A.start = A.AT;
                A.Clear();
            }
        }
        public void Consume(Branches B)
        {
            GOOD = B.GOOD;
            Stray(B.stray);
            if (GOOD)
            {
                AddRange(B.Best);
                start = B.Best.AT;
            }
            
        }
    }
}