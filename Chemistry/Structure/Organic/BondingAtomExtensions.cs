using Chemistry;
using System.Linq;

namespace Chemistry.Structure.Organic
{
    public static class BondingAtomExtensions
    {
        public static int HydrogenCount(this BondingAtom b)
        {
            return Elements.Valence(b.Element) - b.Bonds.Sum(bond => bond.Order);
        }

        public static bool IsNonAlkyl(this BondingAtom b)
        {
            if (b.Element != Element.C) throw new System.ArgumentException();
            return b.Bonds.Any(bond => bond.Target.Element != Element.C || bond.Order != 1);
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
            else if (b.HasBond(Element.N, 3)) return Group.Nitrile;
            else if (b.HasBond(Element.O, 1)) return Group.Hydroxyl;
            else if (b.HasBond(Element.S, 1)) return Group.Sulfhydryl;
            else if (b.HasBond(Element.N, 1)) return Group.Amine;
            else if (b.HasBond(Element.N, 2)) return Group.Imine;
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
                    return b.HasBond(Element.O, 2) && b.HydrogenCount() > 0;
                case Group.Carboxyl:
                    return b.HasBond(Element.O, 2) && b.HasBond(Element.O, 1);
                default:
                    return false;
            }
        }
    }
}