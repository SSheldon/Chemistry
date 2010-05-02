using Chemistry;

namespace Chemistry.Structure.Organic
{
    public static class BondingAtomExtensions
    {
        public static int HydrogenCount(this BondingAtom b)
        {
            int bondCount = 0;
            foreach (Bond bond in b.Bonds) bondCount += bond.Order;
            return Elements.Valence(b.Element) - bondCount;
        }

        public static bool IsNonAlkyl(this BondingAtom b)
        {
            if (b.Element != Element.C) throw new System.ArgumentException();
            foreach (Bond bond in b.Bonds)
                if (bond.Target.Element != Element.C || bond.Order != 1) return true;
            return false;
        }

        public static Group HighestPrecedenceGroup(this BondingAtom b)
        {
            if (b.Element != Element.C) throw new System.ArgumentException();
            if (b.HasBond(Element.O, 2))
            {
                if (b.HasBond(Element.O, 1)) return Group.Carboxyl;
                else if (b.HydrogenCount() > 0) return Group.Formyl;
                else return Group.Carbonyl;
            }
            else if (b.HasBond(Element.O, 1)) return Group.Hydroxyl;
            else if (b.HasBond(Element.N, 1)) return Group.Amine;
            else if (b.HasBond(Element.C, 3)) return Group.Alkynyl;
            else if (b.HasBond(Element.C, 2)) return Group.Alkenyl;
            else return Group.Alkyl;
        }

        public static bool IsInGroup(this BondingAtom b, Group group)
        {
            if (b.Element != Element.C) throw new System.ArgumentException();
            switch (group)
            {
                case Group.Alkyl:
                    return b.HasBond(Element.C, 1);
                case Group.Alkenyl:
                    return b.HasBond(Element.C, 2);
                case Group.Alkynyl:
                    return b.HasBond(Element.C, 3);
                case Group.Amine:
                    return b.HasBond(Element.N, 1);
                case Group.Hydroxyl:
                    return b.HasBond(Element.O, 1);
                case Group.Carbonyl:
                    return b.HasBond(Element.O, 2);
                case Group.Formyl:
                    return IsInGroup(b, Group.Carbonyl) && b.HydrogenCount() > 0;
                case Group.Carboxyl:
                    return IsInGroup(b, Group.Carboxyl) && IsInGroup(b, Group.Hydroxyl);
                default:
                    return false;
            }
        }
    }
}