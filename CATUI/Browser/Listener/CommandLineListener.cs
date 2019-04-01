/* 
* Copyright (c) 2009, The University of Texas at Austin
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without modification, 
* are permitted provided that the following conditions are met:
*
* 1. Redistributions of source code must retain the above copyright notice, 
* this list of conditions and the following disclaimer.
*
* 2. Redistributions in binary form must reproduce the above copyright notice, 
* this list of conditions and the following disclaimer in the documentation and/or other materials 
* provided with the distribution.
*
* Neither the name of The University of Texas at Austin nor the names of its contributors may be 
* used to endorse or promote products derived from this software without specific prior written 
* permission.
* 
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
* IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
* FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS 
* BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
* PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
* CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF 
* THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Diagnostics;
using System.ServiceModel;

namespace BioBrowser.Listener
{
    /// <summary>
    /// Interface to listen for a command line
    /// </summary>
    [ServiceContract]
    public interface ICommandLineListener
    {
        [OperationContract]
        void SendCommandLine(string[] commandLine);
    }

    /// <summary>
    /// Command line listener class
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CommandLineListener : ICommandLineListener, IDisposable
    {
        private const string ListenerEndpoint = @"net.pipe://localhost/BioBrowser.Singleton.Pipe";
        private readonly ServiceHost _host;
        private static CommandLineListener _instance;
        private readonly Action<string[]> _commandLineHandler;

        public static CommandLineListener Instance
        {
            get { return _instance;  }
        }

        private CommandLineListener(Action<string[]> commandLineHandler)
        {
            Debug.Assert(_instance == null);
            _instance = this;
            _commandLineHandler = commandLineHandler;

            // Create the listening host
            _host = new ServiceHost(this, new Uri(ListenerEndpoint));
            _host.AddServiceEndpoint(typeof(ICommandLineListener), new NetNamedPipeBinding(), "");
            _host.Open();
        }

        public static bool CreateListener(string[] commandLine, Action<string[]> commandLineHandler)
        {
            bool firstInstance;

            try
            {
                new CommandLineListener(commandLineHandler);
                firstInstance = true;
            }
            catch
            {
                firstInstance = false;
            }

            if (!firstInstance)
            {
                if (commandLine != null && commandLine.Length > 0)
                {
                    var factory = new ChannelFactory<ICommandLineListener>(new NetNamedPipeBinding());
                    var listener = factory.CreateChannel(new EndpointAddress(ListenerEndpoint));
                    listener.SendCommandLine(commandLine);
                    ((IDisposable) listener).Dispose();
                }
                return false;
            }

            return true;
        }

        public void SendCommandLine(string[] commandLine)
        {
            if (commandLine != null && commandLine.Length > 0)
            {
                if (_commandLineHandler != null)
                    _commandLineHandler(commandLine);
            }
        }

        public void Dispose()
        {
            if (_host != null)
                _host.Close();
        }
    }
}
