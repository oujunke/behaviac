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
    public class DecoratorWeight : DecoratorNode
    {
        protected override  async Task  load(int version, string agentType, List<property_t> properties)
        {
            await base.load(version, agentType, properties);

            for (int i = 0; i < properties.Count; ++i)
            {
                property_t p = properties[i];

                if (p.name == "Weight")
                {
                    this.m_weight = AgentMeta.ParseProperty(p.value,Workspace);
                }
            }
        }

        protected virtual async Task<int>  GetWeight(behaviac.Agent pAgent)
        {
            if (this.m_weight != null)
            {
                Debugs.Check(this.m_weight is CInstanceMember<int>);
                return await ((CInstanceMember<int>)this.m_weight).GetValue(pAgent);
            }

            return 0;
        }

        public override bool IsManagingChildrenAsSubTrees()
        {
            return false;
        }

        protected override BehaviorTask createTask()
        {
            DecoratorWeightTask pTask = new DecoratorWeightTask(Workspace);

            return pTask;
        }

        protected IInstanceMember m_weight;

        public DecoratorWeight(Workspace workspace) : base(workspace)
        {
        }

        public class DecoratorWeightTask : DecoratorTask
        {
            public DecoratorWeightTask(Workspace workspace) : base(workspace)
            {
            }

            public async Task<int> GetWeight(Agent pAgent)
            {
                Debugs.Check(this.GetNode() is DecoratorWeight);
                DecoratorWeight pNode = (DecoratorWeight)(this.GetNode());

                return pNode != null ?await pNode.GetWeight(pAgent) : 0;
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

            protected override EBTStatus decorate(EBTStatus status)
            {
                return status;
            }
        }
    }
}
