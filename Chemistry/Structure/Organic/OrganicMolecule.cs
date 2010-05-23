using System;
using System.Collections.Generic;
using System.Linq;

namespace Chemistry.Structure.Organic
{
    public enum Group { Alkyl, Alkenyl, Alkynyl, Amine, Sulfhydryl, Hydroxyl, Carbonyl, Formyl, Carboxyl }

    public class OrganicMolecule : Molecule
    {
        BondingAtom[] chain;
        string name;

        public BondingAtom this[int i]
        {
            get { return chain[i]; }
        }

        public int ChainLength
        {
            get { return chain.Length; }
        }

        public OrganicMolecule(Molecule m)
            : base(m.Atoms)
        {
            chain = new CarbonChainFinder(atoms).GetChain();
            if (!VerifyOrder()) chain = chain.Reverse().ToArray();
            name = new OrganicMoleculeNamer(this).GetName();
        }

        public OrganicMolecule(BondingAtom a)
            : this(new Molecule(a)) { }

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

        List<int> SideChainLocations()
        {
            List<int> locs = new List<int>();
            for (int i = 0; i < chain.Length; i++)
            {
                foreach (Bond bond in chain[i].Bonds)
                {
                    if (bond.Order == 1 && !IsInChain(bond.Target, i)) locs.Add(i);
                }
            }
            return locs;
        }

        bool IsInChain(BondingAtom b, int parentIndex)
        {
            if (b.Element != Element.C) return false;
            if (parentIndex != 0 && b == chain[parentIndex - 1]) return true;
            if (parentIndex != chain.Length - 1 && b == chain[parentIndex + 1]) return true;
            return false;
        }

