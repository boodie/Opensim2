/// <summary>
///     Copyright (c) Contributors, http://opensimulator.org/
///     See CONTRIBUTORS.TXT for a full list of copyright holders.
///     For an explanation of the license of each contributor and the content it 
///     covers please see the Licenses directory.
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
/// </summary>

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using log4net;
using OpenSim.Framework;
using OpenSim.Framework.RegionLoader.Filesystem;
using OpenSim.Framework.RegionLoader.Web;
using OpenSim.Region.CoreModules.Agent.AssetTransaction;
using OpenSim.Region.CoreModules.Avatar.InstantMessage;
using OpenSim.Region.CoreModules.Scripting.DynamicTexture;
using OpenSim.Region.CoreModules.Scripting.LoadImageURL;
using OpenSim.Region.CoreModules.Scripting.XMLRPC;

namespace OpenSim.ApplicationPlugins.LoadRegions
{
    public class LoadRegionsPlugin : IApplicationPlugin, IRegionCreator
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event NewRegionCreated OnNewRegionCreated;
        private NewRegionCreated m_newRegionCreatedHandler;

        #region IApplicationPlugin Members

        // TODO: required by IPlugin, but likely not at all right
        private string m_name = "LoadRegionsPlugin";
        private string m_version = "0.0";

        public string Version
        {
            get { return m_version; }
        }

        public string Name
        {
            get { return m_name; }
        }

        protected OpenSimBase m_openSim;

        public void Initialise()
        {
            m_log.Error("[Load Regions]: " + Name + " cannot be default-initialized!");
            throw new PluginNotInitialisedException(Name);
        }

        public void Initialise(OpenSimBase openSim)
        {
            m_openSim = openSim;
            m_openSim.ApplicationRegistry.RegisterInterface<IRegionCreator>(this);
        }

        public void PostInitialise()
        {
            IRegionLoader regionLoader;

            if (m_openSim.ConfigSource.Source.Configs["Startup"].GetString("region_info_source", "filesystem") == "filesystem")
            {
                m_log.Info("[Load Regions]: Loading region configurations from filesystem");
                regionLoader = new RegionLoaderFileSystem();
            }
            else
            {
                m_log.Info("[Load Regions]: Loading region configurations from web");
                regionLoader = new RegionLoaderWebServer();
            }

            m_log.Info("[Load Regions]: Loading region configurations...");

            regionLoader.SetIniConfigSource(m_openSim.ConfigSource.Source);
            RegionInfo[] regionsToLoad = regionLoader.LoadRegions();

            m_log.Info("[Load Regions]: Loading specific shared modules...");
            m_log.Info("[Load Regions]: DynamicTextureModule...");
            m_openSim.ModuleLoader.LoadDefaultSharedModule(new DynamicTextureModule());
            m_log.Info("[Load Regions]: LoadImageURLModule...");
            m_openSim.ModuleLoader.LoadDefaultSharedModule(new LoadImageURLModule());
            m_log.Info("[Load Regions]: XMLRPCModule...");
            m_openSim.ModuleLoader.LoadDefaultSharedModule(new XMLRPCModule());
            m_log.Info("[Load Regions]: AssetTransactionModule...");
            m_openSim.ModuleLoader.LoadDefaultSharedModule(new AssetTransactionModule());
            m_log.Info("[Load Regions]: Done.");

            if (!CheckRegionsForSanity(regionsToLoad))
            {
                m_log.Error("[Load Regions]: Halting startup due to conflicts in region configurations");
                Environment.Exit(1);
            }

            for (int i = 0; i < regionsToLoad.Length; i++)
            {
                IScene scene;
                m_log.Debug("[Load Regions]: Creating Region: " + regionsToLoad[i].RegionName + " (ThreadID: " + Thread.CurrentThread.ManagedThreadId.ToString() + ")");
                m_openSim.PopulateRegionEstateInfo(regionsToLoad[i]);
                m_openSim.CreateRegion(regionsToLoad[i], true, out scene);
                regionsToLoad[i].EstateSettings.Save();

                if (scene != null)
                {
                    m_newRegionCreatedHandler = OnNewRegionCreated;

                    if (m_newRegionCreatedHandler != null)
                    {
                        m_newRegionCreatedHandler(scene);
                    }
                }
            }

            m_openSim.ModuleLoader.PostInitialise();
            m_openSim.ModuleLoader.ClearCache();
        }

        public void Dispose()
        {
        }

        #endregion

        /// <summary>
        ///     Check that region configuration information makes sense.
        /// </summary>
        /// <param name="regions"></param>
        /// <returns>True if we're sane, false if we're insane</returns>
        private bool CheckRegionsForSanity(RegionInfo[] regions)
        {
            if (regions.Length <= 1)
            {
                return true;
            }

            for (int i = 0; i < regions.Length - 1; i++)
            {
                for (int j = i + 1; j < regions.Length; j++)
                {
                    if (regions[i].RegionID == regions[j].RegionID)
                    {
                        m_log.ErrorFormat("[Load Regions]: Regions {0} and {1} have the same UUID {2}", regions[i].RegionName, regions[j].RegionName, regions[i].RegionID);
                        return false;
                    }
                    else if (regions[i].RegionLocX == regions[j].RegionLocX && regions[i].RegionLocY == regions[j].RegionLocY)
                    {
                        m_log.ErrorFormat("[Load Regions]: Regions {0} and {1} have the same grid location ({2}, {3})", regions[i].RegionName, regions[j].RegionName, regions[i].RegionLocX, regions[i].RegionLocY);
                        return false;
                    }
                    else if (regions[i].InternalEndPoint.Port == regions[j].InternalEndPoint.Port)
                    {
                        m_log.ErrorFormat("[Load Regions]: Regions {0} and {1} have the same internal IP port {2}", regions[i].RegionName, regions[j].RegionName, regions[i].InternalEndPoint.Port);
                        return false;
                    }
                }
            }

            return true;
        }

        public void LoadRegionFromConfig(OpenSimBase openSim, ulong regionhandle)
        {
            m_log.Info("[Load Regions]: Load Regions addin being initialised");

            IRegionLoader regionLoader;

            if (openSim.ConfigSource.Source.Configs["Startup"].GetString("region_info_source", "filesystem") == "filesystem")
            {
                m_log.Info("[Load Regions]: Loading Region Info from filesystem");
                regionLoader = new RegionLoaderFileSystem();
            }
            else
            {
                m_log.Info("[Load Regions]: Loading Region Info from web");
                regionLoader = new RegionLoaderWebServer();
            }

            regionLoader.SetIniConfigSource(openSim.ConfigSource.Source);
            RegionInfo[] regionsToLoad = regionLoader.LoadRegions();

            for (int i = 0; i < regionsToLoad.Length; i++)
            {
                if (regionhandle == regionsToLoad[i].RegionHandle)
                {
                    IScene scene;
                    m_log.Debug("[Load Regions]: Creating Region: " + regionsToLoad[i].RegionName + " (ThreadID: " + Thread.CurrentThread.ManagedThreadId.ToString() + ")");
                    openSim.CreateRegion(regionsToLoad[i], true, out scene);
                }
            }
        }
    }
}