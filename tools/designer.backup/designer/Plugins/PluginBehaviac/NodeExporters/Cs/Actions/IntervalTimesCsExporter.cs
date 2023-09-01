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
    internal class IntervalTimesCsExporter : NodeCsExporter
    {
        protected override bool ShouldGenerateClass(Node node)
        {
            return node is IntervalTimes;
        }

        protected override void GenerateConstructor(Node node, StringWriter stream, string indent, string className)
        {
            base.GenerateConstructor(node, stream, indent, className);

            IntervalTimes intervalFrames = node as IntervalTimes;

            if (intervalFrames == null)
            {
                return;
            }

            if (intervalFrames.Time != null)
            {
                RightValueCsExporter.GenerateClassConstructor(node, intervalFrames.Time, stream, indent, "Time");
            }
            if (intervalFrames.Front != null)
            {
                RightValueCsExporter.GenerateClassConstructor(node, intervalFrames.Front, stream, indent, "Front");
            }
        }

        protected override void GenerateMember(Node node, StringWriter stream, string indent)
        {
            base.GenerateMember(node, stream, indent);

            IntervalTimes intervalFrames = node as IntervalTimes;

            if (intervalFrames == null)
            {
                return;
            }

            if (intervalFrames.Time != null)
            {
                RightValueCsExporter.GenerateClassMember(intervalFrames.Time, stream, indent, "Time");
            }
            if (intervalFrames.Front != null)
            {
                RightValueCsExporter.GenerateClassMember(intervalFrames.Front, stream, indent, "Front");
            }
        }

        protected override void GenerateMethod(Node node, StringWriter stream, string indent)
        {
            base.GenerateMethod(node, stream, indent);

            IntervalTimes intervalFrames = node as IntervalTimes;

            if (intervalFrames == null)
            {
                return;
            }

            if (intervalFrames.Time != null)
            {
                stream.WriteLine("{0}\t\tprotected override Task<int> GetTime(Agent pAgent)", indent);
                stream.WriteLine("{0}\t\t{{", indent);

                string retStr = RightValueCsExporter.GenerateCode(node, intervalFrames.Time, stream, indent + "\t\t\t", string.Empty, string.Empty, "Time");

                if (!intervalFrames.Time.IsPublic && (intervalFrames.Time.IsMethod || intervalFrames.Time.Var != null && intervalFrames.Time.Var.IsProperty))
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
