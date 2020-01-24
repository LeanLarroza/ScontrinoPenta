using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScontrinoPenta
{
    public partial class ErroreStampa : Form
    {
        public ErroreStampa()
        {
            InitializeComponent();
        }

        private void StampaScontrino_Load(object sender, EventArgs e)
        {
            this.BringToFront();
            this.TopMost = true;
            this.BringToFront();
        }

        private void Buttonexito_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
