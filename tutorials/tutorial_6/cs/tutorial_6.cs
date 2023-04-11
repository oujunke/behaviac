﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace tutorial_6
{
    class Program
    {
        static FirstAgent g_FirstAgent;
        static behaviac.Workspace Instance;
        static bool InitBehavic()
        {
            Console.WriteLine("InitBehavic");
            Instance = behaviac.Workspace.CreatWorkspace();
            Instance.FilePath = "../../exported";
            Instance.FileFormat = behaviac.Workspace.EFileFormat.EFF_xml;

            return true;
        }

        static bool InitPlayer(string btName)
        {
            Console.WriteLine("InitPlayer : {0}", btName);

            g_FirstAgent = new FirstAgent(Instance);

            bool bRet = g_FirstAgent.btload(btName);
            Debug.Assert(bRet);

            g_FirstAgent.btsetcurrent(btName);

            return bRet;
        }

        static void UpdateLoop()
        {
            Console.WriteLine("UpdateLoop");

            behaviac.EBTStatus status = g_FirstAgent.btexec().Result;
            Debug.Assert(status == behaviac.EBTStatus.BT_RUNNING);

            g_FirstAgent.FireEvent("event_task", 2);
        }

        static void CleanupPlayer()
        {
            Console.WriteLine("CleanupPlayer");

            g_FirstAgent = null;
        }

        static void CleanupBehaviac()
        {
            Console.WriteLine("CleanupBehaviac");

            Instance.Cleanup();
        }

        static void Main(string[] args)
        {
            InitBehavic();

            bool bInit = InitPlayer("maintree_task");

            if (bInit)
            {
                UpdateLoop();

                CleanupPlayer();
            }

            CleanupBehaviac();

            Console.Read();
        }
    }
}
