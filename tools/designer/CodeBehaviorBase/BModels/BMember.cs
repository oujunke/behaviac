using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBehaviorBase.BModels
{
    public class BMember
    {
        public string Name { set; get; }
        public bool Public { set; get; }
        public bool Static { set; get; }
        public bool Readonly { set; get; }
        public string DisplayName { set; get; }
        public string Desc { set; get; }
        public string Class { set; get; }
        public string TypeFullName { set; get; }
        public string Type { set; get; }
        public string DefaultValue { set; get; }
    }
}
