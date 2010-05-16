using System;
using System.IO;
using Chemistry;
using Chemistry.Structure;
using Chemistry.Structure.Organic;

public static class Program
{
    public static void Main(string[] args)
    {
        OrganicMolecule x = new OrganicMolecule(CML.ParseCML(File.OpenRead("test.cml")));
        x.WriteStructure();
        Console.WriteLine(x.ToString());
        Console.ReadLine();
    }
}