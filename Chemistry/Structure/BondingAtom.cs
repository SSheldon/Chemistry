using System.Collections;
using System.Collections.Generic;

namespace Chemistry.Structure
{
    public struct Bond
    {
        BondingAtom target;
        public BondingAtom Target
        {
            get { return target; }
        }
        int order;
        public int Order
        {
            get { return order; }
        }

        public Bond(BondingAtom target, int order)
        {
            this.target = target;
            this.order = order;
        }
    }

    public class BondingAtom : IEnumerable
    {
        List<Bond> bonds;
        public List<Bond> Bonds
        {
            get { return bonds; }
        }
        Element element;
        public Element Element
        {
            get { return element; }
        }

        public BondingAtom(Element e)
        {
            this.element = e;
            bonds = new List<Bond>();
        }

        public void Bond(BondingAtom target, int order = 1)
        {
            Bond(new Bond(target, order));
        }

        public void Bond(Bond b)
        {
            bonds.Add(b);
            b.Target.bonds.Add(new Bond(this, b.Order));
        }

        public bool HasBond(Element e, int order)
        {
            foreach (Bond b in bonds)
                if (b.Target.Element == e && b.Order == order) return true;
            return false;
        }

        public IEnumerator GetEnumerator()
        {
            return new BondingAtomEnumerator(this);
        }

        private class BondingAtomEnumerator : IEnumerator
        {
            int index;
            List<Bond> bonds;

            public BondingAtomEnumerator(BondingAtom b)
            {
                index = -1;
                bonds = b.Bonds;
            }

            public object Current
            {
                get { return bonds[index].Target; }
            }

            public bool MoveNext()
            {
                index++;
                return index < bonds.Count;
            }

            public void Reset()
            {
                index = -1;
            }
        }
    }
}