using System;
using System.Collections.Generic;
using System.Linq;

namespace Chemistry.Structure.Organic
{
    public enum Group { Alkyl, Alkenyl, Alkynyl, Amine, Hydroxyl, Carbonyl, Formyl, Carboxyl }

    public class OrganicMolecule : Molecule
    {
        BondingAtom[] chain;
        Dictionary<Group, List<int>> groups;

        public BondingAtom this[int i]
        {
            get { return chain[i]; }
        }

        public OrganicMolecule(Molecule m)
            : base(m.Atoms)
        {
            Initialize();
        }

        public OrganicMolecule(BondingAtom a)
            : base(a)
        {
            Initialize();
        }

        void Initialize()
        {
            chain = new CarbonChainFinder(atoms).GetChain();
            AssignGroupLocations();
            foreach (KeyValuePair<Group, List<int>> kvp in groups)
            {
                Console.Write(kvp.Key.ToString() + " ");
                foreach (int i in kvp.Value)
                    Console.Write(i + " ");
                Console.WriteLine();
            }
            WriteStructure();
        }

        public void WriteStructure()
        {
            for (int i = 0; i < chain.Length; i++)
            {
                Console.Write("C" + HydrogensToString(chain[i]));
                foreach (Bond bond in chain[i].Bonds)
                {
                    if (!IsInChain(bond.Target, i))
                    {
                        switch (bond.Order)
                        {
                            case 1:
                                Console.Write("-");
                                break;
                            case 2:
                                Console.Write("=");
                                break;
                            case 3:
                                Console.Write("≡");
                                break;
                        }
                        if (bond.Target.Element != Element.C)
                            Console.Write(bond.Target.Element.ToString() + HydrogensToString(bond.Target));
                        else WriteAlkyl(bond.Target, chain[i]);
                    }
                }
                Console.WriteLine();
            }
        }

        string HydrogensToString(BondingAtom b)
        {
            switch (b.HydrogenCount())
            {
                case 0:
                    return "";
                case 1:
                    return "H";
                default:
                    return "H" + b.HydrogenCount();
            }
        }

        void WriteAlkyl(BondingAtom b, BondingAtom parent)
        {
            Console.Write("C" + HydrogensToString(b));
            foreach (BondingAtom child in b)
            {
                if (child != parent)
                {
                    WriteAlkyl(child, b);
                    break;
                }
            }
        }

        void AssignGroupLocations()
        {
            groups = new Dictionary<Group,List<int>>();
            for (int i = 0; i < chain.Length; i++)
            {
                foreach (Bond bond in chain[i].Bonds)
                {
                    switch (bond.Target.Element)
                    {
                        case Element.O:
                            if (bond.Order == 2)
                            {
                                if (chain[i].HasBond(Element.O, 1)) AddGroup(Group.Carboxyl, i);
                                else if (chain[i].HydrogenCount() > 0) AddGroup(Group.Formyl, i);
                                else AddGroup(Group.Carbonyl, i);
                            }
                            else if (bond.Order == 1) AddGroup(Group.Hydroxyl, i);
                            break;
                        case Element.N:
                            if (bond.Order == 1) AddGroup(Group.Amine, i);
                            break;
                        case Element.C:
                            if (bond.Order != 1)
                            {
                                if (i != chain.Length - 1 && bond.Target == chain[i + 1])
                                {
                                    switch (bond.Order)
                                    {
                                        case 2:
                                            AddGroup(Group.Alkenyl, i);
                                            break;
                                        case 3:
                                            AddGroup(Group.Alkynyl, i);
                                            break;
                                    }
                                }
                            }
                            else if (!IsInChain(bond.Target, i))
                                AddGroup(Group.Alkyl, i);
                            break;
                    }
                }
                if (chain[i].IsInGroup(Group.Carbonyl)) groups[Group.Hydroxyl].Remove(i);
            }
            if (groups[Group.Hydroxyl].Count == 0) groups.Remove(Group.Hydroxyl);
            if (!VerifyOrder())
            {
                chain = chain.Reverse().ToArray();
                AssignGroupLocations();
            }
        }

        bool IsInChain(BondingAtom b, int parentIndex)
        {
            if (b.Element != Element.C) return false;
            if (parentIndex != 0 && b == chain[parentIndex - 1]) return true;
            if (parentIndex != chain.Length - 1 && b == chain[parentIndex + 1]) return true;
            return false;
        }

        void AddGroup(Group group, int index)
        {
            if (groups.ContainsKey(group)) groups[group].Add(index);
            else
            {
                List<int> locs = new List<int>();
                locs.Add(index);
                groups.Add(group, locs);
            }
        }

        bool VerifyOrder()
        {
            List<int> locs = groups[HighestPrecedenceGroup()];
            int first = locs.Min();
            int last = locs.Max();
            switch (first.CompareTo(chain.Length - 1 - last))
            {
                case -1:
                    return true;
                case 1:
                    return false;
                case 0:
                    int forwardSum = locs.Sum();
                    int backwardSum = 0;
                    foreach (int i in locs)
                    {
                        backwardSum += chain.Length - 1 - i;
                    }
                    return forwardSum <= backwardSum;
                default:
                    return first <= last;
            }
        }

