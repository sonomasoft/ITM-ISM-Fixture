using NationalInstruments;
using NationalInstruments.UI;
using NationalInstruments.DAQmx;
using NationalInstruments.NI4882;
using NationalInstruments.VisaNS;
using NationalInstruments.NetworkVariable;
using NationalInstruments.NetworkVariable.WindowsForms;
using NationalInstruments.Analysis;
using NationalInstruments.Analysis.Conversion;
using NationalInstruments.Analysis.Dsp;
using NationalInstruments.Analysis.Dsp.Filters;
using NationalInstruments.Analysis.Math;
using NationalInstruments.Analysis.Monitoring;
using NationalInstruments.Analysis.SignalGeneration;
using NationalInstruments.Tdms;
using NationalInstruments.UI.WindowsForms;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Text.RegularExpressions;

using Microsoft.Win32;
using System.IO.Ports;

using System.Net;



namespace ITM_ISM_Fixture
{
    using System.Management;
    

    public partial class MainForm : Form
    {
        private Imports.ps5000aBlockReady _callbackDelegate;

        Mitov.SignalLab.RealBuffer DataBufferx = new Mitov.SignalLab.RealBuffer(1024);


        int instumentStatus = 0;
        private short _handle;
        public const int BUFFER_SIZE = 1024;
        public const int MAX_CHANNELS = 4;
        public const int QUAD_SCOPE = 4;
        public const int DUAL_SCOPE = 2;

        public short maxADCValue;
        public short handle;
        bool _ready = false;

        uint timebase = 5;

        public int numPreTriggerSamples;
        public int numPostTriggerSamples;
        public int timeIntervalNs;
        public int maxSamples;
        public uint segmentIndex;
        public short getTimebase2Status;
        public int totalSamples;
        public uint THDindx;

        public short threshold;

        public uint delay;
        public short autoTriggerMs;

        public int[] inputRanges = { 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000, 20000, 50000, 100000 }; // ranges in mV

        public bool PowerUpCurrentResult = true;

        public bool VoltageResult = true;

        public bool TP102Result = true;
        public bool TP116Result = true;
        public bool TP115Result = true;
        public bool TP130Result = true;

        public bool AuxResult = true;
        public bool MicResult = true;


        public bool BootResult = true;



        public bool ChargeCurrentResult = true;




        public bool ProgramInProgress = false;

        public bool ProgramResult = false;
        public string flashstring = "Data";

        public string IxM_port = null;

        public string Txresponse = null;

        public string comResponseString = null;
        public bool comResponse = false;






        public int progresspercent = 0;

        public int progressSleep = 1500;


        public System.Timers.Timer aTimer = new System.Timers.Timer();

        BackgroundWorker bgw = new BackgroundWorker();  




        // TP130 is mic input/ bias.  CN200 pin 2.   not hooked up on auto fixture.  TP exists, so we can use if needed.


        public double[] VoltageMeasurements = new double[] { 0, 0, 0, 0, 0 };  //TP102,TP116,TP115,TP130,


        public double[] AudioInMeasurements = new double[] { 0, 0, 0 };

        public double powerupCurrent = 0;  // power up current


        public double ChargeCurrent = 0;


        static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();


            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * FROM Win32_SerialPort"))
                collection = searcher.Get();

            foreach (var device in collection)
            {
                devices.Add(new USBDeviceInfo(
                (string)device.GetPropertyValue("DeviceID"),
                (string)device.GetPropertyValue("PNPDeviceID"),
                (string)device.GetPropertyValue("Description")
                ));
            }

            collection.Dispose();
            return devices;
        }





