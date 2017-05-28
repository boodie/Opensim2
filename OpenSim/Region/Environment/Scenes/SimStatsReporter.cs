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
using System.Timers;
using libsecondlife.Packets;
using OpenSim.Framework;

namespace OpenSim.Region.Environment.Scenes
{
    public class SimStatsReporter
    {
        public delegate void SendStatResult(SimStatsPacket pack);

        public event SendStatResult OnSendStatsResult;

        private SendStatResult handlerSendStatResult = null;

        private enum Stats : uint
        {
            TimeDilation = 0,
            SimFPS = 1,
            PhysicsFPS = 2,
            AgentUpdates = 3,
            FrameMS = 4,
            NetMS = 5,
            OtherMS = 6,
            PhysicsMS = 7,
            AgentMS = 8,
            ImageMS = 9,
            ScriptMS = 10,
            TotalPrim = 11,
            ActivePrim = 12,
            Agents = 13,
            ChildAgents = 14,
            ActiveScripts = 15,
            ScriptLinesPerSecond = 16,
            InPacketsPerSecond = 17,
            OutPacketsPerSecond = 18,
            PendingDownloads = 19,
            PendingUploads = 20,
            UnAckedBytes = 24,
        }

        // Sending a stats update every 3 seconds
        private int statsUpdatesEveryMS = 3000;
        private float statsUpdateFactor = 0;
        private float m_timeDilation = 0;
        private int m_fps = 0;
        private float m_pfps = 0;
        private int m_agentUpdates = 0;

        private int m_frameMS = 0;
        private int m_netMS = 0;
        private int m_agentMS = 0;
        private int m_physicsMS = 0;
        private int m_imageMS = 0;
        private int m_otherMS = 0;

//Ckrinke: (3-21-08) Comment out to remove a compiler warning. Bring back into play when needed.
//Ckrinke        private int m_scriptMS = 0;

        private int m_rootAgents = 0;
        private int m_childAgents = 0;
        private int m_numPrim = 0;
        private int m_inPacketsPerSecond = 0;
        private int m_outPacketsPerSecond = 0;
        private int m_activePrim = 0;
        private int m_unAckedBytes = 0;
        private int m_pendingDownloads = 0;
        private int m_pendingUploads = 0;
        private int m_activeScripts = 0;
        private int m_scriptLinesPerSecond = 0;

        private int objectCapacity = 45000;


        SimStatsPacket.StatBlock[] sb = new SimStatsPacket.StatBlock[21];
        SimStatsPacket.RegionBlock rb = new SimStatsPacket.RegionBlock();
        SimStatsPacket statpack = (SimStatsPacket)PacketPool.Instance.GetPacket(PacketType.SimStats);


        private RegionInfo ReportingRegion;

        private Timer m_report = new Timer();


        public SimStatsReporter(RegionInfo regionData)
        {

            statsUpdateFactor = (float)(statsUpdatesEveryMS / 1000);
            ReportingRegion = regionData;
            for (int i = 0; i<21;i++)
            {
                sb[i] = new SimStatsPacket.StatBlock();
            }
            m_report.AutoReset = true;
            m_report.Interval = statsUpdatesEveryMS;
            m_report.Elapsed += new ElapsedEventHandler(statsHeartBeat);
            m_report.Enabled = true;
        }

        public void SetUpdateMS(int ms)
        {
            statsUpdatesEveryMS = ms;
            statsUpdateFactor = (float)(statsUpdatesEveryMS / 1000);
            m_report.Interval = statsUpdatesEveryMS;
        }

