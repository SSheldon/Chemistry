using System.Collections.Generic;
using System.Linq;

namespace Chemistry.Structure
{
    public class Molecule
    {
        protected List<BondingAtom> atoms;
        public List<BondingAtom> Atoms
        {
            get { return atoms; }
        }

        public Molecule(List<BondingAtom> atoms)
        {
            this.atoms = atoms;
        }

        public Molecule(BondingAtom b)
        {
            atoms = new List<BondingAtom>();
            atoms.Add(b);
            AddChildren(b, null);
        }

        private void AddChildren(BondingAtom b, BondingAtom parent)
        {
            foreach (BondingAtom child in b)
            {
                if (child != parent)
                {
                    atoms.Add(child);
                    AddChildren(child, b);
                }
            }
        }

        public virtual Dictionary<Element, int> GetElementCounts()
        {
            Dictionary<Element, int> counts = new Dictionary<Element, int>();
            foreach (BondingAtom a in atoms)
            {
                if (counts.ContainsKey(a.Element)) counts[a.Element]++;
                else counts.Add(a.Element, 1);
            }
            return counts;
        }

        public virtual int GetElementCount(Element e)
        {
            return atoms.Count(a => a.Element == e);
        }

        public override string ToString()
        {
            string s = "";
            foreach (KeyValuePair<Element, int> count in GetElementCounts())
                s += count.Key.ToString() + (count.Value > 1 ? count.Value.ToString() : "");
            return s;
        }
    }
}