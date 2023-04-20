using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace behaviac
{
    public class IntervalFrames : Sequence
    {
        protected override void load(int version, string agentType, List<property_t> properties)
        {
            base.load(version, agentType, properties);

            for (int i = 0; i < properties.Count; ++i)
            {
                property_t p = properties[i];

                if (p.name == "Count")
                {
                    this.m_count = AgentMeta.ParseProperty(p.value);
                }else if(p.name== "Front")
                {
                    this.m_front = AgentMeta.ParseProperty(p.value);
                }
            }
        }
        protected virtual int GetCount(Agent pAgent)
        {
            if (this.m_count != null)
            {
                Debug.Check(this.m_count is CInstanceMember<int>);
                int count = ((CInstanceMember<int>)this.m_count).GetValue(pAgent);

                return count;
            }

            return 0;
        }
        protected virtual bool GetFront(Agent pAgent)
        {
            if (this.m_front != null)
            {
                Debug.Check(this.m_front is CInstanceMember<int>);
                var front = ((CInstanceMember<bool>)this.m_front).GetValue(pAgent);

                return front;
            }

            return false;
        }
        protected override BehaviorTask createTask()
        {
            return new IntervalFramesTask();
        }
        protected IInstanceMember m_count;
        protected IInstanceMember m_front;
        public class IntervalFramesTask : SequenceTask
        {
            public override void copyto(BehaviorTask target)
            {
                base.copyto(target);

                Debug.Check(target is IntervalFramesTask);
                IntervalFramesTask ttask = (IntervalFramesTask)target;

                ttask.m_n = this.m_n;
                ttask.m_f = this.m_f;
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
            protected override bool onenter(Agent pAgent)
            {
                base.onenter(pAgent);

                int count = this.GetCount(pAgent);

                if (count == 0)
                {
                    return false;
                }

                this.m_n = count;
                this.m_f = this.GetFront(pAgent);
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
            protected override EBTStatus update(Agent pAgent, EBTStatus childStatus)
            {
                if (c_n++ < m_n)
                {
                    return EBTStatus.BT_SUCCESS;
                }
                c_n = 0;
                return base.update(pAgent, childStatus);
            }
            protected override EBTStatus update_current(Agent pAgent, EBTStatus childStatus)
            {
                return base.update_current(pAgent, childStatus);
            }
            public int GetCount(Agent pAgent)
            {
                Debug.Check(this.GetNode() is IntervalFrames);
                IntervalFrames pDecoratorCountNode = (IntervalFrames)(this.GetNode());

                return pDecoratorCountNode != null ? pDecoratorCountNode.GetCount(pAgent) : 0;
            }
            public bool GetFront(Agent pAgent)
            {
                Debug.Check(this.GetNode() is IntervalFrames);
                IntervalFrames pDecoratorCountNode = (IntervalFrames)(this.GetNode());

                return pDecoratorCountNode != null ? pDecoratorCountNode.GetFront(pAgent) : false;
            }
            protected int m_n;
            protected bool m_f;
            protected int c_n;
        }
    }
}
