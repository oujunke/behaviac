using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace tutorial_10
{
    class Program
    {
        static FirstAgent g_FirstAgent;
        static MyFileManager g_MyFileManager;
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

            g_FirstAgent = new FirstAgent(new FirstAgentImp(), Instance);

            bool bRet = g_FirstAgent.btload("FirstBT");
            Debug.Assert(bRet);

            g_FirstAgent.btsetcurrent("FirstBT");

            return bRet;
        }

        static  void UpdateLoop()
        {
            Console.WriteLine("UpdateLoop");

            int frames = 0;
            behaviac.EBTStatus status = behaviac.EBTStatus.BT_RUNNING;

            while (status == behaviac.EBTStatus.BT_RUNNING)
            {
                Console.WriteLine("frame {0}", ++frames);

                status =g_FirstAgent.btexec().Result;
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

            g_MyFileManager = null;
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
