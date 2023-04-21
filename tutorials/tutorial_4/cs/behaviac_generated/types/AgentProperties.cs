﻿// ---------------------------------------------------------------------
// THIS FILE IS AUTO-GENERATED BY BEHAVIAC DESIGNER, SO PLEASE DON'T MODIFY IT BY YOURSELF!
// ---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace behaviac
{

	public class BehaviorLoaderImplement : BehaviorLoader
	{
		public BehaviorLoaderImplement(Workspace workspace) : base(workspace)
			{
			}
		private class CMethod_behaviac_Agent_VectorAdd : CAgentMethodVoidBase
		{
			IInstanceMember _param0;
			IInstanceMember _param1;

			public CMethod_behaviac_Agent_VectorAdd(Workspace workspace):base(workspace)
			{
			}

			public CMethod_behaviac_Agent_VectorAdd(CMethod_behaviac_Agent_VectorAdd rhs,Workspace workspace) : base(workspace,rhs)
			{
			}

			public override IMethod Clone()
			{
				return new CMethod_behaviac_Agent_VectorAdd(this, Workspace);
			}

			public override void Load(string instance, string[] paramStrs)
			{
				Debugs.Check(paramStrs.Length == 2);

				_instance = instance;
				_param0 = AgentMeta.ParseProperty<IList>(paramStrs[0], Workspace);
				_param1 = AgentMeta.ParseProperty<System.Object>(paramStrs[1], Workspace);
			}

			public override async Task Run(Agent self)
			{
				Debugs.Check(_param0 != null);
				Debugs.Check(_param1 != null);

				await behaviac.Agent.VectorAdd((IList)(await _param0.GetValueObject(self)), (System.Object)(await _param1.GetValueObject(self)));
				
			}
		}

		private class CMethod_behaviac_Agent_VectorClear : CAgentMethodVoidBase
		{
			IInstanceMember _param0;

			public CMethod_behaviac_Agent_VectorClear(Workspace workspace):base(workspace)
			{
			}

			public CMethod_behaviac_Agent_VectorClear(CMethod_behaviac_Agent_VectorClear rhs,Workspace workspace) : base(workspace,rhs)
			{
			}

			public override IMethod Clone()
			{
				return new CMethod_behaviac_Agent_VectorClear(this, Workspace);
			}

			public override void Load(string instance, string[] paramStrs)
			{
				Debugs.Check(paramStrs.Length == 1);

				_instance = instance;
				_param0 = AgentMeta.ParseProperty<IList>(paramStrs[0], Workspace);
			}

			public override async Task Run(Agent self)
			{
				Debugs.Check(_param0 != null);

				await behaviac.Agent.VectorClear((IList)(await _param0.GetValueObject(self)));
				
			}
		}

		private class CMethod_behaviac_Agent_VectorContains : CAgentMethodBase<bool>
		{
			IInstanceMember _param0;
			IInstanceMember _param1;

			public CMethod_behaviac_Agent_VectorContains(Workspace workspace):base(workspace)
			{
			}

			public CMethod_behaviac_Agent_VectorContains(CMethod_behaviac_Agent_VectorContains rhs,Workspace workspace) : base(rhs,workspace)
			{
			}

			public override IMethod Clone()
			{
				return new CMethod_behaviac_Agent_VectorContains(this, Workspace);
			}

			public override void Load(string instance, string[] paramStrs)
			{
				Debugs.Check(paramStrs.Length == 2);

				_instance = instance;
				_param0 = AgentMeta.ParseProperty<IList>(paramStrs[0], Workspace);
				_param1 = AgentMeta.ParseProperty<System.Object>(paramStrs[1], Workspace);
			}

			public override async Task Run(Agent self)
			{
				Debugs.Check(_param0 != null);
				Debugs.Check(_param1 != null);

				_returnValue.value =await behaviac.Agent.VectorContains((IList)(await _param0.GetValueObject(self)), (System.Object)(await _param1.GetValueObject(self)));
				
			}
		}

		private class CMethod_behaviac_Agent_VectorLength : CAgentMethodBase<int>
		{
			IInstanceMember _param0;

			public CMethod_behaviac_Agent_VectorLength(Workspace workspace):base(workspace)
			{
			}

			public CMethod_behaviac_Agent_VectorLength(CMethod_behaviac_Agent_VectorLength rhs,Workspace workspace) : base(rhs,workspace)
			{
			}

			public override IMethod Clone()
			{
				return new CMethod_behaviac_Agent_VectorLength(this, Workspace);
			}

			public override void Load(string instance, string[] paramStrs)
			{
				Debugs.Check(paramStrs.Length == 1);

				_instance = instance;
				_param0 = AgentMeta.ParseProperty<IList>(paramStrs[0], Workspace);
			}

			public override async Task Run(Agent self)
			{
				Debugs.Check(_param0 != null);

				_returnValue.value =await behaviac.Agent.VectorLength((IList)(await _param0.GetValueObject(self)));
				
			}
		}

		private class CMethod_behaviac_Agent_VectorRemove : CAgentMethodVoidBase
		{
			IInstanceMember _param0;
			IInstanceMember _param1;

			public CMethod_behaviac_Agent_VectorRemove(Workspace workspace):base(workspace)
			{
			}

			public CMethod_behaviac_Agent_VectorRemove(CMethod_behaviac_Agent_VectorRemove rhs,Workspace workspace) : base(workspace,rhs)
			{
			}

			public override IMethod Clone()
			{
				return new CMethod_behaviac_Agent_VectorRemove(this, Workspace);
			}

			public override void Load(string instance, string[] paramStrs)
			{
				Debugs.Check(paramStrs.Length == 2);

				_instance = instance;
				_param0 = AgentMeta.ParseProperty<IList>(paramStrs[0], Workspace);
				_param1 = AgentMeta.ParseProperty<System.Object>(paramStrs[1], Workspace);
			}

			public override async Task Run(Agent self)
			{
				Debugs.Check(_param0 != null);
				Debugs.Check(_param1 != null);

				await behaviac.Agent.VectorRemove((IList)(await _param0.GetValueObject(self)), (System.Object)(await _param1.GetValueObject(self)));
				
			}
		}


		public override bool Load()
		{
			AgentMeta.GetMetaGlobal(Workspace).TotalSignature = 2205687817;

			AgentMeta meta;

			// behaviac.Agent
			meta = new AgentMeta(Workspace,24743406);
			AgentMeta.GetMetaGlobal(Workspace)._AgentMetas_[2436498804] = meta;
			meta.RegisterMethod(1045109914, new CAgentStaticMethodVoid<string>(delegate(string param0) { behaviac.Agent.LogMessage(param0, Workspace);return Task.CompletedTask; }, Workspace));
			meta.RegisterMethod(2521019022, new CMethod_behaviac_Agent_VectorAdd(Workspace));
			meta.RegisterMethod(2306090221, new CMethod_behaviac_Agent_VectorClear(Workspace));
			meta.RegisterMethod(3483755530, new CMethod_behaviac_Agent_VectorContains(Workspace));
			meta.RegisterMethod(505785840, new CMethod_behaviac_Agent_VectorLength(Workspace));
			meta.RegisterMethod(502968959, new CMethod_behaviac_Agent_VectorRemove(Workspace));

			// FirstAgent
			meta = new AgentMeta(Workspace,2417680568);
			AgentMeta.GetMetaGlobal(Workspace)._AgentMetas_[1778122110] = meta;
			meta.RegisterMemberProperty(2082220067, new CMemberProperty<int>("p1", delegate(Agent self, int value) { ((FirstAgent)self)._set_p1(value); }, delegate(Agent self) { return ((FirstAgent)self)._get_p1(); }, Workspace));
			meta.RegisterMethod(1045109914, new CAgentStaticMethodVoid<string>(delegate(string param0) { FirstAgent.LogMessage(param0, Workspace);return Task.CompletedTask; }, Workspace));
			meta.RegisterMethod(702722749, new CAgentMethodVoid<string>(delegate(Agent self, string value) {return ((FirstAgent)self).Say(value);}, Workspace));
			meta.RegisterMethod(2521019022, new CMethod_behaviac_Agent_VectorAdd(Workspace));
			meta.RegisterMethod(2306090221, new CMethod_behaviac_Agent_VectorClear(Workspace));
			meta.RegisterMethod(3483755530, new CMethod_behaviac_Agent_VectorContains(Workspace));
			meta.RegisterMethod(505785840, new CMethod_behaviac_Agent_VectorLength(Workspace));
			meta.RegisterMethod(502968959, new CMethod_behaviac_Agent_VectorRemove(Workspace));

			AgentMeta.Register<behaviac.Agent>("behaviac.Agent",Workspace);
			AgentMeta.Register<FirstAgent>("FirstAgent",Workspace);
			return true;
		}

		public override bool UnLoad()
		{
			AgentMeta.UnRegister<behaviac.Agent>("behaviac.Agent",Workspace);
			AgentMeta.UnRegister<FirstAgent>("FirstAgent",Workspace);
			return true;
		}
	}
}
