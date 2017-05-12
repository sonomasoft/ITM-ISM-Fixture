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
    public partial class ResultGood : Form
    {
        MainForm frmMain;
        public ResultGood(MainForm parent)
        {
            InitializeComponent();
            frmMain = parent;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // run abort and start
            Hide();

            frmMain.button1.PerformClick();



            // turn off power supply

            //frmMain.port.Write("SOUT1\r");

            ////frmMain.abortBTN.Text = "Abort";
            ////frmMain.abortBTN.PerformClick();
            ////frmMain.startBTN.PerformClick();
          

            //frmMain.abortMe();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Just close the window
            Hide();

           //// frmMain.abortBTN.Text = "Clear";
        }

        private void ResultGood_Load(object sender, EventArgs e)
        {
           // button1.Enabled = false;
            //button2.Enabled = false;

        }
    }
}
