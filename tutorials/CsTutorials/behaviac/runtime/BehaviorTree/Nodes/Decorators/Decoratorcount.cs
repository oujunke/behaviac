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
    public abstract class DecoratorCount : DecoratorNode
    {
        protected override  async Task  load(int version, string agentType, List<property_t> properties)
        {
            await base.load(version, agentType, properties);

            for (int i = 0; i < properties.Count; ++i)
            {
                property_t p = properties[i];

                if (p.name == "Count")
                {
                    this.m_count = AgentMeta.ParseProperty(p.value, Workspace);
                }
            }
        }

        protected virtual async Task<int> GetCount(Agent pAgent)
        {
            if (this.m_count != null)
            {
                Debugs.Check(this.m_count is CInstanceMember<int>);
                int count = await ((CInstanceMember<int>)this.m_count).GetValue(pAgent);

                return count;
            }

            return 0;
        }

        protected IInstanceMember m_count;

        protected DecoratorCount(Workspace workspace) : base(workspace)
        {
        }

        protected abstract class DecoratorCountTask : DecoratorTask
        {
            public override void copyto(BehaviorTask target)
            {
                base.copyto(target);

                Debugs.Check(target is DecoratorCountTask);
                DecoratorCountTask ttask = (DecoratorCountTask)target;

                ttask.m_n = this.m_n;
            }

            public override void save(ISerializableNode node)
            {
                base.save(node);

                CSerializationID countId = new CSerializationID("count");
                node.setAttr(countId, this.m_n);
            }

            public override void load(ISerializableNode node)
            {
                base.load(node);
            }

            protected override async Task<bool> onenter(Agent pAgent)
            {
                await base.onenter(pAgent);

                int count =await this.GetCount(pAgent);

                if (count == 0)
                {
                    return false;
                }

                this.m_n = count;

                return true;
            }

            public async Task<int> GetCount(Agent pAgent)
            {
                Debugs.Check(this.GetNode() is DecoratorCount);
                DecoratorCount pDecoratorCountNode = (DecoratorCount)(this.GetNode());

                return pDecoratorCountNode != null ?await pDecoratorCountNode.GetCount(pAgent) : 0;
            }

            protected int m_n;

            protected DecoratorCountTask(Workspace workspace) : base(workspace)
            {
            }
        }
    }
}
