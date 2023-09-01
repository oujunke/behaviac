/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Tencent is pleased to support the open source community by making behaviac available.
//
// Copyright (C) 2015-2017 THL A29 Limited, a Tencent company. All rights reserved.
//
// Licensed under the BSD 3-Clause License (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at http://opensource.org/licenses/BSD-3-Clause
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Behaviac.Design;
using Behaviac.Design.Nodes;

namespace PluginBehaviac.NodeExporters
{
    public class TasksCsExporter : NodeCsExporter
    {
        protected override bool ShouldGenerateClass(Node node)
        {
            Tasks task = node as Tasks;
            return (task != null);
        }

        protected override void GenerateConstructor(Node node, StringWriter stream, string indent, string className)
        {
            base.GenerateConstructor(node, stream, indent, className);

            Tasks task = node as Tasks;

            if (task == null)
            {
                return;
            }

            if (task.Prototype != null)
            {
                string method = task.Prototype.GetExportValue();
                method = method.Replace("\"", "\\\"");
                stream.WriteLine("{0}\t\t\tthis.m_task = AgentMeta.ParseMethod(\"{1}\");", indent, method);
                stream.WriteLine("{0}\t\t\tDebug.Check(this.m_task != null);", indent);
                stream.WriteLine("{0}\t\t\tthis.m_bHTN = {1};", indent, task.IsHTN ? "true" : "false");
            }
        }
    }
}
