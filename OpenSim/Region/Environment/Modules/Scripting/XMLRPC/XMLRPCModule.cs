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
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading;
using libsecondlife;
using log4net;
using Nini.Config;
using Nwc.XmlRpc;
using OpenSim.Framework;
using OpenSim.Framework.Servers;
using OpenSim.Region.Environment.Interfaces;
using OpenSim.Region.Environment.Scenes;

/*****************************************************
 *
 * XMLRPCModule
 *
 * Module for accepting incoming communications from
 * external XMLRPC client and calling a remote data
 * procedure for a registered data channel/prim.
 *
 *
 * 1. On module load, open a listener port
 * 2. Attach an XMLRPC handler
 * 3. When a request is received:
 * 3.1 Parse into components: channel key, int, string
 * 3.2 Look up registered channel listeners
 * 3.3 Call the channel (prim) remote data method
 * 3.4 Capture the response (llRemoteDataReply)
 * 3.5 Return response to client caller
 * 3.6 If no response from llRemoteDataReply within
 *     RemoteReplyScriptTimeout, generate script timeout fault
 *
 * Prims in script must:
 * 1. Open a remote data channel
 * 1.1 Generate a channel ID
 * 1.2 Register primid,channelid pair with module
 * 2. Implement the remote data procedure handler
 *
 * llOpenRemoteDataChannel
 * llRemoteDataReply
 * remote_data(integer type, key channel, key messageid, string sender, integer ival, string sval)
 * llCloseRemoteDataChannel
 *
 * **************************************************/

namespace OpenSim.Region.Environment.Modules.Scripting.XMLRPC
{
    public class XMLRPCModule : IRegionModule, IXMLRPC
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string m_name = "XMLRPCModule";

        // <channel id, RPCChannelInfo>
        private Dictionary<LLUUID, RPCChannelInfo> m_openChannels;
        private Dictionary<LLUUID, SendRemoteDataRequest> m_pendingSRDResponses;
        private int m_remoteDataPort = 0;

        private Dictionary<LLUUID, RPCRequestInfo> m_rpcPending;
        private Dictionary<LLUUID, RPCRequestInfo> m_rpcPendingResponses;
        private List<Scene> m_scenes = new List<Scene>();
        private int RemoteReplyScriptTimeout = 9000;
        private int RemoteReplyScriptWait = 300;
        private object XMLRPCListLock = new object();

        #region IRegionModule Members

        public void Initialise(Scene scene, IConfigSource config)
        {
            // We need to create these early because the scripts might be calling
            // But since this gets called for every region, we need to make sure they
            // get called only one time (or we lose any open channels)
            if (null == m_openChannels)
            {
                m_openChannels = new Dictionary<LLUUID, RPCChannelInfo>();
                m_rpcPending = new Dictionary<LLUUID, RPCRequestInfo>();
                m_rpcPendingResponses = new Dictionary<LLUUID, RPCRequestInfo>();
                m_pendingSRDResponses = new Dictionary<LLUUID, SendRemoteDataRequest>();

                try
                {
                    m_remoteDataPort = config.Configs["Network"].GetInt("remoteDataPort", m_remoteDataPort);
                }
                catch (Exception)
                {
                }
            }

            if (!m_scenes.Contains(scene))
            {
                m_scenes.Add(scene);

                scene.RegisterModuleInterface<IXMLRPC>(this);
            }
        }

        public void PostInitialise()
        {
            if (IsEnabled())
            {
                // Start http server
                // Attach xmlrpc handlers
                m_log.Info("[REMOTE_DATA]: " +
                           "Starting XMLRPC Server on port " + m_remoteDataPort + " for llRemoteData commands.");
                BaseHttpServer httpServer = new BaseHttpServer((uint) m_remoteDataPort);
                httpServer.AddXmlRPCHandler("llRemoteData", XmlRpcRemoteData);
                httpServer.Start();
            }
        }

        public void Close()
        {
        }

        public string Name
        {
            get { return m_name; }
        }

        public bool IsSharedModule
        {
            get { return true; }
        }

        #endregion

        #region IXMLRPC Members

        public bool IsEnabled()
        {
            return (m_remoteDataPort > 0);
        }

        /**********************************************
         * OpenXMLRPCChannel
         *
         * Generate a LLUUID channel key and add it and
         * the prim id to dictionary <channelUUID, primUUID>
         *
         * A custom channel key can be proposed.
         * Otherwise, passing LLUUID.Zero will generate
         * and return a random channel
         *
         * First check if there is a channel assigned for
         * this itemID.  If there is, then someone called
         * llOpenRemoteDataChannel twice.  Just return the
         * original channel.  Other option is to delete the
         * current channel and assign a new one.
         *
         * ********************************************/

