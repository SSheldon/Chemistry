using System;
using System.Collections.Generic;

public static class ChemIons
{
    private static readonly string[] formulae = {"NH₄⁺",
        "CH₃COO⁻",
        "CO₃²⁻",
        "HCO₃⁻",
        "ClO⁻",
        "ClO₂⁻",
        "ClO₃⁻",
        "ClO₄⁻",
        "MnO₄⁻",
        "CrO₄²⁻",
        "Cr₂O₇²⁻",
        "NO₃⁻",
        "NO₂⁻",
        "IO₃⁻",
        "O₂²⁻",
        "C₂O₄²⁻",
        "CN⁻",
        "OCN⁻",
        "SCN⁻",
        "PO₄³⁻",
        "HPO₄²⁻",
        "H₂PO₄⁻",
        "SO₄²⁻",
        "HSO₄⁻",
        "S₂O₃²⁻",
        "HS⁻",
        "SO₃²⁻",
        "HSO₃⁻",
        "OH⁻",
        "H₃O⁺",
        "H⁺",
        "Li⁺",
        "Na⁺",
        "K⁺",
        "Rb⁺",
        "Cs⁺",
        "Fr⁺",
        "Be²⁺",
        "Mg²⁺",
        "Ca²⁺",
        "Sr²⁺",
        "Ba²⁺",
        "Ra²⁺",
        "Fe²⁺",
        "Fe³⁺",
        "Cu²⁺",
        "Cu³⁺",
        "Ag⁺",
        "Au⁺",
        "Au³⁺",
        "Zn²⁺",
        "Hg₂²⁺",
        "Hg²⁺",
        "Al³⁺",
        "Sn²⁺",
        "Sn⁴⁺",
        "Pb²⁺",
        "Pb⁴⁺",
        "N³⁻",
        "P³⁻",
        "As³⁻",
        "O²⁻",
        "S²⁻",
        "Se²⁻",
        "Te²⁻",
        "H⁻",
        "F⁻",
        "Cl⁻",
        "Br⁻",
        "I⁻",
        "At⁻"};

    private static readonly string[] names = {"Ammonium",
        "Acetate",
        "Carbonate",
        "Hydrogen carbonate",
        "Hypochlorite",
        "Chlorite",
        "Chlorate",
        "Perchlorate",
        "Permanganate",
        "Chromate",
        "Dichromate",
        "Nitrate",
        "Nitrite",
        "Iodate",
        "Peroxide",
        "Oxalate",
        "Cyanide",
        "Cyanate",
        "Thiocyanate",
        "Phosphate",
        "Monohydrogen phosphate",
        "Dihydrogen phosphate",
        "Sulfate",
        "Hydrogen sulfate",
        "Thiosulfate",
        "Hydrogen sulfide",
        "Sulfite",
        "Hydrogen sulfite",
        "Hydroxide",
        "Hydronium",
        "Hydrogen",
        "Lithium",
        "Sodium",
        "Potassium",
        "Rubidium",
        "Cesium",
        "Francium",
        "Beryllium",
        "Magnesium",
        "Calcium",
        "Strontium",
        "Barium",
        "Radium",
        "Iron(II)",
        "Iron(III)",
        "Copper(II)",
        "Copper(III)",
        "Silver",
        "Gold(I)",
        "Gold(III)",
        "Zinc",
        "Mercury(I)",
        "Mercury(II)",
        "Aluminum",
        "Tin(II)",
        "Tin(IV)",
        "Lead(II)",
        "Lead(IV)",
        "Nitride",
        "Phosphide",
        "Arsenide",
        "Oxide",
        "Sulfide",
        "Selenide",
        "Telluride",
        "Hydride",
        "Fluoride",
        "Chloride",
        "Bromide",
        "Iodide",
        "Astatide"};

    public static string GetName(string formula)
    {
        for (int counter = 0; counter < formulae.Length; counter++)
        {
            if (formulae[counter] == formula) return names[counter];
        }
        return "";
    }

    public static string GetFormula(string name)
    {
        for (int counter = 0; counter < names.Length; counter++)
        {
            if (names[counter] == name) return formulae[counter];
        }
        return "";
    }

    public static string GetName(int index)
    {
        return names[index];
    }

    public static string GetFormula(int index)
    {
        return formulae[index];
    }

    public static bool NameIsCorrect(int index, string name)
    {
        return names[index] == name;
    }

    public static bool FormulaIsCorrect(int index, string formula)
    {
        return formulae[index] == formula;
    }
}

public class ChemIonsQuizzer
{
    private int[] indices;
    private int counter;

    public ChemIonsQuizzer(bool includeMonatomic, bool includePolyatomic)
    {
        indices = new int[(includeMonatomic ? 41 : 0) + (includePolyatomic ? 30 : 0)];
        if (includePolyatomic)
        {
            for (int i = 0; i < 30; i++)
            {
                indices[i] = i;
            }
        }
        if (includeMonatomic)
        {
            for (int i = 30; i < 71; i++)
            {
                indices[i - (includePolyatomic ? 0 : 30)] = i;
            }
        }
        RandomizeIndices();
    }

    public ChemIonsQuizzer() : this(true, true) { }

    private void RandomizeIndices()
    {
        List<int> inputList = new List<int>(indices);
        List<int> randomList = new List<int>();
        Random r = new Random();
        int randomIndex = 0;
        while (inputList.Count > 0)
        {
            randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
            randomList.Add(inputList[randomIndex]); //add it to the new, random list
            inputList.RemoveAt(randomIndex); //remove to avoid duplicates
        }

        randomList.CopyTo(indices);
        counter = 0;

        //clean up
        inputList.Clear();
        inputList = null;
        randomList.Clear();
        randomList = null;
        r = null;        
    }

    public int NextIndex()
    {
        if (counter >= indices.Length)
            RandomizeIndices();
        counter++;
        return indices[counter - 1];
    }
}