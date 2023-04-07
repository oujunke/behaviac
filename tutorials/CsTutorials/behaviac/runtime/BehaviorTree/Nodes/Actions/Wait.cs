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

namespace behaviac
{
    public class Wait : BehaviorNode
    {
        protected override void load(int version, string agentType, List<property_t> properties)
        {
            base.load(version, agentType, properties);

            for (int i = 0; i < properties.Count; ++i)
            {
                property_t p = properties[i];

                if (p.name == "Time")
                {
                    int pParenthesis = p.value.IndexOf('(');

                    if (pParenthesis == -1)
                    {
                        this.m_time = AgentMeta.ParseProperty(p.value, Workspace);
                    }
                    else
                    {
                        this.m_time = AgentMeta.ParseMethod(p.value, Workspace);
                    }
                }
            }
        }

        protected virtual double GetTime(Agent pAgent)
        {
            double time = 0;

            if (this.m_time != null)
            {
                if (this.m_time is CInstanceMember<double>)
                {
                    time = ((CInstanceMember<double>)this.m_time).GetValue(pAgent);
                }
                else if (this.m_time is CInstanceMember<float>)
                {
                    time = ((CInstanceMember<float>)this.m_time).GetValue(pAgent);
                }
                else if (this.m_time is CInstanceMember<int>)
                {
                    time = ((CInstanceMember<int>)this.m_time).GetValue(pAgent);
                }
            }

            return time;
        }

        protected virtual int GetIntTime(Agent pAgent)
        {
            int time = 0;

            if (this.m_time != null)
            {
                if (this.m_time is CInstanceMember<int>)
                {
                    time = ((CInstanceMember<int>)this.m_time).GetValue(pAgent);
                }
            }

            return time;
        }

        protected IInstanceMember m_time;

        protected override BehaviorTask createTask()
        {
            WaitTask pTask = new WaitTask();

            return pTask;
        }

        private class WaitTask : LeafTask
        {
            private double m_start = 0;
            private double m_time = 0;
            private long m_intStart = 0;
            private int m_intTime = 0;

            public override void copyto(BehaviorTask target)
            {
                base.copyto(target);

                Debugs.Check(target is WaitTask);
                WaitTask ttask = (WaitTask)target;

                ttask.m_start = this.m_start;
                ttask.m_time = this.m_time;

                ttask.m_intStart = this.m_intStart;
                ttask.m_intTime = this.m_intTime;
            }

            public override void save(ISerializableNode node)
            {
                base.save(node);

                CSerializationID startId = new CSerializationID("start");
                node.setAttr(startId, this.m_start);

                CSerializationID timeId = new CSerializationID("time");
                node.setAttr(timeId, this.m_time);

                CSerializationID intStartId = new CSerializationID("intstart");
                node.setAttr(intStartId, this.m_intStart);

                CSerializationID intTimeId = new CSerializationID("inttime");
                node.setAttr(intTimeId, this.m_intTime);
            }

            public override void load(ISerializableNode node)
            {
                base.load(node);
            }

            private double GetTime(Agent pAgent)
            {
                Wait pWaitNode = this.GetNode() as Wait;

                return pWaitNode != null ? pWaitNode.GetTime(pAgent) : 0;
            }

            private int GetIntTime(Agent pAgent)
            {
                Wait pWaitNode = this.GetNode() as Wait;

                return pWaitNode != null ? pWaitNode.GetIntTime(pAgent) : 0;
            }

            protected override bool onenter(Agent pAgent)
            {
                if (Workspace.UseIntValue)
                {
                    this.m_intStart = Workspace.IntValueSinceStartup;
                    this.m_intTime = this.GetIntTime(pAgent);

                    return (this.m_intTime >= 0);
                }
                else
                {
                    this.m_start = Workspace.DoubleValueSinceStartup;
                    this.m_time = this.GetTime(pAgent);

                    return (this.m_time >= 0);
                }
            }

            protected override void onexit(Agent pAgent, EBTStatus s)
            {
            }

            protected override Task<EBTStatus> update(Agent pAgent, EBTStatus childStatus)
            {
                Debugs.Check(childStatus == EBTStatus.BT_RUNNING);

                if (Workspace.UseIntValue)
                {
                    if (Workspace.IntValueSinceStartup - this.m_intStart >= this.m_intTime)
                    {
                        return EBTStatus.BT_SUCCESS;
                    }
                }
                else
                {
                    if (Workspace.DoubleValueSinceStartup - this.m_start >= this.m_time)
                    {
                        return EBTStatus.BT_SUCCESS;
                    }
                }

                return EBTStatus.BT_RUNNING;
            }
        }
    }
}