        Group HighestPrecedenceGroup()
        {
            Group highest = Group.Alkyl;
            foreach (BondingAtom b in chain)
            {
                highest = (Group)Math.Max((int)highest, (int)b.HighestPrecedenceGroup());
            }
            return highest;
        }

        public override int GetElementCount(Element e)
        {
            if (e == Element.H)
            {
                int count = 0;
                foreach (BondingAtom a in atoms)
                {
                    count += a.HydrogenCount();
                }
                return count;
            }
            else return base.GetElementCount(e);
        }

        public override Dictionary<Element, int> GetElementCounts()
        {
            Dictionary<Element, int> counts = base.GetElementCounts();
            counts.Add(Element.H, GetElementCount(Element.H));
            return counts;
        }

        public static string Prefix(int length)
        {
            switch (length)
            {
                case 1:
                    return "meth";
                case 2:
                    return "eth";
                case 3:
                    return "prop";
                case 4:
                    return "but";
                case 5:
                    return "pent";
                case 6:
                    return "hex";
                case 7:
                    return "hept";
                case 8:
                    return "oct";
                case 9:
                    return "non";
                case 10:
                    return "dec";
                case 11:
                    return "undec";
                case 12:
                    return "dodec";
                case 13:
                    return "tridec";
                case 14:
                    return "tetradec";
                case 15:
                    return "pentadec";
                case 20:
                    return "eicos";
                case 30:
                    return "triacont";
                default:
                    throw new ArgumentException();
            }
        }

        private class CarbonChainFinder
        {
            List<BondingAtom> atoms, chain;

            public CarbonChainFinder(List<BondingAtom> atoms)
            {
                this.atoms = atoms;
                chain = new List<BondingAtom>();
                AssignCarbonChain();
                if (!VerifyChain()) throw new InvalidOperationException();
            }

            bool HasNonAlkylChild(BondingAtom b, BondingAtom parent)
            {
                foreach (Bond bond in b.Bonds)
                {
                    if (bond.Target != parent)
                    {
                        if (bond.Target.Element != Element.C || bond.Order != 1) return true;
                        if (HasNonAlkylChild(bond.Target, b)) return true;
                    }
                }
                return false;
            }

            BondingAtom ChildWithLongestChain(BondingAtom b, BondingAtom parent)
            {
                int max = 0;
                BondingAtom maxChild = null;
                foreach (BondingAtom child in b)
                {
                    if (child != parent && child.Element == Element.C)
                    {
                        int childChainLength = LongestChildChain(child, b) + 1;
                        if (childChainLength > max) maxChild = child;
                        max = Math.Max(max, childChainLength);
                    }
                }
                return maxChild;
            }

            int LongestChildChain(BondingAtom b, BondingAtom parent)
            {
                int max = 0;
                foreach (BondingAtom child in b)
                {
                    if (child != parent && child.Element == Element.C)
                    {
                        max = Math.Max(max, LongestChildChain(child, b) + 1);
                    }
                }
                return max;
            }

            BondingAtom AtomWithLongestChain()
            {
                int maxLength = 0;
                BondingAtom atom = null;
                foreach (BondingAtom b in chain)
                {
                    int length = LongestChildChain(b, null);
                    if (length > maxLength)
                    {
                        maxLength = length;
                        atom = b;
                    }
                }
                return atom;
            }

            void AssignCarbonChain()
            {
                BondingAtom b = atoms.Find((BondingAtom atom) => atom.Element == Element.C && atom.IsNonAlkyl()); //null if not found?
                if (b == null) b = AtomWithLongestChain();
                chain.Add(b);
                //find the 2 children with the best chains and add their chains
                AssignChildren(b, null);
                AssignChildren(b, b.Bonds.Find((Bond bond) => chain.Contains(bond.Target)).Target);
            }

            void AssignChildren(BondingAtom b, BondingAtom parent)
            {
                foreach (BondingAtom child in b)
                {
                    if (child != parent && child.Element == Element.C)
                    {
                        if (HasNonAlkylChild(child, b))
                        {
                            chain.Add(child);
                            AssignChildren(child, b);
                            return;
                        }
                    }
                }
                //what if all children were alkyl?
                BondingAtom child2 = ChildWithLongestChain(b, parent);
                if (child2 != null)
                {
                    chain.Add(child2);
                    AssignChildren(child2, b);
                }
            }

            bool VerifyChain()
            {
                foreach (BondingAtom b in atoms)
                {
                    if (b.Element == Element.C && b.IsNonAlkyl() && !chain.Contains(b)) return false;
                }
                return true;
            }

            bool IsEnd(BondingAtom b)
            {
                int bondedCInChain = 0;
                foreach (BondingAtom atom in b)
                {
                    if (chain.Contains(atom)) bondedCInChain++;
                }
                return bondedCInChain < 2;
            }

            public BondingAtom[] GetChain()
            {
                BondingAtom[] bAs = new BondingAtom[chain.Count];
                bAs[0] = chain.Find((BondingAtom b) => IsEnd(b));
                BondingAtom parent = null;
                for (int i = 1; i < bAs.Length; i++)
                {
                    foreach (BondingAtom b in bAs[i - 1])
                    {
                        if (b.Element == Element.C && b != parent && chain.Contains(b))
                        {
                            parent = bAs[i - 1];
                            bAs[i] = b;
                            break;
                        }
                    }
                }
                return bAs;
            }
        }
    }
}