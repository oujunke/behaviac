using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBehaviorBase.BModels
{
    public class BBehavior
    {
        public class Comment
        {
            public string Background { set; get; }
            public string Text { set; get; }
        }
        public class DescriptorRefs
        {
            public string Value { set; get; }
        }
        public class Nodes
        {
            public string Class { set; get; }
            public bool Enable { set; get; }
            public bool HasOwnPrefabData { set; get; }
            public int Id { set; get; }
            public string AgentType { set; get; }
            public string Domains { set; get; }
            public string PrefabName { set; get; }
            public int PrefabNodeId { set; get; }
            public Comment Comment { set; get; }
            public DescriptorRefs DescriptorRefs { set; get; }
            public List<Connector> Connectors { set; get; }
        }
        public class Connector
        {
            public string Identifier { set; get; }
            public List<Nodes> Node { set; get; }
        }
        public string Version { set; get; }
        public bool NoError { set; get; }
        public Nodes Node { set; get; }
    }
}
