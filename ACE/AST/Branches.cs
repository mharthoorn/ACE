using System.Collections.Generic;
using System.Linq;

namespace ACE
{
    public class Branches : List<Assembly>
    {
        private int at;
        public Branches(int at)
        {
            this.at = at;
        }
        public Assembly Branche()
        {
            Assembly nodes = new Assembly(at);
            Add(nodes);
            return nodes;
        }
        public Assembly Best
        {
            get
            {
                Assembly best = new Assembly(at); // leeg
                foreach (Assembly branche in this)
                {
                    if ((branche.GOOD) && (best.AT < branche.AT))
                    {
                        best = branche;
                    }
                }
                return best;
            }
        }
        public bool GOOD
        {
            get
            {
                return this.Any(branche => branche.GOOD);
            }
        }
        public Node stray
        {
            get 
            {
                Node N = null;
                foreach (Assembly branche in this)
                {
                    if (branche.stray != null)
                    {
                        if ((N == null) || (N.start < branche.stray.start))
                            N = branche.stray;
                    }
                }
                return N;

            }
        }     
    }
}