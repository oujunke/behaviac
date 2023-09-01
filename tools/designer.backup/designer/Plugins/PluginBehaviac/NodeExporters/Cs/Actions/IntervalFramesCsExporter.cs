using Behaviac.Design.Nodes;
using PluginBehaviac.DataExporters;
using PluginBehaviac.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginBehaviac.NodeExporters
{
    internal class IntervalFramesCsExporter : NodeCsExporter
    {
        protected override bool ShouldGenerateClass(Node node)
        {
            return node is IntervalFrames;
        }

        protected override void GenerateConstructor(Node node, StringWriter stream, string indent, string className)
        {
            base.GenerateConstructor(node, stream, indent, className);

            IntervalFrames intervalFrames = node as IntervalFrames;

            if (intervalFrames == null)
            {
                return;
            }

            if (intervalFrames.Count != null)
            {
                RightValueCsExporter.GenerateClassConstructor(node, intervalFrames.Count, stream, indent, "Count");
            }
            if (intervalFrames.Front != null)
            {
                RightValueCsExporter.GenerateClassConstructor(node, intervalFrames.Front, stream, indent, "Front");
            }
        }

        protected override void GenerateMember(Node node, StringWriter stream, string indent)
        {
            base.GenerateMember(node, stream, indent);

            IntervalFrames intervalFrames = node as IntervalFrames;

            if (intervalFrames == null)
            {
                return;
            }

            if (intervalFrames.Count != null)
            {
                RightValueCsExporter.GenerateClassMember(intervalFrames.Count, stream, indent, "Frames");
            }
            if (intervalFrames.Front != null)
            {
                RightValueCsExporter.GenerateClassMember(intervalFrames.Front, stream, indent, "Front");
            }
        }

        protected override void GenerateMethod(Node node, StringWriter stream, string indent)
        {
            base.GenerateMethod(node, stream, indent);

            IntervalFrames intervalFrames = node as IntervalFrames;

            if (intervalFrames == null)
            {
                return;
            }

            if (intervalFrames.Count != null)
            {
                stream.WriteLine("{0}\t\tprotected override Task<int> GetCount(Agent pAgent)", indent);
                stream.WriteLine("{0}\t\t{{", indent);

                string retStr = RightValueCsExporter.GenerateCode(node, intervalFrames.Count, stream, indent + "\t\t\t", string.Empty, string.Empty, "Count");

                if (!intervalFrames.Count.IsPublic && (intervalFrames.Count.IsMethod || intervalFrames.Count.Var != null && intervalFrames.Count.Var.IsProperty))
                {
                    retStr = string.Format("Convert.ToInt32({0})", retStr);
                }

                stream.WriteLine("{0}\t\t\treturn {1};", indent, retStr);
                stream.WriteLine("{0}\t\t}}", indent);
            }
            if (intervalFrames.Front != null)
            {
                stream.WriteLine("{0}\t\tprotected override Task<bool> GetFront(Agent pAgent)", indent);
                stream.WriteLine("{0}\t\t{{", indent);

                string retStr = RightValueCsExporter.GenerateCode(node, intervalFrames.Front, stream, indent + "\t\t\t", string.Empty, string.Empty, "Front");

                if (!intervalFrames.Front.IsPublic && (intervalFrames.Front.IsMethod || intervalFrames.Front.Var != null && intervalFrames.Front.Var.IsProperty))
                {
                    retStr = string.Format("Convert.ToBoolean({0})", retStr);
                }

                stream.WriteLine("{0}\t\t\treturn {1};", indent, retStr);
                stream.WriteLine("{0}\t\t}}", indent);
            }
        }
    }
}
