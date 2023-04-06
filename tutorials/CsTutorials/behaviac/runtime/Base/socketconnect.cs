/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Tencent is pleased to support the open source community by making behaviac available.
//
// Copyright (C) 2015-2017 THL A29 Limited, a Tencent company. All rights reserved.
//
// Licensed under the BSD 3-Clause License (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at http://opensource.org/licenses/BSD-3-Clause
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if !BEHAVIAC_RELEASE

using System;
using System.Runtime.InteropServices;

namespace behaviac
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct InitialSettingsPacket
    {
        public void Init()
        {
            messageSize = 0;
            command = (byte)CommandId.CMDID_INITIAL_SETTINGS;
            platform = (byte)Platform.WINDOWS;

            System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
            this.processId = process.Id;
        }

        public int PrepareToSend()
        {
            messageSize = (byte)(Marshal.SizeOf(typeof(InitialSettingsPacket)) - 1);
            return messageSize + 1;
        }

        public byte[] GetData()
        {
            int len = this.PrepareToSend();
            byte[] data = new byte[len];

            data[0] = messageSize;
            data[1] = command;
            data[2] = platform;
            byte[] iarray = BitConverter.GetBytes(this.processId);

            Array.Copy(iarray, 0, data, 3, sizeof(int));

            return data;
        }

        public byte messageSize;
        public byte command;
        public byte platform;
        public int processId;
    };

    internal class ConnectorImpl : ConnectorInterface
    {
        public ConnectorImpl(Workspace workspace):base(workspace)
        {
            m_workspaceSent = false;
            //don't handle message automatically
            m_bHandleMessage = false;
        }

        //~ConnectorImpl()
        //{
        //}

        private void SendInitialSettings()
        {
            InitialSettingsPacket initialPacket = new InitialSettingsPacket();
            initialPacket.Init();
            int bytesWritten = 0;

            if (!SocketBase.Write(m_writeSocket, initialPacket.GetData(), ref bytesWritten,Workspace))
            {
                Log("behaviac: Couldn't send initial settings.\n");
            }

            gs_packetsStats.init++;
        }

        protected override void OnConnection()
        {
            Log("behaviac: sending initial settings.\n");

            this.SendInitialSettings();

            Workspace.SocketUtil.SendWorkspaceSettings();

            this.SendInitialProperties();

            {
                Log("behaviac: sending packets before connecting.\n");

                this.SendExistingPackets();
            }

            Workspace.SocketUtil.SendText("[connected]precached message done\n");

            //when '[connected]' is handled in the designer, it will send back all the breakpoints if any and '[breakcpp]' and '[start]'
            //here we block until all those messages have been received, otherwise, if we don't block here to wait for all those messages
            //the breakpoints checking might be wrong.
            bool bLoop = true;

            while (bLoop && m_isDisconnected.Get() == 0 &&
                   this.m_writeSocket != null && this.m_writeSocket.Connected)
            {
                //sending packets if any
                if (m_packetsCount > 0)
                {
                    SendAllPackets();
                }

                string kStartMsg = "[start]";
                bool bFound = this.ReceivePackets(kStartMsg);

                if (bFound)
                {
                    bLoop = false;
                }
                else
                {
                    System.Threading.Thread.Sleep(1);
                }
            }

            //this.m_bHandleMessage = false;
        }

        private void SendInitialProperties()
        {
            Workspace.LogCurrentStates();
        }

        public bool IsWorkspaceSent()
        {
            return m_workspaceSent;
        }

        public void SetWorkspaceSent(bool bSent)
        {
            m_workspaceSent = bSent;
        }

        private bool m_workspaceSent;

        protected override void Clear()
        {
            base.Clear();

            m_workspaceSent = false;
        }
    };
}

#endif

namespace behaviac
{
    public  class SocketUtils
    {
#if !BEHAVIAC_RELEASE
        private  ConnectorImpl s_tracer;
#endif
        public Workspace Workspace { get; private set; }
        private Config Configs { set; get; }
        public Debug Debugs { set; get; }
        public SocketUtils(Workspace workspace)
        {
            Workspace = workspace;
            Configs = workspace.Configs;
            Debugs = workspace.Debugs;
            s_tracer = new ConnectorImpl(workspace);
        }
        internal  bool SetupConnection(bool bBlocking, ushort port)
        {
#if !BEHAVIAC_RELEASE

            if (Configs.IsSocketing)
            {
                if (!s_tracer.IsInited())
                {
                    const int kMaxThreads = 16;

                    if (!s_tracer.Init(kMaxThreads, port, bBlocking))
                    {
                        return false;
                    }
                }

                Debugs.Log("behaviac: SetupConnection successful\n");

                return true;
            }

#endif
            return false;
        }

        internal void ShutdownConnection()
        {
#if !BEHAVIAC_RELEASE

            if (Configs.IsSocketing)
            {
                s_tracer.Close();

                Debugs.Log("behaviac: ShutdownConnection\n");
            }

#endif
        }

        public void SendText(string text)
        {
#if !BEHAVIAC_RELEASE

            if (Configs.IsSocketing)
            {
                s_tracer.SendText(text, (byte)CommandId.CMDID_TEXT);
            }

#endif
        }

        public bool ReadText(ref string text)
        {
#if !BEHAVIAC_RELEASE

            if (Configs.IsSocketing)
            {
                return s_tracer.ReadText(ref text);
            }

#endif
            return false;
        }

        public bool IsConnected()
        {
#if !BEHAVIAC_RELEASE

            if (Configs.IsSocketing)
            {
                return s_tracer.IsConnected();
            }

#endif

            return false;
        }

        public void Flush()
        {
#if !BEHAVIAC_RELEASE

            if (Configs.IsSocketing)
            {
                while (s_tracer.GetPacketsCount() > 0)
                {
                    System.Threading.Thread.Sleep(1);
                }
            }

#endif
        }

        public void SendWorkspaceSettings()
        {
#if !BEHAVIAC_RELEASE

            if (Configs.IsSocketing)
            {
                if (!s_tracer.IsWorkspaceSent() && s_tracer.IsConnected())
                {
                    Workspace.LogWorkspaceInfo();

                    s_tracer.SetWorkspaceSent(true);
                }
            }

#endif
        }

        public int GetMemoryOverhead()
        {
#if !BEHAVIAC_RELEASE

            if (Configs.IsSocketing)
            {
                return s_tracer.GetMemoryOverhead();
            }

#endif
            return 0;
        }

        public int GetNumTrackedThreads()
        {
#if !BEHAVIAC_RELEASE

            if (Configs.IsSocketing)
            {
                return s_tracer.GetNumTrackedThreads();
            }

#endif
            return 0;
        }

        public void UpdatePacketsStats()
        {
#if !BEHAVIAC_RELEASE

            if (Configs.IsSocketing)
            {
                //uint overhead = (behaviac.GetMemoryOverhead());
                //BEHAVIAC_SETTRACEDVAR("Stats.Vars", gs_packetsStats.vars);
            }

#endif
        }
    }
} // behaviac
