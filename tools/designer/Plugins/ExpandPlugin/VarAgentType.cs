using Behaviac.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpandPlugin
{
    public class VarAgentType : AgentType
    {
        public VarAgentType(bool isCustomized, bool isImplemented, string name, string oldName, AgentType baseAgent, string exportLocation, string disp, string desc) : base(isCustomized, isImplemented, name, oldName, baseAgent, exportLocation, disp, desc)
        {
        }
    }
}
