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
    /**
    the Selector runs the children from the first sequentially until the child which returns success.
    for SelectorStochastic, the children are not sequentially selected, instead it is selected stochasticly.

    for example: the children might be [0, 1, 2, 3, 4]
    Selector always select the child by the order of 0, 1, 2, 3, 4
    while SelectorStochastic, sometime, it is [4, 2, 0, 1, 3], sometime, it is [2, 3, 0, 4, 1], etc.
    */

    public class SelectorStochastic : CompositeStochastic
    {
        public SelectorStochastic(Workspace workspace) : base(workspace)
        {
        }

        protected override async Task load(int version, string agentType, List<property_t> properties)
        {
            await base.load(version, agentType, properties);
        }

        public override bool IsValid(Agent pAgent, BehaviorTask pTask)
        {
            if (!(pTask.GetNode() is SelectorStochastic))
            {
                return false;
            }

            return base.IsValid(pAgent, pTask);
        }

        protected override BehaviorTask createTask()
        {
            SelectorStochasticTask pTask = new SelectorStochasticTask(Workspace);

            return pTask;
        }

        private class SelectorStochasticTask : CompositeStochasticTask
        {
            public SelectorStochasticTask(Workspace workspace) : base(workspace)
            {
            }

            protected override void addChild(BehaviorTask pBehavior)
            {
                base.addChild(pBehavior);
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

            protected override async Task<bool> onenter(Agent pAgent)
            {
                await base.onenter(pAgent);

                return true;
            }

            protected override void onexit(Agent pAgent, EBTStatus s)
            {
                base.onexit(pAgent, s);
                base.onexit(pAgent, s);
            }

            protected override async Task<EBTStatus> update(Agent pAgent, EBTStatus childStatus)
            {
                EBTStatus s = childStatus;
                Debugs.Check(this.m_activeChildIndex < this.m_children.Count);

                SelectorStochastic node = this.m_node as SelectorStochastic;

                // Keep going until a child behavior says its running.
                for (; ; )
                {
                    if (s == EBTStatus.BT_RUNNING)
                    {
                        int childIndex = this.m_set[this.m_activeChildIndex];
                        BehaviorTask pBehavior = this.m_children[childIndex];

                        if (await node.CheckIfInterrupted(pAgent))
                        {
                            return EBTStatus.BT_FAILURE;
                        }

                        s = await pBehavior.exec(pAgent);
                    }

                    // If the child succeeds, or keeps running, do the same.
                    if (s != EBTStatus.BT_FAILURE)
                    {
                        return s;
                    }

                    // Hit the end of the array, job done!
                    ++this.m_activeChildIndex;

                    if (this.m_activeChildIndex >= this.m_children.Count)
                    {
                        return EBTStatus.BT_FAILURE;
                    }

                    s = EBTStatus.BT_RUNNING;
                }
            }
        }
    }
}
