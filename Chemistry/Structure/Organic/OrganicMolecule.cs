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
        string name;

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
            AssignName();
        }

        #region StructureWriting
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
        #endregion

        #region ChainAssignment
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
        #endregion

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

        public override string ToString()
        {
            return name;
        }

        #region Naming
        static string ChainLengthPrefix(int digit, int place)
        {
            switch (place)
            {
                case 0:
                    switch (digit)
                    {
                        case 0: return "";
                        case 1: return "hen";
                        case 2: return "do";
                        case 3: return "tri";
                        case 4: return "tetr";
                        case 5: return "pent";
                        case 6: return "hex";
                        case 7: return "hept";
                        case 8: return "oct";
                        case 9: return "non";
                    }
                    throw new ArgumentException();
                case 1:
                    switch (digit)
                    {
                        case 0: return "";
                        case 1: return "dec";
                        case 2: return "cos";
                        default: return ChainLengthPrefix(digit, 0) + "acont";
                    }
                default: throw new ArgumentException();
            }
        }

        static string ChainLengthPrefix(int length)
        {
            switch (length)
            {
                case 1: return "meth";
                case 2: return "eth";
                case 3: return "prop";
                case 4: return "but";
                case 11:
                    return "un" + ChainLengthPrefix(1, 1);
                case 20:
                    return "i" + ChainLengthPrefix(2, 1);
                case 21:
                    return ChainLengthPrefix(1, 0) + "i" + ChainLengthPrefix(2, 1);
            }
            string s = "";
            for (int i = 0; length > 0; i++)
            {
                int digit = length % 10;
                if (digit != 0)
                {
                    s += ChainLengthPrefix(digit, i);
                    if (i == 0 && length > 10 && digit > 3) s += "a";
                }
                length /= 10;
            }
            return s;
        }

        void AssignName()
        {
            name = GetNamePrefix() + Suffix(ChainLengthPrefix(chain.Length), GetNameSuffix());
        }

        string GetNamePrefix()
        {
            Dictionary<string, List<int>> prefixes = new Dictionary<string, List<int>>();
            Group highest = HighestPrecedenceGroup();
            foreach (Group group in groups.Keys)
            {
                if (group != Group.Alkyl && group != Group.Alkenyl && group != Group.Alkynyl && group != highest)
                    prefixes.Add(GroupPrefix(group), groups[group]);
            }
            if (groups.ContainsKey(Group.Alkyl)) AddAlkylPrefixes(prefixes);
            return Affix(prefixes);
        }

        void AddAlkylPrefixes(Dictionary<string, List<int>> prefixes)
        {
            for (int i = 0; i < chain.Length; i++)
            {
                foreach (BondingAtom child in chain[i])
                {
                    if (child.Element == Element.C && !IsInChain(child, i))
                    {
                        //an alkyl!
                        string prefix = ChainLengthPrefix(AlkylLength(child, chain[i])) + "yl";
                        if (!prefixes.ContainsKey(prefix))
                            prefixes.Add(prefix, new List<int>());
                        prefixes[prefix].Add(i);
                    }
                }
            }
        }

        int AlkylLength(BondingAtom b, BondingAtom parent)
        {
            foreach (BondingAtom child in b)
            {
                if (child != parent) return AlkylLength(child, b) + 1;
            }
            return 1;
        }

        string GetNameSuffix()
        {            
            Group highest = HighestPrecedenceGroup();
            if (highest == Group.Alkyl) return "ane";
            string suffix = "";
            if (groups.ContainsKey(Group.Alkenyl)) suffix = Suffix(suffix, Affix(GroupSuffix(Group.Alkenyl), groups[Group.Alkenyl]));
            if (groups.ContainsKey(Group.Alkynyl)) suffix = Suffix(suffix, Affix(GroupSuffix(Group.Alkynyl), groups[Group.Alkynyl]));
            switch (highest)
            {
                case Group.Alkenyl:
                case Group.Alkynyl:
                    return suffix;
                case Group.Carboxyl:
                case Group.Formyl:
                    return Suffix(suffix, GroupCountPrefix(groups[highest].Count) + GroupSuffix(highest));
                default:
                    return Suffix(suffix, Affix(GroupSuffix(highest), groups[highest]));
            }
        }

        string Suffix(string stem, string suffix)
        {
            if (stem.Length != 0 && suffix.Length != 0 && char.IsDigit(suffix[0])) return stem + "-" + suffix;
            else return stem + suffix;
        }

        string Prefix(string stem, string prefix)
        {
            if (stem.Length != 0 && char.IsDigit(stem[0])) return prefix + "-" + stem;
            else return prefix + stem;
        }

        string Affix(Dictionary<string, List<int>> affixes)
        {
            string affix = "";
            while (affixes.Count != 0)
            {
                string group = Lowest(affixes.Keys);
                affix = Suffix(affix, Affix(group, affixes[group]));
                affixes.Remove(group);
            }
            return affix;
        }

        string Affix(string affix, List<int> locations)
        {
            locations.Sort();
            string s = "";
            s += locations[0] + 1;
            for (int i = 1; i < locations.Count; i++)
                s += "," + (locations[i] + 1);
            s += "-" + GroupCountPrefix(locations.Count) + affix;
            return s;
        }

        string Lowest(IEnumerable<string> affixes)
        {
            string lowest = null;
            foreach (string affix in affixes)
            {
                if (lowest == null || affix.CompareTo(lowest) < 0) lowest = affix;
            }
            return lowest;
        }

        string GroupCountPrefix(int count)
        {
            switch (count)
            {
                case 1: return "";
                case 2: return "di";
                case 3: return "tri";
                case 4: return "tetra";
            }
            throw new ArgumentException();
        }

        string GroupPrefix(Group group)
        {
            switch (group)
            {
                case Group.Amine: return "amino";
                case Group.Hydroxyl: return "hydroxy";
                case Group.Carbonyl: return "oxo";
                case Group.Formyl: return "formyl";
                case Group.Carboxyl: return "carboxy";
            }
            throw new ArgumentException();
        }

        string GroupSuffix(Group group)
        {
            switch (group)
            {
                case Group.Alkyl: return "yl";
                case Group.Alkenyl: return "ene";
                case Group.Alkynyl: return "yne";
                case Group.Amine: return "amine";
                case Group.Hydroxyl: return "anol";
                case Group.Carbonyl: return "anone";
                case Group.Formyl: return "anal";
                case Group.Carboxyl: return "anoic acid";
            }
            throw new ArgumentException();
        }
        #endregion

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