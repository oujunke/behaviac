using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Behaviac.Design;
using Behaviac.Design.Nodes;
using PluginBehaviac.DataExporters;
using PluginBehaviac.NodeExporters;
using ExpandPlugin.Nodes;

namespace ExpandPlugin.NodeExporters
{
    public class ExpandNodeCppExporter : NodeCppExporter
    {
        protected override bool ShouldGenerateClass(Node node)
        {
            return true;
        }

        protected override void GenerateConstructor(Node node, StringWriter stream, string indent, string className)
        {
            base.GenerateConstructor(node, stream, indent, className);
            AddVarNode wait = node as AddVarNode;
            Debug.Check(wait != null);

            stream.WriteLine("{0}\t\t\tm_ignoreTimeScale = {1};", indent, wait.IgnoreTimeScale ? "true" : "false");
        }
        protected override void GenerateMethod(Node node, StringWriter stream, string indent)
        {
            base.GenerateMethod(node, stream, indent);

            AddVarNode wait = node as AddVarNode;
            Debug.Check(wait != null);

            if (wait.Time != null)
            {
                stream.WriteLine("{0}\t\tvirtual float GetTime(Agent* pAgent) const", indent);
                stream.WriteLine("{0}\t\t{{", indent);
                stream.WriteLine("{0}\t\t\tBEHAVIAC_UNUSED_VAR(pAgent);", indent);
               var retStr = VariableCppExporter.GenerateCode(node, wait.Time, false, stream, indent + "\t\t\t", string.Empty, string.Empty, string.Empty);
                stream.WriteLine("{0}\t\t\treturn {1};", indent, retStr);
                stream.WriteLine("{0}\t\t}}", indent);
            }
        }
    }
}
