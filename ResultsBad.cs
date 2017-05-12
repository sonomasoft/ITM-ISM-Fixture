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
    public partial class ResultsBad : Form
    {
        MainForm frmMain;
        public ResultsBad(MainForm parent)
        {
            InitializeComponent();
            frmMain = parent;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            ////frmMain.abortBTN.Text = "Abort";

            frmMain.button1.PerformClick();
            // turn off power supply

           // frmMain.port.Write("SOUT1\r");

            ////frmMain.abortBTN.PerformClick();
           //// frmMain.startBTN.PerformClick();







           // frmMain.abortMe();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
            ////frmMain.abortBTN.Text = "Clear";
        }

        private void ResultsBad_Load(object sender, EventArgs e)
        {

        }
    }
}
