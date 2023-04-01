using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBehaviorBase.Behaviacs
{
    public class And:NodeList
    {
        public override string? CheckError()
        {
            if (GenericChildren.Count != 2||GenericChildren.Count(n=>n==null)>0)
            {
                return $"两个操作数错误";
            }
            return null;
        }
    }
}
