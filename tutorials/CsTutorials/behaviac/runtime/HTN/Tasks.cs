using System.Collections.Generic;

namespace behaviac
{
    public class Tasks : BehaviorNode
    {
        public const string LOCAL_TASK_PARAM_PRE = "_$local_task_param_$_";

        protected IMethod m_task;

        protected bool m_bHTN;
        public bool IsHTN
        {
            get
            {
                return this.m_bHTN;
            }
        }

        public int FindMethodIndex(Methods method)
        {
            for (int i = 0; i < this.GetChildrenCount(); ++i)
            {
                BehaviorNode child = this.GetChild(i);

                if (child == method)
                {
                    return i;
                }
            }

            return -1;
        }

        public override bool IsValid(Agent pAgent, BehaviorTask pTask)
        {
            if (!(pTask.GetNode() is Tasks))
            {
                return false;
            }

            return base.IsValid(pAgent, pTask);
        }

        protected override BehaviorTask createTask()
        {
            TaskTask pTask = new TaskTask();

            return pTask;
        }

#if BEHAVIAC_USE_HTN
        public override bool decompose(BehaviorNode node, PlannerTaskComplex seqTask, int depth, Planner planner)
        {
            bool bOk = false;
            Task task = (Task)node;
            PlannerTask childTask = planner.decomposeTask((Task)task, depth);

            if (childTask != null)
            {
                seqTask.AddChild(childTask);
                bOk = true;
            }

            return bOk;
        }
#endif//

        protected override void load(int version, string agentType, List<property_t> properties)
        {
            base.load(version, agentType, properties);

            for (int i = 0; i < properties.Count; ++i)
            {
                property_t p = properties[i];

                if (p.name == "Prototype")
                {
                    this.m_task = AgentMeta.ParseMethod(p.value, Workspace);
                }
                else if (p.name == "IsHTN")
                {
                    this.m_bHTN = (p.value == "true");
                }
            }
        }
    }

    internal class TaskTask : Sequence.SequenceTask
    {
#if BEHAVIAC_USE_HTN
        private Planner _planner = new Planner();
#endif//

        public override void copyto(BehaviorTask target)
        {
            base.copyto(target);
        }

        public override void Init(BehaviorNode node)
        {
            Debugs.Check(node is Tasks, "node is not an Method");
            Tasks pTaskNode = (Tasks)(node);

            if (pTaskNode.IsHTN)
            {
                this.m_bIgnoreChildren = true;
            }

            base.Init(node);
        }

        protected override void addChild(BehaviorTask pBehavior)
        {
            base.addChild(pBehavior);
        }

        protected override bool onenter(Agent pAgent)
        {
            //reset the action child as it will be checked in the update
            this.m_activeChildIndex = CompositeTask.InvalidChildIndex;
            Debugs.Check(this.m_activeChildIndex == CompositeTask.InvalidChildIndex);
#if BEHAVIAC_USE_HTN
            Task pMethodNode = (Task)(this.GetNode());

            _planner.Init(pAgent, pMethodNode);
#endif//

            return base.onenter(pAgent);
        }

        protected override void onexit(Agent pAgent, EBTStatus s)
        {
#if BEHAVIAC_USE_HTN
            _planner.Uninit();
#endif//

            base.onexit(pAgent, s);
        }

        protected override Task<EBTStatus> update(Agent pAgent, EBTStatus childStatus)
        {
            EBTStatus status = childStatus;

            if (childStatus == EBTStatus.BT_RUNNING)
            {
                Debugs.Check(this.GetNode() is Tasks, "node is not an Method");
                Tasks pTaskNode = (Tasks)(this.GetNode());

                if (pTaskNode.IsHTN)
                {
#if BEHAVIAC_USE_HTN
                    status = _planner.Update();
#endif//
                }
                else
                {
                    Debugs.Check(this.m_children.Count == 1);
                    BehaviorTask c = this.m_children[0];
                    status = c.exec(pAgent);
                }
            }
            else
            {
                Debugs.Check(true);
            }

            return status;
        }
    }
}
