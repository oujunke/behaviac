using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBehaviorBase.Behaviacs
{
    public abstract class NodeList : Node
    {
        public Node Condition { set; get; }
        public List<Node> GenericChildren { get; set; }
        public NodeList(Node condition, List<Node> genericChildren)
        {
            Condition = condition;
            GenericChildren = genericChildren;
        }
        public NodeList(params Node[] genericChildrens)
        {
            Condition = condition;
            GenericChildren = genericChildren;
        }
    }
}
