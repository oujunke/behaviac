using behaviac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tutorial_12
{
    internal class FirstAgentImp : IFirstAgentImp
    {
        public Workspace Workspaces;
        public FirstAgentImp(Workspace workspace)
        {
            Workspaces = workspace;
        }
        public Task<EBTStatus> Say(string value, bool isLatent)
        {
            if (isLatent && Workspaces.FrameSinceStartup < 3)
            {
                Console.WriteLine("\n{0} [Running]\n", value);

                return Task.FromResult(behaviac.EBTStatus.BT_RUNNING);
            }

            Console.WriteLine("\n{0} [Success]\n", value);

            return Task.FromResult(behaviac.EBTStatus.BT_SUCCESS);
        }
    }
}
