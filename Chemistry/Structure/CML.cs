using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;

namespace Chemistry.Structure
{
    public static class CML
    {
        public static Molecule ParseCML(FileStream file)
        {
            Dictionary<int, BondingAtom> atoms = new Dictionary<int, BondingAtom>();
            XmlTextReader reader = new XmlTextReader(file);
            reader.ReadToFollowing("molecule");
            reader.ReadToDescendant("atomArray");
            reader.ReadToDescendant("atom");
            do
            {
                atoms.Add(int.Parse(reader.GetAttribute("id").Substring(1)) - 1, new BondingAtom(Elements.FromSymbol(reader.GetAttribute("elementType"))));
            } while (reader.ReadToNextSibling("atom"));
            reader.ReadToNextSibling("bondArray");
            reader.ReadToDescendant("bond");
            do
            {
                string[] atomRefs = reader.GetAttribute("atomRefs2").Split(' ');
                int order = 0;
                switch (reader.GetAttribute("order"))
                {
                    case "S":
                        order = 1;
                        break;
                    case "D":
                        order = 2;
                        break;
                    case "T":
                        order = 3;
                        break;
                }
                atoms[int.Parse(atomRefs[0].Substring(1)) - 1].Bond(atoms[int.Parse(atomRefs[1].Substring(1)) - 1], order);
            } while (reader.ReadToNextSibling("bond"));
            return new Molecule(new List<BondingAtom>(atoms.Values));
        }
    }
}