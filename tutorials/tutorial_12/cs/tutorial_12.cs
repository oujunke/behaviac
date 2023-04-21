using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace tutorial_12
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

        static bool InitPlayer()
        {
            Console.WriteLine("InitPlayer");

            g_FirstAgent = new FirstAgent(new FirstAgentImp(Instance),Instance);

            bool bRet = g_FirstAgent.btload("ParallelBT");
            Debug.Assert(bRet);

            g_FirstAgent.btsetcurrent("ParallelBT");

            return bRet;
        }

        static void UpdateLoop()
        {
            Console.WriteLine("UpdateLoop");

            Instance.FrameSinceStartup = 0;

            behaviac.EBTStatus status = behaviac.EBTStatus.BT_RUNNING;

            while (status == behaviac.EBTStatus.BT_RUNNING)
            {
                Instance.FrameSinceStartup++;

                Console.WriteLine("frame {0}", Instance.FrameSinceStartup);

                status = g_FirstAgent.btexec().Result;
            }
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

            InitPlayer();

            UpdateLoop();

            CleanupPlayer();

            CleanupBehaviac();

            Console.Read();
        }
    }
}
