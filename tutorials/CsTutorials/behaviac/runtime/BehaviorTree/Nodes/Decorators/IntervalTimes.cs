using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace behaviac
{
    public class IntervalTimes : Sequence
    {
        protected override void load(int version, string agentType, List<property_t> properties)
        {
            base.load(version, agentType, properties);

            for (int i = 0; i < properties.Count; ++i)
            {
                property_t p = properties[i];

                if (p.name == "Time")
                {
                    this.m_time = AgentMeta.ParseProperty(p.value);
                }else if(p.name== "Front")
                {
                    this.m_front = AgentMeta.ParseProperty(p.value);
                }
            }
        }
        protected virtual int GetTime(Agent pAgent)
        {
            if (this.m_time != null)
            {
                Debug.Check(this.m_time is CInstanceMember<int>);
                int count = ((CInstanceMember<int>)this.m_time).GetValue(pAgent);

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
            return new IntervalTimesTask();
        }
        protected IInstanceMember m_time;
        protected IInstanceMember m_front;
        public class IntervalTimesTask : SequenceTask
        {
            public override void copyto(BehaviorTask target)
            {
                base.copyto(target);

                Debug.Check(target is IntervalTimesTask);
                IntervalTimesTask ttask = (IntervalTimesTask)target;

                ttask.m_n = this.m_n;
                ttask.m_f = this.m_f;
            }

            public override void save(ISerializableNode node)
            {
                base.save(node);

                CSerializationID countId = new CSerializationID("time");
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
                c_t = DateTime.MinValue;
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
                if (c_t == DateTime.MinValue)
                {
                    if (m_f)
                    {
                        c_t = DateTime.Now;
                    }
                    else
                    {
                        c_t = DateTime.Now.AddSeconds(-m_n);
                    }
                }
                return true;
            }
            protected override EBTStatus update(Agent pAgent, EBTStatus childStatus)
            {
                if ((DateTime.Now-c_t).TotalSeconds < m_n)
                {
                    return EBTStatus.BT_SUCCESS;
                }
                c_t = DateTime.Now;
                return base.update(pAgent, childStatus);
            }
            protected override EBTStatus update_current(Agent pAgent, EBTStatus childStatus)
            {
                return base.update_current(pAgent, childStatus);
            }
            public int GetCount(Agent pAgent)
            {
                Debug.Check(this.GetNode() is IntervalTimes);
                IntervalTimes pDecoratorCountNode = (IntervalTimes)(this.GetNode());

                return pDecoratorCountNode != null ? pDecoratorCountNode.GetTime(pAgent) : 0;
            }
            public bool GetFront(Agent pAgent)
            {
                Debug.Check(this.GetNode() is IntervalTimes);
                IntervalTimes pDecoratorCountNode = (IntervalTimes)(this.GetNode());

                return pDecoratorCountNode != null ? pDecoratorCountNode.GetFront(pAgent) : false;
            }
            protected int m_n;
            protected bool m_f;
            protected DateTime c_t;
        }
    }
}
