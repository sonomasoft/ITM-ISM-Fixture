//------------------------------------------------------------------------------
// <autogenerated>
//    This code was generated by Measurement Studio.
//    Runtime Version: 15.0.0.49153
//
//    Changes to this file may cause incorrect behavior and will be lost if
//    the code is regenerated.
// <autogenerated>
//------------------------------------------------------------------------------

using System;
using System.IO;                    // Needed for Stream definitions.
using System.Text;                  // Needed for StringBuilder, ASCII Decoding
using System.Threading;             // Needed in case Sleep is used
using System.Collections;           // Needed for ArrayList
using System.Globalization;         // Needed for NumberStyles (Used in Parse methods)
using NationalInstruments.VisaNS;

namespace ITM_ISM_Fixture
{


    /// <summary>
    /// Class that contains the parsed values from an Instrument I/O task.  The task's Run method will return an instance of this class populated with the parsed values.
    /// </summary>    
    public sealed class BK2831E_ReadCurrentResults
    {
        private string _token2;
        private string _token;
        
        public string Token2
        {
            get { return _token2; }
            set { _token2 = value; }
        }
        
        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }
    }
    

    public sealed class BK2831E_ReadCurrent : IDisposable
    {
        MessageBasedSession _instrumentSession = null;
        bool _handleSessionLifetime = true;
        MessageBasedSessionReader _reader;

        private const string DefaultSessionName = "BK2831E";
        
        private static MessageBasedSession OpenSession(string sessionName)
        {
            if (sessionName == null)
                throw new ArgumentNullException("sessionName");

            return (MessageBasedSession)ResourceManager.GetLocalManager().Open(sessionName, (AccessModes)4, 0);
        }
        
        /// <summary>
        /// This task will open a MessageBasedSession for the VISA resource name configured in the I/O Assistant.  The task will close the MessageBasedSession when the task is disposed.
        /// </summary>
        public BK2831E_ReadCurrent() : this(DefaultSessionName)
        {
        }

        /// <summary>
        /// This task will open a MessageBasedSession for the VISA resource name passed in.  The task will close the MessageBasedSession when the task is disposed.
        /// </summary>
        /// <param name="sessionName">The VISA resource name of the instrument for which the task will open a MessageBasedSession.  The task will close the MessageBasedSession when the task is disposed.</param>
        public BK2831E_ReadCurrent(string sessionName) : this (OpenSession(sessionName), true)
        {
        }
        
        /// <summary>
        /// This task will use the MessageBasedSession passed in.  The task will not close the MessageBasedSession when the task is disposed; the caller is responsible for closing the session.
        /// </summary>
        /// <param name="session">MessageBasedSession used by this task.</param>
        public BK2831E_ReadCurrent(MessageBasedSession session) : this(session, false)
        {
        }

        /// <summary>
        /// This task will use the MessageBasedSession passed in.  This task can either the MessageBasedSession or leave it open for the caller to close.
        /// </summary>
        /// <param name="session">MessageBasedSession used by this task.</param>
        /// <param name="taskHandlesSessionLifetime">If true, the task will close session when the task is disposed. If false, the caller is responsible for closing session.</param>
        public BK2831E_ReadCurrent(MessageBasedSession session, bool taskHandlesSessionLifetime)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            _instrumentSession = session;
            _instrumentSession.Timeout = 4000;
            SerialSession ss = (SerialSession)_instrumentSession;
            ss.ReadTermination = SerialTerminationMethod.TerminationCharacter;
            _instrumentSession.TerminationCharacterEnabled = true;
            _instrumentSession.TerminationCharacter = 10;
            
            // The caller can control the VISA session lifetime by passing in false for taskHandlesSessionLifetime.  If taskHandlesSessionLifetime
            // is true, then the VISA session will be closed when the caller disposes this task.
            _handleSessionLifetime = taskHandlesSessionLifetime;
        
            // The MessageBasedSessionReader is used to read and parse data returned from the instrument
            _reader = new MessageBasedSessionReader(_instrumentSession);
        }
    
        public void Dispose()
        {
            if(_handleSessionLifetime)
            {
                if(_instrumentSession != null)
                {
                    ((IDisposable)(_instrumentSession)).Dispose();
                    _instrumentSession = null;
                }
            }
            _reader = null;
            GC.SuppressFinalize(this);
        }
    
        /// <summary>
        /// Executes the instrument I/O task.
        /// </summary>
        public BK2831E_ReadCurrentResults Run( )
        {
            if (_instrumentSession == null)
                throw new ArgumentNullException("_instrumentSession");
        
            BK2831E_ReadCurrentResults outputs = new BK2831E_ReadCurrentResults();

            // Query step
            // Does a VISA Write
            _instrumentSession.Write(":FETC?\n");
            // Parses out one ASCII string separated by one or more delimiters
            outputs.Token2 = _reader.ReadMismatch(",;\r\n\t");
            _reader.ReadMatch(",;\r\n\t");
            // Read and discard the rest of the response
            _reader.DiscardUnreadData();

            // Query step
            // Does a VISA Write
            _instrumentSession.Write(":FETC?\n");
            // Parses out one ASCII string separated by one or more delimiters
            outputs.Token = _reader.ReadMismatch(",;\r\n\t");
            _reader.ReadMatch(",;\r\n\t");
            // Read and discard the rest of the response
            _reader.DiscardUnreadData();

            return outputs;
        }
    }
}