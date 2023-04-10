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
    public class DecoratorFrames : DecoratorNode
    {

        public DecoratorFrames(Workspace workspace):base(workspace)
        {
        }
        protected override  async Task  load(int version, string agentType, List<property_t> properties)
        {
            await base.load(version, agentType, properties);

            for (int i = 0; i < properties.Count; ++i)
            {
                property_t p = properties[i];

                if (p.name == "Frames")
                {
                    int pParenthesis = p.value.IndexOf('(');

                    if (pParenthesis == -1)
                    {
                        this.m_frames = AgentMeta.ParseProperty(p.value,Workspace);
                    }
                    else
                    {
                        this.m_frames = AgentMeta.ParseMethod(p.value, Workspace);
                    }
                }
            }
        }

        protected virtual async Task<int> GetFrames(Agent pAgent)
        {
            if (this.m_frames != null)
            {
                Debugs.Check(this.m_frames is CInstanceMember<int>);
                return await((CInstanceMember<int>)this.m_frames).GetValue(pAgent);
            }

            return 0;
        }

        protected override BehaviorTask createTask()
        {
            DecoratorFramesTask pTask = new DecoratorFramesTask(Workspace);

            return pTask;
        }

        protected IInstanceMember m_frames;

        private class DecoratorFramesTask : DecoratorTask
        {
            public override void copyto(BehaviorTask target)
            {
                base.copyto(target);

                Debugs.Check(target is DecoratorFramesTask);
                DecoratorFramesTask ttask = (DecoratorFramesTask)target;

                ttask.m_start = this.m_start;
                ttask.m_frames = this.m_frames;
            }

            public override void save(ISerializableNode node)
            {
                base.save(node);

                CSerializationID startId = new CSerializationID("start");
                node.setAttr(startId, this.m_start);

                CSerializationID framesId = new CSerializationID("frames");
                node.setAttr(framesId, this.m_frames);
            }

            public override void load(ISerializableNode node)
            {
                base.load(node);
            }

            protected override async Task<bool> onenter(Agent pAgent)
            {
                await base.onenter(pAgent);

                this.m_start = Workspace.FrameSinceStartup;
                this.m_frames =await this.GetFrames(pAgent);

                return this.m_frames >= 0;
            }

            protected override EBTStatus decorate(EBTStatus status)
            {
                if (Workspace.FrameSinceStartup - this.m_start + 1 >= this.m_frames)
                {
                    return EBTStatus.BT_SUCCESS;
                }

                return EBTStatus.BT_RUNNING;
            }

            private async Task<int> GetFrames(Agent pAgent)
            {
                Debugs.Check(this.GetNode() is DecoratorFrames);
                DecoratorFrames pNode = (DecoratorFrames)(this.GetNode());

                return pNode != null ?await pNode.GetFrames(pAgent) : 0;
            }

            private int m_start = 0;
            private int m_frames = 0;

            public DecoratorFramesTask(Workspace workspace) : base(workspace)
            {
            }
        }
    }
}
