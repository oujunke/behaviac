using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace tutorial_3
{
    class Program
    {
        static FirstAgent g_FirstAgent;
        static SecondAgent g_SecondAgent;
        static SecondAgent g_ThirdAgent;
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

            g_FirstAgent = new FirstAgent(Instance);
            bool bRet = g_FirstAgent.btload("InstanceBT");
            Debug.Assert(bRet);
            g_FirstAgent.btsetcurrent("InstanceBT");

            g_SecondAgent = new SecondAgent(Instance);
            g_FirstAgent._set_pInstance(g_SecondAgent);

            g_ThirdAgent = new SecondAgent(Instance);
            behaviac.Agent.BindInstance(g_ThirdAgent, "SecondAgentInstance");

            return bRet;
        }

        static void UpdateLoop()
        {
            Console.WriteLine("UpdateLoop");

            int frames = 0;
            behaviac.EBTStatus status = behaviac.EBTStatus.BT_RUNNING;

            while (status == behaviac.EBTStatus.BT_RUNNING)
            {
                Console.WriteLine("frame {0}", ++frames);

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