        public LLUUID OpenXMLRPCChannel(uint localID, LLUUID itemID, LLUUID channelID)
        {
            LLUUID newChannel = LLUUID.Zero;

            // This should no longer happen, but the check is reasonable anyway
            if (null == m_openChannels)
            {
                m_log.Warn("[RemoteDataReply] Attempt to open channel before initialization is complete");
                return newChannel;
            }

            //Is a dupe?
            foreach (RPCChannelInfo ci in m_openChannels.Values)
            {
                if (ci.GetItemID().Equals(itemID))
                {
                    // return the original channel ID for this item
                    newChannel = ci.GetChannelID();
                    break;
                }
            }

            if (newChannel == LLUUID.Zero)
            {
                newChannel = (channelID == LLUUID.Zero) ? LLUUID.Random() : channelID;
                RPCChannelInfo rpcChanInfo = new RPCChannelInfo(localID, itemID, newChannel);
                lock (XMLRPCListLock)
                {
                    m_openChannels.Add(newChannel, rpcChanInfo);
                }
            }

            return newChannel;
        }

        // Delete channels based on itemID
        // for when a script is deleted
        public void DeleteChannels(LLUUID itemID)
        {
            if (m_openChannels != null)
            {
                ArrayList tmp = new ArrayList();

                lock (XMLRPCListLock)
                {
                    foreach (RPCChannelInfo li in m_openChannels.Values)
                    {
                        if (li.GetItemID().Equals(itemID))
                        {
                            tmp.Add(itemID);
                        }
                    }

                    IEnumerator tmpEnumerator = tmp.GetEnumerator();
                    while (tmpEnumerator.MoveNext())
                        m_openChannels.Remove((LLUUID) tmpEnumerator.Current);
                }
            }
        }

        /**********************************************
         * Remote Data Reply
         *
         * Response to RPC message
         *
         *********************************************/

        public void RemoteDataReply(string channel, string message_id, string sdata, int idata)
        {
            LLUUID message_key = new LLUUID(message_id);
            LLUUID channel_key = new LLUUID(channel);

            RPCRequestInfo rpcInfo = null;

            if (message_key == LLUUID.Zero)
            {
                foreach (RPCRequestInfo oneRpcInfo in m_rpcPendingResponses.Values)
                    if (oneRpcInfo.GetChannelKey() == channel_key)
                        rpcInfo = oneRpcInfo;
            }
            else
            {
                m_rpcPendingResponses.TryGetValue(message_key, out rpcInfo);
            }

            if (rpcInfo != null)
            {
                rpcInfo.SetStrRetval(sdata);
                rpcInfo.SetIntRetval(idata);
                rpcInfo.SetProcessed(true);
                m_rpcPendingResponses.Remove(message_key);
            }
            else
            {
                m_log.Warn("[RemoteDataReply]: Channel or message_id not found");
            }
        }

        /**********************************************
         * CloseXMLRPCChannel
         *
         * Remove channel from dictionary
         *
         *********************************************/

        public void CloseXMLRPCChannel(LLUUID channelKey)
        {
            if (m_openChannels.ContainsKey(channelKey))
                m_openChannels.Remove(channelKey);
        }


        public bool hasRequests()
        {
            lock (XMLRPCListLock)
            {
                if (m_rpcPending != null)
                    return (m_rpcPending.Count > 0);
                else
                    return false;
            }
        }

        public RPCRequestInfo GetNextCompletedRequest()
        {
            if (m_rpcPending != null)
            {
                lock (XMLRPCListLock)
                {
                    foreach (LLUUID luid in m_rpcPending.Keys)
                    {
                        RPCRequestInfo tmpReq;

                        if (m_rpcPending.TryGetValue(luid, out tmpReq))
                        {
                            if (!tmpReq.IsProcessed()) return tmpReq;
                        }
                    }
                }
            }
            return null;
        }

        public void RemoveCompletedRequest(LLUUID id)
        {
            lock (XMLRPCListLock)
            {
                RPCRequestInfo tmp;
                if (m_rpcPending.TryGetValue(id, out tmp))
                {
                    m_rpcPending.Remove(id);
                    m_rpcPendingResponses.Add(id, tmp);
                }
                else
                {
                    Console.WriteLine("UNABLE TO REMOVE COMPLETED REQUEST");
                }
            }
        }