        bool VerifyOrder()
        {
            List<int> locs = HighestPrecendenceGroupLocations();
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

        List<int> HighestPrecendenceGroupLocations()
        {            
            Group highest = HighestPrecedenceGroup();
            if (highest == Group.Alkyl) return SideChainLocations();
            else
            {
                List<int> locs = new List<int>();
                for (int i = 0; i < chain.Length; i++)
                {
                    if (chain[i].HighestPrecedenceGroup() == highest) locs.Add(i);
                }
                return locs;
            }
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

        private class OrganicMoleculeNamer
        {
            BondingAtom[] chain;
            Dictionary<string, List<int>> prefixes;
            Dictionary<string, List<int>> suffixes;
            Group highest;

            public OrganicMoleculeNamer(BondingAtom[] chain, Group highest)
            {
                this.chain = chain;
                this.highest = highest;
                prefixes = new Dictionary<string, List<int>>();
                suffixes = new Dictionary<string, List<int>>();
                EnumerateChain();
            }

            public OrganicMoleculeNamer(OrganicMolecule mol)
                : this(mol.chain, mol.HighestPrecedenceGroup()) { }

            void EnumerateChain()
            {
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
                                else if (bond.Order == 1 && !chain[i].HasBond(Element.O, 2)) AddGroup(Group.Hydroxyl, i);
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
                                                AddSuffix("en", i);
                                                break;
                                            case 3:
                                                AddSuffix("yn", i);
                                                break;
                                        }
                                    }
                                }
                                else if (!IsInChain(bond.Target, i))
                                    AddPrefix(ChainLengthPrefix(AlkylLength(bond.Target, chain[i])) + "yl", i);
                                break;
                            case Element.F:
                                AddPrefix("fluoro", i);
                                break;
                            case Element.Cl:
                                AddPrefix("chloro", i);
                                break;
                            case Element.Br:
                                AddPrefix("bromo", i);
                                break;
                            case Element.I:
                                AddPrefix("iodo", i);
                                break;
                            case Element.S:
                                if (bond.Order == 1) AddGroup(Group.Sulfhydryl, i);
                                break;
                        }
                    }
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
                if (group == Group.Alkenyl || group == Group.Alkynyl || group == highest)
                    AddSuffix(GroupSuffix(group), index);
                else
                    AddPrefix(GroupPrefix(group), index);
            }

            void AddPrefix(string prefix, int index)
            {
                AddAffix(prefixes, prefix, index);
            }

            void AddSuffix(string suffix, int index)
            {
                AddAffix(suffixes, suffix, index);
            }

            static void AddAffix(Dictionary<string, List<int>> affixes, string affix, int index)
            {
                if (affixes.ContainsKey(affix)) affixes[affix].Add(index);
                else
                {
                    List<int> locs = new List<int>();
                    locs.Add(index);
                    affixes.Add(affix, locs);
                }
            }

            public string GetName()
            {
                return GetPrefix() + Suffix(ChainLengthPrefix(chain.Length), GetSuffix());
            }

            string GetPrefix()
            {
                string prefix = "";
                while (prefixes.Count != 0)
                {
                    string group = Lowest(prefixes.Keys);
                    prefix = Suffix(prefix, Affix(group, prefixes[group]));
                    prefixes.Remove(group);
                }
                return prefix;
            }

            int AlkylLength(BondingAtom b, BondingAtom parent)
            {
                foreach (BondingAtom child in b)
                {
                    if (child != parent) return AlkylLength(child, b) + 1;
                }
                return 1;
            }

            string GetSuffix()
            {
                string suffix = "";
                if (suffixes.ContainsKey("en")) suffix = Suffix(suffix, Affix("en", suffixes["en"]));
                if (suffixes.ContainsKey("yn")) suffix = Suffix(suffix, Affix("yn", suffixes["yn"]));
                if (suffix.Length == 0) suffix = "an";
                switch (highest)
                {
                    case Group.Alkyl:
                    case Group.Alkenyl:
                    case Group.Alkynyl:
                        return suffix + "e";
                    default:
                        string highestGroupSuffix = GroupSuffix(highest);
                        string molGroupSuffix;
                        if (highest == Group.Carboxyl || highest == Group.Formyl)
                            molGroupSuffix = GroupCountPrefix(suffixes[highestGroupSuffix].Count) + highestGroupSuffix;
                        else molGroupSuffix = Affix(highestGroupSuffix, suffixes[highestGroupSuffix]);
                        if (FirstLetterIsConsonant(molGroupSuffix)) suffix += "e";
                        return Suffix(suffix, molGroupSuffix);
                }
            }

            static bool FirstLetterIsConsonant(string s)
            {
                foreach (char c in s)
                {
                    if (char.IsLetter(c)) return !(c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u' || c == 'y');
                }
                return false;
            }

            static string Suffix(string stem, string suffix)
            {
                if (stem.Length != 0 && suffix.Length != 0 && char.IsDigit(suffix[0])) return stem + "-" + suffix;
                else return stem + suffix;
            }

            static string Prefix(string stem, string prefix)
            {
                if (stem.Length != 0 && char.IsDigit(stem[0])) return prefix + "-" + stem;
                else return prefix + stem;
            }

            static string Affix(string affix, List<int> locations)
            {
                locations.Sort();
                string s = "";
                s += locations[0] + 1;
                for (int i = 1; i < locations.Count; i++)
                    s += "," + (locations[i] + 1);
                s += "-" + GroupCountPrefix(locations.Count) + affix;
                return s;
            }

            static string Lowest(IEnumerable<string> affixes)
            {
                string lowest = null;
                foreach (string affix in affixes)
                {
                    if (lowest == null || affix.CompareTo(lowest) < 0) lowest = affix;
                }
                return lowest;
            }

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

            static string GroupCountPrefix(int count)
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

            static string GroupPrefix(Group group)
            {
                switch (group)
                {
                    case Group.Amine: return "amino";
                    case Group.Sulfhydryl: return "sulfanyl";
                    case Group.Hydroxyl: return "hydroxy";
                    case Group.Carbonyl: return "oxo";
                    case Group.Formyl: return "formyl";
                    case Group.Carboxyl: return "carboxy";
                }
                throw new ArgumentException();
            }

            static string GroupSuffix(Group group)
            {
                switch (group)
                {
                    case Group.Amine: return "amine";
                    case Group.Sulfhydryl: return "thiol";
                    case Group.Hydroxyl: return "ol";
                    case Group.Carbonyl: return "one";
                    case Group.Formyl: return "al";
                    case Group.Carboxyl: return "oic acid";
                }
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