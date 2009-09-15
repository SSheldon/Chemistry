using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chem_Ions
{
    public partial class Form1 : Form
    {
        private int currentIndex;
        private ChemIonsQuizzer q;

        public Form1()
        {
            InitializeComponent();
            q = new ChemIonsQuizzer(false, true);
            NextIndex();
        }

        private void NextIndex()
        {
            currentIndex = q.NextIndex();
            this.label1.Text = "What is the formula of a " + ChemIons.GetName(currentIndex) + " ion?";
            this.textBox1.Text = "";
            this.comboBox1.SelectedIndex = -1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "button2":
                    this.textBox1.Text += "₂";
                    break;
                case "button3":
                    this.textBox1.Text += "₃";
                    break;
                case "button4":
                    this.textBox1.Text += "₄";
                    break;
                case "button5":
                    this.textBox1.Text += "₇";
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string formula = textBox1.Text;
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    formula += "³⁻";
                    break;
                case 1:
                    formula += "²⁻";
                    break;
                case 2:
                    formula += "⁻";
                    break;
                case 3:
                    formula += "⁺";
                    break;
                case 4:
                    formula += "²⁺";
                    break;
                case 5:
                    formula += "³⁺";
                    break;
                case 6:
                    formula += "⁴ ⁺";
                    break;
            }
            if (ChemIons.FormulaIsCorrect(currentIndex, formula))
            {
                NextIndex();
            }
        }
    }
}
