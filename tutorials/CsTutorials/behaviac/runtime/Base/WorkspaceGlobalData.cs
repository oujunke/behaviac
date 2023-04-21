using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static behaviac.Agent;

namespace behaviac.runtime.Base
{
    public class WorkspaceGlobalData
    {
        internal Dictionary<string, AgentName_t> AgentNames { private set; get; } = new Dictionary<string, AgentName_t>();
    }
}