        private void BlockCallback(short handle, short status, IntPtr pVoid)
        {
            // flag to say done reading data
            if (status != (short)Imports.PICO_CANCELLED)
                _ready = true;
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // begin test sequence

            // forces exception
         // VoltageMeasurements[10] = 0;

            chgOff();

            //Test_01();

            // initilize all test equipment
            led1.OffColor = Color.Yellow;
            this.Refresh();


            BK1685B_INIT initBK1685B = new BK1685B_INIT();



            initBK1685B.Run();

            // init picoscope
            if (instumentStatus == 0)
                instumentStatus = initScope();


            // check for bk 2831E

            if (instumentStatus == 0)
                instumentStatus = init2831E();


            if (instumentStatus == 0)
            {
                led1.OffColor = Color.LimeGreen;


            }
            else
            {
                led1.OffColor = Color.Red;
                MessageBox.Show("Not all equipment initilized Correctly.  Check your connections.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);



            }


            // bring power up and measure current.

            if (instumentStatus == 0)
            {

                BK1685C_0N BK1685bOn = new BK1685C_0N();


                BK1685bOn.Run();

                Thread.Sleep(4000);  // stablize



               Test_ONCurrent();  // move this to the power supply for measurement 


                



            }


            Thread.Sleep(1000);  // stablize


            // check our power supplies
            if (instumentStatus == 0)
            {
                // prompt manual switch to TP102


                led3.OffColor = Color.Yellow;
                this.Refresh();

              


              
                
               

                

               
                /*
                MessageBox.Show("Swich fixture to TP102.",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1);

                */
                switchChannel0(0);

                Test_TP102();
                this.Refresh();

                /*

                MessageBox.Show("Swich fixture to TP116.",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1);

                */
                switchChannel0(1);
                Test_TP116();
                this.Refresh();

                /*
                MessageBox.Show("Swich fixture to TP115.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);
                */

                switchChannel0(2);
                Test_TP115();
                this.Refresh();

                /*
                MessageBox.Show("Swich fixture to TP130.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);
                */

                switchChannel0(3);
                Test_TP130();
                this.Refresh();






                // check all of our voltage results and see if we passed

                if (TP102Result & TP116Result & TP115Result & TP130Result)
                {
                    VoltageResult = true;

                    led3.OffColor = Color.LimeGreen;
                }
                else
                {

                    VoltageResult = false;

                    led3.OffColor = Color.Red;


                }


            }

            this.Refresh();

                // program the board
           // ExecuteCommand("flashme.bat");





            /*  remark out program step for now

            if (instumentStatus == 0)
            {
                led4.OffColor = Color.Yellow;
                this.Refresh();

                // start timer for progress bar
                
          




                MessageBox.Show("Flip COM Switch to program",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);

                ProgramInProgress = true;

                progressBar1.Visible = true;
               

               // create background worker for status bar
                bgw.DoWork += new DoWorkEventHandler(bgw_DoWork);
                bgw.ProgressChanged += new ProgressChangedEventHandler(bgw_ProgressChanged);
                bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_RunWorkerCompleted);
                bgw.WorkerReportsProgress = true;
                bgw.RunWorkerAsync();

                // Start the child process.
                Process p = new Process();
                // Redirect the output stream of the child process.


                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "flashme.bat";
                p.StartInfo.CreateNoWindow = true;
                p.OutputDataReceived += (s, f) => myMethod(f);

                p.Start();

                p.BeginOutputReadLine();  // this make it async



            }
            


                // Do not wait for the child process to exit before
                // reading to the end of its redirected stream.
                // p.WaitForExit();
                // Read the output stream first and then wait.


           
                


                
          
                
               

                //string output = p.StandardOutput.ReadToEnd();
               // p.WaitForExit();
            

                while(ProgramInProgress)  // wait until we have programmed
                {
                    Application.DoEvents();


                }
            
             
               

                if (ProgramResult==true)
                {
                    led4.OffColor = Color.LimeGreen;
                    this.Refresh();

                    MessageBox.Show("Programming Complete.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);

                }
                else
                {

                    led4.OffColor = Color.Red;
                    this.Refresh();

                    MessageBox.Show("Programming Failed!.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);

                    


                }


                
                




                progressBar1.Visible = false;

           
              */


            // test Aux inputs

            
                if (instumentStatus == 0)
                {


                    led7.OffColor = Color.Yellow;   /// seems to be crashing here!!!!!!
                    this.Refresh();


                    /*
                    MessageBox.Show("Switch Out to Tp402 and In/Out to Aux R.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);
                     * */

                    switchChannel0(5);
                    switchChannel1(0);  // inject signal



                    short status;


                    // turn on Generator
                    status = Imports.SetSiggenBuiltIn(handle, 0, 500000, Imports.SiggenWaveType.Sine, 1000, 1000, 0, 0, Imports.SiggenSweepType.Up, false, 1, 1, Imports.SiggenTrigType.Rising, Imports.SiggenTrigSource.None, 0);
                    // allow some settling time

                    Thread.Sleep(1000);


                    GetChanB();
                    Thread.Sleep(500);
                    GetChanB();  // have to do twice due to offset in buffer.. I don't know why yet.


                    // get value for aux from label

                    string rmsValue = Regex.Match(label19.Text, @"\d+").Value;


                    AudioInMeasurements[0] = Convert.ToDouble(rmsValue);



                    /*
                    MessageBox.Show("Switch Out to Tp402 and In/Out to Aux L.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);
                    */
                    this.Refresh();
                   // switchChannel0(5);
                    switchChannel1(1);
                    GetChanB();

                    // get value for aux from label
                    rmsValue = Regex.Match(label19.Text, @"\d+").Value;

                    AudioInMeasurements[1] = Convert.ToDouble(rmsValue);
                    this.Refresh();


                    // validate measurements

                    if ((AudioInMeasurements[0] > 90) & (AudioInMeasurements[0] < 120) & (AudioInMeasurements[1] > 90) & (AudioInMeasurements[1] < 120))
                    {

                        AuxResult = true;

                        led7.OffColor = Color.LimeGreen;


                    }
                    else
                    {


                        AuxResult = false;

                        led7.OffColor = Color.Red;

                    }


                    this.Refresh();

                }


                // test mic input


                if (instumentStatus == 0)
                {


                    led8.OffColor = Color.Yellow;   /// seems to be crashing here!!!!!!
                    this.Refresh();

                    MessageBox.Show("Switch Out to Tp402 and In/Out to Mic.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);


                    short status;


                    status = Imports.SetSiggenBuiltIn(handle, 0, 25000, Imports.SiggenWaveType.Sine, 1000, 1000, 0, 0, Imports.SiggenSweepType.Up, false, 1, 1, Imports.SiggenTrigType.Rising, Imports.SiggenTrigSource.None, 0);
                    // allow some settling time

                    Thread.Sleep(1000);


                    GetChanB();
                    Thread.Sleep(500);
                    GetChanB();  // have to do twice due to offset in buffer.. I don't know why yet.


                    // get value for aux from label

                    string rmsValue = Regex.Match(label19.Text, @"\d+").Value;


                    AudioInMeasurements[2] = Convert.ToDouble(rmsValue);

                    if ((AudioInMeasurements[2] > 625) & (AudioInMeasurements[2] < 675))
                    {
                        MicResult = true;
                        led8.OffColor = Color.LimeGreen;

                    }
                    else
                    {
                        MicResult = false;
                        led8.OffColor = Color.Red;


                    }



                }


                // charge current test

                if (instumentStatus == 0)
                {


                    // 1.  apply 5 V on VBUSS to enter charge mode





                    chgOn();




                    

 


                  

                    // 2.  run boot test


                    // need to wait for tx to enumerate 

                    // do not enable charge until we have enumerated.  Keep termister out of circuit until we connect.
                    // begin a timer here for valid boot.  It takes about 30 seconds to boot.

                    led5.OffColor = Color.Yellow;

                    this.Refresh();

                    timer3.Interval = 20000; // 40 seconds
                    timer3.Enabled = true;

                    IxM_port = findIxM();

                    while (IxM_port == "NoIxM")
                    {

                        IxM_port = findIxM();
                        Application.DoEvents();

                    }



                    Console.WriteLine("Found Transmitter on {0}", IxM_port);


                    DUTport.PortName = IxM_port;

                    DUTport.Open();
                    Txresponse = TxCommand("ver");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                    //BootTest();
                    Thread.Sleep(500);  // need delay between transactions



                    if (Txresponse.Contains("Firmware:"))
                    {

                        led5.OffColor = Color.LimeGreen;


                    }
                    else
                    {
                        led5.OffColor = Color.Red;


                    }


                    this.Refresh();




                    // 2.  measure charge current


                    ReadChargeCurrent();



                    // 3. connect to shell to make sure boot worked.




                }


                 // set up DUT for mfg test and run

                if (instumentStatus == 0)
                {


                    float IRDutycyle = 0;
                    // turn LED's ON  

                    Txresponse = TxCommand("IRLED = 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr
                   
                    
                    
                    // setup mfg mode



                    Txresponse = TxCommand("mfgt= 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr



                    timer2.Interval = 1000;


                    // unmute

                   // IrLedOn();

                    Thread.Sleep(500);  // need delay between transactions

                   // SetMFGMode();


                    Thread.Sleep(500);  // need delay between transactions

                    // set to channel A via shell

                    // SetChannel(1);  CHI = 0

                    Txresponse = TxCommand("CHI = 0");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    led9.OffColor = Color.Yellow;

                    this.Refresh();

                    // measure duty cycle
                    //button2.PerformClick();
                    timer2.Enabled = true;


                    // Adjust to 25%
                    IRDutycyle = getdutycycle();



                    if ((IRDutycyle > 27) | (IRDutycyle < 23))
                        AdjustDuty();


                    while (timer2.Enabled == true)
                    {

                        Application.DoEvents();

                    }


                    // validate


                    IRDutycyle = getdutycycle();
                    if ((IRDutycyle < 27) | (IRDutycyle > 23))
                    {
                        led9.OffColor = Color.LimeGreen;


                    }
                    else
                    {
                        led9.OffColor = Color.Red;

                    }

                    this.Refresh();
                   


                    // measure Current


                    //BK2831E_2_SetCurrent setupCurrent = new BK2831E_2_SetCurrent();

                    BK2831E_2_ReadCurrent GetIRCurrent = new BK2831E_2_ReadCurrent();



                    //setupCurrent.Run();


                    // set to low coverage mode

                    Txresponse = TxCommand("cov = 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    BK2831E_2_ReadCurrentResults results = GetIRCurrent.Run();

                    string myCurrent = results.Token2.ToString();



                    double IRCurrent = Convert.ToDouble(myCurrent);






                    // adjust to desired value



                  //  SetChannel(2);


                    Txresponse = TxCommand("CHI = 1");
                    timer3.Enabled = false;




                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    led10.OffColor = Color.Yellow;

                    this.Refresh();

                    // measure duty cycle
                   // button2.PerformClick();


                    timer2.Enabled = true;

                    // Adjust to 25%
                    IRDutycyle = getdutycycle();



                    if ((IRDutycyle > 27) | (IRDutycyle < 23))
                        AdjustDuty();


                    while (timer2.Enabled == true)
                    {

                        Application.DoEvents();

                    }

                    // validate


                    IRDutycyle = getdutycycle();
                    if ((IRDutycyle < 27) | (IRDutycyle > 23))
                    {
                        led10.OffColor = Color.LimeGreen;


                    }
                    else
                    {
                        led10.OffColor = Color.Red;

                    }

                    this.Refresh();

                    // Adjust to 25%


                    // measure Current


                    // adjust to desired value

                    //SetChannel(3);

                    Txresponse = TxCommand("CHI = 2");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    led11.OffColor = Color.Yellow;

                    this.Refresh();

                    // measure duty cycle
                   // button2.PerformClick();

                    timer2.Enabled = true;


                    // Adjust to 25%
                    IRDutycyle = getdutycycle();



                    if ((IRDutycyle > 27) | (IRDutycyle < 23))
                        AdjustDuty();


                    while (timer2.Enabled == true)
                    {

                        Application.DoEvents();

                    }


                    IRDutycyle = getdutycycle();
                    if ((IRDutycyle < 27) | (IRDutycyle > 23))
                    {
                        led11.OffColor = Color.LimeGreen;


                    }
                    else
                    {
                        led11.OffColor = Color.Red;

                    }

                    this.Refresh();

                    // Adjust to 25%


                    // measure Current


                    // adjust to desired value

                  //  SetChannel(4);

                    Txresponse = TxCommand("CHI = 3");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    led12.OffColor = Color.Yellow;

                    this.Refresh();

                    // measure duty cycle
                   // button2.PerformClick();


                    timer2.Enabled = true;

                    // Adjust to 25%
                    IRDutycyle = getdutycycle();



                    if ((IRDutycyle > 27) | (IRDutycyle < 23))
                        AdjustDuty();


                    while (timer2.Enabled == true)
                    {

                        Application.DoEvents();

                    }


                    IRDutycyle = getdutycycle();
                    if ((IRDutycyle < 27) | (IRDutycyle > 23))
                    {
                        led12.OffColor = Color.LimeGreen;


                    }
                    else
                    {
                        led12.OffColor = Color.Red;

                    }

                    this.Refresh();
                    // Adjust to 25%


                    // measure Current


                    // adjust to desired value



                  //  SetChannel(5);


                    Txresponse = TxCommand("CHI = 4");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    led13.OffColor = Color.Yellow;

                    this.Refresh();

                    // measure duty cycle
                    //button2.PerformClick();

                    timer2.Enabled = true;

                    // Adjust to 25%
                    IRDutycyle = getdutycycle();



                    if ((IRDutycyle > 27) | (IRDutycyle < 23))
                        AdjustDuty();


                    while (timer2.Enabled == true)
                    {

                        Application.DoEvents();

                    }

                    IRDutycyle = getdutycycle();
                    if ((IRDutycyle < 27) | (IRDutycyle > 23))
                    {
                        led13.OffColor = Color.LimeGreen;


                    }
                    else
                    {
                        led13.OffColor = Color.Red;

                    }

                    this.Refresh();

                    // Save Settings!!














                    





                    // look at output signal







                }






               // chgOff(); // turn off charge voltage

        }


        private void AdjustDuty()
        {
            // start at .25 IRPW and step by .005 until we get to 25%

            float IRDutycycle;
            float setduty = 0.25f;
            bool dutyOK = false;
            string txcommandstring;









            do
            {


                txcommandstring = "irpw = " + setduty.ToString();

                Txresponse = TxCommand(txcommandstring);



                Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                IRDutycycle = getdutycycle();

                this.Refresh();




                if ((IRDutycycle < 27) & (IRDutycycle > 23))
                {

                    dutyOK = true;
                }
                else
                {

                    setduty = setduty - .005f;

                }

            } while (!dutyOK);






        }


        private void myMethod(DataReceivedEventArgs f)
        {
            //Do something with e.Data

            // this is troughing a null reference.
            
            
            try
            {


                
                flashstring = f.Data.ToString();
            }
            catch
            {
                Console.Write("Error... \n");


            }
            



            Console.Write("Return Data: " + flashstring + "\n");

            // look for programming success so we can move on.



           

            if (flashstring.Contains("Verify passed"))
            {
                ProgramResult = true;

                ProgramInProgress = false;
                progressSleep = 10;  // speed up progress bar


            }

            if(flashstring.Contains("Failed"))
            {
                ProgramResult = false;

                ProgramInProgress = false;  // only trigger on errore
                progressSleep = 10; 

            }
             




        }

        void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            int total = 57; //some number (this is your variable to change)!!

            for (int i = 0; i <= total; i++) //some number (total)
            {
                System.Threading.Thread.Sleep(progressSleep);
                int percents = (i * 100) / total;
                bgw.ReportProgress(percents, i);
                //2 arguments:
                //1. procenteges (from 0 t0 100) - i do a calcumation 
                //2. some current value!
            }
        }

        void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
           // label1.Text = String.Format("Progress: {0} %", e.ProgressPercentage);
            //label2.Text = String.Format("Total items transfered: {0}", e.UserState);
        }

        void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //do the code when bgv completes its work
        }

//------------------------------------------------------------------------------------------------------

        private void SetChannel(int Channel)
        {


            SetChannel_A Ch_A = new SetChannel_A();

            SetChannel_B Ch_B = new SetChannel_B();

            SetChannel_C Ch_C = new SetChannel_C();

            SetChannel_D Ch_D = new SetChannel_D();

            SetChannel_E Ch_E = new SetChannel_E();




            switch (Channel)
            {
                case 1:
                    Ch_A.Run();

                    break;
                case 2:

                    Ch_B.Run();
                    break;

                case 3:

                    Ch_C.Run();
                    break;

                case 4:

                    Ch_D.Run();
                    break;

                case 5:
                    Ch_E.Run();
                    break;





                default:
                    Ch_A.Run();

                    break;


            }





        }




        private void IrLedOn()
        {

            IrLEDOn IRON = new IrLEDOn();


            IRON.Run();



        }
        
        private void SetMFGMode()
        {
            DUTMfgMode MFGMode = new DUTMfgMode();

            DUTMfgModeResults results = MFGMode.Run();


            string returnstring = results.Token2;


            if (returnstring.Contains("mfgtest = 1"))
            {

                Debug.Print(" DUT now in MFG Mode!");


            }



        }
        private void BootTest()
        {


            led5.OffColor = Color.Yellow;

            this.Refresh();

          //  DUTBoot DBoot = new DUTBoot();

          //  DUTBootResults results = DBoot.Run();

            // BK2831E_ReadCurrentResults results = myReadCurrent.Run();







            //string DUTVer = results.Token;


            /*
            if (DUTVer.Contains("Firmware: 1.02"))
            {

                led5.OffColor = Color.LimeGreen;

                BootResult = true;
                instumentStatus = 0;




            }
            else
            {
                led5.OffColor = Color.Red;

                BootResult = false;

                instumentStatus = 1;
            }

            */

            this.Refresh();



        }


        private void Ton()
        {
            try
            {
                using (Task digitalWriteTask = new Task())
                {
                    digitalWriteTask.DOChannels.CreateChannel("NI-USB-6501/port1/line7", "",
                        ChannelLineGrouping.OneChannelForAllLines);
                    bool[] dataArray = new bool[1];
                    dataArray[0] = false;
                    DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalWriteTask.Stream);
                    writer.WriteSingleSampleMultiLine(true, dataArray);
                }
            }
            catch (DaqException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {

            }



        }

        private void Toff()
        {

            try
            {
                using (Task digitalWriteTask = new Task())
                {
                    digitalWriteTask.DOChannels.CreateChannel("NI-USB-6501/port1/line7", "",
                        ChannelLineGrouping.OneChannelForAllLines);
                    bool[] dataArray = new bool[1];
                    dataArray[0] = true;
                    DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalWriteTask.Stream);
                    writer.WriteSingleSampleMultiLine(true, dataArray);
                }
            }
            catch (DaqException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {

            }


        }


        private void chgOn()
        {
            try
            {
                using (Task digitalWriteTask = new Task())
                {
                    digitalWriteTask.DOChannels.CreateChannel("NI-USB-6501/port1/line6", "",
                        ChannelLineGrouping.OneChannelForAllLines);
                    bool[] dataArray = new bool[1];
                    dataArray[0] = false;
                    DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalWriteTask.Stream);
                    writer.WriteSingleSampleMultiLine(true, dataArray);
                }
            }
            catch (DaqException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {

            }





        }


        private void chgOff()
        {

            try
            {
                using (Task digitalWriteTask = new Task())
                {
                    digitalWriteTask.DOChannels.CreateChannel("NI-USB-6501/port1/line6", "",
                        ChannelLineGrouping.OneChannelForAllLines);
                    bool[] dataArray = new bool[1];
                    dataArray[0] = true;
                    DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalWriteTask.Stream);
                    writer.WriteSingleSampleMultiLine(true, dataArray);
                }
            }
            catch (DaqException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {

            }



        }






        /* Brief
         * 
         * Reads power up Current
         * 
         */

        private void ReadChargeCurrent()
        {
            TPLabel.Text = "Charge Current";
            led29.OffColor = Color.Yellow;
            this.Refresh();


      


            BK1685B_2_INIT BKPS2_INIT = new BK1685B_2_INIT();



         

            BK1685B_2_GETD ReadChargeCurrent = new BK1685B_2_GETD();


            
            
           
            
            // put thermistor in circut to activate charge

            Ton();


            Thread.Sleep(3000);


            // read charge current
            
            
            
            
            BK1685B_2_GETDResults results = ReadChargeCurrent.Run();


            // display results




            Toff();   // remove thermistor


       



            //MAY HAVE TO PUT A DELAY HERE

            Thread.Sleep(1000);

            string current = results.Token.Substring(results.Token.Length - 3);

            double Ccurrent;
            Ccurrent = Convert.ToDouble(current);

            MeterReading.Text = Ccurrent.ToString() + " mA";

            ChargeCurrent = Math.Abs(Ccurrent);



            if ((ChargeCurrent > 200) & (powerupCurrent < 520))  // takes a bit to settle back on boot
            {
                ChargeCurrentResult = true;
                led29.OffColor = Color.LimeGreen;


            }
            else
            {
                ChargeCurrentResult = false;
                led29.OffColor = Color.Red;

                /*
                BK1685B_OFF BK1685bOff = new BK1685B_OFF();

                BK1685bOff.Run();

                */
                // ToDo   exit test!


                instumentStatus = 1;

            }

            this.Refresh();

      



        }

        private void Test_ONCurrent()
        {

            TPLabel.Text = "Current";

            led2.OffColor = Color.Yellow;
            this.Refresh();


            BK1685BGETD ReadChargeCurrent = new BK1685BGETD();



            BK1685BGETDResults results = ReadChargeCurrent.Run();


            // CURRENT IS LAST THREE DIGITS

            string current = results.Token2.Substring(results.Token2.Length - 3);

          

            Thread.Sleep(1000);


            double Ccurrent = 0;
             Ccurrent = Convert.ToDouble(current);

            MeterReading.Text = Ccurrent.ToString() + " mA";

            powerupCurrent = Math.Abs(Ccurrent);


            if ((powerupCurrent > 20) & (powerupCurrent < 50))  // takes a bit to settle back on boot
            {
                PowerUpCurrentResult = true;
                led2.OffColor = Color.LimeGreen;


            }
            else
            {
                PowerUpCurrentResult = false;
                led2.OffColor = Color.Red;

                /*
                BK1685B_OFF BK1685bOff = new BK1685B_OFF();

                BK1685bOff.Run();

                */
                // ToDo   exit test!


                instumentStatus = 1;

            }

            this.Refresh();

          //  SetVoltage.Run();  // switch back to voltage measurents.


           
           


        }

        private void Test_01()
        {

            BK2831E_ReadVoltage ReadVoltage = new BK2831E_ReadVoltage();

            BK2831E_ReadVoltageResults results = ReadVoltage.Run();

            // Display value
            double Vvalue;
            Vvalue = Convert.ToDouble(results.Token);


            MeterReading.Text = Vvalue.ToString() + " VDC";


        }


        /* Breif
         * Read voltage on Test Point 102
         * Regulated 3V power supply for micro-controller
         * 
         * note: Switching is stubed out with user prompt until switching is integrated 
         * Using manual test fixture to develop for now
         * 
         */

        private void Test_TP102()
        {

          

            TPLabel.Text = "TP102";

            BK2831E_ReadVoltage ReadVoltage = new BK2831E_ReadVoltage();

            BK2831E_ReadVoltageResults results = ReadVoltage.Run();

            // Display value
            double Vvalue;
            Vvalue = Convert.ToDouble(results.Token);


            MeterReading.Text = Vvalue.ToString() + " VDC";

            
            // record value in data

            VoltageMeasurements[0] = Vvalue;

            if ((Vvalue > 2.95) & (Vvalue < 3.05))
            {
                TP102Result = true;

            }
            else
            {
                TP102Result = false;


            }


             


        }

        private void Test_TP116()
        {

           TPLabel.Text = "TP116";
          

            BK2831E_ReadVoltage ReadVoltage = new BK2831E_ReadVoltage();

            BK2831E_ReadVoltageResults results = ReadVoltage.Run();

            // Display value
            double Vvalue;
            Vvalue = Convert.ToDouble(results.Token);


            MeterReading.Text = Vvalue.ToString() + " VDC";

            
            // record value in data

            VoltageMeasurements[1] = Vvalue;

            if ((Vvalue > 3.25) & (Vvalue < 3.35))
            {
                TP116Result = true;

            }
            else
            {
                TP116Result = false;


            }



        }



        private void Test_TP115()
        {

            TPLabel.Text = "TP115";


            BK2831E_ReadVoltage ReadVoltage = new BK2831E_ReadVoltage();

            BK2831E_ReadVoltageResults results = ReadVoltage.Run();

            // Display value
            double Vvalue;
            Vvalue = Convert.ToDouble(results.Token);


            MeterReading.Text = Vvalue.ToString() + " VDC";


            // record value in data

            VoltageMeasurements[2] = Vvalue;

            if ((Vvalue > 3.25) & (Vvalue < 3.35))
            {
                TP115Result = true;

            }
            else
            {
                TP115Result = false;


            }



        }




        private void Test_TP130()
        {

            TPLabel.Text = "TP130";


            BK2831E_ReadVoltage ReadVoltage = new BK2831E_ReadVoltage();

            BK2831E_ReadVoltageResults results = ReadVoltage.Run();

            // Display value
            double Vvalue;
            Vvalue = Convert.ToDouble(results.Token);


            MeterReading.Text = Vvalue.ToString() + " VDC";


            // record value in data

            VoltageMeasurements[3] = Vvalue;

            if ((Vvalue > 1.5) & (Vvalue < 1.7))
            {
                TP130Result = true;

            }
            else
            {
                TP130Result = false;


            }



        }

        private int init2831E()
        {
            pingBK2831e PingMeter = new pingBK2831e();

            pingBK2831eResults results = PingMeter.Run();

            string MeterString;

            MeterString = results.Token;


            // check our value;

            if (MeterString.Contains( "2831E"))
            {
            return 0;
            }
            else
            {
                return 1;
            }


        }

        private int initScope()
        {
            // initilize pico scope



            Console.WriteLine("Frontrow ITM/ISM Test fixture with PS5000a driver ");
            Console.WriteLine("Version 1.0\n");

            //open unit and show splash screen
            Console.WriteLine("\n\nOpening the device...");



           // Imports.DeviceResolution resolution = Imports.DeviceResolution.PS5000A_DR_16BIT;
            Imports.DeviceResolution resolution = Imports.DeviceResolution.PS5000A_DR_8BIT;




            short status = Imports.OpenUnit(out handle, null, resolution);



            //short status = Imports.OpenUnit(out handle);
            Console.WriteLine("Handle: {0}", handle);
            if (status == 0)
            {
                Console.WriteLine("Device is DC powered\n");
                Console.WriteLine("Device opened successfully\n");
                // WaitForKey();
                // show msgbox
                //MessageBox.Show("PicoScope Not Found. Check your connections.");





            }
            else if (status == 282)
            {


                status = Imports.ChangePowerSource(handle, status);
                Console.WriteLine("Device is USB powered\n");
                Console.WriteLine("Device opened successfully\n");


                StringBuilder UnitInfo = new StringBuilder(80);



                string[] description = {
                           "Driver Version    ",
                           "USB Version       ",
                           "Hardware Version  ",
                           "Variant Info      ",
                           "Serial            ",
                           "Cal Date          ",
                           "Kernel Ver        ",
                           "Digital Hardware  ",
                           "Analogue Hardware "
                         };



            }
            else
            {
                _handle = handle;

                //ConsoleExample consoleExample = new ConsoleExample(handle);
                //consoleExample.Run();

                Console.WriteLine("Device  not opened successfully\n");
                Console.WriteLine("Error code : {0}", status);

                MessageBox.Show("PicoScope Not Found. Check your connections.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);




                Imports.CloseUnit(handle);
                // Close();
                return 1;

                // kill the programm



            }


            // set our coupling

            return 0;




  



        }


        private void GetChanB()
        {

            //' Setup channel B

            short status;
            long total = 0;
            float ave = 0;


            status = Imports.SetChannel(handle, Imports.Channel.ChannelB, 1, 0, Imports.Range.Range_5V, 0);  // aC coupled.  

            // Turn off other channels (just A for this model)

            status = Imports.SetChannel(handle, Imports.Channel.ChannelA, 0, 1, Imports.Range.Range_5V, 0);



            //Find maximum ADC count (resolution dependent)

            status = Imports.MaximumValue(handle, out maxADCValue);

            Console.WriteLine("maxADCValue : {0}", maxADCValue);



            timebase = 65;  // was 65  for audio
            numPreTriggerSamples = 0;
            numPostTriggerSamples = 0;  // was 10k
            totalSamples = numPreTriggerSamples + numPostTriggerSamples;

            timeIntervalNs = 0;  // this was a CSng directive in vb
            maxSamples = 0;
            segmentIndex = 0;
            getTimebase2Status = 14; // Initialise as invalid timebase


            while (getTimebase2Status != 0)
            {
                //getTimebase2Status = ps5000aGetTimebase2(handle, timebase, totalSamples, timeIntervalNs, maxSamples, segmentIndex)
                getTimebase2Status = Imports.GetTimebase(handle, timebase, totalSamples, out timeIntervalNs, out maxSamples, segmentIndex);

                if (getTimebase2Status != 0)
                    timebase++;


            }

            Console.WriteLine("Timebase: {0} Sample interval: {1}ns, Max Samples: {2}", timebase, timeIntervalNs, maxSamples);


            // Setup trigger




            threshold = mvToAdc(0, (int)Imports.Range.Range_5V);

            Console.WriteLine("Trigger threshold: {0}mV ({1} ADC Counts)", threshold, 200);

            delay = 0;
            autoTriggerMs = 0;
            int timeIndisposed;


            bool retry;
            uint sampleCount = 2048;

            string data;
            int x;
            int _channelCount = 1;


            short[] minBuffers = new short[sampleCount];
            short[] maxBuffers = new short[sampleCount];
            PinnedArray<short>[] minPinned = new PinnedArray<short>[_channelCount];
            PinnedArray<short>[] maxPinned = new PinnedArray<short>[_channelCount];

            minPinned[0] = new PinnedArray<short>(minBuffers);
            maxPinned[0] = new PinnedArray<short>(maxBuffers);
            status = Imports.SetDataBuffers(handle, Imports.Channel.ChannelB, maxBuffers, minBuffers, (int)sampleCount, 0, Imports.RatioMode.None);
            Console.WriteLine("BlockData\n");


            status = Imports.SetSimpleTrigger(handle, 0, Imports.Channel.ChannelB, threshold, Imports.ThresholdDirection.Rising, delay, autoTriggerMs);


            //Capture block

            //Create instance of delegate
            //ps5000aBlockCallback = New ps5000aBlockReady(AddressOf BlockCallback)




            _ready = false;
            _callbackDelegate = BlockCallback;



            Thread.Sleep(500);

            do
            {
                retry = false;
                status = Imports.RunBlock(handle, 0, (int)sampleCount, timebase, out timeIndisposed, 0, _callbackDelegate, IntPtr.Zero);
                if (status == (short)Imports.PICO_POWER_SUPPLY_CONNECTED || status == (short)Imports.PICO_POWER_SUPPLY_NOT_CONNECTED || status == (short)Imports.PICO_POWER_SUPPLY_UNDERVOLTAGE)
                {
                    retry = true;
                }
                else
                {
                    Console.WriteLine("Run Block Called\n");
                }
            }
            while (retry);

            Console.WriteLine("Waiting for Data\n");

            while (!_ready)
            {
                Thread.Sleep(100);
            }

            Imports.Stop(handle);

            int[] mVBuf = new int[2048];

            float high = 0;
            float low = 0;

            int cyclecount = 0;
            bool iscountinglow = false;
            bool iscountinghigh = false;

            bool isTriggered = false;


            double rms;

            double dBV;
            double dBm;





            Mitov.SignalLab.RealBuffer DataBuffer = new Mitov.SignalLab.RealBuffer(2048);  // set up buffer for anylisis

            if (_ready)
            {
                short overflow;
                status = Imports.GetValues(handle, 0, ref sampleCount, 1, Imports.DownSamplingMode.None, 0, out overflow);
                if (status == (short)Imports.PICO_OK)
                {

                    Console.WriteLine("Have Data\n");
                    for (x = 0; x < sampleCount; x++)
                    {
                        //data = maxBuffers[x].ToString();

                        //textData.AppendText("\n");

                        // keep a running total for averaging our samples
                        // 5V = 32768 (16 bit) So we need to scale our values accorddingly



                        int mymV = adcToMv(maxBuffers[x], (int)Imports.Range.Range_5V);
                        //data = mymV.ToString();
                        //Console.WriteLine(data);

                        mVBuf[x] = mymV;

                        // calculate duty cycle on one full cycle of wave form





                        total = ((Math.Abs(mymV * mymV))) + total;  // convert to mV  // don't square

                        // for duty cycle calculation set high Threshold to
                        // set low threshold 

                        DataBuffer[x] = Convert.ToDouble(mymV);





                    }




                    // calculate average over our 1000 samples


                    genericReal1.SendData(DataBuffer); // send the data we have



             




















                    // label18.Text = "Frequency = " + (frequency.ToString());


                }
                else
                {
                    Console.WriteLine("No Data\n");

                }
            }
            else
            {
                Console.WriteLine("data collection aborted\n");
            }


            // calculate average over our 1000 samples


            ave = total / sampleCount;


            rms = Math.Sqrt(ave);


            Console.WriteLine("RMS value = {0}mV.", rms);


            label19.Text = rms.ToString() + " Vrms";

            dBV = 20 * Math.Log10(rms / 1000);

            dBm = 10 * Math.Log10(rms / 1000);


            Console.WriteLine("dBV value = {0}dBV.", dBV);

            Console.WriteLine("dBm value = {0}dBm.", dBm);


            Imports.Stop(_handle);

            foreach (PinnedArray<short> p in minPinned)
            {
                if (p != null)
                    p.Dispose();
            }
            foreach (PinnedArray<short> p in maxPinned)
            {
                if (p != null)
                    p.Dispose();
            }



            Console.WriteLine("Ping status = {0}.", status);




        }

        private void button2_Click(object sender, EventArgs e)
        {


            //' Setup channel A

            short status;
            long total = 0;
            float ave = 0;


            status = Imports.SetChannel(handle, Imports.Channel.ChannelA, 1, 1, Imports.Range.Range_5V, 0);  // dC coupled.  

            // Turn off other channels (just b for this model)

            status = Imports.SetChannel(handle, Imports.Channel.ChannelB, 0, 1, Imports.Range.Range_5V, 0);



            //Find maximum ADC count (resolution dependent)

            status = Imports.MaximumValue(handle, out maxADCValue);

            Console.WriteLine("maxADCValue : {0}", maxADCValue);



            timebase = 1;  // was 65
            numPreTriggerSamples = 0;
            numPostTriggerSamples = 0;  // was 10k
            totalSamples = numPreTriggerSamples + numPostTriggerSamples;

            timeIntervalNs = 0;  // this was a CSng directive in vb
            maxSamples = 0;
            segmentIndex = 0;
            getTimebase2Status = 14; // Initialise as invalid timebase


            while (getTimebase2Status != 0)
            {
                //getTimebase2Status = ps5000aGetTimebase2(handle, timebase, totalSamples, timeIntervalNs, maxSamples, segmentIndex)
                getTimebase2Status = Imports.GetTimebase(handle, timebase, totalSamples, out timeIntervalNs, out maxSamples, segmentIndex);

                if (getTimebase2Status != 0)
                    timebase++;


            }

            Console.WriteLine("Timebase: {0} Sample interval: {1}ns, Max Samples: {2}", timebase, timeIntervalNs, maxSamples);


            // Setup trigger




            threshold = mvToAdc(0, (int)Imports.Range.Range_5V);

            Console.WriteLine("Trigger threshold: {0}mV ({1} ADC Counts)", threshold, 200);

            delay = 0;
            autoTriggerMs = 0;
            int timeIndisposed;


            bool retry;
            uint sampleCount = 2048;

            string data;
            int x;
            int _channelCount = 1;


            short[] minBuffers = new short[sampleCount];
            short[] maxBuffers = new short[sampleCount];
            PinnedArray<short>[] minPinned = new PinnedArray<short>[_channelCount];
            PinnedArray<short>[] maxPinned = new PinnedArray<short>[_channelCount];

            minPinned[0] = new PinnedArray<short>(minBuffers);
            maxPinned[0] = new PinnedArray<short>(maxBuffers);
            status = Imports.SetDataBuffers(handle, Imports.Channel.ChannelA, maxBuffers, minBuffers, (int)sampleCount, 0, Imports.RatioMode.None);
            Console.WriteLine("BlockData\n");


            status = Imports.SetSimpleTrigger(handle, 0, Imports.Channel.ChannelA, threshold, Imports.ThresholdDirection.Rising, delay, autoTriggerMs);


            //Capture block

            //Create instance of delegate
            //ps5000aBlockCallback = New ps5000aBlockReady(AddressOf BlockCallback)




            _ready = false;
            _callbackDelegate = BlockCallback;



            Thread.Sleep(500);

            do
            {
                retry = false;
                status = Imports.RunBlock(handle, 0, (int)sampleCount, timebase, out timeIndisposed, 0, _callbackDelegate, IntPtr.Zero);
                if (status == (short)Imports.PICO_POWER_SUPPLY_CONNECTED || status == (short)Imports.PICO_POWER_SUPPLY_NOT_CONNECTED || status == (short)Imports.PICO_POWER_SUPPLY_UNDERVOLTAGE)
                {
                    retry = true;
                }
                else
                {
                    Console.WriteLine("Run Block Called\n");
                }
            }
            while (retry);

            Console.WriteLine("Waiting for Data\n");

            while (!_ready)
            {
                Thread.Sleep(100);
            }

            Imports.Stop(handle);

            int[] mVBuf = new int[2048];

            float high = 0;
            float low = 0;

            int cyclecount = 0;
            bool iscountinglow = false;
            bool iscountinghigh = false;

            bool isTriggered = false;



            




            Mitov.SignalLab.RealBuffer DataBuffer = new Mitov.SignalLab.RealBuffer(2048);  // set up buffer for anylisis

            if (_ready)
            {
                short overflow;
                status = Imports.GetValues(handle, 0, ref sampleCount, 1, Imports.DownSamplingMode.None, 0, out overflow);
                if (status == (short)Imports.PICO_OK)
                {

                    Console.WriteLine("Have Data\n");
                    for (x = 0; x < sampleCount; x++)
                    {
                        //data = maxBuffers[x].ToString();

                        //textData.AppendText("\n");

                        // keep a running total for averaging our samples
                        // 5V = 32768 (16 bit) So we need to scale our values accorddingly



                        int mymV = adcToMv(maxBuffers[x], (int)Imports.Range.Range_5V);
                        //data = mymV.ToString();
                        //Console.WriteLine(data);

                        mVBuf[x] = mymV;

                        // calculate duty cycle on one full cycle of wave form

                        


                        
/* move out of the capture loop 

                        if (mymV < 0)
                        {


                            if (!isTriggered)
                            if(x>1)
                            if (mVBuf[x - 1] > 0)   // only do once!
                            {
                                iscountinglow = true;

                                iscountinghigh = true;
                                isTriggered = true;
                            }

                       

                            if(iscountinglow)
                               low++;


                            


                          

                        }
                        else
                        {

                            if (iscountinghigh)
                            {

                                if (iscountinglow)
                                    iscountinglow = false;

                                high++;

                            }




                            if (isTriggered)
                                if (x > 1)
                                    if (mVBuf[x - 1] < 0)
                                    {


                                        iscountinghigh = false;
                                    }
                       



                        }

*/
                        total = ((Math.Abs(mymV * mymV))) + total;  // convert to mV  // don't square

                        // for duty cycle calculation set high Threshold to
                        // set low threshold 

                        DataBuffer[x] = Convert.ToDouble(mymV);





                    }




                    // calculate average over our 1000 samples
               

                       genericReal1.SendData(DataBuffer); // send the data we have



                    // calculate duty cycle  = (time on/ time off) *100


                    // find full cycle between peeks

                       int RisingEdge=0;
                       int FallingEdge=0;
                       int Risingedge2 = 0;


                       for (x = 1; x < sampleCount; x++)
                       {

                           if(RisingEdge==0)
                           if(mVBuf[x]>1700)
                           if (mVBuf[x - 1] < 1700)
                               RisingEdge = x;

                           if(RisingEdge>0)  // only do after we find the first rising edge
                           if(FallingEdge==0)
                               if (mVBuf[x] < 1600)    // find the next rising edge for 1 full cycle
                               if (mVBuf[x - 1] > 1600)
                                   FallingEdge = x;

                       }




                       for (x = FallingEdge; x < sampleCount; x++)
                       {
                           if (Risingedge2==0)
                               if (mVBuf[x] > 1700)
                                   if (mVBuf[x - 1] < 1700)
                                       Risingedge2 = x;

                       }



                       for (x = RisingEdge; x < Risingedge2; x++)
                       {
                           if (mVBuf[x] < 1700)
                           {
                               low++;
                           }
                           else
                           {

                               high++;
                           }

                           


                       }


                       float dutycycle;

                       dutycycle = (high / (high+low)) * 100;

                       label17.Text = "Duty Cycle = " + (dutycycle.ToString());

                    // for frequency we just need to kno the sample period  and count the wavelength and invert
                       double frequency;
                       frequency = 1 / (2e-9 * (high + low));


                      // label18.Text = "Frequency = " + (frequency.ToString());


                }
                else
                {
                    Console.WriteLine("No Data\n");

                }
            }
            else
            {
                Console.WriteLine("data collection aborted\n");
            }

        }




        public float getdutycycle()
        {
            //' Setup channel A

            short status;
            long total = 0;
            float ave = 0;
            float dutycycle=0;


            status = Imports.SetChannel(handle, Imports.Channel.ChannelA, 1, 1, Imports.Range.Range_5V, 0);  // dC coupled.  

            // Turn off other channels (just b for this model)

            status = Imports.SetChannel(handle, Imports.Channel.ChannelB, 0, 1, Imports.Range.Range_5V, 0);



            //Find maximum ADC count (resolution dependent)

            status = Imports.MaximumValue(handle, out maxADCValue);

            Console.WriteLine("maxADCValue : {0}", maxADCValue);



            timebase = 1;  // was 65
            numPreTriggerSamples = 0;
            numPostTriggerSamples = 0;  // was 10k
            totalSamples = numPreTriggerSamples + numPostTriggerSamples;

            timeIntervalNs = 0;  // this was a CSng directive in vb
            maxSamples = 0;
            segmentIndex = 0;
            getTimebase2Status = 14; // Initialise as invalid timebase


            while (getTimebase2Status != 0)
            {
                //getTimebase2Status = ps5000aGetTimebase2(handle, timebase, totalSamples, timeIntervalNs, maxSamples, segmentIndex)
                getTimebase2Status = Imports.GetTimebase(handle, timebase, totalSamples, out timeIntervalNs, out maxSamples, segmentIndex);

                if (getTimebase2Status != 0)
                    timebase++;


            }

            Console.WriteLine("Timebase: {0} Sample interval: {1}ns, Max Samples: {2}", timebase, timeIntervalNs, maxSamples);


            // Setup trigger




            threshold = mvToAdc(0, (int)Imports.Range.Range_5V);

            Console.WriteLine("Trigger threshold: {0}mV ({1} ADC Counts)", threshold, 200);

            delay = 0;
            autoTriggerMs = 0;
            int timeIndisposed;


            bool retry;
            uint sampleCount = 2048;

            string data;
            int x;
            int _channelCount = 1;


            short[] minBuffers = new short[sampleCount];
            short[] maxBuffers = new short[sampleCount];
            PinnedArray<short>[] minPinned = new PinnedArray<short>[_channelCount];
            PinnedArray<short>[] maxPinned = new PinnedArray<short>[_channelCount];

            minPinned[0] = new PinnedArray<short>(minBuffers);
            maxPinned[0] = new PinnedArray<short>(maxBuffers);
            status = Imports.SetDataBuffers(handle, Imports.Channel.ChannelA, maxBuffers, minBuffers, (int)sampleCount, 0, Imports.RatioMode.None);
            Console.WriteLine("BlockData\n");


            status = Imports.SetSimpleTrigger(handle, 0, Imports.Channel.ChannelA, threshold, Imports.ThresholdDirection.Rising, delay, autoTriggerMs);


            //Capture block

            //Create instance of delegate
            //ps5000aBlockCallback = New ps5000aBlockReady(AddressOf BlockCallback)




            _ready = false;
            _callbackDelegate = BlockCallback;



            Thread.Sleep(500);

            do
            {
                retry = false;
                status = Imports.RunBlock(handle, 0, (int)sampleCount, timebase, out timeIndisposed, 0, _callbackDelegate, IntPtr.Zero);
                if (status == (short)Imports.PICO_POWER_SUPPLY_CONNECTED || status == (short)Imports.PICO_POWER_SUPPLY_NOT_CONNECTED || status == (short)Imports.PICO_POWER_SUPPLY_UNDERVOLTAGE)
                {
                    retry = true;
                }
                else
                {
                    Console.WriteLine("Run Block Called\n");
                }
            }
            while (retry);

            Console.WriteLine("Waiting for Data\n");

            while (!_ready)
            {
                Thread.Sleep(100);
            }

            Imports.Stop(handle);

            int[] mVBuf = new int[2048];

            float high = 0;
            float low = 0;

            int cyclecount = 0;
            bool iscountinglow = false;
            bool iscountinghigh = false;

            bool isTriggered = false;








            Mitov.SignalLab.RealBuffer DataBuffer = new Mitov.SignalLab.RealBuffer(2048);  // set up buffer for anylisis

            if (_ready)
            {
                short overflow;
                status = Imports.GetValues(handle, 0, ref sampleCount, 1, Imports.DownSamplingMode.None, 0, out overflow);
                if (status == (short)Imports.PICO_OK)
                {

                    Console.WriteLine("Have Data\n");
                    for (x = 0; x < sampleCount; x++)
                    {
                        //data = maxBuffers[x].ToString();

                        //textData.AppendText("\n");

                        // keep a running total for averaging our samples
                        // 5V = 32768 (16 bit) So we need to scale our values accorddingly



                        int mymV = adcToMv(maxBuffers[x], (int)Imports.Range.Range_5V);
                        //data = mymV.ToString();
                        //Console.WriteLine(data);

                        mVBuf[x] = mymV;

                        // calculate duty cycle on one full cycle of wave form





  
                        total = ((Math.Abs(mymV * mymV))) + total;  // convert to mV  // don't square

                        // for duty cycle calculation set high Threshold to
                        // set low threshold 

                        DataBuffer[x] = Convert.ToDouble(mymV);





                    }




                    // calculate average over our 1000 samples


                    genericReal1.SendData(DataBuffer); // send the data we have



                    // calculate duty cycle  = (time on/ time off) *100


                    // find full cycle between peeks

                    int RisingEdge = 0;
                    int FallingEdge = 0;
                    int Risingedge2 = 0;


                    for (x = 1; x < sampleCount; x++)
                    {

                        if (RisingEdge == 0)
                            if (mVBuf[x] > 1700)
                                if (mVBuf[x - 1] < 1700)
                                    RisingEdge = x;

                        if (RisingEdge > 0)  // only do after we find the first rising edge
                            if (FallingEdge == 0)
                                if (mVBuf[x] < 1600)    // find the next rising edge for 1 full cycle
                                    if (mVBuf[x - 1] > 1600)
                                        FallingEdge = x;

                    }




                    for (x = FallingEdge; x < sampleCount; x++)
                    {
                        if (Risingedge2 == 0)
                            if (mVBuf[x] > 1700)
                                if (mVBuf[x - 1] < 1700)
                                    Risingedge2 = x;

                    }



                    for (x = RisingEdge; x < Risingedge2; x++)
                    {
                        if (mVBuf[x] < 1700)
                        {
                            low++;
                        }
                        else
                        {

                            high++;
                        }




                    }


                    

                    dutycycle = (high / (high + low)) * 100;

                    label17.Text = "Duty Cycle = " + (dutycycle.ToString());

                    // for frequency we just need to kno the sample period  and count the wavelength and invert
                    double frequency;
                    frequency = 1 / (2e-9 * (high + low));


                    // label18.Text = "Frequency = " + (frequency.ToString());


                }
                else
                {
                    Console.WriteLine("No Data\n");

                }
            }
            else
            {
                Console.WriteLine("data collection aborted\n");
            }



            return dutycycle;


        }

        // ******************************************************************************************************************************************************************
        // adcToMv - Converts from raw ADC values to mV values. The mV value returned depends upon the ADC count, and the voltage range set for the channel. 
        //
        // Parameters - raw    - An integer holding the ADC count to be converted to mV
        //            - range  - A value indicating where in the 'inputRanges' array the range value can be found
        //
        // Returns    - value converted into mV
        // *******************************************************************************************************************************************************************

        public int adcToMv(int raw, int range)
        {

            int mVVal;        //Use this variable to force data to be returned as an integer

            mVVal = (raw * inputRanges[range]) / maxADCValue;

            return mVVal;



        }        
        
        //******************************************************************************************************************************************************************
        // mvToAdc - Converts from mV into ADC value. The ADC count returned depends upon the mV value, and the voltage range set for the channel. 
        //
        // Parameters - mv     - An short holding the mv value to be converted to the ADC count
        //            - range  - A value indicating where in the 'inputRanges' array the range value can be found
        //
        // Returns    - value converted into an ADC count
        // *******************************************************************************************************************************************************************
        public short mvToAdc(short mv, int range)
        {
            short adcCount;


            adcCount = (short)((mv / inputRanges[range]) * maxADCValue);

            return adcCount;

        }

        private void noiseStats1_FrequencyResult(object Sender, Mitov.SignalLab.FrequencyEventArgs Args)
        {
            double freq;
            freq = Args.Frequency;

            label18.Text = "Frequency = " + freq.ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Console.Write("Tick  ");

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            GetChanB();

        }



        private void switchChannel0(byte RelayNo)
        {


            uint relaydata = 0x01;


            uint indx;

            for (indx = 0; indx < RelayNo; indx++)
                relaydata = (relaydata << 1);






            try
            {

              

                uint[] generatedData = new uint[writePort_0Component1.NumberOfChannelsToWrite];


      

             
                //throw new System.NotImplementedException("You must populate the data to write before writing the data.");
                generatedData[0] = ~relaydata;   // TURN on relay requested.
              

                writePort_0Component1.WriteAsync(generatedData);

               

                double[] controlData = NationalInstruments.DataConverter.Convert<double[]>(generatedData);
               // numericEditArray1.SetValues(controlData);  // used for control
            }
            catch (NationalInstruments.DAQmx.DaqException ex)
            {
                MessageBox.Show(ex.Message, "DAQ Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // button1.Enabled = true;
                Close();


            }

        }



        private void switchChannel1(byte RelayNo)
        {


            uint relaydata = 0x01;


            uint indx;

            for (indx = 0; indx < RelayNo; indx++)
                relaydata = (relaydata << 1);






            try
            {



                uint[] generatedData = new uint[writePort_1Component1.NumberOfChannelsToWrite];





                //throw new System.NotImplementedException("You must populate the data to write before writing the data.");
                generatedData[0] = ~relaydata;   // TURN on relay requested.


                writePort_1Component1.WriteAsync(generatedData);



                double[] controlData = NationalInstruments.DataConverter.Convert<double[]>(generatedData);
                // numericEditArray1.SetValues(controlData);  // used for control
            }
            catch (NationalInstruments.DAQmx.DaqException ex)
            {
                MessageBox.Show(ex.Message, "DAQ Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // button1.Enabled = true;
                Close();


            }

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Enabled = false;

        }




        public string findIxM()
        {
            bool success = false;
            string myIxM = null;

            var usbDevices = GetUSBDevices();

            foreach (var usbDevice in usbDevices)
            {
                Console.WriteLine("Device ID: {0}, PNP Device ID: {1}, Description: {2}",
                    usbDevice.DeviceID, usbDevice.PnpDeviceID, usbDevice.Description);


                // test

                if (usbDevice.Description == "FrontRow ISM-01 Pass-around Microphone")
                {
                    Console.WriteLine("Got it!!!!!!!!!!!!!!!!!!");

                    Console.WriteLine(usbDevice.DeviceID);
                    // save the port name;
                    myIxM = usbDevice.DeviceID;
                    success = true;

                }

                if (usbDevice.Description == "FrontRow ITM-01 Pendant Microphone")
                {
                    Console.WriteLine("Got it!!!!!!!!!!!!!!!!!!");

                    Console.WriteLine(usbDevice.DeviceID);
                    // save the port name;
                    myIxM = usbDevice.DeviceID;
                    success = true;

                }
            }

            if (!success)
            {
                /*
                MessageBox.Show("Juno not found.  Check your setup.",
               "Error",
               MessageBoxButtons.OK,
               MessageBoxIcon.Exclamation,
               MessageBoxDefaultButton.Button1);
                instumentStatus = 0;
                 */
                //Close();



                return "NoIxM";
            }



            return myIxM;



        }

        private void timer3_Tick(object sender, EventArgs e)
        {

            timer3.Enabled = false;

            MessageBox.Show("No Transmitter found.  Check your setup.",
           "Error",
           MessageBoxButtons.OK,
           MessageBoxIcon.Exclamation,
           MessageBoxDefaultButton.Button1);
            instumentStatus = 1;

            IxM_port = "Error!!!";
        }



        public string TxCommand(string command)
        {
            string theResponse;

            DUTport.Write(command + "\n\r");

            // wait for response
            // set a timer for timeouts
            timer4.Interval = 30000;
            timer4.Enabled = true;



            while (!comResponse)
            {
                Application.DoEvents();

                //TODO  look for timeout
            }

            Thread.Sleep(200);  // allow for slow com port

            // parse response

            // Console.WriteLine("Response From Juno: {0}", comResponseString); // may have to capture this as soon as we have cr

            theResponse = comResponseString;

            comResponse = false;

            //if (!JunoSeralClearLine)
                comResponseString = null;

            return theResponse;



        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            // timeout on com port;

            timer4.Enabled = false;
            comResponse = true;   // so we don't get stuck
        }


        private void DUTport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {


            // Show all the incoming data in the port's buffer

            // we need to wait until there is a cr


            comResponseString = comResponseString + DUTport.ReadExisting();

            // test for CR
            //if (!JunoSeralClearLine)   // so we can use with fsk, we use a timer to set comResponse on a FSK task
                if (comResponseString.Contains((char)13))
                    comResponse = true;




            // Console.WriteLine(comResponseString);

        }

        private void DUTport_DataReceived_1(object sender, SerialDataReceivedEventArgs e)
        {
            comResponseString = comResponseString + DUTport.ReadExisting();

            // test for CR
            //if (!JunoSeralClearLine)   // so we can use with fsk, we use a timer to set comResponse on a FSK task
            if (comResponseString.Contains((char)13))
                comResponse = true;
        }


    }


    class USBDeviceInfo
    {
        public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
        {
            this.DeviceID = deviceID;
            this.PnpDeviceID = pnpDeviceID;
            this.Description = description;
        }
        public string DeviceID { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string Description { get; private set; }
    }
}
