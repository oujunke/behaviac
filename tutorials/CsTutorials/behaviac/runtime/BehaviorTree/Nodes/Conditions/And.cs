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
    public class And : ConditionBase
    {
        public And(Workspace workspace):base(workspace)
        {
        }

        //~And()
        //{
        //}

        protected override  async Task  load(int version, string agentType, List<property_t> properties)
        {
            await base.load(version, agentType, properties);
        }

        public override bool IsValid(Agent pAgent, BehaviorTask pTask)
        {
            if (!(pTask.GetNode() is And))
            {
                return false;
            }

            return base.IsValid(pAgent, pTask);
        }

        public override async Task<bool> Evaluate(Agent pAgent)
        {
            bool ret = true;

            for (int i = 0; i < this.m_children.Count; ++i)
            {
                BehaviorNode c = this.m_children[i];
                ret =await c.Evaluate(pAgent);

                if (!ret)
                {
                    break;
                }
            }

            return ret;
        }

        protected override BehaviorTask createTask()
        {
            AndTask pTask = new AndTask(Workspace);

            return pTask;
        }
    }

    // ============================================================================
    internal class AndTask : Sequence.SequenceTask
    {
        public AndTask(Workspace workspace) : base(workspace)
        {
        }

        //~AndTask()
        //{
        //}

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

        protected override async Task<EBTStatus> update(Agent pAgent, EBTStatus childStatus)
        {
            Debugs.Check(childStatus == EBTStatus.BT_RUNNING);
            //Debug.Check(this.m_children.Count == 2);

            for (int i = 0; i < this.m_children.Count; ++i)
            {
                BehaviorTask pBehavior = this.m_children[i];
                EBTStatus s =await pBehavior.exec(pAgent);

                // If the child fails, fails
                if (s == EBTStatus.BT_FAILURE)
                {
                    return s;
                }

                Debugs.Check(s == EBTStatus.BT_SUCCESS);
            }

            return EBTStatus.BT_SUCCESS;
        }
    }
}
