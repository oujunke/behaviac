using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CodeBehaviorBase.BModels
{
    public class BAgentType
    {
        public string Type { set; get; }
        public bool IsCustomized { set; get; }
        public bool IsImplemented { set; get; }
        public bool IsRefType { set; get; }
        public string DisplayName { set; get; }
        public string Desc { set; get; }
        public string Base { set; get; }
    }
}
