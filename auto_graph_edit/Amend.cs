using System;
using System.Drawing;
using System.Windows.Forms;
using DrawingEngine.Drawing2D;
using DrawingEngine.Objects;
using DrawingEngine.Drawing3D;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using DrawingEngine.Common;

namespace DrawingEngine
{
    public partial class Amend : Form
    {
        Point3D point;
        public Amend(ref Point3D point)
        {
            InitializeComponent();
            this.point = point;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            point.X = Convert.ToDouble(txtX.Text);
            point.Y = Convert.ToDouble(txtY.Text);
            point.Z = Convert.ToDouble(txtZ.Text);
            this.Close();
        }
    }
}
