using System;
using System.IO;
using Chemistry;
using Chemistry.Structure;
using Chemistry.Structure.Organic;
using System.Windows.Forms;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Chemical Markup Language file (*.cml)|*.cml";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            OrganicMolecule x = new OrganicMolecule(CML.ParseCML(File.OpenRead(openFileDialog.FileName)));
            Console.WriteLine(SMILES.SMILESNotation(x));
            Console.WriteLine(x.ToString());
            Console.ReadLine();
        }
    }
}