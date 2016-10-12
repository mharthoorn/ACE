using System.Collections.Generic;

namespace ACE
{
    public class Nodes : List<Node>
    {
        public void New(Definition D)
        {
            Node node = new Node(D, null);
            Add(node);
        }
        public Node New(Definition D, IElement E)
        {
            Node node = new Node(D, E);
            Add(node);
            return node;
        }
        public Node New(Definition D, IElement E, Token T)
        {
            Node node = new Node(D, E, T);
            Add(node);
            return node;
        }
        public void Dump()
        {
            foreach (Node N in this)
            {
                N.Dump();
            }
        }
    }
}
