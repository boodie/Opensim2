/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Nini.Config;
using OpenSim.Region.Environment.Interfaces;
using OpenSim.Region.Environment.Scenes;

namespace OpenSim.Region.ScriptEngine.Common.ScriptEngineBase
{
    /// <summary>
    /// This is the root object for ScriptEngine. Objects access each other trough this class.
    /// </summary>
    ///
    [Serializable]
    public abstract class ScriptEngine : IRegionModule, ScriptServerInterfaces.ScriptEngine, iScriptEngineFunctionModule
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static List<ScriptEngine> ScriptEngines = new List<ScriptEngine>();
        public Scene World;
        public EventManager m_EventManager;                         // Handles and queues incoming events from OpenSim
        public EventQueueManager m_EventQueueManager;               // Executes events, handles script threads
        public ScriptManager m_ScriptManager;                       // Load, unload and execute scripts
        public AppDomainManager m_AppDomainManager;                 // Handles loading/unloading of scripts into AppDomains
        public AsyncCommandManager m_ASYNCLSLCommandManager;     // Asyncronous LSL commands (commands that returns with an event)
        public static MaintenanceThread m_MaintenanceThread;        // Thread that does different kinds of maintenance, for example refreshing config and killing scripts that has been running too long

        public IConfigSource ConfigSource;
        public IConfig ScriptConfigSource;
        public abstract string ScriptEngineName { get; }

        /// <summary>
        /// How many seconds between re-reading config-file. 0 = never. ScriptEngine will try to adjust to new config changes.
        /// </summary>
        public int RefreshConfigFileSeconds {
            get { return (int)(RefreshConfigFilens / 10000000); }
            set { RefreshConfigFilens = value * 10000000; }
        }
        public long RefreshConfigFilens;

        public ScriptManager GetScriptManager()
        {
            return _GetScriptManager();
        }

        public abstract ScriptManager _GetScriptManager();

        public ILog Log
        {
            get { return m_log; }
        }

        public ScriptEngine()
        {
            Common.mySE = this;                 // For logging, just need any instance, doesn't matter
            lock (ScriptEngines)
            {
                ScriptEngines.Add(this); // Keep a list of ScriptEngines for shared threads to process all instances
            }
        }

        public void InitializeEngine(Scene Sceneworld, IConfigSource config, bool HookUpToServer, ScriptManager newScriptManager)
        {
            World = Sceneworld;
            ConfigSource = config;
            m_log.Info("[" + ScriptEngineName + "]: ScriptEngine initializing");

            // Make sure we have config
            if (ConfigSource.Configs[ScriptEngineName] == null)
                ConfigSource.AddConfig(ScriptEngineName);
            ScriptConfigSource = ConfigSource.Configs[ScriptEngineName];

            //m_log.Info("[" + ScriptEngineName + "]: InitializeEngine");

            // Create all objects we'll be using
            m_EventQueueManager = new EventQueueManager(this);
            m_EventManager = new EventManager(this, HookUpToServer);
            // We need to start it
            newScriptManager.Start();
            m_ScriptManager = newScriptManager;
            m_AppDomainManager = new AppDomainManager(this);
            m_ASYNCLSLCommandManager = new AsyncCommandManager(this);
            if (m_MaintenanceThread == null)
                m_MaintenanceThread = new MaintenanceThread();

            m_log.Info("[" + ScriptEngineName + "]: Reading configuration from config section \"" + ScriptEngineName + "\"");
            ReadConfig();

            // Should we iterate the region for scripts that needs starting?
            // Or can we assume we are loaded before anything else so we can use proper events?
        }

        public void Shutdown()
        {
            // We are shutting down
            lock (ScriptEngines)
            {
                ScriptEngines.Remove(this);
            }
        }

        ScriptServerInterfaces.RemoteEvents ScriptServerInterfaces.ScriptEngine.EventManager()
        {
            return this.m_EventManager;
        }

        public void ReadConfig()
        {
#if DEBUG
            //m_log.Debug("[" + ScriptEngineName + "]: Refreshing configuration for all modules");
#endif
            RefreshConfigFileSeconds = ScriptConfigSource.GetInt("RefreshConfig", 30);


        // Create a new object (probably not necessary?)
//            ScriptConfigSource = ConfigSource.Configs[ScriptEngineName];

            if (m_EventQueueManager != null) m_EventQueueManager.ReadConfig();
            if (m_EventManager != null) m_EventManager.ReadConfig();
            if (m_ScriptManager != null) m_ScriptManager.ReadConfig();
            if (m_AppDomainManager != null) m_AppDomainManager.ReadConfig();
            if (m_ASYNCLSLCommandManager != null) m_ASYNCLSLCommandManager.ReadConfig();
            if (m_MaintenanceThread != null) m_MaintenanceThread.ReadConfig();
        }

        #region IRegionModule

        public abstract void Initialise(Scene scene, IConfigSource config);

        public void PostInitialise()
        {
        }

        public void Close()
        {
        }

        public string Name
        {
            get { return "Common." + ScriptEngineName; }
        }

        public bool IsSharedModule
        {
            get { return false; }
        }

        #endregion
    }
}