using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace behaviac
{
    public class IntervalFrames : Sequence
    {
        protected override async Task load(int version, string agentType, List<property_t> properties)
        {
            await base.load(version, agentType, properties);

            for (int i = 0; i < properties.Count; ++i)
            {
                property_t p = properties[i];

                if (p.name == "Count")
                {
                    this.m_count = AgentMeta.ParseProperty(p.value, Workspace);
                }
                else if (p.name == "Front")
                {
                    this.m_front = AgentMeta.ParseProperty(p.value, Workspace);
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
        protected virtual async Task<bool> GetFront(Agent pAgent)
        {
            if (this.m_front != null)
            {
                Debugs.Check(this.m_front is CInstanceMember<int>);
                var front = await ((CInstanceMember<bool>)this.m_front).GetValue(pAgent);

                return front;
            }

            return false;
        }
        protected override BehaviorTask createTask()
        {
            return new IntervalFramesTask(Workspace);
        }
        protected IInstanceMember m_count;
        protected IInstanceMember m_front;

        public IntervalFrames(Workspace workspace) : base(workspace)
        {
        }

        public class IntervalFramesTask : SequenceTask
        {
            public override void copyto(BehaviorTask target)
            {
                base.copyto(target);

                Debugs.Check(target is IntervalFramesTask);
                IntervalFramesTask task = (IntervalFramesTask)target;

                task.m_n = this.m_n;
                task.m_f = this.m_f;
            }

            public override void save(ISerializableNode node)
            {
                base.save(node);

                CSerializationID countId = new CSerializationID("count");
                node.setAttr(countId, this.m_n);
                CSerializationID frontId = new CSerializationID("front");
                node.setAttr(frontId, this.m_f);
            }

            public override void load(ISerializableNode node)
            {
                base.load(node);
            }
            public override void Init(BehaviorNode node)
            {
                base.Init(node);
                c_n = -1;
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
                this.m_f =await this.GetFront(pAgent);
                if (c_n == -1)
                {
                    if (m_f)
                    {
                        c_n = 0;
                    }
                    else
                    {
                        c_n = count;
                    }
                }
                return true;
            }
            protected override async Task<EBTStatus> update(Agent pAgent, EBTStatus childStatus)
            {
                //todo:修改
                if(m_status == EBTStatus.BT_RUNNING)
                {
                    return await base.update(pAgent, childStatus);
                }
                if (++c_n < m_n)
                {
                    return EBTStatus.BT_SUCCESS;
                }
                c_n = 0;
                return await base.update(pAgent, childStatus);
            }
            protected override Task<EBTStatus> update_current(Agent pAgent, EBTStatus childStatus)
            {
                return base.update_current(pAgent, childStatus);
            }
            public async Task<int> GetCount(Agent pAgent)
            {
                Debugs.Check(this.GetNode() is IntervalFrames);
                IntervalFrames pDecoratorCountNode = (IntervalFrames)(this.GetNode());

                return pDecoratorCountNode != null ?await pDecoratorCountNode.GetCount(pAgent) : 0;
            }
            public async Task<bool>  GetFront(Agent pAgent)
            {
                Debugs.Check(this.GetNode() is IntervalFrames);
                IntervalFrames pDecoratorCountNode = (IntervalFrames)(this.GetNode());

                return pDecoratorCountNode != null ?await pDecoratorCountNode.GetFront(pAgent) : false;
            }
            protected int m_n;
            protected bool m_f;
            protected int c_n;

            public IntervalFramesTask(Workspace workspace) : base(workspace)
            {
            }
        }
    }
}
