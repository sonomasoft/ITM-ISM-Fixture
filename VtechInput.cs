using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ITM_ISM_Fixture
{
    public partial class VtechInput : Form
    {
        MainForm frmMain;
        public VtechInput(MainForm parent)
        {
            InitializeComponent();
            frmMain = parent;

            // set focus on textbox
            StationName.Select();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // validate text exists in all boxes

            if ((StationName.Text == "") || (RouteStep.Text == "") || (ProductName.Text == "") || (OrderNumber.Text == "") || (Operator.Text == ""))
            {
                // show dialog for bad value
                MessageBox.Show("You must enter all data to continue.",
                         "Routing Information",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Exclamation,
                         MessageBoxDefaultButton.Button1);


                // set focus on textbox
                StationName.Select();

            }
            else
            {





                // save the information for the Vtech log file.





                // hide the box
                this.Hide();

            }


        }
    }
}