        public LLUUID SendRemoteData(uint localID, LLUUID itemID, string channel, string dest, int idata, string sdata)
        {
            SendRemoteDataRequest req = new SendRemoteDataRequest(
                localID, itemID, channel, dest, idata, sdata
                );
            m_pendingSRDResponses.Add(req.GetReqID(), req);
            return req.process();
        }

        public SendRemoteDataRequest GetNextCompletedSRDRequest()
        {
            if (m_pendingSRDResponses != null)
            {
                lock (XMLRPCListLock)
                {
                    foreach (LLUUID luid in m_pendingSRDResponses.Keys)
                    {
                        SendRemoteDataRequest tmpReq;

                        if (m_pendingSRDResponses.TryGetValue(luid, out tmpReq))
                        {
                            if (tmpReq.finished)
                                return tmpReq;
                        }
                    }
                }
            }
            return null;
        }

        public void RemoveCompletedSRDRequest(LLUUID id)
        {
            lock (XMLRPCListLock)
            {
                SendRemoteDataRequest tmpReq;
                if (m_pendingSRDResponses.TryGetValue(id, out tmpReq))
                {
                    m_pendingSRDResponses.Remove(id);
                }
            }
        }

        public void CancelSRDRequests(LLUUID itemID)
        {
            if (m_pendingSRDResponses != null)
            {
                lock (XMLRPCListLock)
                {
                    foreach (SendRemoteDataRequest li in m_pendingSRDResponses.Values)
                    {
                        if (li.m_itemID.Equals(itemID))
                            m_pendingSRDResponses.Remove(li.GetReqID());
                    }
                }
            }
        }

        #endregion

