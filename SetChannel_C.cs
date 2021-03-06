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


    public sealed class SetChannel_C : IDisposable
    {
        MessageBasedSession _instrumentSession = null;
        bool _handleSessionLifetime = true;
        MessageBasedSessionWriter _writer;
        private const string DefaultSessionName = "DUT";
        
        private static MessageBasedSession OpenSession(string sessionName)
        {
            if (sessionName == null)
                throw new ArgumentNullException("sessionName");

            return (MessageBasedSession)ResourceManager.GetLocalManager().Open(sessionName, (AccessModes)4, 0);
        }
        
        /// <summary>
        /// This task will open a MessageBasedSession for the VISA resource name configured in the I/O Assistant.  The task will close the MessageBasedSession when the task is disposed.
        /// </summary>
        public SetChannel_C() : this(DefaultSessionName)
        {
        }

        /// <summary>
        /// This task will open a MessageBasedSession for the VISA resource name passed in.  The task will close the MessageBasedSession when the task is disposed.
        /// </summary>
        /// <param name="sessionName">The VISA resource name of the instrument for which the task will open a MessageBasedSession.  The task will close the MessageBasedSession when the task is disposed.</param>
        public SetChannel_C(string sessionName) : this (OpenSession(sessionName), true)
        {
        }
        
        /// <summary>
        /// This task will use the MessageBasedSession passed in.  The task will not close the MessageBasedSession when the task is disposed; the caller is responsible for closing the session.
        /// </summary>
        /// <param name="session">MessageBasedSession used by this task.</param>
        public SetChannel_C(MessageBasedSession session) : this(session, false)
        {
        }

        /// <summary>
        /// This task will use the MessageBasedSession passed in.  This task can either the MessageBasedSession or leave it open for the caller to close.
        /// </summary>
        /// <param name="session">MessageBasedSession used by this task.</param>
        /// <param name="taskHandlesSessionLifetime">If true, the task will close session when the task is disposed. If false, the caller is responsible for closing session.</param>
        public SetChannel_C(MessageBasedSession session, bool taskHandlesSessionLifetime)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            _instrumentSession = session;
            _instrumentSession.Timeout = 2000;
            SerialSession ss = (SerialSession)_instrumentSession;
            ss.ReadTermination = SerialTerminationMethod.TerminationCharacter;
            _instrumentSession.TerminationCharacterEnabled = true;
            _instrumentSession.TerminationCharacter = 10;
            
            // The caller can control the VISA session lifetime by passing in false for taskHandlesSessionLifetime.  If taskHandlesSessionLifetime
            // is true, then the VISA session will be closed when the caller disposes this task.
            _handleSessionLifetime = taskHandlesSessionLifetime;
        
            // The MessageBasedSessionWriter is used to write formatted data to the instrument
            _writer = new MessageBasedSessionWriter(_instrumentSession);
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
            _writer = null;
            GC.SuppressFinalize(this);
        }
    
        /// <summary>
        /// Executes the instrument I/O task.
        /// </summary>
        public void Run( )
        {
            if (_instrumentSession == null)
                throw new ArgumentNullException("_instrumentSession");
        
            

            // Write step
            // Format input value into the write buffer
            _writer.Write("chi = 2\n");
            // Send buffered data to the instrument
            _writer.Flush();
        }
    }
}