        private void statsHeartBeat(object sender, EventArgs e)
        {
            // Know what's not thread safe in Mono... modifying timers.
            // System.Console.WriteLine("Firing Stats Heart Beat");
            lock (m_report)
            {
                // Packet is already initialized and ready for data insert


                statpack.Region = rb;
                statpack.Region.RegionX = ReportingRegion.RegionLocX;
                statpack.Region.RegionY = ReportingRegion.RegionLocY;
                try
                {
                    statpack.Region.RegionFlags = (uint) ReportingRegion.EstateSettings.regionFlags;
                }
                catch (Exception)
                {
                    statpack.Region.RegionFlags = (uint) 0;
                }
                statpack.Region.ObjectCapacity = (uint) objectCapacity;

#region various statistic googly moogly

                // Our FPS is actually 10fps, so multiplying by 5 to get the amount that people expect there
                // 0-50 is pretty close to 0-45
                float simfps = (int) ((m_fps * 5));

                //if (simfps > 45)
                //simfps = simfps - (simfps - 45);
                //if (simfps < 0)
                //simfps = 0;

                //
                float physfps = ((m_pfps / 1000));

                //if (physfps > 600)
                //physfps = physfps - (physfps - 600);

                if (physfps < 0)
                    physfps = 0;

#endregion

                //Our time dilation is 0.91 when we're running a full speed,
                // therefore to make sure we get an appropriate range,
                // we have to factor in our error.   (0.10f * statsUpdateFactor)
                // multiplies the fix for the error times the amount of times it'll occur a second
                // / 10 divides the value by the number of times the sim heartbeat runs (10fps)
                // Then we divide the whole amount by the amount of seconds pass in between stats updates.

                sb[0].StatID = (uint) Stats.TimeDilation;
                sb[0].StatValue = m_timeDilation ; //((((m_timeDilation + (0.10f * statsUpdateFactor)) /10)  / statsUpdateFactor));

                sb[1].StatID = (uint) Stats.SimFPS;
                sb[1].StatValue = simfps/statsUpdateFactor;

                sb[2].StatID = (uint) Stats.PhysicsFPS;
                sb[2].StatValue = physfps / statsUpdateFactor;

                sb[3].StatID = (uint) Stats.AgentUpdates;
                sb[3].StatValue = (m_agentUpdates / statsUpdateFactor);

                sb[4].StatID = (uint) Stats.Agents;
                sb[4].StatValue = m_rootAgents;

                sb[5].StatID = (uint) Stats.ChildAgents;
                sb[5].StatValue = m_childAgents;

                sb[6].StatID = (uint) Stats.TotalPrim;
                sb[6].StatValue = m_numPrim;

                sb[7].StatID = (uint) Stats.ActivePrim;
                sb[7].StatValue = m_activePrim;

                sb[8].StatID = (uint)Stats.FrameMS;
                sb[8].StatValue = m_frameMS / statsUpdateFactor;

                sb[9].StatID = (uint)Stats.NetMS;
                sb[9].StatValue = m_netMS / statsUpdateFactor;

                sb[10].StatID = (uint)Stats.PhysicsMS;
                sb[10].StatValue = m_physicsMS / statsUpdateFactor;

                sb[11].StatID = (uint)Stats.ImageMS ;
                sb[11].StatValue = m_imageMS / statsUpdateFactor;

                sb[12].StatID = (uint)Stats.OtherMS;
                sb[12].StatValue = m_otherMS / statsUpdateFactor;

                sb[13].StatID = (uint)Stats.InPacketsPerSecond;
                sb[13].StatValue = (m_inPacketsPerSecond);

                sb[14].StatID = (uint)Stats.OutPacketsPerSecond;
                sb[14].StatValue = (m_outPacketsPerSecond / statsUpdateFactor);

                sb[15].StatID = (uint)Stats.UnAckedBytes;
                sb[15].StatValue = m_unAckedBytes;

                sb[16].StatID = (uint)Stats.AgentMS;
                sb[16].StatValue = m_agentMS / statsUpdateFactor;

                sb[17].StatID = (uint)Stats.PendingDownloads;
                sb[17].StatValue = m_pendingDownloads;

                sb[18].StatID = (uint)Stats.PendingUploads;
                sb[18].StatValue = m_pendingUploads;

                sb[19].StatID = (uint)Stats.ActiveScripts;
                sb[19].StatValue = m_activeScripts;

                sb[20].StatID = (uint)Stats.ScriptLinesPerSecond;
                sb[20].StatValue = m_scriptLinesPerSecond / statsUpdateFactor;

                statpack.Stat = sb;

                handlerSendStatResult = OnSendStatsResult;
                if (handlerSendStatResult != null)
                {
                    handlerSendStatResult(statpack);
                }
                resetvalues();
            }
        }

        private void resetvalues()
        {
            m_timeDilation = 0;
            m_fps = 0;
            m_pfps = 0;
            m_agentUpdates = 0;
            m_inPacketsPerSecond = 0;
            m_outPacketsPerSecond = 0;
            m_unAckedBytes = 0;
            m_scriptLinesPerSecond = 0;

            m_frameMS = 0;
            m_agentMS = 0;
            m_netMS = 0;
            m_physicsMS = 0;
            m_imageMS = 0;
            m_otherMS = 0;

//Ckrinke This variable is not used, so comment to remove compiler warning until it is used.
//Ckrinke            m_scriptMS = 0;
        }

        # region methods called from Scene
        // The majority of these functions are additive
        // so that you can easily change the amount of
        // seconds in between sim stats updates

        public void AddTimeDilation(float td)
        {
            //float tdsetting = td;
            //if (tdsetting > 1.0f)
                //tdsetting = (tdsetting - (tdsetting - 0.91f));

            //if (tdsetting < 0)
                //tdsetting = 0.0f;
            m_timeDilation = td;
        }

        public void SetRootAgents(int rootAgents)
        {
            m_rootAgents = rootAgents;
        }

        public void SetChildAgents(int childAgents)
        {
            m_childAgents = childAgents;
        }

        public void SetObjects(int objects)
        {
            m_numPrim = objects;
        }

        public void SetActiveObjects(int objects)
        {
            m_activePrim = objects;
        }

        public void AddFPS(int frames)
        {
            m_fps += frames;
        }

        public void AddPhysicsFPS(float frames)
        {
            m_pfps += frames;
        }

        public void AddAgentUpdates(int numUpdates)
        {
            m_agentUpdates += numUpdates;
        }

        public void AddInPackets(int numPackets)
        {
            m_inPacketsPerSecond += numPackets;
        }

        public void AddOutPackets(int numPackets)
        {
            m_outPacketsPerSecond += numPackets;
        }

        public void AddunAckedBytes(int numBytes)
        {
            m_unAckedBytes += numBytes;
        }

        public void addFrameMS(int ms)
        {
            m_frameMS += ms;
        }
        public void addNetMS(int ms)
        {
            m_netMS += ms;
        }
        public void addAgentMS(int ms)
        {
            m_agentMS += ms;
        }
        public void addPhysicsMS(int ms)
        {
            m_physicsMS += ms;
        }
        public void addImageMS(int ms)
        {
            m_imageMS += ms;
        }
        public void addOtherMS(int ms)
        {
            m_otherMS += ms;
        }

//        private static readonly log4net.ILog m_log
//            = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void addPendingDownload(int count)
        {
            m_pendingDownloads += count;
            //m_log.InfoFormat("[stats]: Adding {0} to pending downloads to make {1}", count, m_pendingDownloads);
        }

        public void addScriptLines(int count)
        {
            m_scriptLinesPerSecond += count;
        }

        public void SetActiveScripts(int count)
        {
            m_activeScripts = count;
        }

        public void SetObjectCapacity(int objects)
        {
            objectCapacity = objects;
        }

        #endregion
    }
}