        public XmlRpcResponse XmlRpcRemoteData(XmlRpcRequest request)
        {
            XmlRpcResponse response = new XmlRpcResponse();

            Hashtable requestData = (Hashtable) request.Params[0];
            bool GoodXML = (requestData.Contains("Channel") && requestData.Contains("IntValue") &&
                            requestData.Contains("StringValue"));

            if (GoodXML)
            {
                LLUUID channel = new LLUUID((string) requestData["Channel"]);
                RPCChannelInfo rpcChanInfo;
                if (m_openChannels.TryGetValue(channel, out rpcChanInfo))
                {
                    string intVal = Convert.ToInt32(requestData["IntValue"]).ToString();
                    string strVal = (string) requestData["StringValue"];

                    RPCRequestInfo rpcInfo;

                    lock (XMLRPCListLock)
                    {
                        rpcInfo =
                            new RPCRequestInfo(rpcChanInfo.GetLocalID(), rpcChanInfo.GetItemID(), channel, strVal,
                                               intVal);
                        m_rpcPending.Add(rpcInfo.GetMessageID(), rpcInfo);
                    }

                    int timeoutCtr = 0;

                    while (!rpcInfo.IsProcessed() && (timeoutCtr < RemoteReplyScriptTimeout))
                    {
                        Thread.Sleep(RemoteReplyScriptWait);
                        timeoutCtr += RemoteReplyScriptWait;
                    }
                    if (rpcInfo.IsProcessed())
                    {
                        Hashtable param = new Hashtable();
                        param["StringValue"] = rpcInfo.GetStrRetval();
                        param["IntValue"] = rpcInfo.GetIntRetval();

                        ArrayList parameters = new ArrayList();
                        parameters.Add(param);

                        response.Value = parameters;
                        rpcInfo = null;
                    }
                    else
                    {
                        response.SetFault(-1, "Script timeout");
                        rpcInfo = null;
                    }
                }
                else
                {
                    response.SetFault(-1, "Invalid channel");
                }
            }

            return response;
        }
    }

    public class RPCRequestInfo
    {
        private LLUUID m_ChannelKey;
        private string m_IntVal;
        private LLUUID m_ItemID;
        private uint m_localID;
        private LLUUID m_MessageID;
        private bool m_processed;
        private int m_respInt;
        private string m_respStr;
        private string m_StrVal;

        public RPCRequestInfo(uint localID, LLUUID itemID, LLUUID channelKey, string strVal, string intVal)
        {
            m_localID = localID;
            m_StrVal = strVal;
            m_IntVal = intVal;
            m_ItemID = itemID;
            m_ChannelKey = channelKey;
            m_MessageID = LLUUID.Random();
            m_processed = false;
            m_respStr = String.Empty;
            m_respInt = 0;
        }

        public bool IsProcessed()
        {
            return m_processed;
        }

        public LLUUID GetChannelKey()
        {
            return m_ChannelKey;
        }

        public void SetProcessed(bool processed)
        {
            m_processed = processed;
        }

        public void SetStrRetval(string resp)
        {
            m_respStr = resp;
        }

        public string GetStrRetval()
        {
            return m_respStr;
        }

        public void SetIntRetval(int resp)
        {
            m_respInt = resp;
        }

        public int GetIntRetval()
        {
            return m_respInt;
        }

        public uint GetLocalID()
        {
            return m_localID;
        }

        public LLUUID GetItemID()
        {
            return m_ItemID;
        }

        public string GetStrVal()
        {
            return m_StrVal;
        }

        public int GetIntValue()
        {
            return int.Parse(m_IntVal);
        }

        public LLUUID GetMessageID()
        {
            return m_MessageID;
        }
    }

    public class RPCChannelInfo
    {
        private LLUUID m_ChannelKey;
        private LLUUID m_itemID;
        private uint m_localID;

        public RPCChannelInfo(uint localID, LLUUID itemID, LLUUID channelID)
        {
            m_ChannelKey = channelID;
            m_localID = localID;
            m_itemID = itemID;
        }

        public LLUUID GetItemID()
        {
            return m_itemID;
        }

        public LLUUID GetChannelID()
        {
            return m_ChannelKey;
        }

        public uint GetLocalID()
        {
            return m_localID;
        }
    }

    public class SendRemoteDataRequest
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public string channel;
        public string destURL;
        public bool finished;
        private Thread httpThread;
        public int idata;
        public LLUUID m_itemID;
        public uint m_localID;
        public LLUUID reqID;
        public XmlRpcRequest request;
        public int response_idata;
        public string response_sdata;
        public string sdata;

        public SendRemoteDataRequest(uint localID, LLUUID itemID, string channel, string dest, int idata, string sdata)
        {
            this.channel = channel;
            destURL = dest;
            this.idata = idata;
            this.sdata = sdata;
            m_itemID = itemID;
            m_localID = localID;

            reqID = LLUUID.Random();
        }

        public LLUUID process()
        {
            httpThread = new Thread(SendRequest);
            httpThread.Name = "HttpRequestThread";
            httpThread.Priority = ThreadPriority.BelowNormal;
            httpThread.IsBackground = true;
            finished = false;
            httpThread.Start();
            ThreadTracker.Add(httpThread);

            return reqID;
        }

        /*
         * TODO: More work on the response codes.  Right now
         * returning 200 for success or 499 for exception
         */

        public void SendRequest()
        {
            Hashtable param = new Hashtable();

            // Check if channel is an LLUUID
            // if not, use as method name
            LLUUID parseUID;
            string mName = "llRemoteData";
            if ((channel != null) && (channel != ""))
                if (!LLUUID.TryParse(channel, out parseUID))
                    mName = channel;
                else
                    param["Channel"] = channel;

            param["StringValue"] = sdata;
            param["IntValue"] = Convert.ToString(idata);

            ArrayList parameters = new ArrayList();
            parameters.Add(param);
            XmlRpcRequest req = new XmlRpcRequest(mName, parameters);
            try
            {
                XmlRpcResponse resp = req.Send(destURL, 30000);
                if (resp != null)
                {
                    Hashtable respParms;
                    if (resp.Value.GetType().Equals(Type.GetType("System.Collections.Hashtable")))
                    {
                        respParms = (Hashtable) resp.Value;
                    }
                    else
                    {
                        ArrayList respData = (ArrayList) resp.Value;
                        respParms = (Hashtable) respData[0];
                    }
                    if (respParms != null)
                    {
                        if (respParms.Contains("StringValue"))
                        {
                            sdata = (string) respParms["StringValue"];
                        }
                        if (respParms.Contains("IntValue"))
                        {
                            idata = Convert.ToInt32((string) respParms["IntValue"]);
                        }
                        if (respParms.Contains("faultString"))
                        {
                            sdata = (string) respParms["faultString"];
                        }
                        if (respParms.Contains("faultCode"))
                        {
                            idata = Convert.ToInt32(respParms["faultCode"]);
                        }
                    }
                }
            }
            catch (WebException we)
            {
                sdata = we.Message;
                m_log.Warn("[SendRemoteDataRequest]: Request failed");
                m_log.Warn(we.StackTrace);
            }

            finished = true;
        }

        public void Stop()
        {
            try
            {
                httpThread.Abort();
            }
            catch (Exception)
            {
            }
        }

        public LLUUID GetReqID()
        {
            return reqID;
        }
    }
}