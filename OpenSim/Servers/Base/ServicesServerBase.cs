/// <license>
///     Copyright (c) Contributors, http://opensimulator.org/
///     See CONTRIBUTORS.TXT for a full list of copyright holders.
/// 
///     Redistribution and use in source and binary forms, with or without
///     modification, are permitted provided that the following conditions are met:
///         * Redistributions of source code must retain the above copyright
///         notice, this list of conditions and the following disclaimer.
///         * Redistributions in binary form must reproduce the above copyright
///         notice, this list of conditions and the following disclaimer in the
///         documentation and/or other materials provided with the distribution.
///         * Neither the name of the OpenSim Project nor the
///         names of its contributors may be used to endorse or promote products
///         derived from this software without specific prior written permission.
/// 
///     THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
///     EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
///     WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
///     DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
///     DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
///     (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
///     LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
///     ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
///     (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
///     SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
/// </license>

using System;
using System.Xml;
using System.Threading;
using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Repository;
using Nini.Config;
using OpenSim.Framework.Console;

namespace OpenSim.Servers.Base
{
    public class ServicesServerBase
    {
        // Logger
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // Command line args
        protected string[] m_Arguments;

        // Configuration
        protected IConfigSource m_Config = null;

        public IConfigSource Config
        {
            get { return m_Config; }
        }

        // Run flag
        private bool m_Running = true;

        // Handle all the automagical stuff
        public ServicesServerBase(string prompt, string[] args)
        {
            // Save raw arguments
            m_Arguments = args;

            // Read command line
            ArgvConfigSource argvConfig = new ArgvConfigSource(args);

            argvConfig.AddSwitch("Startup", "console", "c");
            argvConfig.AddSwitch("Startup", "logfile", "l");
            argvConfig.AddSwitch("Startup", "inifile", "i");

            // Automagically create the ini file name
            string fullName = Assembly.GetEntryAssembly().FullName;
            AssemblyName assemblyName = new AssemblyName(fullName);

            string iniFile = assemblyName.Name + ".ini";

            // Check if a file name was given on the command line
            IConfig startupConfig = argvConfig.Configs["Startup"];

            if (startupConfig != null)
            {
                iniFile = startupConfig.GetString("inifile", iniFile);
            }

            // Find out of the file name is a URI and remote load it
            // if it's possible. Load it as a local file otherwise.
            Uri configUri;

            try
            {
                if (Uri.TryCreate(iniFile, UriKind.Absolute, out configUri) && configUri.Scheme == Uri.UriSchemeHttp)
                {
                    XmlReader r = XmlReader.Create(iniFile);
                    m_Config = new XmlConfigSource(r);
                }
                else
                {
                    m_Config = new IniConfigSource(iniFile);
                }
            }
            catch (Exception)
            {
                System.Console.WriteLine("Error reading from config source {0}", iniFile);
                Thread.CurrentThread.Abort();
            }

            // Merge the configuration from the command line into the
            // loaded file
            m_Config.Merge(argvConfig);

            // Refresh the startupConfig post merge
            startupConfig = argvConfig.Configs["Startup"];

            // Allow derived classes to load config before the console is
            // opened.
            ReadConfig();

            // Create main console
            string consoleType = "local";

            if (startupConfig != null)
            {
                consoleType = startupConfig.GetString("console", consoleType);
            }

            if (consoleType == "basic")
            {
                MainConsole.Instance = new CommandConsole(prompt);
            }
            else
            {
                MainConsole.Instance = new LocalConsole(prompt);
            }

            // Configure the appenders for log4net
            OpenSimAppender consoleAppender = null;
            FileAppender fileAppender = null;

            XmlConfigurator.Configure();

            ILoggerRepository repository = LogManager.GetRepository();
            IAppender[] appenders = repository.GetAppenders();

            foreach (IAppender appender in appenders)
            {
                if (appender.Name == "Console")
                {
                    consoleAppender = (OpenSimAppender)appender;
                }

                if (appender.Name == "LogFileAppender")
                {
                    fileAppender = (FileAppender)appender;
                }
            }

            if (consoleAppender == null)
            {
                System.Console.WriteLine("No console appender found. Server can't start");
                Thread.CurrentThread.Abort();
            }
            else
            {
                consoleAppender.Console = MainConsole.Instance;

                if (null == consoleAppender.Threshold)
                {
                    consoleAppender.Threshold = Level.All;
                }
            }

            // Set log file
            if (fileAppender != null)
            {
                if (startupConfig != null)
                {
                    fileAppender.File = startupConfig.GetString("logfile", assemblyName.Name + ".log");
                }
            }

            // Register the quit command
            MainConsole.Instance.Commands.AddCommand("base", false, "quit",
                    "quit",
                    "Quit the application", HandleQuit);

            // Allow derived classes to perform initialization that
            // needs to be done after the console has opened
            Initialise();
        }

        public virtual int Run()
        {
            while (m_Running)
            {
                MainConsole.Instance.Prompt();
            }

            return 0;
        }

        protected virtual void HandleQuit(string module, string[] args)
        {
            m_Running = false;
            m_log.Info("[Console] Quitting");
        }

        protected virtual void ReadConfig()
        {
        }

        protected virtual void Initialise()
        {
        }
    }
}