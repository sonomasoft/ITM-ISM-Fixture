using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;




namespace ITM_ISM_Fixture
{

    public partial class barCode : Form
    {
        MainForm frmMain;
        public barCode(MainForm parent)
        {
            InitializeComponent();
            frmMain = parent;

            // set focus on textbox
            textBox1.Select();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // check for value 
            if (textBox1.Text == "")
            {
                // show dialog for bad value
                MessageBox.Show("You must enter a serial number to continue." +              
                        "\n您必须输入一个序列号，以继续",
                         "Serial Number",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Exclamation,
                         MessageBoxDefaultButton.Button1);


                // set focus on textbox
                textBox1.Select();

            }
            else
            {
                // populate mainform box and hide

               
                frmMain.textBox1.Text = textBox1.Text;
                Hide();




            }


            // present, load on main forme
        }

        private void barCode_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (Control.IsKeyLocked(Keys.CapsLock)) // Checks Capslock is on
            {
                const int KEYEVENTF_EXTENDEDKEY = 0x1;
                const int KEYEVENTF_KEYUP = 0x2;
                keyboardImports.keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
                keyboardImports.keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                (UIntPtr)0);
            }

        }
    }


    class keyboardImports
    {
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,
        UIntPtr dwExtraInfo);
    }
}
