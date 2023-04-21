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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace behaviac
{
    public class Assignment : BehaviorNode
    {
        protected override async Task load(int version, string agentType, List<property_t> properties)
        {
            await base.load(version, agentType, properties);

            for (int i = 0; i < properties.Count; ++i)
            {
                property_t p = properties[i];

                if (p.name == "CastRight")
                {
                    this.m_bCast = (p.value == "true");
                }
                else if (p.name == "Opl")
                {
                    this.m_opl = AgentMeta.ParseProperty(p.value, Workspace);
                }
                else if (p.name == "Opr")
                {
                    int pParenthesis = p.value.IndexOf('(');

                    if (pParenthesis == -1)
                    {
                        this.m_opr = AgentMeta.ParseProperty(p.value, Workspace);
                    }
                    else
                    {
                        this.m_opr = AgentMeta.ParseMethod(p.value, Workspace);
                    }
                }
            }
        }

        public override bool IsValid(Agent pAgent, BehaviorTask pTask)
        {
            if (!(pTask.GetNode() is Assignment))
            {
                return false;
            }

            return base.IsValid(pAgent, pTask);
        }

        protected override BehaviorTask createTask()
        {
            return new AssignmentTask(Workspace);
        }

        protected IInstanceMember m_opl;
        protected IInstanceMember m_opr;
        protected bool m_bCast = false;

        public Assignment(Workspace workspace) : base(workspace)
        {
        }

        private class AssignmentTask : LeafTask
        {
            public AssignmentTask(Workspace workspace) : base(workspace)
            {
            }

            public override void copyto(BehaviorTask target)
            {
                base.copyto(target);
            }

            public override void save(ISerializableNode node)
            {
                base.save(node);
            }

            public override void load(ISerializableNode node)
            {
                base.load(node);
            }

            protected override Task<bool> onenter(Agent pAgent)
            {
                return Task.FromResult(true);
            }

            protected override void onexit(Agent pAgent, EBTStatus s)
            {
            }

            protected override async Task<EBTStatus> update(Agent pAgent, EBTStatus childStatus)
            {
                Debugs.Check(childStatus == EBTStatus.BT_RUNNING);

                Debugs.Check(this.GetNode() is Assignment);
                Assignment pAssignmentNode = (Assignment)(this.GetNode());

                EBTStatus result = EBTStatus.BT_SUCCESS;

                if (pAssignmentNode.m_opl != null)
                {
                    if (pAssignmentNode.m_bCast)
                    {
                        await pAssignmentNode.m_opl.SetValueAs(pAgent, pAssignmentNode.m_opr);
                    }
                    else
                    {
                        await pAssignmentNode.m_opl.SetValue(pAgent, pAssignmentNode.m_opr);
                    }
                }
                else
                {
                    result = await pAssignmentNode.update_impl(pAgent, childStatus);
                }

                return result;
            }
        }
    }
}
