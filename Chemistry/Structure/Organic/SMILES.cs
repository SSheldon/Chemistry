namespace Chemistry.Structure.Organic
{
    public static class SMILES
    {
        public static string SMILESNotation(OrganicMolecule mol)
        {
            return new SMILESGenerator(mol).SMILESNotation;
        }

        private class SMILESGenerator
        {
            OrganicMolecule mol;
            string smiles;

            public SMILESGenerator(OrganicMolecule mol)
            {
                this.mol = mol;
                smiles = "";
                GenerateSMILES();
            }

            public string SMILESNotation
            {
                get { return smiles; }
            }

            void GenerateSMILES()
            {
                for (int i = 0; i < mol.ChainLength; i++)
                {
                    smiles += mol[i].Element.ToString();
                    //add the things attached to it
                    foreach (Bond bond in mol[i].Bonds)
                    {
                        if (bond.Target.Element == Element.C)
                        {
                            if (!IsInChain(bond.Target, i))
                                smiles += "(" + AlkylSMILES(bond.Target, mol[i]) + ")";
                        }
                        else smiles += "(" + BondOrderSymbol(bond.Order) + bond.Target.Element.ToString() + ")";
                    }
                    smiles += BondOrderSymbol(BondOrderToNext(i));
                }
            }

            static string BondOrderSymbol(int order)
            {
                switch (order)
                {
                    //case 1: return "-";
                    case 2: return "=";
                    case 3: return "#";
                    default: return "";
                }
            }

            static string AlkylSMILES(BondingAtom b, BondingAtom parent)
            {
                string smiles = b.Element.ToString();
                foreach (BondingAtom child in b)
                {
                    if (child != parent) smiles += AlkylSMILES(child, b);
                }
                return smiles;
            }

            int BondOrderToNext(int index)
            {
                if (index >= mol.ChainLength - 1) return 0;
                foreach (Bond bond in mol[index].Bonds)
                {
                    if (bond.Target == mol[index + 1]) return bond.Order;
                }
                return 0;
            }

            bool IsInChain(BondingAtom b, int parentIndex)
            {
                if (b.Element != Element.C) return false;
                if (parentIndex != 0 && b == mol[parentIndex - 1]) return true;
                if (parentIndex != mol.ChainLength - 1 && b == mol[parentIndex + 1]) return true;
                return false;
            }
        }
    }
}