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
    public partial class StampaScontrino : Form
    {
        public StampaScontrino()
        {
            InitializeComponent();
        }

        private void StampaScontrino_Load(object sender, EventArgs e)
        {
            this.BringToFront();
            this.TopMost = true;
        }
    }
}
