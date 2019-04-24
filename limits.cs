using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ITM_ISM_Fixture
{
    class limits
    {
    // this class contains the test limits for Lasso II

        // current limuts
        public static double minPowerUpCurrent = 0;  // set low for debug
        public static double maxPowerUpCurrent = 400;// set to level of old test fixture


      // DC voltage limits

        public static double TP102VoltageMax = 3.05;
        public static double TP102VoltageMin = 2.95;

        public static double TP116VoltageMax = 3.35;
        public static double TP116VoltageMin = 3.25;

        public static double TP115VoltageMax = 3.35;
        public static double TP115VoltageMin = 3.25;

        public static double TP130VolgageMax = 1.7;
        public static double TP130VolgageMin = 1.5;


        





        // IRCurrent Limits (subtraction reference current)


        public static double ldCurrentNominal = .060;

        public static double mdCurrentNominal = .075;

        public static double hdCurrentNominal = .090;


        public static double IRCurrentTolerance = .003;  


// aux level limits

        // full volume -11.5 nominal 2 dB window
        public static double auxMaxLevel = -5;

        public static double auxMinLevel = -12;


        // mic levels  -- adjusted to +/- 3dB on 1/10 @ vTech

        public static double Mic500low = 20;
        public static double Mic500high = 320;    // 10 dB tolerance for 500Hz


        public static double Mic1klow = 80;
        public static double Mic1khigh = 320;

        public static double Mic5klow = 75;
        public static double Mic5khigh = 300;




        //half volume -20-23 nominal


        public static double auxHalfMaxLevel = -17;
        public static double auxHalfMinLevel = -26;



        public static double maxAuxDistortion = 2.00;

        public static double maxAuxDistortionHalf = 3.00;



        public static double AuxFrequency = 1000;  // needed due to false positives on THD


        // PA level limits
        // note -- seems our xfmr can't handle the current from the PA.  need to source a new one


        public static double PAMaxLevel = -8;

        public static double PAMinLevel = -17;

        public static double maxPADistortion = 2.00;

        



        //IR Test Limits

        // 29 nominal

        public static double IRMaxLevel = -2;

        public static double IRMinLevel = -8;


        public static double SensorMaxLevel = -16;

        public static double SensorMinLevel = -22;



        public static double maxIRDistortion = 3;

        // squelch test

        public static double SquelchMaxLevel = -28;
        public static double SquelchDistortion = 50; // this should be a high number

        // pgo test

        public static double PGOOffLevel = -20;
        public static double PGOOnLevel = -15;


        // CM-Juno Version
        public static string CMJunoVersion = "1.9.0.43-2.87";

        public static string ICRVersion = "2.0.0.1-3.11";

        // dc voltage limists

        public double Voltage5VTol = .5;
        public double Voltage18VTol = .5;
       
/*
 * 
 * This is data taken from a sample unit (aux sweep)

        -26.00,-36.55,-28.80,-25.46,-23.49,-22.70,-21.82,-21.53,-21.23,-21.08,
        -20.96,-20.44,-20.33,-20.30,-20.28,-20.28,-20.27,-20.27,-20.26,-20.25,
        -20.23,-20.20,-20.20,-20.23,-20.30,-20.39,-20.52,-20.67,-20.84,-21.04,
        -21.24,-21.47,-21.70,-21.95,-22.20,-22.46,-22.73,-23.00,-23.26  --59
         * 
         * This data has been modified to limits that are +/- 2dB 
         * -26.00,-36.36,-28.79,-25.57,-23.59,-22.60,

*/

        // lowered about 7dB

        // add 16 dB



        // shifted?
        public static double[] AuxFreqResponseNominal = new double[] {-35,-25.1324857785444,-21.5347733158371,-17.9767518717045,-16.6130451098522,
                                                                      -14.2345134382421,-13.8679264789624,-12.8135716760429,-12.2153761671364,
                                                                      -11.6483597300407,-11.349552783069,-9.93149039675728,-9.73826413204003,
                                                                      -9.56511971648439,-9.49921995646607,-9.46545074974136,-9.50239448237524,
                                                                      -9.43312350209392,-9.44419096091672,-9.46998429599692,-9.55766078043396,
                                                                      -9.87614170084812,-10.1103992960966,-10.3767329074393,-10.8026834168884,
                                                                      -11.3637977972968,-11.8403726823414,-12.4446814380452,-12.9289843342332,
                                                                      -13.7094736227161,-14.4515966987917,-15.1235265248238,-15.620117497805,
                                                                      -16.2556328501829,-17.0504904553473,-17.647493884156,-18.2390874094432,
                                                                      -19};




        public static double[] EQFreqResponseNominal = new double[]    {-28.2333006733185,-21.5107228677292,-15.6899024966631,-13.4857659924095,
                                                                        -11.9210659012851,-11.0457745396059,-10.6347117811339,-10.2375092478328,
                                                                        -10.026747881476,-9.85718935237285,-9.25600506667423,-9.02777407480099,
                                                                        -8.90337099500728,-8.90880623869922,-8.91218559428551,-8.94758990897536,
                                                                        -8.9893376250804,-9.10670798703696,-9.09332893952969,-10.3229761461839,
                                                                        -12.5473778362244,-15.0344395050371,-17.6321090059071,-20.4100633496967,
                                                                        -22.8760286859329,-25.0376245483326,-26.3376387628171,-26.5698550284923,
                                                                        -26.2745619924093,-25.8103616929638,-25.5160289654223,-25.1427857351842,
                                                                        -24.9675422853489,-24.943074925878,-24.8306819113199,-24.8267211770563,
                                                                        -24.9620931694282};








    }




  
}
