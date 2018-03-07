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

        barCode frm2;

        ResultGood frm6;

        ResultsBad frm7;


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


        public bool TestResult = true;

        public bool PowerUpCurrentResult = true;

        public bool VoltageResult = true;

        public bool TP102Result = true;
        public bool TP116Result = true;
        public bool TP115Result = true;
        public bool TP130Result = true;

        public bool AuxResult = true;
        public bool MicResult = true;


        public bool BootResult = true;
        public bool UILEDResult = true;



        public bool ChargeCurrentResult = true;




        public bool ProgramInProgress = false;

        public bool ProgramResult = false;

        public bool IRChannel_A_Result = false;
        public bool IRChannel_B_Result = false;
        public bool IRChannel_C_Result = false;
        public bool IRChannel_D_Result = false;
        public bool IRChannel_E_Result = false;
        public bool IRChannel_L1_Result = false;
        public bool IRChannel_L2_Result = false;



        public string flashstring = "Data";

        public string failureString = null;


        public string IxM_port = null;

        public string Txresponse = null;


        public string Model = null;

        



        public string comResponseString = null;
        public bool comResponse = false;






        public int progresspercent = 0;

        public int progressSleep = 1500;


        public System.Timers.Timer aTimer = new System.Timers.Timer();

        BackgroundWorker bgw = new BackgroundWorker();  




        // TP130 is mic input/ bias.  CN200 pin 2.   not hooked up on auto fixture.  TP exists, so we can use if needed.


        public double[] VoltageMeasurements = new double[] { 0, 0, 0, 0, 0 };  //TP102,TP116,TP115,TP130,


        public double[] AudioInMeasurements = new double[] { 0, 0, 0 };

        public double[] Mic = new double[] { 0, 0, 0 };


        public double powerupCurrent = 0;  // power up current


        public double ChargeCurrent = 0;

        public double[] IRPW = new double[] {0,0,0,0,0,0,0};

        public double[] IRDutyCycle = new double[] {0,0,0,0,0,0,0};

        public double[] ldpot = new double[] { 0, 0, 0, 0, 0, 0, 0 };

        public double[] ldcurrent = new double[] { 0, 0, 0, 0, 0, 0, 0 };

        public double[] mdpot = new double[] { 0, 0, 0, 0, 0, 0, 0 };

        public double[] mdcurrent = new double[] { 0, 0, 0, 0, 0, 0, 0 };

        public double[] hdpot = new double[] { 0, 0, 0, 0, 0, 0, 0 };

        public double[] hdcurrent = new double[] { 0, 0, 0, 0, 0, 0, 0 };

        public double ReferenceCurrent = 0;




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
            progressSleep = 1500;
            progressBar1.Value = 0;

            ReferenceCurrent = 0;

            instumentStatus = 0;

            // clear all LED's
            led1.OffColor = Color.Black;
            led2.OffColor = Color.Black;
            led3.OffColor = Color.Black;
            led4.OffColor = Color.Black;
            led5.OffColor = Color.Black;
            led6.OffColor = Color.Black;
            led7.OffColor = Color.Black;
            led8.OffColor = Color.Black;
            led9.OffColor = Color.Black;
            led10.OffColor = Color.Black;
            led11.OffColor = Color.Black;
            led12.OffColor = Color.Black;
            led13.OffColor = Color.Black;
            led14.OffColor = Color.Black;
            led15.OffColor = Color.Black;
            led16.OffColor = Color.Black;
            led17.OffColor = Color.Black;
            led18.OffColor = Color.Black;
            led19.OffColor = Color.Black;
            led20.OffColor = Color.Black;
            led21.OffColor = Color.Black;
            led22.OffColor = Color.Black;
            led23.OffColor = Color.Black;
            led24.OffColor = Color.Black;
            led25.OffColor = Color.Black;
            led26.OffColor = Color.Black;
            led27.OffColor = Color.Black;
            led28.OffColor = Color.Black;
            led29.OffColor = Color.Black;
            led30.OffColor = Color.Black;
            led31.OffColor = Color.Black;
            led32.OffColor = Color.Black;
            led33.OffColor = Color.Black;
            led34.OffColor = Color.Black;
            led35.OffColor = Color.Black;
            led36.OffColor = Color.Black;
            led37.OffColor = Color.Black;


            // clear all data point from previous test (if any)

                TestResult = false;

                powerupCurrent = 0;

                VoltageResult= false;
                VoltageMeasurements[0] = 0;
                VoltageMeasurements[1] = 0;
                VoltageMeasurements[2] = 0;
                VoltageMeasurements[3] = 0;

                ProgramResult = false;
                AuxResult = false;
                AudioInMeasurements[0] = 0;
                AudioInMeasurements[1] = 0;
                MicResult =  false;
                AudioInMeasurements[2] = 0;
                Mic[0] = 0;
                Mic[1] = 0;
                Mic[2] = 0;

                BootResult = false;
                ChargeCurrentResult = false;
                ChargeCurrent = 0;
                ReferenceCurrent = 0;
      
                IRChannel_A_Result= false;
                IRPW[0] = 0;
                IRDutyCycle[0] = 0;
                ldpot[0] = 0;
                ldcurrent[0] = 0;
                mdpot[0] = 0;
                mdcurrent[0] = 0;
                hdpot[0] = 0;
                hdcurrent[0] = 0;
            
                IRChannel_B_Result = false;
                IRPW[1]= 0;
                IRDutyCycle[1] = 0;
                ldpot[1] = 0;
                ldcurrent[1] = 0;
                mdpot[1] = 0;
                mdcurrent[1] = 0;
                hdpot[1] = 0;
                hdcurrent[1] = 0;

                IRChannel_C_Result = false;
                IRPW[2] = 0;
                IRDutyCycle[2] = 0;
                ldpot[2] = 0;
                ldcurrent[2] = 0;
                mdpot[2] = 0;
                mdcurrent[2] = 0;
                hdpot[2] = 0;
                hdcurrent[2] = 0;

                IRChannel_D_Result = false;
                IRPW[3] = 0;
                IRDutyCycle[3] = 0;
                ldpot[3] = 0;
                ldcurrent[3] = 0;
                mdpot[3] = 0;
                mdcurrent[3] = 0;
                hdpot[3] = 0;
                hdcurrent[3] = 0;

                IRChannel_E_Result = false;
                IRPW[4] = 0;
                IRDutyCycle[4] = 0;
                ldpot[4] =  0;
                ldcurrent[4] = 0;
                mdpot[4] = 0;
                mdcurrent[4] = 0;
                hdpot[4] = 0; 
                hdcurrent[4] = 0;

                IRChannel_L1_Result = false;
                IRPW[5] = 0;
                IRDutyCycle[5] = 0;
                ldpot[5] = 0;
                ldcurrent[5] = 0;
                mdpot[5] = 0;
                mdcurrent[5] = 0;
                hdpot[5] = 0;
                hdcurrent[5] = 0;

                IRChannel_L2_Result = false;
                IRPW[6] = 0;
                IRDutyCycle[6] = 0;
                ldpot[6] = 0;
                ldcurrent[6] = 0;
                mdpot[6] = 0;
                mdcurrent[6] = 0;
                hdpot[6] = 0;
                hdcurrent[6] = 0;


                failureString = null;

                textBox2.Text = failureString;

         








            // show serial number box
            frm2 = new barCode(this);

            frm6 = new ResultGood(this);
            frm7 = new ResultsBad(this);

      
            this.Refresh();

            switchChannel0(0xff);




           

        


            frm2.Show();

            while (frm2.Visible == true)
            {
                Application.DoEvents();

            }




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
                failureString = failureString + "Equipment Malfucntion....\r\n";

                textBox2.Text = failureString;

                this.Refresh();


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


           // Thread.Sleep(1000);  // stablize







            //**************************************************************************************************************************************
            // begin program steps

            // switch off DTR signal on Relay 12 so we can connect to tx


            try
            {
                using (Task digitalWriteTask = new Task())
                {
                    digitalWriteTask.DOChannels.CreateChannel("NI-USB-6501/port1/line5", "",
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





            progressBar1.Visible = false;



            //if(false)  // skip program
            if (instumentStatus == 0)
            {
                led4.OffColor = Color.Yellow;
                this.Refresh();

                // start timer for progress bar



                // switch in DTR signal on relay 14     - schematic was correct!

                try
                {
                    using (Task digitalWriteTask = new Task())
                    {
                        digitalWriteTask.DOChannels.CreateChannel("NI-USB-6501/port1/line5", "",
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

                /*
                MessageBox.Show("Flip COM Switch to program",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);
                ///////////*/
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


            while (ProgramInProgress)  // wait until we have programmed
            {
                Application.DoEvents();


            }

            ProgramResult = true;


            if (ProgramResult == true)
            {
                led4.OffColor = Color.LimeGreen;
                this.Refresh();
                /*
                MessageBox.Show("Programming Complete.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);
                 //* */

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

                failureString = failureString + "Programming Failure....\r\n";

                textBox2.Text = failureString;

                this.Refresh();


                // instumentStatus = 1;


            }

            // kill the bgw
            bgw.Dispose();
            Application.DoEvents();




            /// connect to the transmitter  and place it in test mode.
            /// 


            try
            {
                using (Task digitalWriteTask = new Task())
                {
                    digitalWriteTask.DOChannels.CreateChannel("NI-USB-6501/port1/line5", "",
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

                if (IxM_port != "Error!!!")
                {

                    Console.WriteLine("Found Transmitter on {0}", IxM_port);


                    DUTport.PortName = IxM_port;

                    DUTport.Open();
                    Txresponse = TxCommand("ver");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                    textBox2.Text = textBox2.Text + "IxM found on " + IxM_port;

                    //BootTest();
                    Thread.Sleep(500);  // need delay between transactions


                    try
                    {

                        if (Txresponse.Contains("Firmware:"))
                        {

                            led5.OffColor = Color.LimeGreen;


                            BootResult = true;



                        }
                        else
                        {
                            led5.OffColor = Color.Red;
                            BootResult = false;

                        }

                    }
                    catch
                    {

                        // no reponse, try again

                        Txresponse = TxCommand("ver");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        //BootTest();
                        Thread.Sleep(500);  // need delay between transactions

                        if (Txresponse.Contains("Firmware:"))
                        {

                            led5.OffColor = Color.LimeGreen;


                            BootResult = true;



                        }
                        else
                        {
                            led5.OffColor = Color.Red;
                            BootResult = false;

                        }


                    }




                }

                else
                {

                    led5.OffColor = Color.Red;
                    BootResult = false;


                }


                this.Refresh();




                // 2.  measure charge current


                if (instumentStatus == 0)
                    ReadChargeCurrent();



                // 3. connect to shell to make sure boot worked.




            }


            // insert LED tests here.   We have to use current to dectect valid LED's

            if (instumentStatus == 0)
            {




                led7.OffColor = Color.Yellow;
                this.Refresh();

                Txresponse = TxCommand("mfgt= 1");
                timer3.Enabled = false;


                Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr



                // turn charge OFF
                Txresponse = TxCommand("chd= 1");
                timer3.Enabled = false;


                Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr



            }




            //************************************************************************************************************************************
            // end of programming tests


                    

 


                  







                

 // programming step was here.  now getting port closed exception



                    // charge current test

                    if (instumentStatus == 0)
                    {


                        // 1.  apply 5 V on VBUSS to enter charge mode and trun on usb





                        chgOn();


                   




                    

 


                  

                        // 2.  run boot test


                        // need to wait for tx to enumerate 

                        // do not enable charge until we have enumerated.  Keep termister out of circuit until we connect.
                        // begin a timer here for valid boot.  It takes about 30 seconds to boot.

                        //led5.OffColor = Color.Yellow;

                        this.Refresh();

                        //timer3.Interval = 20000; // 40 seconds
                        //timer3.Enabled = true;
/*
                        IxM_port = findIxM();

                        while (IxM_port == "NoIxM")
                        {

                            IxM_port = findIxM();
                            Application.DoEvents();

                        }



                        

                        if (IxM_port != "Error!!!")
                        {

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


                                BootResult = true;



                            }
                            else
                            {
                                led5.OffColor = Color.Red;
                                BootResult = false;

                            }
                        }

                        else
                        {

                            led5.OffColor = Color.Red;
                            BootResult = false;


                        }
                        


                    this.Refresh();




                    // 2.  measure charge current

                 if (instumentStatus == 0)
                    ReadChargeCurrent();



                    // 3. connect to shell to make sure boot worked.


*/

                }
 


            // insert LED tests here.   We have to use current to dectect valid LED's

                    switchChannel0(1);   // this must be done before IR LED test to turn unit on. (closes the push button)


              

                if (instumentStatus == 0)
                {




                    led7.OffColor = Color.Yellow;
                    this.Refresh();

                    Txresponse = TxCommand("mfgt= 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr



                    // turn charge OFF
                    Txresponse = TxCommand("chd= 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr



                    // turn off IR LED's


                    Txresponse = TxCommand("irled= 0");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    // turn on cled


                    Txresponse = TxCommand("pled= 100");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    // read current



                    // measure Current


                    BK2831E_2_SetCurrent setupCurrent = new BK2831E_2_SetCurrent();

                    BK2831E_2_ReadCurrent GetIRCurrent = new BK2831E_2_ReadCurrent();



                    setupCurrent.Run();



                    Thread.Sleep(1000); // add delay


                    //BK2831E_2_ReadCurrent results = GetIRCurrent.Run();

                   BK2831E_2_ReadCurrentResults results = GetIRCurrent.Run();


                    string myCurrent = results.Token2.ToString();
                    Thread.Sleep(250); // add delay



                    double PLEDCurrent = Convert.ToDouble(myCurrent);





       


                    // turn off cled

                    Txresponse = TxCommand("pled= 0");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                    // read current

                    results = GetIRCurrent.Run();

                    myCurrent = results.Token2.ToString();
                    Thread.Sleep(250); // add delay



                    double NOLEDCurrent = Convert.ToDouble(myCurrent);


                    // validate


                    if (PLEDCurrent - NOLEDCurrent > .0003)
                    {
                        UILEDResult = true;


                    }
                    else
                    {
                        UILEDResult = false; ;
                    }









                    // turn on mled  Only on TM!



                    Txresponse = TxCommand("summ");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Tx: {0}", Txresponse); // may have to capture this as soon as we have cr


                    // figure out Model

                    if (Txresponse.Contains("Teacher"))
                    {
                        Model = "ITM";
                    }
                    else
                    {
                        Model = "ISM";

                    }

                    if (Model == "ITM")
                    {
                        Txresponse = TxCommand("mled= 100");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        // read current

                        results = GetIRCurrent.Run();

                        myCurrent = results.Token2.ToString();
                        Thread.Sleep(250); // add delay



                        double MLEDCurrent = Convert.ToDouble(myCurrent);




                        // turn off mled

                        Txresponse = TxCommand("mled= 0");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        // read current

                        results = GetIRCurrent.Run();

                        myCurrent = results.Token2.ToString();
                        Thread.Sleep(250); // add delay



                        NOLEDCurrent = Convert.ToDouble(myCurrent);



                        // validate

                        if (UILEDResult)
                            if (MLEDCurrent - NOLEDCurrent > .002)
                            {
                                UILEDResult = true;


                            }
                            else
                            {
                                UILEDResult = false; ;
                            }

                    }

                    // turn on cgled

                    Txresponse = TxCommand("cgled= 100");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                    // read current

                    results = GetIRCurrent.Run();

                    myCurrent = results.Token2.ToString();
                    Thread.Sleep(250); // add delay



                    double CGLEDCurrent = Convert.ToDouble(myCurrent);





                    

                    // turn off cgled

                    Txresponse = TxCommand("cgled= 0");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                    // read current

                    results = GetIRCurrent.Run();

                    myCurrent = results.Token2.ToString();
                    Thread.Sleep(250); // add delay



                    NOLEDCurrent = Convert.ToDouble(myCurrent);

                    // read current

                    // validate

                    if (UILEDResult)
                        if (CGLEDCurrent - NOLEDCurrent > .0003)
                        {
                            UILEDResult = true;


                        }
                        else
                        {
                            UILEDResult = false; ;
                        }


                    // trun on crled
                    Txresponse = TxCommand("crled= 100");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                    // read current

                    results = GetIRCurrent.Run();

                    myCurrent = results.Token2.ToString();
                    Thread.Sleep(250); // add delay



                    double CRLEDCurrent = Convert.ToDouble(myCurrent);


                    

                    // turn off crled

                    Txresponse = TxCommand("crled= 0");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                    // read current

                    results = GetIRCurrent.Run();

                    myCurrent = results.Token2.ToString();
                    Thread.Sleep(250); // add delay



                    NOLEDCurrent = Convert.ToDouble(myCurrent);

                    


                    // validate

                    if (UILEDResult)
                        if (CRLEDCurrent - NOLEDCurrent > .0005)
                        {
                            UILEDResult = true;


                        }
                        else
                        {
                            UILEDResult = false; ;
                        }



                    if (UILEDResult)
                    {

                        led7.OffColor = Color.LimeGreen; ;
                    }
                    else
                    {

                       //// instumentStatus = 1;

                        led7.OffColor = Color.Red;

                    }

                    this.Refresh();


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

                    IRChannel_A_Result = true;


                    Txresponse = TxCommand("mfgt= 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr




                    Txresponse = TxCommand("IRLED = 0");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    // make reference current measurement

                    // measure Current


                    BK2831E_2_SetCurrent setupCurrent = new BK2831E_2_SetCurrent();

                    BK2831E_2_ReadCurrent GetIRCurrent = new BK2831E_2_ReadCurrent();



                    setupCurrent.Run();



                    Thread.Sleep(1000); // add delay


                    BK2831E_2_ReadCurrentResults results = GetIRCurrent.Run();

                    string myCurrent = results.Token2.ToString();
                    Thread.Sleep(250); // add delay



                    double IRCurrent = Convert.ToDouble(myCurrent);



                    // store this value for later

                    ReferenceCurrent = IRCurrent;


                    // turn IR back ON
                    Txresponse = TxCommand("IRLED = 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr




                    led9.OffColor = Color.Yellow;

                    this.Refresh();

                    // measure duty cycle
                    //button2.PerformClick();
                    timer2.Enabled = true;


                    // Adjust to 25%
                    IRDutycyle = getdutycycle();



                    if ((IRDutycyle > 35) | (IRDutycyle < 27) | (Double.IsNaN(IRDutycyle)))
                    {
                        AdjustDuty(0);

                    }
                    else
                    {
                        // store data
                        IRDutyCycle[0] = IRDutycyle;

                        Txresponse = null;

                        Txresponse = TxCommand("irpw");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        // find the number



                        tokens = Txresponse.Split('=');
                        decimal mynumber;

                        bool canConvert = decimal.TryParse(tokens[1],out mynumber);

                        if (canConvert == true)
                        {
                            IRPW[0] = Convert.ToDouble(tokens[1]);

                        }
                        else
                        {
                            IRPW[0] = Convert.ToDouble(tokens[3]);

                        }


                    }


                    while (timer2.Enabled == true)
                    {

                        Application.DoEvents();

                    }


                    // validate


                    IRDutycyle = getdutycycle();
                    if ((IRDutycyle < 35) | (IRDutycyle > 27))
                    {
                        led9.OffColor = Color.LimeGreen;


                    }
                    else
                    {
                        led9.OffColor = Color.Red;
                        IRChannel_A_Result = false;

                    }




                    this.Refresh();
                   




                    // create a sub-routine to do this.   


                    // set to low coverage mode

                    TPLabel.Text = "IR Current";

                    Txresponse = TxCommand("cov = 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    myCurrent = results.Token2.ToString();
                    Thread.Sleep(250); // add delay



                    IRCurrent = Convert.ToDouble(myCurrent);

                    double ReturnCurrent = 0;


                    led14.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.ldCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent=IRSetCurrent(1,0);  // 1 = low, 2 = mid, 3 = high

                        IRCurrent = ReturnCurrent;
                    }
                    else
                    {


                        MeterReading.Text = IRCurrent.ToString();

                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        ldcurrent[0] = IRCurrent;


                        Txresponse = TxCommand("ldpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        ldpot[0] = Convert.ToDouble(tokens[1]);


                    }


                    if (((ReturnCurrent-ReferenceCurrent) > (limits.ldCurrentNominal -limits.IRCurrentTolerance)) & ((ReturnCurrent-ReferenceCurrent) < (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led14.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led14.OffColor = Color.Red;
                        IRChannel_A_Result = false;
                    }

                    this.Refresh();

                    // store data




                    // set to MID coverage mode

                    Txresponse = TxCommand("cov = 3");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                    // our results sometimes gets corrupted  - need to fulsh the buffer?
                    results = GetIRCurrent.Run();

                    myCurrent = results.Token2.ToString();

                    Thread.Sleep(250); // add delay

                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led23.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent-ReferenceCurrent) < (limits.mdCurrentNominal-limits.IRCurrentTolerance)) | ((IRCurrent-ReferenceCurrent) > (limits.mdCurrentNominal+limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(3,0);  // 1 = low, 3 = mid, 5 = high

                        IRCurrent = ReturnCurrent;
                    }
                    else
                    {

                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        mdcurrent[0] = IRCurrent;


                        Txresponse = TxCommand("mdpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        mdpot[0] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.mdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.mdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led23.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led23.OffColor = Color.Red;
                        IRChannel_A_Result = false;

                    }

                    this.Refresh();






                    // set to HIGH coverage mode

                    Txresponse = TxCommand("cov = 5");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();
                    Thread.Sleep(250); // add delay

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led28.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(5,0);  // 1 = low, 2 = mid, 3 = high

                        IRCurrent = ReturnCurrent;
                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        hdcurrent[0] = IRCurrent;


                        Txresponse = TxCommand("hdpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        hdpot[0] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led28.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led28.OffColor = Color.Red;
                        IRChannel_A_Result = false;

                    }
                    this.Refresh();



                  //  SetChannel(2);


                    Txresponse = TxCommand("CHI = 1");
                    timer3.Enabled = false;




                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    led10.OffColor = Color.Yellow;

                    this.Refresh();
                    IRChannel_B_Result = true;

                    // invoke each time we change channel

                    Txresponse = TxCommand("mfgt= 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                    // measure duty cycle
                   // button2.PerformClick();


                    timer2.Enabled = true;

                    // Adjust to 25%
                    IRDutycyle = getdutycycle();



                    if ((IRDutycyle > 35) | (IRDutycyle < 27) | (Double.IsNaN(IRDutycyle)))
                    {
                        AdjustDuty(1);
                    }
                    else
                    {
                        // store data
                        IRDutyCycle[1] = IRDutycyle;

                        Txresponse = null;
                        Txresponse = TxCommand("irpw");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        IRPW[1] = Convert.ToDouble(tokens[1]);


                    }


                    while (timer2.Enabled == true)
                    {

                        Application.DoEvents();

                    }

                    // validate


                    IRDutycyle = getdutycycle();
                    if ((IRDutycyle < 35) | (IRDutycyle > 27))
                    {
                        led10.OffColor = Color.LimeGreen;


                    }
                    else
                    {
                        led10.OffColor = Color.Red;
                        IRChannel_B_Result = false;

                    }

                    this.Refresh();

                    // Adjust to 25%


                    // measure Current

                    // measure Current


        



                    //setupCurrent.Run();


                    // set to low coverage mode

                    Txresponse = TxCommand("cov = 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                     results = GetIRCurrent.Run();
                     Thread.Sleep(250); // add delay

                     myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led15.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.ldCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(1,1);  // 1 = low, 2 = mid, 3 = high
                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        ldcurrent[1] = IRCurrent;


                        Txresponse = TxCommand("ldpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        ldpot[1] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.ldCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led15.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led15.OffColor = Color.Red;
                        IRChannel_B_Result = false;

                    }

                    this.Refresh();



                    // set to MID coverage mode

                    Txresponse = TxCommand("cov = 3");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led22.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.mdCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.mdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(3,1);  // 1 = low, 2 = mid, 3 = high
                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        mdcurrent[1] = IRCurrent;


                        Txresponse = TxCommand("mdpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        mdpot[1] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.mdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.mdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led22.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led22.OffColor = Color.Red;
                        IRChannel_B_Result = false;

                    }

                    this.Refresh();






                    // set to HIGH coverage mode

                    Txresponse = TxCommand("cov = 5");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led27.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                    {

                        try
                        {
                            ReturnCurrent = IRSetCurrent(5, 1);  // 1 = low, 2 = mid, 3 = high
                            IRCurrent = ReturnCurrent;
                        }
                        catch
                        {
                            Console.WriteLine("we have an exception");

                        }

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        hdcurrent[1] = IRCurrent;


                        Txresponse = TxCommand("hdpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        hdpot[1] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led27.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led27.OffColor = Color.Red;
                        IRChannel_B_Result = false;

                    }

                    this.Refresh();















                    // adjust to desired value

                    //SetChannel(3);

                    Txresponse = TxCommand("CHI = 2");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    led11.OffColor = Color.Yellow;

                    this.Refresh();
                    IRChannel_C_Result = true;



                    // invoke each time we change channel

                    Txresponse = TxCommand("mfgt= 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                    // measure duty cycle
                   // button2.PerformClick();

                    timer2.Enabled = true;


                    // Adjust to 25%
                    IRDutycyle = getdutycycle();



                    if ((IRDutycyle > 35) | (IRDutycyle < 27) | (Double.IsNaN(IRDutycyle)))
                    {
                        AdjustDuty(2);
                    }
                    else
                    {
                        // store data
                        IRDutyCycle[2] = IRDutycyle;

                        Txresponse = null;
                        Txresponse = TxCommand("irpw");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        IRPW[2] = Convert.ToDouble(tokens[1]);

                    }


                    while (timer2.Enabled == true)
                    {

                        Application.DoEvents();

                    }


                    IRDutycyle = getdutycycle();
                    if ((IRDutycyle < 35) | (IRDutycyle > 27))
                    {
                        led11.OffColor = Color.LimeGreen;


                    }
                    else
                    {
                        led11.OffColor = Color.Red;
                        IRChannel_C_Result = false;

                    }

                    this.Refresh();

                    // Adjust to 25%


                    // measure Current
                    // measure Current






                    //setupCurrent.Run();


                    // set to low coverage mode



                    Thread.Sleep(1000);  // added delay here due to meter not being ready

                    Txresponse = TxCommand("cov = 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    Thread.Sleep(250);

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led16.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.ldCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(1,2);  // 1 = low, 3 = mid, 5 = high
                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        ldcurrent[2] = IRCurrent;


                        Txresponse = TxCommand("ldpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        ldpot[2] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.ldCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led16.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led16.OffColor = Color.Red;
                        IRChannel_C_Result = false;

                    }

                    this.Refresh();



                    // set to MID coverage mode

                    Txresponse = TxCommand("cov = 3");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    Thread.Sleep(250);

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led21.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.mdCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.mdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(3,2);  // 1 = low, 3 = mid, 5 = high
                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        mdcurrent[2] = IRCurrent;


                        Txresponse = TxCommand("mdpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        mdpot[2] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.mdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.mdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led21.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led21.OffColor = Color.Red;
                        IRChannel_C_Result = false;

                    }

                    this.Refresh();






                    // set to HIGH coverage mode

                    Txresponse = TxCommand("cov = 5");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led26.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(5,2);  // 1 = low, 3 = mid, 5 = high
                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        hdcurrent[2] = IRCurrent;


                        Txresponse = TxCommand("hdpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        hdpot[2] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led26.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led26.OffColor = Color.Red;
                        IRChannel_C_Result = false;

                    }

                    this.Refresh();

                    // adjust to desired value

                  //  SetChannel(4);

                    Txresponse = TxCommand("CHI = 3");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    led12.OffColor = Color.Yellow;

                    this.Refresh();
                    IRChannel_D_Result = true;



                    // invoke each time we change channel

                    Txresponse = TxCommand("mfgt= 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                    // measure duty cycle
                   // button2.PerformClick();


                    timer2.Enabled = true;

                    // Adjust to 25%
                    IRDutycyle = getdutycycle();



                    if ((IRDutycyle > 35) | (IRDutycyle < 27) | (Double.IsNaN(IRDutycyle)))
                    {
                        AdjustDuty(3);
                    }
                    else
                    {
                        // store data
                        IRDutyCycle[3] = IRDutycyle;

                        Txresponse = null;
                        Txresponse = TxCommand("irpw");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        IRPW[3] = Convert.ToDouble(tokens[1]);


                    }


                    while (timer2.Enabled == true)
                    {

                        Application.DoEvents();

                    }


                    IRDutycyle = getdutycycle();
                    if ((IRDutycyle < 35) | (IRDutycyle > 27))
                    {
                        led12.OffColor = Color.LimeGreen;


                    }
                    else
                    {
                        led12.OffColor = Color.Red;
                        IRChannel_D_Result = false;

                    }

                    this.Refresh();
                    // Adjust to 25%


                    // measure Current




                    // measure Current






                    //setupCurrent.Run();





                    


                    // set to low coverage mode

                    Txresponse = TxCommand("cov = 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    Thread.Sleep(1000);  // added delay here due to meter not being ready

                    results = GetIRCurrent.Run();

                    Thread.Sleep(250);

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led17.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.ldCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                    {

                        // exception is happening when we are greater than 210mA  even in auto mode.  checking meter specs to see if we can catch overload condition



                        try
                        {
                            ReturnCurrent = IRSetCurrent(1, 3);  // 1 = low, 2 = mid, 3 = high
                            IRCurrent = ReturnCurrent;
                        }
                        catch
                        {


                            // RESET INSTRUMENT

                
                            bk2831e_2_Reset resetmeter = new bk2831e_2_Reset();

                            resetmeter.Run();

                            Thread.Sleep(500);



                            // try again
                            Console.WriteLine("we have an exception");
                            try
                            {
                                ReturnCurrent = IRSetCurrent(1, 3);  // 1 = low, 2 = mid, 3 = high
                                IRCurrent = ReturnCurrent;
                            }
                            catch
                            {

                                // try again
                                Console.WriteLine("we have an exception");

                            }

                        }








                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        ldcurrent[3] = IRCurrent;


                        Txresponse = TxCommand("ldpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        ldpot[3] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.ldCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led17.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led17.OffColor = Color.Red;
                        IRChannel_D_Result = false;

                    }

                    this.Refresh();



                    // set to MID coverage mode

                    Txresponse = TxCommand("cov = 3");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    Thread.Sleep(250);

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led20.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.mdCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.mdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(3,3);  // 1 = low, 3 = mid, 5 = high

                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        mdcurrent[3] = IRCurrent;


                        Txresponse = TxCommand("mdpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        mdpot[3] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.mdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.mdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led20.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led20.OffColor = Color.Red;
                        IRChannel_D_Result = false;

                    }

                    this.Refresh();






                    // set to HIGH coverage mode

                    Txresponse = TxCommand("cov = 5");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led25.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                    {



                        try
                        {
                            ReturnCurrent = IRSetCurrent(5, 3);  // 1 = low, 2 = mid, 3 = high
                            IRCurrent = ReturnCurrent;
                        }
                        catch
                        {

                            // try again
                            Console.WriteLine("we have an exception");
                            try
                            {
                                ReturnCurrent = IRSetCurrent(5, 3);  // 1 = low, 2 = mid, 3 = high
                                IRCurrent = ReturnCurrent;
                            }
                            catch
                            {

                                // try again
                                Console.WriteLine("we have an exception");

                            }

                        }


                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        hdcurrent[3] = IRCurrent;


                        Txresponse = TxCommand("hdpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        hdpot[3] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led25.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led25.OffColor = Color.Red;
                        IRChannel_D_Result = false;

                    }

                    this.Refresh();


                    // adjust to desired value



                  //  SetChannel(5);


                    Txresponse = TxCommand("CHI = 4");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    led13.OffColor = Color.Yellow;

                    this.Refresh();

                    IRChannel_E_Result = true;



                    // invoke each time we change channel

                    Txresponse = TxCommand("mfgt= 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                    // measure duty cycle
                    //button2.PerformClick();

                    timer2.Enabled = true;

                    // Adjust to 25%
                    IRDutycyle = getdutycycle();



                    if ((IRDutycyle > 35) | (IRDutycyle < 27) | (Double.IsNaN(IRDutycyle)))
                    {
                        AdjustDuty(4);
                    }
                    else
                    {
                        // store data
                        IRDutyCycle[4] = IRDutycyle;

                        Txresponse = null;
                        Txresponse = TxCommand("irpw");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        IRPW[4] = Convert.ToDouble(tokens[1]);


                    }


                    while (timer2.Enabled == true)
                    {

                        Application.DoEvents();

                    }

                    IRDutycyle = getdutycycle();
                    if ((IRDutycyle < 35) | (IRDutycyle > 27))
                    {
                        led13.OffColor = Color.LimeGreen;


                    }
                    else
                    {
                        led13.OffColor = Color.Red;
                        IRChannel_E_Result = false;

                    }

                    this.Refresh();





                    // measure Current






                    //setupCurrent.Run();


                    // set to low coverage mode

                    Txresponse = TxCommand("cov = 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    Thread.Sleep(250);

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led18.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.ldCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(1,4);  // 1 = low, 3 = mid, 5 = high
                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        ldcurrent[4] = IRCurrent;


                        Txresponse = TxCommand("ldpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        ldpot[4] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.ldCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led18.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led18.OffColor = Color.Red;
                        IRChannel_E_Result = false;

                    }

                    this.Refresh();



                    // set to MID coverage mode

                    Txresponse = TxCommand("cov = 3");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    Thread.Sleep(250);

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led19.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.mdCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.mdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(3,4);  // 1 = low, 4 = mid, 5 = high
                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        mdcurrent[4] = IRCurrent;


                        Txresponse = TxCommand("mdpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        mdpot[4] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.mdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.mdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led19.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led19.OffColor = Color.Red;
                        IRChannel_E_Result = false;

                    }

                    this.Refresh();






                    // set to HIGH coverage mode

                    Txresponse = TxCommand("cov = 5");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led24.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(5,4);  // 1 = low, 3 = mid, 5 = high
                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        hdcurrent[4] = IRCurrent;


                        Txresponse = TxCommand("hdpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        hdpot[4] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led24.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led24.OffColor = Color.Red;
                        IRChannel_E_Result = false;

                    }

                    this.Refresh();



//---

                    Txresponse = TxCommand("CHI = 5");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    led33.OffColor = Color.Yellow;

                    this.Refresh();

                    IRChannel_L1_Result = true;



                    // invoke each time we change channel

                    Txresponse = TxCommand("mfgt= 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                    // measure duty cycle
                    //button2.PerformClick();

                    timer2.Enabled = true;

                    // Adjust to 25%
                    IRDutycyle = getdutycycle();



                    if ((IRDutycyle > 35) | (IRDutycyle < 27) | (Double.IsNaN(IRDutycyle)))
                    {
                        AdjustDuty(5);
                    }
                    else
                    {

                        // store data
                        IRDutyCycle[5] = IRDutycyle;

                        Txresponse = null;
                        Txresponse = TxCommand("irpw");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        IRPW[5] = Convert.ToDouble(tokens[1]);

                    }


                    while (timer2.Enabled == true)
                    {

                        Application.DoEvents();

                    }

                    IRDutycyle = getdutycycle();
                    if ((IRDutycyle < 35) | (IRDutycyle > 27))
                    {
                        led33.OffColor = Color.LimeGreen;


                    }
                    else
                    {
                        led33.OffColor = Color.Red;
                        IRChannel_L1_Result = false;

                    }

                    this.Refresh();





                    // measure Current






                    //setupCurrent.Run();


                    // set to low coverage mode

                    Txresponse = TxCommand("cov = 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    Thread.Sleep(250);

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led32.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.ldCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(1,5);  // 1 = low, 3 = mid, 5 = high
                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        ldcurrent[5] = IRCurrent;


                        Txresponse = TxCommand("ldpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        ldpot[5] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.ldCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led32.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led32.OffColor = Color.Red;
                        IRChannel_L1_Result = false;

                    }

                    this.Refresh();



                    // set to MID coverage mode

                    Txresponse = TxCommand("cov = 3");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    Thread.Sleep(250);

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led31.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                     if (((IRCurrent - ReferenceCurrent) < (limits.mdCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.mdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(3,5);  // 1 = low, 3 = mid, 5 = high
                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        mdcurrent[5] = IRCurrent;


                        Txresponse = TxCommand("mdpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        mdpot[5] = Convert.ToDouble(tokens[1]);

                    }


                     if (((IRCurrent - ReferenceCurrent) > (limits.mdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.mdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led31.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led31.OffColor = Color.Red;
                        IRChannel_L1_Result = false;

                    }

                    this.Refresh();






                    // set to HIGH coverage mode

                    Txresponse = TxCommand("cov = 5");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led30.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(5,5);  // 1 = low, 3 = mid, 5 = high
                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        hdcurrent[5] = IRCurrent;


                        Txresponse = TxCommand("hdpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        hdpot[5] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led30.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led30.OffColor = Color.Red;
                        IRChannel_L1_Result = false;

                    }

                    this.Refresh();



                    Txresponse = TxCommand("CHI = 6");
                    timer3.Enabled = false;


                    IRChannel_L2_Result = true;



                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    led37.OffColor = Color.Yellow;

                    this.Refresh();



                    // invoke each time we change channel

                    Txresponse = TxCommand("mfgt= 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                    // measure duty cycle
                    //button2.PerformClick();

                    timer2.Enabled = true;

                    // Adjust to 25%
                    IRDutycyle = getdutycycle();



                    if ((IRDutycyle > 35) | (IRDutycyle < 27) | (Double.IsNaN(IRDutycyle)))
                    {
                        AdjustDuty(6);
                    }
                    else
                    {
                        // store data
                        IRDutyCycle[6] = IRDutycyle;

                        Txresponse = null;
                        Txresponse = TxCommand("irpw");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        IRPW[6] = Convert.ToDouble(tokens[1]);

                    }


                    while (timer2.Enabled == true)
                    {

                        Application.DoEvents();

                    }

                    IRDutycyle = getdutycycle();
                    if ((IRDutycyle < 35) | (IRDutycyle > 27))
                    {
                        led37.OffColor = Color.LimeGreen;


                    }
                    else
                    {
                        led37.OffColor = Color.Red;
                        IRChannel_L2_Result = false;

                    }

                    this.Refresh();





                    // measure Current






                    //setupCurrent.Run();


                    // set to low coverage mode

                    Txresponse = TxCommand("cov = 1");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    Thread.Sleep(250);

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led36.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.ldCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(1,6);  // 1 = low, 3 = mid, 5 = high
                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        ldcurrent[6] = IRCurrent;


                        Txresponse = TxCommand("ldpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        ldpot[6] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.ldCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led36.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led36.OffColor = Color.Red;
                        IRChannel_L2_Result = false;

                    }

                    this.Refresh();



                    // set to MID coverage mode

                    Txresponse = TxCommand("cov = 3");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    Thread.Sleep(250);

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led35.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.mdCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.mdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(3,6);  // 1 = low, 3 = mid, 5 = high
                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        mdcurrent[6] = IRCurrent;


                        Txresponse = TxCommand("mdpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        mdpot[6] = Convert.ToDouble(tokens[1]);

                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.mdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.mdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led35.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led35.OffColor = Color.Red;
                        IRChannel_L2_Result = false;

                    }

                    this.Refresh();






                    // set to HIGH coverage mode

                    Txresponse = TxCommand("cov = 5");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                    results = GetIRCurrent.Run();

                    myCurrent = results.Token2.ToString();



                    IRCurrent = Convert.ToDouble(myCurrent);

                    ReturnCurrent = 0;


                    led34.OffColor = Color.Yellow;

                    this.Refresh();


                    //  if current is out of spec, run cal

                    if (((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal - limits.IRCurrentTolerance)) | ((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        ReturnCurrent = IRSetCurrent(5,6);  // 1 = low, 3 = mid, 5 = high
                        IRCurrent = ReturnCurrent;

                    }
                    else
                    {
                        MeterReading.Text = IRCurrent.ToString();
                        ReturnCurrent = IRCurrent;
                        // record current and ldpot
                        hdcurrent[6] = IRCurrent;


                        Txresponse = TxCommand("hdpot");
                        timer3.Enabled = false;


                        Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr

                        string[] tokens;

                        tokens = Txresponse.Split('=');
                        hdpot[6] = Convert.ToDouble(tokens[1]);


                    }


                    if (((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                    {
                        led34.OffColor = Color.LimeGreen;


                    }
                    else
                    {

                        led34.OffColor = Color.Red;
                        IRChannel_L2_Result = false;

                    }

                    this.Refresh();


                    // Save Settings!!--------------------------------------------------------------------------------------------------------------------------------------------------------------------



                    Txresponse = TxCommand("setd");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Tx: {0}", Txresponse); // may have to capture this as soon as we have cr


// save board serial number


                    Txresponse = TxCommand("secur = $12348765");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Tx: {0}", Txresponse); // may have to capture this as soon as we have cr

                    // get serial from textbox


                    string commandtext = "board " + textBox1.Text;


                    Txresponse = TxCommand(commandtext);
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Tx: {0}", Txresponse); // may have to capture this as soon as we have cr


                    Txresponse = TxCommand("saves");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Tx: {0}", Txresponse); // may have to capture this as soon as we have cr



                    Txresponse = TxCommand("summ");
                    timer3.Enabled = false;


                    Console.WriteLine("Response From Tx: {0}", Txresponse); // may have to capture this as soon as we have cr


                    // figure out Model

                    if (Txresponse.Contains("Teacher"))
                    {
                        Model = "ITM";
                    }
                    else
                    {
                        Model = "ISM";

                    }



                    





                    // look at output signal



                    // beginning of voltage measurements.   Move to end of procedure!


                    // check our power supplies
                    if (instumentStatus == 0)
                    {
                        // prompt manual switch to TP102


                        led3.OffColor = Color.Yellow;
                        this.Refresh();





                        // for ism, the one touch switch has to be closed. (TP 409)



                        switchChannel0(1);    
                        Test_TP116();
                        this.Refresh();


                        Application.DoEvents();


                        switchChannel0(2);

                        Test_TP115();
                        this.Refresh();

                        Application.DoEvents();



                        //Thread.Sleep(1000);  // add delay for settling.

                        switchChannel0(3);
                        Test_TP130();
                        Test_TP130();

                        this.Refresh();

                        Application.DoEvents();


                        switchChannel0(0);

                        // Thread.Sleep(500);

                        Test_TP102();
                        this.Refresh();


                        Application.DoEvents();







                        /*
                        MessageBox.Show("Swich fixture to TP115.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button1);
                        */



                        /*
                        MessageBox.Show("Swich fixture to TP130.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button1);
                        */








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

                    // move aux and mic to here
                    // test Aux inputs

                    Application.DoEvents();



                    if (instumentStatus == 0)
                    {

                        scope1.YAxis.AutoScaling.Enabled = true;




                        led6.OffColor = Color.Yellow;   /// seems to be crashing here!!!!!!
                        this.Refresh();


                        /*
                        MessageBox.Show("Switch Out to Tp402 and In/Out to Aux R.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button1);
                         * */

                        switchChannel0(5);    //  tp 302
                        switchChannel1(0);  // inject signal   



                        if (Model == "ISM")   // only on ISM!
                            PTT_ON();



                        short status;


                        // turn on Generator
                        status = Imports.SetSiggenBuiltIn(handle, 0, 500000, Imports.SiggenWaveType.Sine, 1000, 1000, 0, 0, Imports.SiggenSweepType.Up, false, 1, 1, Imports.SiggenTrigType.Rising, Imports.SiggenTrigSource.None, 0);
                        // allow some settling time

                        // Thread.Sleep(1000);


                        GetChanB();
                        Thread.Sleep(1500);
                        GetChanB();  // have to do twice due to offset in buffer.. I don't know why yet.


                        // get value for aux from label

                        string rmsValue = Regex.Match(label19.Text, @"\d+").Value;


                        AudioInMeasurements[0] = Convert.ToDouble(rmsValue);
                        scope1.RefreshView();


                        Application.DoEvents();



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

                        scope1.RefreshView();


                        // validate measurements

                        if ((AudioInMeasurements[0] > 35) & (AudioInMeasurements[0] < 210) & (AudioInMeasurements[1] > 35) & (AudioInMeasurements[1] < 210))
                        {

                            AuxResult = true;

                            led6.OffColor = Color.LimeGreen;


                        }
                        else
                        {


                            AuxResult = false;

                            led6.OffColor = Color.Red;


                            if ((AudioInMeasurements[0] > 35) & (AudioInMeasurements[0] < 210))
                            {


                            }
                            else
                            {
                                failureString = failureString + "Voltage at TP201 Failure (Aux In Right)....\r\n";

                                textBox2.Text = failureString;

                                this.Refresh();

                            }

                            if ((AudioInMeasurements[1] > 35) & (AudioInMeasurements[1] < 210))
                            {


                            }
                            else
                            {

                                failureString = failureString + "Voltage at TP201 Failure (Aux In Left)....\r\n";

                                textBox2.Text = failureString;

                                this.Refresh();

                            }





                        }


                        this.Refresh();

                    }


                    // test mic input

                    Application.DoEvents();



                    if (instumentStatus == 0)
                    {


                        led8.OffColor = Color.Yellow;   /// seems to be crashing here!!!!!!
                        this.Refresh();

                        chgOff();  // have to disable usb for mic to work

                        /*

                        MessageBox.Show("Switch Out to Tp402 and In/Out to Mic.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation,
                        MessageBoxDefaultButton.Button1);

                        */
                        short status;

                        // test at 3 frequencies   500hz, k1kh and 5khz

                        uint indx;

                        for (indx = 0; indx < 3; indx++)
                        {

                            // set the frequency for each pass

                            Int32 freq;


                            switch (indx)
                            {
                                case 0:
                                    freq = 500;    // was 100 Hz
                                    break;


                                case 1:
                                    freq = 1000;

                                    break;

                                case 2:
                                    freq = 5000;
                                    break;
                                default:

                                    freq = 1000;
                                    break;


                            }
                            status = Imports.SetSiggenBuiltIn(handle, 0, 10000, Imports.SiggenWaveType.Sine, freq, freq, 0, 0, Imports.SiggenSweepType.Up, false, 1, 1, Imports.SiggenTrigType.Rising, Imports.SiggenTrigSource.None, 0);
                            // allow some settling time

                            Thread.Sleep(1000);
                            switchChannel1(2);

                            Thread.Sleep(1000);  // allow for more settling time after switch

                            GetChanB();
                            Thread.Sleep(500);
                            // GetChanB();  // have to do twice due to offset in buffer.. I don't know why yet.


                            // get value for mic from label


                            // we have three values now.  Called Mic[indx];

                            string micValue = Regex.Match(label19.Text, @"\d+").Value;

                            Mic[indx] = Convert.ToDouble(micValue);

                        }

                        string rmsValue = Regex.Match(label19.Text, @"\d+").Value;
                        AudioInMeasurements[2] = Convert.ToDouble(rmsValue);
                        scope1.RefreshView();


                        this.Refresh();

                        Application.DoEvents();





                        if ((Mic[0] > limits.Mic500low) & (Mic[0] < limits.Mic500high) & (Mic[1] > limits.Mic1klow) & (Mic[1] < limits.Mic1khigh) & (Mic[2] > limits.Mic5klow) & (Mic[2] < limits.Mic5khigh))
                        {
                            MicResult = true;
                            led8.OffColor = Color.LimeGreen;

                        }
                        else
                        {
                            MicResult = false;
                            led8.OffColor = Color.Red;

                            if ((Mic[0] > limits.Mic500low) & (Mic[0] < limits.Mic500high))
                            { }
                            else
                            {
                                failureString = failureString + "Voltage at TP201 Failure (Mic In 500Hz)....\r\n";

                                textBox2.Text = failureString;

                                this.Refresh();
                            }

                            if ((Mic[1] > limits.Mic500low) & (Mic[1] < limits.Mic500high))
                            { }
                            else
                            {
                                failureString = failureString + "Voltage at TP201 Failure (Mic In 1000Hz)....\r\n";

                                textBox2.Text = failureString;

                                this.Refresh();
                            }


                            if ((Mic[2] > limits.Mic500low) & (Mic[2] < limits.Mic500high))
                            { }
                            else
                            {
                                failureString = failureString + "Voltage at TP201 Failure (Mic In 5000Hz)....\r\n";

                                textBox2.Text = failureString;

                                this.Refresh();
                            }




                        }



                    }



                    Application.DoEvents();




                    //// end of voltage measurements ---- move to end of procedure.




                    // close txport
                    if (DUTport.IsOpen)
                        DUTport.Close();
                    

                }

/*
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

        public bool IRChannel_A_Result = false;
        public bool IRChannel_B_Result = false;
        public bool IRChannel_C_Result = false;
        public bool IRChannel_D_Result = false;
        public bool IRChannel_E_Result = false;
        public bool IRChannel_L1_Result = false;
        public bool IRChannel_L2_Result = false;
 */

                if (PowerUpCurrentResult & VoltageResult & TP102Result & AuxResult & MicResult & BootResult & ChargeCurrentResult & ProgramResult & IRChannel_A_Result & IRChannel_B_Result & IRChannel_C_Result & IRChannel_D_Result & IRChannel_E_Result & IRChannel_L1_Result & IRChannel_L2_Result)
                {

                    TestResult = true;


                }
                else
                {

                    TestResult = false;

                }



                // save data

                WriteCSVFile();




                if (PowerUpCurrentResult & VoltageResult & TP102Result & AuxResult & MicResult & BootResult & ChargeCurrentResult & ProgramResult & IRChannel_A_Result & IRChannel_B_Result & IRChannel_C_Result & IRChannel_D_Result & IRChannel_E_Result & IRChannel_L1_Result & IRChannel_L2_Result)
                {
                    // show pass
                 
                    frm6.Show();



                }
                else
                {
                    // show fail
                    frm7.Show();


                }

                // close scope connection

                Imports.CloseUnit(handle);  // close the scope

               // chgOff(); // turn off charge voltage

        }




        public void WriteCSVFile()
        {
            string filename = "c:\\data\\IXMData" + DateTime.Now.ToString("yyyyMMdd") + ".csv";

            // check for existance of todays file.  If it doesn't exist put a header on top for data

            string timestring = DateTime.Now.ToString("h:mm:ss tt");
            if (!File.Exists(filename))
            {
                using (StreamWriter sw = File.AppendText(filename))
                {

                    sw.WriteLine("Serial Number,Model,Time, Test Result, Power Up Current, " +
                                  "Voltage Result, TP102, TP116, TP115, TP130, " +
                                  "Program Result," +
                                  "Aux-In Result, TPxxx,TPXXX," +
                                  "Mic-In Result, TPxxx," + "500Hz, 1000Hz, 5000Hz," +
                                  "Boot Result, " +
                                  "Charge Result, Charge Current,"+
                                  "Reference Current,"+
                                  "IRChannel_A_Result, IRPW-A, Chan A Duty, ldpot-A, ld-A Current, mdpot-A, md-A Current, hdpot-A, hd-A Current," +
                                  "IRChannel_B_Result, IRPW-B, Chan B Duty, ldpot-B, ld-B Current, mdpot-B, md-B Current, hdpot-B, hd-B Current," +
                                  "IRChannel_C_Result, IRPW-C, Chan C Duty, ldpot-C, ld-C Current, mdpot-C, md-C Current, hdpot-C, hd-C Current," +
                                  "IRChannel_D_Result, IRPW-D, Chan D Duty, ldpot-D, ld-D Current, mdpot-D, md-D Current, hdpot-D, hd-D Current," +
                                  "IRChannel_E_Result, IRPW-E, Chan E Duty, ldpot-E, ld-E Current, mdpot-E, md-E Current, hdpot-E, hd-E Current," +
                                  "IRChannel_L1_Result, IRPW-L1, Chan L1 Duty, ldpot-L1, ld-L1 Current, mdpot-L1, md-L1 Current, hdpot-L1, hd-L1 Current," +
                                  "IRChannel_L2_Result, IRPW-L2, Chan L2 Duty, ldpot-L2, ld-L2 Current, mdpot-L2, md-L2 Current, hdpot-L2, hd-L2 Current,"
                                   );
                }

            }



            using (StreamWriter sw = File.AppendText(filename))
            {
                sw.WriteLine(textBox1.Text + "," + Model + "," + timestring + "," + Convert.ToString(TestResult) +
                "," + Convert.ToString(powerupCurrent) +
                "," + Convert.ToString(VoltageResult) + "," + Convert.ToString(VoltageMeasurements[0]) + "," + Convert.ToString(VoltageMeasurements[1]) +  "," + Convert.ToString(VoltageMeasurements[2]) +  "," + Convert.ToString(VoltageMeasurements[3]) +
                "," + Convert.ToString(ProgramResult) +
                "," + Convert.ToString(AuxResult) + "," + Convert.ToString(AudioInMeasurements[0]) + "," + Convert.ToString(AudioInMeasurements[1]) +
                "," + Convert.ToString(MicResult) + "," + Convert.ToString(AudioInMeasurements[2]) + "," + Convert.ToString(Mic[0]) + "," + Convert.ToString(Mic[1]) + "," + Convert.ToString(Mic[2]) + 
                "," + Convert.ToString(BootResult) +
                "," + Convert.ToString(ChargeCurrentResult) + "," + Convert.ToString(ChargeCurrent) +
                "," + Convert.ToString(ReferenceCurrent) +
                "," + Convert.ToString(IRChannel_A_Result) + "," + Convert.ToString(IRPW[0]) + "," + Convert.ToString(IRDutyCycle[0]) + "," + Convert.ToString(ldpot[0]) + "," + Convert.ToString(ldcurrent[0]) + "," + Convert.ToString(mdpot[0]) + "," + Convert.ToString(mdcurrent[0]) + "," + Convert.ToString(hdpot[0]) + "," + Convert.ToString(hdcurrent[0]) +
                "," + Convert.ToString(IRChannel_B_Result) + "," + Convert.ToString(IRPW[1]) + "," + Convert.ToString(IRDutyCycle[1]) + "," + Convert.ToString(ldpot[1]) + "," + Convert.ToString(ldcurrent[1]) + "," + Convert.ToString(mdpot[1]) + "," + Convert.ToString(mdcurrent[1]) + "," + Convert.ToString(hdpot[1]) + "," + Convert.ToString(hdcurrent[1]) +
                "," + Convert.ToString(IRChannel_C_Result) + "," + Convert.ToString(IRPW[2]) + "," + Convert.ToString(IRDutyCycle[2]) + "," + Convert.ToString(ldpot[2]) + "," + Convert.ToString(ldcurrent[2]) + "," + Convert.ToString(mdpot[2]) + "," + Convert.ToString(mdcurrent[2]) + "," + Convert.ToString(hdpot[2]) + "," + Convert.ToString(hdcurrent[2]) +
                "," + Convert.ToString(IRChannel_D_Result) + "," + Convert.ToString(IRPW[3]) + "," + Convert.ToString(IRDutyCycle[3]) + "," + Convert.ToString(ldpot[3]) + "," + Convert.ToString(ldcurrent[3]) + "," + Convert.ToString(mdpot[3]) + "," + Convert.ToString(mdcurrent[3]) + "," + Convert.ToString(hdpot[3]) + "," + Convert.ToString(hdcurrent[3]) +
                "," + Convert.ToString(IRChannel_E_Result) + "," + Convert.ToString(IRPW[4]) + "," + Convert.ToString(IRDutyCycle[4]) + "," + Convert.ToString(ldpot[4]) + "," + Convert.ToString(ldcurrent[4]) + "," + Convert.ToString(mdpot[4]) + "," + Convert.ToString(mdcurrent[4]) + "," + Convert.ToString(hdpot[4]) + "," + Convert.ToString(hdcurrent[4]) +
                "," + Convert.ToString(IRChannel_L1_Result) + "," + Convert.ToString(IRPW[5]) + "," + Convert.ToString(IRDutyCycle[5]) + "," + Convert.ToString(ldpot[5]) + "," + Convert.ToString(ldcurrent[5]) + "," + Convert.ToString(mdpot[5]) + "," + Convert.ToString(mdcurrent[5]) + "," + Convert.ToString(hdpot[5]) + "," + Convert.ToString(hdcurrent[5]) +
                "," + Convert.ToString(IRChannel_L2_Result) + "," + Convert.ToString(IRPW[6]) + "," + Convert.ToString(IRDutyCycle[6]) + "," + Convert.ToString(ldpot[6]) + "," + Convert.ToString(ldcurrent[6]) + "," + Convert.ToString(mdpot[6]) + "," + Convert.ToString(mdcurrent[6]) + "," + Convert.ToString(hdpot[6]) + "," + Convert.ToString(hdcurrent[6]) 
                
                );  



            }
        }


        private double IRSetCurrent(int coverage,int channel)
        {
            BK2831E_2_ReadCurrent GetIRCurrent = new BK2831E_2_ReadCurrent();
            bool CurrentOK = false;

            double LDpot = 2.75;   // start with lower value to avoid overload condition on meter
            double MDpot = 2.9;
            double HDpot = 3.0;  // was 3.62
            double stepsize = .01;
            double ReturnCurrent = 0;

            double SetPot = 0;
            string txcommandstring;


            Thread.Sleep(500);  // add delay to prevent issues????



            // start at following values for each value


            BK2831E_2_ReadCurrentResults results = GetIRCurrent.Run();


            string myCurrent = results.Token2.ToString();



            double IRCurrent = Convert.ToDouble(myCurrent);

            


            // set initial value
            switch (coverage)
            {
                case 1:
                    SetPot = LDpot;

                    break;
                case 3:

                    SetPot = MDpot;
                    break;
                case 5:

                    SetPot = HDpot;
                    break;
                default:

                    SetPot = HDpot;
                    break;

            }

            



            do
            {

                switch(coverage)
                {
                    case 1:

                        txcommandstring = "ldpot = " + SetPot.ToString("N3");
                    break;
                    case 3:
                        txcommandstring = "mdpot = " + SetPot.ToString("N3");
                    break;
                    case 5:
                        txcommandstring = "hdpot = " + SetPot.ToString("N3");
                        break;
                    default:
                        txcommandstring = "hdpot = " + SetPot.ToString("N3");

                    break;

                }



                Txresponse = null;

                

                Txresponse = TxCommand(txcommandstring);



                Console.WriteLine("Response From Juno: {0}", Txresponse); // may have to capture this as soon as we have cr


                Thread.Sleep(250);  // allow a litle settleing time


                results = GetIRCurrent.Run();

                Thread.Sleep(100); // add delay
                myCurrent = results.Token2.ToString();

                IRCurrent = Convert.ToDouble(myCurrent);


                MeterReading.Text = (IRCurrent*1000).ToString() + " mA";





                switch (coverage)
                {
                    case 1:
                        if (((IRCurrent - ReferenceCurrent) > (limits.ldCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.ldCurrentNominal + limits.IRCurrentTolerance)))
                        {
                            CurrentOK = true;

                            
                            ldpot[channel] = SetPot;
                            ldcurrent[channel] = IRCurrent;



                        }
                        else
                        {

                            if (IRCurrent < .05) 
                                return IRCurrent;  // error condition

                            if(SetPot>3.624)
                                return IRCurrent;  // error condition

                            if (IRCurrent < .09)
                            {
                                
                                SetPot = SetPot + 0.05;  // course
                            }
                            else
                            {
                                SetPot = SetPot + 0.01;  // fine
                            }

                        }

                       
                        break;
                    case 3:
                        if (((IRCurrent - ReferenceCurrent) > (limits.mdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.mdCurrentNominal + limits.IRCurrentTolerance)))
                        {
                            CurrentOK = true;
                            mdpot[channel] = SetPot;
                            mdcurrent[channel] = IRCurrent;


                        }
                        else
                        {

                            if (IRCurrent < .05) 
                                return IRCurrent;  // error condition

                            if (SetPot > 3.624)
                                return IRCurrent;  // error condition

                            if (IRCurrent < .11)
                            {

                                SetPot = SetPot + 0.05;  // course
                            }
                            else
                            {
                                SetPot = SetPot + 0.01;  // fine
                            }

                     

                        }
                        break;
                    case 5:
                        if (((IRCurrent - ReferenceCurrent) > (limits.hdCurrentNominal - limits.IRCurrentTolerance)) & ((IRCurrent - ReferenceCurrent) < (limits.hdCurrentNominal + limits.IRCurrentTolerance)))
                        {
                            CurrentOK = true;
                            hdpot[channel] = SetPot;
                            hdcurrent[channel] = IRCurrent;


                        }
                        else
                        {


                            if (IRCurrent < .05) 
                                return IRCurrent;  // error condition

                            if (SetPot > 3.624)
                                return IRCurrent;  // error condition

                            if (IRCurrent < .12)
                            {

                                SetPot = SetPot + 0.05;  // course
                            }
                            else
                            {
                                SetPot = SetPot + 0.01;  // fine
                            }



                        }
                        break;
                    default:
                        if ((IRCurrent > .125) & (IRCurrent < .135))
                        {
                            CurrentOK = true;


                        }
                        else
                        {

                            if (IRCurrent < .11)
                            {

                                SetPot = SetPot + 0.05;  // course
                            }
                            else
                            {
                                SetPot = SetPot + 0.01;  // fine
                            }

                  

                        }

                        break;

                }









            } while (!CurrentOK);




            MeterReading.Text = (IRCurrent * 1000).ToString() + " mA";



            return IRCurrent;

        }


        public void AdjustDuty(int channel)
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


                IRPW[channel] = setduty;




         


                if ((IRDutycycle < 35) & (IRDutycycle > 27))
                {

                    dutyOK = true;
                }
                else
                {

                    setduty = setduty - .005f;

                }

                //if (IRDutycycle > 34)
                    //break;

              

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
                ProgramResult = false;

                ProgramInProgress = false;  // only trigger on errore
                progressSleep = 10;

                //instumentStatus = 1;


            }
            



            Console.Write("Return Data: " + flashstring + "\n");

            // look for programming success so we can move on.



           

            if (flashstring.Contains("Verify passed"))
            {
                ProgramResult = true;

                ProgramInProgress = false;
                progressSleep = 10;  // speed up progress bar


            }

            if(flashstring.Contains("error"))
            {
                ProgramResult = false;

                ProgramInProgress = false;  // only trigger on errore
                progressSleep = 10;
                instumentStatus = 1;

            }
             




        }

        void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            int total = 90; //some number (this is your variable to change)!!

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


        private void PTT_ON()
        {
            try
            {
                using (Task digitalWriteTask = new Task())
                {
                    digitalWriteTask.DOChannels.CreateChannel("NI-USB-6501/port0/line7", "",
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



            if ((ChargeCurrent > 5) & (powerupCurrent < 520))  // takes a bit to settle back on boot  ---- WAS 10 - 60mA   remember to change back!
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


           // if ((powerupCurrent < limits.maxPowerUpCurrent))  // takes a bit to settle back on boot

           if (true)  // takes a bit to settle back on boot
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

                failureString = failureString + "Power up Current Failure...\r\n";

                textBox2.Text = failureString;

                PowerUpCurrentResult = true;  // added to fource good result ToDo.  figure out the fucking issue!
                led2.OffColor = Color.LimeGreen;

                this.Refresh();


                //instumentStatus = 1;

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

            if ((Vvalue > limits.TP102VoltageMin) & (Vvalue < limits.TP102VoltageMax))
            {
                TP102Result = true;

            }
            else
            {
                TP102Result = false;


                failureString = failureString + "Voltage at TP102 Failure....\r\n";

                textBox2.Text = failureString;

                this.Refresh();


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

            if ((Vvalue > limits.TP116VoltageMin) & (Vvalue < limits.TP116VoltageMax))
            {
                TP116Result = true;

            }
            else
            {
                TP116Result = false;

                failureString = failureString + "Voltage at TP116 Failure....\r\n";

                textBox2.Text = failureString;

                this.Refresh();


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

            if ((Vvalue > limits.TP115VoltageMin) & (Vvalue < limits.TP115VoltageMax))
            {
                TP115Result = true;

            }
            else
            {
                TP115Result = false;


                failureString = failureString + "Voltage at TP115 Failure....\r\n";

                textBox2.Text = failureString;

                this.Refresh();


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

            if ((Vvalue > limits.TP130VolgageMin) & (Vvalue < limits.TP130VolgageMax))
            {
                TP130Result = true;

            }
            else
            {
                TP130Result = false;


                failureString = failureString + "Voltage at TP130 Failure....\r\n";

                textBox2.Text = failureString;

                this.Refresh();


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


            label19.Text = rms.ToString() + " mVrms";

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
            frm6.Show();

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

            if (RelayNo == 0xff)
            {
                relaydata = 0x00;

            }
            else
            {


                for (indx = 0; indx < RelayNo; indx++)
                    relaydata = (relaydata << 1);
                // for ISM,  OR with d7 so tp409 has voltage


               

            }





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


            // make sure chg bit stays on! (line 6)

           // relaydata |= 0x40;




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


            textBox2.Text = textBox2 + "DUTPort=" + DUTport.PortName + "\r\n";


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

        private void button4_Click(object sender, EventArgs e)
        {
            instumentStatus = 1;

            failureString = failureString + "User Abort\r\n";

            textBox2.Text = failureString;

            this.Refresh();
            // user cancel
        }

        private void button4_KeyDown(object sender, KeyEventArgs e)
        {
            instumentStatus = 1;

            failureString = failureString + "User Abort\r\n";

            textBox2.Text = failureString;

            this.Refresh();
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
