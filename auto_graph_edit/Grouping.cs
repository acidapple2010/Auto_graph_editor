using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace DrawingEngine
{
    public partial class Grouping : Form
    {
        private IntHolder toMerge;

        public Grouping(ref IntHolder toMerge, System.Windows.Forms.ComboBox.ObjectCollection objCollection)
        {
            InitializeComponent();
            foreach (Object cont in objCollection)
            {
                this.cbObjects.Items.Add(cont);
            }
            this.toMerge = toMerge;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (cbObjects.SelectedIndex == 0)
            {
                lblErrors.Text = "Выберете объект!";
                return;
            }
            toMerge.value = cbObjects.SelectedIndex;
            this.Close();
        }

    }
}
