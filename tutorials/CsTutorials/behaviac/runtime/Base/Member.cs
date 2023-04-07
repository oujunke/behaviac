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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace behaviac
{
    public interface IValue
    {
        void Log(Agent agent, string name, bool bForce);
    }

    public class TValue<T> : IValue
    {
        public T value;

        public TValue(T v, Workspace workspace)
        {
            Utils.Clone(ref value, v, workspace);
        }

        public TValue(TValue<T> rhs, Workspace workspace)
        {
            Utils.Clone(ref value, rhs.value, workspace);
        }

        public TValue<T> Clone(Workspace workspace)
        {
            return new TValue<T>(this, workspace);
        }

        public void Log(Agent agent, string name, bool bForce)
        {
#if !BEHAVIAC_RELEASE
            T currentValue = agent.GetVariable<T>(name);

            if (bForce || OperationUtils.Compare<T>(currentValue, this.value, EOperatorType.E_NOTEQUAL, agent.Workspace))
            {
                agent.Workspace.LogManagers.LogVarValue(agent, name, currentValue);
                this.value = currentValue;
            }

#endif
        }
    }

    public interface IInstanceMember
    {
        int GetCount(Agent self);
        void SetValue(Agent self, IInstanceMember right, int index);

        object GetValueObject(Agent self);

        void SetValue(Agent self, object value);

        void SetValue(Agent self, IInstanceMember right);

        void SetValueAs(Agent self, IInstanceMember right);

        bool Compare(Agent self, IInstanceMember right, EOperatorType comparisonType);

        void Compute(Agent self, IInstanceMember right1, IInstanceMember right2, EOperatorType computeType);

        Task Run(Agent self);
    }

    public interface IProperty
    {
        string Name
        {
            get;
        }

        void SetValue(Agent self, IInstanceMember right);

        void SetValueFromString(Agent self, string valueStr);

        object GetValueObject(Agent self);

        object GetValueObject(Agent self, int index);

        IInstanceMember CreateInstance(string instance, IInstanceMember indexMember);

        IValue CreateIValue();

#if !BEHAVIAC_RELEASE
        bool IsArrayItem
        {
            get;
        }
#endif
    }

    public class CInstanceMember<T> : IInstanceMember
    {
        protected string _instance = "Self";
        protected IInstanceMember _indexMember = null;
        public Workspace Workspace { get; private set; }
        public Config Configs { set; get; }
        public Debug Debugs { set; get; }
        public CInstanceMember(Workspace workspace)
        {
            Workspace = workspace;
            Configs = workspace.Configs;
            Debugs = workspace.Debugs;
            _indexMember = null;
        }

        public CInstanceMember(string instance, IInstanceMember indexMember, Workspace workspace) : this(workspace)
        {
            _instance = instance;
            _indexMember = indexMember;
        }

        public CInstanceMember(CInstanceMember<T> rhs, Workspace workspace) : this(workspace)
        {
            _instance = rhs._instance;
            _indexMember = rhs._indexMember;
        }

        public int GetCount(Agent self)
        {
            //Agent agent = Utils.GetParentAgent(self, _instance);
            IList list = (IList)this.GetValueObject(self);

            if (list != null)
            {
                return list.Count;
            }

            return 0;
        }

        public void SetValue(Agent self, IInstanceMember right, int index)
        {
            //Agent agent = Utils.GetParentAgent(self, _instance);
            object rightObject = right.GetValueObject(self);
            Debugs.Check(rightObject is IList);
            IList il = (IList)rightObject;
            List<T> list = (List<T>)il;

            T item = list[index];

            this.SetValue(self, item);
        }

        public virtual T GetValue(Agent self)
        {
            Debugs.Check(false);
            return default(T);
        }

        public object GetValueObject(Agent self)
        {
            return GetValue(self);
        }

        public virtual void SetValue(Agent self, T value)
        {
            Debugs.Check(false);
        }

        public void SetValue(Agent self, object value)
        {
            Debugs.Check(value == null || !value.GetType().IsValueType);

            SetValue(self, (T)value);
        }

        public void SetValueAs(Agent self, IInstanceMember right)
        {
            if (typeof(T).IsValueType)
            {
                // this will cause boxing/unboxing
                object v = right.GetValueObject(self);

                object vv = Convert.ChangeType(v, typeof(T));

                T t = (T)vv;

                SetValue(self, t);
            }
            else
            {
                object v = right.GetValueObject(self);

                SetValue(self, v);
            }
        }

        public void SetValue(Agent self, IInstanceMember right)
        {
            SetValue(self, (CInstanceMember<T>)right);
        }

        public void SetValue(Agent self, CInstanceMember<T> right)
        {
            SetValue(self, right.GetValue(self));
        }

        public bool Compare(Agent self, IInstanceMember right, EOperatorType comparisonType)
        {
            T leftValue = this.GetValue(self);
            T rightValue = ((CInstanceMember<T>)right).GetValue(self);

            return OperationUtils.Compare(leftValue, rightValue, comparisonType, Workspace);
        }

        public void Compute(Agent self, IInstanceMember right1, IInstanceMember right2, EOperatorType computeType)
        {
            T rightValue1 = ((CInstanceMember<T>)right1).GetValue(self);
            T rightValue2 = ((CInstanceMember<T>)right2).GetValue(self);

            SetValue(self, OperationUtils.Compute(rightValue1, rightValue2, computeType, Workspace));
        }

        public virtual Task Run(Agent self)
        {
        }
    }

    public class CProperty<T> : IProperty
    {
        string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

#if !BEHAVIAC_RELEASE
        public virtual bool IsArrayItem
        {
            get
            {
                return false;
            }
        }
#endif
        public Workspace Workspace { get; private set; }
        public Config Configs { set; get; }
        public Debug Debugs { set; get; }
        public CProperty(string name, Workspace workspace)
        {
            Workspace = workspace;
            Configs = workspace.Configs;
            Debugs = workspace.Debugs;
            _name = name;
        }

        public IInstanceMember CreateInstance(string instance, IInstanceMember indexMember)
        {
            return new CInstanceProperty<T>(instance, indexMember, this, Workspace);
        }

        public IValue CreateIValue()
        {
            return new TValue<T>(default(T), Workspace);
        }

        public object GetValueObject(Agent self)
        {
            return GetValue(self);
        }

        public object GetValueObject(Agent self, int index)
        {
            return GetValue(self, index);
        }

        public void SetValueFromString(Agent self, string valueStr)
        {
            T value;
            ValueConverter<T>.Convert(valueStr, out value, Workspace);

            SetValue(self, value);
        }

        public void SetValue(Agent self, IInstanceMember right)
        {
            T rightValue = ((CInstanceMember<T>)right).GetValue(self);

            SetValue(self, rightValue);
        }

        public virtual void SetValue(Agent self, T value)
        {
            Debugs.Check(false);
        }

        public virtual void SetValue(Agent self, T value, int index)
        {
            Debugs.Check(false);
        }

        public virtual T GetValue(Agent self)
        {
            Debugs.Check(false);
            return default(T);
        }

        public virtual T GetValue(Agent self, int index)
        {
            Debugs.Check(false);
            return default(T);
        }

    }

    public class CInstanceProperty<T> : CInstanceMember<T>
    {
        CProperty<T> _property;

        public CInstanceProperty(string instance, IInstanceMember indexMember, CProperty<T> prop, Workspace workspace)
        : base(instance, indexMember, workspace)
        {
            _property = prop;
        }

        public override T GetValue(Agent self)
        {
            Agent agent = Utils.GetParentAgent(self, _instance);

            if (_indexMember != null)
            {
                int indexValue = ((CInstanceMember<int>)_indexMember).GetValue(self);
                return _property.GetValue(agent, indexValue);
            }

            return _property.GetValue(agent);
        }

        public override void SetValue(Agent self, T value)
        {
            Agent agent = Utils.GetParentAgent(self, _instance);

            if (_indexMember != null)
            {
                int indexValue = ((CInstanceMember<int>)_indexMember).GetValue(self);
                _property.SetValue(agent, value, indexValue);
            }
            else
            {
                _property.SetValue(agent, value);
            }
        }
    }

    public class CStaticMemberProperty<T> : CProperty<T>
    {
        public delegate void SetFunctionPointer(T v);
        public delegate T GetFunctionPointer();

        SetFunctionPointer _sfp;
        GetFunctionPointer _gfp;

        public CStaticMemberProperty(string name, SetFunctionPointer sfp, GetFunctionPointer gfp, Workspace workspace)
        : base(name, workspace)
        {
            _sfp = sfp;
            _gfp = gfp;
        }

        public override T GetValue(Agent self)
        {
            Debugs.Check(_gfp != null);

            return _gfp();
        }

        public override void SetValue(Agent self, T value)
        {
            Debugs.Check(_sfp != null);

            _sfp(value);
        }
    }

    public class CStaticMemberArrayItemProperty<T> : CProperty<T>
    {
        public delegate void SetFunctionPointer(T v, int index);
        public delegate T GetFunctionPointer(int index);

        SetFunctionPointer _sfp;
        GetFunctionPointer _gfp;

        public CStaticMemberArrayItemProperty(string name, SetFunctionPointer sfp, GetFunctionPointer gfp, Workspace workspace)
        : base(name, workspace)
        {
            _sfp = sfp;
            _gfp = gfp;
        }

#if !BEHAVIAC_RELEASE
        public override bool IsArrayItem
        {
            get
            {
                return true;
            }
        }
#endif
        public override T GetValue(Agent self, int index)
        {
            Debugs.Check(_gfp != null);

            return _gfp(index);
        }

        public override void SetValue(Agent self, T value, int index)
        {
            Debugs.Check(_sfp != null);

            _sfp(value, index);
        }
    }

    public class CMemberProperty<T> : CProperty<T>
    {
        public delegate void SetFunctionPointer(Agent a, T v);
        public delegate T GetFunctionPointer(Agent a);

        SetFunctionPointer _sfp;
        GetFunctionPointer _gfp;

        public CMemberProperty(string name, SetFunctionPointer sfp, GetFunctionPointer gfp, Workspace workspace)
        : base(name, workspace)
        {
            _sfp = sfp;
            _gfp = gfp;
        }

        public override T GetValue(Agent self)
        {
            Debugs.Check(_gfp != null);

#if BEHAVIAC_USE_HTN

            if (self.PlanningTop > -1)
            {
                uint id = Utils.MakeVariableId(this.Name);
                IInstantiatedVariable pVar = self.Variables.GetVariable(id);

                if (pVar != null)
                {
                    CVariable<T> pTVar = (CVariable<T>)pVar;
                    return pTVar.GetValue(self);
                }
            }

#endif

            return _gfp(self);
        }

        public override void SetValue(Agent self, T value)
        {
            Debugs.Check(_sfp != null);

#if BEHAVIAC_USE_HTN

            if (self.PlanningTop > -1)
            {
                uint id = Utils.MakeVariableId(this.Name);
                IInstantiatedVariable pVar = self.Variables.GetVariable(id);

                if (pVar == null)
                {
                    pVar = new CVariable<T>(this.Name, value);
                    self.Variables.AddVariable(id, pVar, 1);
                }
                else
                {
                    CVariable<T> pTVar = (CVariable<T>)pVar;
                    pTVar.SetValue(self, value);
                }
            }
            else
#endif//
            {
                _sfp(self, value);
            }
        }
    }

    public class CMemberArrayItemProperty<T> : CProperty<T>
    {
        public delegate void SetFunctionPointer(Agent a, T v, int index);
        public delegate T GetFunctionPointer(Agent a, int index);

        SetFunctionPointer _sfp;
        GetFunctionPointer _gfp;

        public CMemberArrayItemProperty(string name, SetFunctionPointer sfp, GetFunctionPointer gfp, Workspace workspace)
        : base(name, workspace)
        {
            _sfp = sfp;
            _gfp = gfp;
        }

#if !BEHAVIAC_RELEASE
        public override bool IsArrayItem
        {
            get
            {
                return true;
            }
        }
#endif

        public override T GetValue(Agent self, int index)
        {
            Debugs.Check(_gfp != null);

            return _gfp(self, index);
        }

        public override void SetValue(Agent self, T value, int index)
        {
            Debugs.Check(_sfp != null);

            _sfp(self, value, index);
        }
    }

    public interface ICustomizedProperty : IProperty
    {
        IInstantiatedVariable Instantiate();
    }

    public interface IInstantiatedVariable
    {
        object GetValueObject(Agent self);

        object GetValueObject(Agent self, int index);

        void SetValueFromString(Agent self, string valueStr);

        void SetValue(Agent self, object value);

        void SetValue(Agent self, object value, int index);

        string Name
        {
            get;
        }

        void Log(Agent self);

        IInstantiatedVariable clone();

        void CopyTo(Agent pAgent);
        void Save(ISerializableNode node);
    }

    public class CCustomizedProperty<T> : CProperty<T>, ICustomizedProperty
    {
        uint _id;
        T _defaultValue;

        public CCustomizedProperty(uint id, string name, string valueStr, Workspace workspace)
        : base(name, workspace)
        {
            _id = id;
            ValueConverter<T>.Convert(valueStr, out _defaultValue, workspace);
        }

        public override T GetValue(Agent self)
        {
            if (self != null)
            {
                T value;

                if (self.GetVarValue<T>(_id, out value))
                {
                    return value;
                }
            }

            return _defaultValue;
        }

        public override void SetValue(Agent self, T value)
        {
            bool bOk = self.SetVarValue<T>(_id, value);
            Debugs.Check(bOk);
        }

        public IInstantiatedVariable Instantiate()
        {
            T value = default(T);
            Utils.Clone(ref value, _defaultValue, Workspace);
            return new CVariable<T>(this.Name, value, Workspace);
        }
    }

    public class CCustomizedArrayItemProperty<T> : CProperty<T>, ICustomizedProperty
    {
        uint _parentId;

        public CCustomizedArrayItemProperty(uint parentId, string parentName, Workspace workspace)
        : base(parentName, workspace)
        {
            _parentId = parentId;
        }

#if !BEHAVIAC_RELEASE
        public override bool IsArrayItem
        {
            get
            {
                return true;
            }
        }
#endif

        public override T GetValue(Agent self, int index)
        {
            List<T> arrayValue = self.GetVariable<List<T>>(_parentId);
            Debugs.Check(arrayValue != null);

            if (arrayValue != null)
            {
                Debugs.Check(index >= 0 && index < arrayValue.Count);
                return arrayValue[index];
            }

            return default(T);
        }

        public override void SetValue(Agent self, T value, int index)
        {
            List<T> arrayValue = self.GetVariable<List<T>>(_parentId);
            Debugs.Check(arrayValue != null);

            if (arrayValue != null)
            {
                arrayValue[index] = value;
            }
        }

        public IInstantiatedVariable Instantiate()
        {
            return new CArrayItemVariable<T>(_parentId, this.Name, Workspace);
        }
    }

    public class CVariable<T> : IInstantiatedVariable
    {
        T _value;

        string _name;
        public Workspace Workspace { set; get; }
#if !BEHAVIAC_RELEASE
        bool _isModified = false;
        internal bool IsModified
        {
            get
            {
                return this._isModified;
            }

            set
            {
                _isModified = value;
            }
        }
#endif

        public CVariable(string name, T value, Workspace workspace)
        {
            Utils.Clone<T>(ref this._value, value, workspace);
            Workspace = workspace;
            _name = name;

#if !BEHAVIAC_RELEASE
            this._isModified = true;
#endif
        }

        public CVariable(string name, string valueStr, Workspace workspace)
        {
            ValueConverter<T>.Convert(valueStr, out _value, workspace);
            Workspace = workspace;
            _name = name;

#if !BEHAVIAC_RELEASE
            this._isModified = true;
#endif
        }

        public T GetValue(Agent self)
        {
            return _value;
        }

        public object GetValueObject(Agent self)
        {
            return _value;
        }

        public object GetValueObject(Agent self, int index)
        {
            IList values = _value as IList;
            return (values != null) ? values[index] : _value;
        }

        public void SetValueFromString(Agent self, string valueStr)
        {
            ValueConverter<T>.Convert(valueStr, out _value, self.Workspace);
        }

        public void SetValue(Agent self, T value)
        {
            _value = value;

#if !BEHAVIAC_RELEASE
            _isModified = true;
#endif
        }

        public void SetValue(Agent self, object value)
        {
            SetValue(self, (T)value);
        }

        public void SetValue(Agent self, object value, int index)
        {
            self.Workspace.Debugs.Check(false);
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public void Log(Agent self)
        {
#if !BEHAVIAC_RELEASE

            if (_isModified)
            {
                self.Workspace.LogManagers.LogVarValue(self, this.Name, this._value);

                // clear it
                _isModified = false;
            }

#endif
        }

        public void CopyTo(Agent pAgent)
        {
            //TODO:
            pAgent.Debugs.Check(false);
        }

        public void Save(ISerializableNode node)
        {
            //base.Save(node);
            CSerializationID variableId = new CSerializationID("var");
            ISerializableNode varNode = node.newChild(variableId);

            CSerializationID nameId = new CSerializationID("name");
            varNode.setAttr(nameId, this._name);

            CSerializationID valueId = new CSerializationID("value");
            varNode.setAttr(valueId, this._value);
        }

        public IInstantiatedVariable clone()
        {
            CVariable<T> p = new CVariable<T>(this._name, this._value, Workspace);

            return p;
        }
    }

    public class CArrayItemVariable<T> : IInstantiatedVariable
    {
        string _name;
        uint _parentId;
        Workspace Workspace { set; get; }
        public CArrayItemVariable(uint parentId, string name, Workspace workspace)
        {
            _parentId = parentId;
            _name = name;
            Workspace = workspace;
        }

        public T GetValue(Agent self, int index)
        {
            IInstantiatedVariable v = self.GetInstantiatedVariable(this._parentId);

            if (v != null)
            {
                if (typeof(T).IsValueType)
                {
                    CVariable<List<T>> arrayVar = (CVariable<List<T>>)v;

                    if (arrayVar != null)
                    {
                        return arrayVar.GetValue(self)[index];
                    }
                }

                return (T)v.GetValueObject(self, index);
            }

            return default(T);
        }

        public void SetValueFromString(Agent self, string valueStr)
        {
            self.Debugs.Check(false);
        }

        public void SetValue(Agent self, T value, int index)
        {
            IInstantiatedVariable v = self.GetInstantiatedVariable(this._parentId);
            CVariable<List<T>> arrayVar = (CVariable<List<T>>)v;

            if (arrayVar != null)
            {
                arrayVar.GetValue(self)[index] = value;

#if !BEHAVIAC_RELEASE
                arrayVar.IsModified = true;
#endif
            }
        }

        public object GetValueObject(Agent self)
        {
            self.Debugs.Check(false);
            return null;
        }

        public object GetValueObject(Agent self, int index)
        {
            return GetValue(self, index);
        }

        public void SetValue(Agent self, object value)
        {
            self.Debugs.Check(false);
        }

        public void SetValue(Agent self, object value, int index)
        {
            SetValue(self, (T)value, index);
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public void Log(Agent self)
        {
#if !BEHAVIAC_RELEASE
            IInstantiatedVariable v = self.GetInstantiatedVariable(this._parentId);
            CVariable<List<T>> arrayVar = (CVariable<List<T>>)v;

            if (arrayVar != null && arrayVar.IsModified)
            {
                self.Workspace.LogManagers.LogVarValue(self, this.Name, arrayVar);
            }

#endif
        }

        public void CopyTo(Agent pAgent)
        {
            pAgent.Debugs.Check(false);
        }

        public void Save(ISerializableNode node)
        {

            Workspace.Debugs.Check(false);
        }

        public IInstantiatedVariable clone()
        {
            CArrayItemVariable<T> p = new CArrayItemVariable<T>(this._parentId, this._name, Workspace);

            return p;
        }
    }

    public class CInstanceCustomizedProperty<T> : CInstanceMember<T>
    {
        uint _id;

        public CInstanceCustomizedProperty(string instance, IInstanceMember indexMember, uint id, Workspace workspace)
        : base(instance, indexMember, workspace)
        {
            _id = id;
        }

        public override T GetValue(Agent self)
        {
            if (self != null)
            {
                Agent agent = Utils.GetParentAgent(self, _instance);

                if (_indexMember != null)
                {
                    int indexValue = ((CInstanceMember<int>)_indexMember).GetValue(self);
                    return agent.GetVariable<T>(_id, indexValue);
                }
                else
                {
                    return agent.GetVariable<T>(_id);
                }
            }

            return default(T);
        }

        public override void SetValue(Agent self, T value)
        {
            Agent agent = Utils.GetParentAgent(self, _instance);

            if (_indexMember != null)
            {
                int indexValue = ((CInstanceMember<int>)_indexMember).GetValue(self);
                agent.SetVariable<T>("", _id, value, indexValue);
            }
            else
            {
                agent.SetVariable<T>("", _id, value);
            }
        }
    }

    public class CInstanceConst<T> : CInstanceMember<T>
    {
        protected T _value;

        public CInstanceConst(string typeName, string valueStr, Workspace workspace) : base(workspace)
        {
            this._value = (T)AgentMeta.ParseTypeValue(typeName, valueStr, workspace);
        }

        public override T GetValue(Agent self)
        {
            return _value;
        }

        public override void SetValue(Agent self, T value)
        {
            _value = value;
        }
    }

    public interface IMethod : IInstanceMember
    {
        IMethod Clone();

        void Load(string instance, string[] paramStrs);

        IValue GetIValue(Agent self);

        IValue GetIValue(Agent self, IInstanceMember firstParam);

        void SetTaskParams(Agent self, BehaviorTreeTask treeTask);
    }

    public class CAgentMethodBase<T> : CInstanceMember<T>, IMethod
    {
        protected TValue<T> _returnValue;

        public CAgentMethodBase(Workspace workspace) : base(workspace)
        {
            _returnValue = new TValue<T>(default(T), workspace);
        }

        public CAgentMethodBase(CAgentMethodBase<T> rhs, Workspace workspace) : base(workspace)
        {
            _returnValue = rhs._returnValue.Clone(workspace);
        }

        public virtual IMethod Clone()
        {
            Debugs.Check(false);
            return null;
        }

        public virtual void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(false);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(false);
        }

        public override T GetValue(Agent self)
        {
            if (!System.Object.ReferenceEquals(self, null))
            {
                Run(self);
            }

            return _returnValue.value;
        }

        public virtual IValue GetIValue(Agent self)
        {
            if (!System.Object.ReferenceEquals(self, null))
            {
                Run(self);
            }

            return _returnValue;
        }

        public virtual IValue GetIValue(Agent self, IInstanceMember firstParam)
        {
            Agent agent = Utils.GetParentAgent(self, _instance);
            firstParam.Run(agent);

            return GetIValue(self);
        }

        public virtual void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            Debugs.Check(false);
        }
    }

    public class CAgentMethod<T> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a);

        FunctionPointer _fp;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 0);

            _instance = instance;
        }

        public override void Run(Agent self)
        {
            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent);
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
        }
    }

    public class CAgentMethod<T, P1> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a, P1 p1);

        FunctionPointer _fp;
        IInstanceMember _p1;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T, P1> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T, P1>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 1);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent, ((CInstanceMember<P1>)_p1).GetValue(self));
        }

        public override IValue GetIValue(Agent self, IInstanceMember firstParam)
        {
            Debugs.Check(_p1 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent, ((CInstanceMember<P1>)firstParam).GetValue(self));

            return _returnValue;
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));
        }
    }

    public class CAgentMethod<T, P1, P2> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a, P1 p1, P2 p2);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T, P1, P2> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T, P1, P2>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 2);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent,
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));
        }
    }

    public class CAgentMethod<T, P1, P2, P3> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T, P1, P2, P3> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T, P1, P2, P3>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 3);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent,
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));
        }
    }

    public class CAgentMethod<T, P1, P2, P3, P4> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T, P1, P2, P3, P4> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T, P1, P2, P3, P4>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 4);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent,
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));
        }
    }

    public class CAgentMethod<T, P1, P2, P3, P4, P5> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T, P1, P2, P3, P4, P5> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T, P1, P2, P3, P4, P5>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 5);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent,
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));
        }
    }

    public class CAgentMethod<T, P1, P2, P3, P4, P5, P6> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T, P1, P2, P3, P4, P5, P6> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T, P1, P2, P3, P4, P5, P6>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 6);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent,
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));
        }
    }

    public class CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 7);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent,
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));
        }
    }

    public class CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 8);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent,
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self),
                                     ((CInstanceMember<P8>)_p8).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));
        }
    }

    public class CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 9);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent,
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self),
                                     ((CInstanceMember<P8>)_p8).GetValue(self),
                                     ((CInstanceMember<P9>)_p9).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));
        }
    }

    public class CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 10);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent,
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self),
                                     ((CInstanceMember<P8>)_p8).GetValue(self),
                                     ((CInstanceMember<P9>)_p9).GetValue(self),
                                     ((CInstanceMember<P10>)_p10).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));
        }
    }

    public class CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 11);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent,
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self),
                                     ((CInstanceMember<P8>)_p8).GetValue(self),
                                     ((CInstanceMember<P9>)_p9).GetValue(self),
                                     ((CInstanceMember<P10>)_p10).GetValue(self),
                                     ((CInstanceMember<P11>)_p11).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));
        }
    }

    public class CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11, P12 p12);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;
        IInstanceMember _p12;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
            _p12 = rhs._p12;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 12);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
            _p12 = AgentMeta.ParseProperty<P12>(paramStrs[11], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);
            Debugs.Check(_p12 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent,
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self),
                                     ((CInstanceMember<P8>)_p8).GetValue(self),
                                     ((CInstanceMember<P9>)_p9).GetValue(self),
                                     ((CInstanceMember<P10>)_p10).GetValue(self),
                                     ((CInstanceMember<P11>)_p11).GetValue(self),
                                     ((CInstanceMember<P12>)_p12).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 11);
            treeTask.SetVariable(paramName, ((CInstanceMember<P12>)_p12).GetValue(self));
        }
    }

    public class CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11, P12 p12, P13 p13);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;
        IInstanceMember _p12;
        IInstanceMember _p13;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
            _p12 = rhs._p12;
            _p13 = rhs._p13;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 13);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
            _p12 = AgentMeta.ParseProperty<P12>(paramStrs[11], Workspace);
            _p13 = AgentMeta.ParseProperty<P13>(paramStrs[12], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);
            Debugs.Check(_p12 != null);
            Debugs.Check(_p13 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent,
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self),
                                     ((CInstanceMember<P8>)_p8).GetValue(self),
                                     ((CInstanceMember<P9>)_p9).GetValue(self),
                                     ((CInstanceMember<P10>)_p10).GetValue(self),
                                     ((CInstanceMember<P11>)_p11).GetValue(self),
                                     ((CInstanceMember<P12>)_p12).GetValue(self),
                                     ((CInstanceMember<P13>)_p13).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 11);
            treeTask.SetVariable(paramName, ((CInstanceMember<P12>)_p12).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 12);
            treeTask.SetVariable(paramName, ((CInstanceMember<P13>)_p13).GetValue(self));
        }
    }

    public class CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11, P12 p12, P13 p13, P14 p14);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;
        IInstanceMember _p12;
        IInstanceMember _p13;
        IInstanceMember _p14;

        public CAgentMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethod(CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
            _p12 = rhs._p12;
            _p13 = rhs._p13;
            _p14 = rhs._p14;
        }

        public override IMethod Clone()
        {
            return new CAgentMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 14);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
            _p12 = AgentMeta.ParseProperty<P12>(paramStrs[11], Workspace);
            _p13 = AgentMeta.ParseProperty<P13>(paramStrs[12], Workspace);
            _p14 = AgentMeta.ParseProperty<P14>(paramStrs[13], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);
            Debugs.Check(_p12 != null);
            Debugs.Check(_p13 != null);
            Debugs.Check(_p14 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _returnValue.value = _fp(agent,
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self),
                                     ((CInstanceMember<P8>)_p8).GetValue(self),
                                     ((CInstanceMember<P9>)_p9).GetValue(self),
                                     ((CInstanceMember<P10>)_p10).GetValue(self),
                                     ((CInstanceMember<P11>)_p11).GetValue(self),
                                     ((CInstanceMember<P12>)_p12).GetValue(self),
                                     ((CInstanceMember<P13>)_p13).GetValue(self),
                                     ((CInstanceMember<P14>)_p14).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 11);
            treeTask.SetVariable(paramName, ((CInstanceMember<P12>)_p12).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 12);
            treeTask.SetVariable(paramName, ((CInstanceMember<P13>)_p13).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 13);
            treeTask.SetVariable(paramName, ((CInstanceMember<P14>)_p14).GetValue(self));
        }
    }

    public class CAgentStaticMethod<T> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer();

        FunctionPointer _fp;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 0);

            _instance = instance;
        }

        public override void Run(Agent self)
        {
            _returnValue.value = _fp();
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
        }
    }

    public class CAgentStaticMethod<T, P1> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(P1 p1);

        FunctionPointer _fp;
        IInstanceMember _p1;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T, P1> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T, P1>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 1);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);

            _returnValue.value = _fp(((CInstanceMember<P1>)_p1).GetValue(self));
        }

        public override IValue GetIValue(Agent self, IInstanceMember firstParam)
        {
            Debugs.Check(_p1 != null);

            _returnValue.value = _fp(((CInstanceMember<P1>)firstParam).GetValue(self));

            return _returnValue;
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));
        }
    }

    public class CAgentStaticMethod<T, P1, P2> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(P1 p1, P2 p2);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T, P1, P2> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T, P1, P2>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 2);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);

            _returnValue.value = _fp(((CInstanceMember<P1>)_p1).GetValue(self), ((CInstanceMember<P2>)_p2).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));
        }
    }

    public class CAgentStaticMethod<T, P1, P2, P3> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(P1 p1, P2 p2, P3 p3);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T, P1, P2, P3> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T, P1, P2, P3>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 3);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);

            _returnValue.value = _fp(
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));
        }
    }

    public class CAgentStaticMethod<T, P1, P2, P3, P4> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T, P1, P2, P3, P4> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T, P1, P2, P3, P4>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 4);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);

            _returnValue.value = _fp(
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));
        }
    }

    public class CAgentStaticMethod<T, P1, P2, P3, P4, P5> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T, P1, P2, P3, P4, P5> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T, P1, P2, P3, P4, P5>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 5);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);

            _returnValue.value = _fp(
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));
        }
    }

    public class CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 6);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);

            _returnValue.value = _fp(
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));
        }
    }

    public class CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 7);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);

            _returnValue.value = _fp(
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));
        }
    }

    public class CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 8);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);

            _returnValue.value = _fp(
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self),
                                     ((CInstanceMember<P8>)_p8).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));
        }
    }

    public class CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 9);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);

            _returnValue.value = _fp(
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self),
                                     ((CInstanceMember<P8>)_p8).GetValue(self),
                                     ((CInstanceMember<P9>)_p9).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));
        }
    }

    public class CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 10);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);

            _returnValue.value = _fp(
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self),
                                     ((CInstanceMember<P8>)_p8).GetValue(self),
                                     ((CInstanceMember<P9>)_p9).GetValue(self),
                                     ((CInstanceMember<P10>)_p10).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));
        }
    }

    public class CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 11);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);

            _returnValue.value = _fp(
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self),
                                     ((CInstanceMember<P8>)_p8).GetValue(self),
                                     ((CInstanceMember<P9>)_p9).GetValue(self),
                                     ((CInstanceMember<P10>)_p10).GetValue(self),
                                     ((CInstanceMember<P11>)_p11).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));
        }
    }

    public class CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11, P12 p12);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;
        IInstanceMember _p12;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
            _p12 = rhs._p12;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 12);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
            _p12 = AgentMeta.ParseProperty<P12>(paramStrs[11], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);
            Debugs.Check(_p12 != null);

            _returnValue.value = _fp(
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self),
                                     ((CInstanceMember<P8>)_p8).GetValue(self),
                                     ((CInstanceMember<P9>)_p9).GetValue(self),
                                     ((CInstanceMember<P10>)_p10).GetValue(self),
                                     ((CInstanceMember<P11>)_p11).GetValue(self),
                                     ((CInstanceMember<P12>)_p12).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 11);
            treeTask.SetVariable(paramName, ((CInstanceMember<P12>)_p12).GetValue(self));
        }
    }

    public class CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11, P12 p12, P13 p13);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;
        IInstanceMember _p12;
        IInstanceMember _p13;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
            _p12 = rhs._p12;
            _p13 = rhs._p13;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 13);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
            _p12 = AgentMeta.ParseProperty<P12>(paramStrs[11], Workspace);
            _p13 = AgentMeta.ParseProperty<P13>(paramStrs[12], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);
            Debugs.Check(_p12 != null);
            Debugs.Check(_p13 != null);

            _returnValue.value = _fp(
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self),
                                     ((CInstanceMember<P8>)_p8).GetValue(self),
                                     ((CInstanceMember<P9>)_p9).GetValue(self),
                                     ((CInstanceMember<P10>)_p10).GetValue(self),
                                     ((CInstanceMember<P11>)_p11).GetValue(self),
                                     ((CInstanceMember<P12>)_p12).GetValue(self),
                                     ((CInstanceMember<P13>)_p13).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 11);
            treeTask.SetVariable(paramName, ((CInstanceMember<P12>)_p12).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 12);
            treeTask.SetVariable(paramName, ((CInstanceMember<P13>)_p13).GetValue(self));
        }
    }

    public class CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> : CAgentMethodBase<T>
    {
        public delegate T FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11, P12 p12, P13 p13, P14 p14);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;
        IInstanceMember _p12;
        IInstanceMember _p13;
        IInstanceMember _p14;

        public CAgentStaticMethod(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethod(CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> rhs, Workspace workspace)
        : base(rhs, workspace)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
            _p12 = rhs._p12;
            _p13 = rhs._p13;
            _p14 = rhs._p14;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethod<T, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 14);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
            _p12 = AgentMeta.ParseProperty<P12>(paramStrs[11], Workspace);
            _p13 = AgentMeta.ParseProperty<P13>(paramStrs[12], Workspace);
            _p14 = AgentMeta.ParseProperty<P14>(paramStrs[13], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);
            Debugs.Check(_p12 != null);
            Debugs.Check(_p13 != null);
            Debugs.Check(_p14 != null);

            _returnValue.value = _fp(
                                     ((CInstanceMember<P1>)_p1).GetValue(self),
                                     ((CInstanceMember<P2>)_p2).GetValue(self),
                                     ((CInstanceMember<P3>)_p3).GetValue(self),
                                     ((CInstanceMember<P4>)_p4).GetValue(self),
                                     ((CInstanceMember<P5>)_p5).GetValue(self),
                                     ((CInstanceMember<P6>)_p6).GetValue(self),
                                     ((CInstanceMember<P7>)_p7).GetValue(self),
                                     ((CInstanceMember<P8>)_p8).GetValue(self),
                                     ((CInstanceMember<P9>)_p9).GetValue(self),
                                     ((CInstanceMember<P10>)_p10).GetValue(self),
                                     ((CInstanceMember<P11>)_p11).GetValue(self),
                                     ((CInstanceMember<P12>)_p12).GetValue(self),
                                     ((CInstanceMember<P13>)_p13).GetValue(self),
                                     ((CInstanceMember<P14>)_p14).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 11);
            treeTask.SetVariable(paramName, ((CInstanceMember<P12>)_p12).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 12);
            treeTask.SetVariable(paramName, ((CInstanceMember<P13>)_p13).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 13);
            treeTask.SetVariable(paramName, ((CInstanceMember<P14>)_p14).GetValue(self));
        }
    }

    public class CAgentMethodVoidBase : IMethod
    {
        protected string _instance = "Self";

        public Workspace Workspace { get; private set; }
        public Config Configs { set; get; }
        public Debug Debugs { set; get; }
        public CAgentMethodVoidBase(Workspace workspace, CAgentMethodVoidBase rhs) : this(workspace)
        {
            _instance = rhs._instance;
        }
        public CAgentMethodVoidBase(Workspace workspace)
        {
            Workspace = workspace;
            Configs = workspace.Configs;
            Debugs = workspace.Debugs;
        }
        public virtual IMethod Clone()
        {
            Debugs.Check(false);
            return null;
        }

        public virtual void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(false);

            _instance = instance;
        }

        public int GetCount(Agent self)
        {
            Debugs.Check(false);
            return 0;
        }

        public void SetValue(Agent self, IInstanceMember right, int index)
        {
            Debugs.Check(false);
        }

        public virtual Task Run(Agent self)
        {
            Debugs.Check(false);
        }

        public IValue GetIValue(Agent self)
        {
            Debugs.Check(false);
            return null;
        }

        public object GetValueObject(Agent self)
        {
            Debugs.Check(false);
            return null;
        }

        public IValue GetIValue(Agent self, IInstanceMember firstParam)
        {
            //Agent agent = Utils.GetParentAgent(self, _instance);
            //firstParam.Run(agent);

            return GetIValue(self);
        }

        public void SetValue(Agent self, IValue value)
        {
            Debugs.Check(false);
        }

        public void SetValue(Agent self, object value)
        {
            Debugs.Check(false);
        }

        public void SetValueAs(Agent self, IInstanceMember right)
        {
            Debugs.Check(false);
        }

        public void SetValue(Agent self, IInstanceMember right)
        {
            Debugs.Check(false);
        }

        public bool Compare(Agent self, IInstanceMember right, EOperatorType comparisonType)
        {
            Debugs.Check(false);
            return false;
        }

        public void Compute(Agent self, IInstanceMember right1, IInstanceMember right2, EOperatorType computeType)
        {
            Debugs.Check(false);
        }

        public virtual void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            Debugs.Check(false);
        }
    }

    public class CAgentMethodVoid : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a);

        FunctionPointer _fp;

        public CAgentMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 0);

            _instance = instance;
        }

        public override void Run(Agent self)
        {
            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent);
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
        }
    }

    public class CAgentMethodVoid<P1> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a, P1 p1);

        FunctionPointer _fp;
        IInstanceMember _p1;

        public CAgentMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid<P1> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid<P1>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 1);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent, ((CInstanceMember<P1>)_p1).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));
        }
    }

    public class CAgentMethodVoid<P1, P2> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a, P1 p1, P2 p2);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;

        public CAgentMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid<P1, P2> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid<P1, P2>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 2);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent, ((CInstanceMember<P1>)_p1).GetValue(self), ((CInstanceMember<P2>)_p2).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));
        }
    }

    public class CAgentMethodVoid<P1, P2, P3> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;

        public CAgentMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid<P1, P2, P3> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid<P1, P2, P3>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 3);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent,
                ((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));
        }
    }

    public class CAgentMethodVoid<P1, P2, P3, P4> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;

        public CAgentMethodVoid(FunctionPointer f, IInstanceMember p1, IInstanceMember p2, IInstanceMember p3, IInstanceMember p4, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid<P1, P2, P3, P4> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid<P1, P2, P3, P4>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 4);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent,
                ((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));
        }
    }

    public class CAgentMethodVoid<P1, P2, P3, P4, P5> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;

        public CAgentMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid<P1, P2, P3, P4, P5> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid<P1, P2, P3, P4, P5>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 5);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent,
                ((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self),
                ((CInstanceMember<P5>)_p5).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));
        }
    }

    public class CAgentMethodVoid<P1, P2, P3, P4, P5, P6> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;

        public CAgentMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid<P1, P2, P3, P4, P5, P6> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid<P1, P2, P3, P4, P5, P6>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 6);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent,
                ((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self),
                ((CInstanceMember<P5>)_p5).GetValue(self),
                ((CInstanceMember<P6>)_p6).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));
        }
    }

    public class CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;

        public CAgentMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 7);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent,
                ((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self),
                ((CInstanceMember<P5>)_p5).GetValue(self),
                ((CInstanceMember<P6>)_p6).GetValue(self),
                ((CInstanceMember<P7>)_p7).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));
        }
    }

    public class CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;

        public CAgentMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 8);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent,
                ((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self),
                ((CInstanceMember<P5>)_p5).GetValue(self),
                ((CInstanceMember<P6>)_p6).GetValue(self),
                ((CInstanceMember<P7>)_p7).GetValue(self),
                ((CInstanceMember<P8>)_p8).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));
        }
    }

    public class CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;

        public CAgentMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 9);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent,
                ((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self),
                ((CInstanceMember<P5>)_p5).GetValue(self),
                ((CInstanceMember<P6>)_p6).GetValue(self),
                ((CInstanceMember<P7>)_p7).GetValue(self),
                ((CInstanceMember<P8>)_p8).GetValue(self),
                ((CInstanceMember<P9>)_p9).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));
        }
    }

    public class CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;

        public CAgentMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 10);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent,
                ((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self),
                ((CInstanceMember<P5>)_p5).GetValue(self),
                ((CInstanceMember<P6>)_p6).GetValue(self),
                ((CInstanceMember<P7>)_p7).GetValue(self),
                ((CInstanceMember<P8>)_p8).GetValue(self),
                ((CInstanceMember<P9>)_p9).GetValue(self),
                ((CInstanceMember<P10>)_p10).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));
        }
    }

    public class CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;

        public CAgentMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> rhs, Workspace workspace) : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 11);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent,
                ((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self),
                ((CInstanceMember<P5>)_p5).GetValue(self),
                ((CInstanceMember<P6>)_p6).GetValue(self),
                ((CInstanceMember<P7>)_p7).GetValue(self),
                ((CInstanceMember<P8>)_p8).GetValue(self),
                ((CInstanceMember<P9>)_p9).GetValue(self),
                ((CInstanceMember<P10>)_p10).GetValue(self),
                ((CInstanceMember<P11>)_p11).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));
        }
    }

    public class CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11, P12 p12);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;
        IInstanceMember _p12;

        public CAgentMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
            _p12 = rhs._p12;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 12);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
            _p12 = AgentMeta.ParseProperty<P12>(paramStrs[11], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);
            Debugs.Check(_p12 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent,
                ((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self),
                ((CInstanceMember<P5>)_p5).GetValue(self),
                ((CInstanceMember<P6>)_p6).GetValue(self),
                ((CInstanceMember<P7>)_p7).GetValue(self),
                ((CInstanceMember<P8>)_p8).GetValue(self),
                ((CInstanceMember<P9>)_p9).GetValue(self),
                ((CInstanceMember<P10>)_p10).GetValue(self),
                ((CInstanceMember<P11>)_p11).GetValue(self),
                ((CInstanceMember<P12>)_p12).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 11);
            treeTask.SetVariable(paramName, ((CInstanceMember<P12>)_p12).GetValue(self));
        }
    }

    public class CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11, P12 p12, P13 p13);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;
        IInstanceMember _p12;
        IInstanceMember _p13;

        public CAgentMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
            _p12 = rhs._p12;
            _p13 = rhs._p13;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 13);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
            _p12 = AgentMeta.ParseProperty<P12>(paramStrs[11], Workspace);
            _p13 = AgentMeta.ParseProperty<P13>(paramStrs[12], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);
            Debugs.Check(_p12 != null);
            Debugs.Check(_p13 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent,
                ((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self),
                ((CInstanceMember<P5>)_p5).GetValue(self),
                ((CInstanceMember<P6>)_p6).GetValue(self),
                ((CInstanceMember<P7>)_p7).GetValue(self),
                ((CInstanceMember<P8>)_p8).GetValue(self),
                ((CInstanceMember<P9>)_p9).GetValue(self),
                ((CInstanceMember<P10>)_p10).GetValue(self),
                ((CInstanceMember<P11>)_p11).GetValue(self),
                ((CInstanceMember<P12>)_p12).GetValue(self),
                ((CInstanceMember<P13>)_p13).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 11);
            treeTask.SetVariable(paramName, ((CInstanceMember<P12>)_p12).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 12);
            treeTask.SetVariable(paramName, ((CInstanceMember<P13>)_p13).GetValue(self));
        }
    }

    public class CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(Agent a, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11, P12 p12, P13 p13, P14 p14);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;
        IInstanceMember _p12;
        IInstanceMember _p13;
        IInstanceMember _p14;

        public CAgentMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentMethodVoid(CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
            _p12 = rhs._p12;
            _p13 = rhs._p13;
            _p14 = rhs._p14;
        }

        public override IMethod Clone()
        {
            return new CAgentMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 14);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
            _p12 = AgentMeta.ParseProperty<P12>(paramStrs[11], Workspace);
            _p13 = AgentMeta.ParseProperty<P13>(paramStrs[12], Workspace);
            _p14 = AgentMeta.ParseProperty<P14>(paramStrs[13], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);
            Debugs.Check(_p12 != null);
            Debugs.Check(_p13 != null);
            Debugs.Check(_p14 != null);

            Agent agent = Utils.GetParentAgent(self, _instance);

            _fp(agent,
                ((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self),
                ((CInstanceMember<P5>)_p5).GetValue(self),
                ((CInstanceMember<P6>)_p6).GetValue(self),
                ((CInstanceMember<P7>)_p7).GetValue(self),
                ((CInstanceMember<P8>)_p8).GetValue(self),
                ((CInstanceMember<P9>)_p9).GetValue(self),
                ((CInstanceMember<P10>)_p10).GetValue(self),
                ((CInstanceMember<P11>)_p11).GetValue(self),
                ((CInstanceMember<P12>)_p12).GetValue(self),
                ((CInstanceMember<P13>)_p13).GetValue(self),
                ((CInstanceMember<P14>)_p14).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 11);
            treeTask.SetVariable(paramName, ((CInstanceMember<P12>)_p12).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 12);
            treeTask.SetVariable(paramName, ((CInstanceMember<P13>)_p13).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 13);
            treeTask.SetVariable(paramName, ((CInstanceMember<P14>)_p14).GetValue(self));
        }
    }

    public class CAgentStaticMethodVoid : CAgentMethodVoidBase
    {
        public delegate Task FunctionPointer();

        FunctionPointer _fp;

        public CAgentStaticMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 0);

            _instance = instance;
        }

        public override Task Run(Agent self)
        {
            return _fp();
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
        }
    }

    public class CAgentStaticMethodVoid<P1> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(P1 p1);

        FunctionPointer _fp;
        IInstanceMember _p1;

        public CAgentStaticMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid<P1> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid<P1>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 1);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);

            _fp(((CInstanceMember<P1>)_p1).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));
        }
    }

    public class CAgentStaticMethodVoid<P1, P2> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(P1 p1, P2 p2);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;

        public CAgentStaticMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid<P1, P2> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid<P1, P2>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 2);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);

            _fp(((CInstanceMember<P1>)_p1).GetValue(self), ((CInstanceMember<P2>)_p2).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));
        }
    }

    public class CAgentStaticMethodVoid<P1, P2, P3> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(P1 p1, P2 p2, P3 p3);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;

        public CAgentStaticMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid<P1, P2, P3> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid<P1, P2, P3>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 3);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);

            _fp(((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));
        }
    }

    public class CAgentStaticMethodVoid<P1, P2, P3, P4> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;

        public CAgentStaticMethodVoid(FunctionPointer f, IInstanceMember p1, IInstanceMember p2, IInstanceMember p3, IInstanceMember p4, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid<P1, P2, P3, P4> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid<P1, P2, P3, P4>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 4);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
        }

        public override void Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);

            _fp(((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));
        }
    }

    public class CAgentStaticMethodVoid<P1, P2, P3, P4, P5> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;

        public CAgentStaticMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid<P1, P2, P3, P4, P5> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid<P1, P2, P3, P4, P5>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 5);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
        }

        public override Task Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);

            _fp(((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self),
                ((CInstanceMember<P5>)_p5).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));
        }
    }

    public class CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;

        public CAgentStaticMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 6);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
        }

        public override Task Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);

            _fp(((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self),
                ((CInstanceMember<P5>)_p5).GetValue(self),
                ((CInstanceMember<P6>)_p6).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));
        }
    }

    public class CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;

        public CAgentStaticMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 7);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
        }

        public override Task Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);

            _fp(((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self),
                ((CInstanceMember<P5>)_p5).GetValue(self),
                ((CInstanceMember<P6>)_p6).GetValue(self),
                ((CInstanceMember<P7>)_p7).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));
        }
    }

    public class CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8> : CAgentMethodVoidBase
    {
        public delegate Task FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;

        public CAgentStaticMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 8);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
        }

        public override Task Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);

            return _fp(((CInstanceMember<P1>)_p1).GetValue(self),
                 ((CInstanceMember<P2>)_p2).GetValue(self),
                 ((CInstanceMember<P3>)_p3).GetValue(self),
                 ((CInstanceMember<P4>)_p4).GetValue(self),
                 ((CInstanceMember<P5>)_p5).GetValue(self),
                 ((CInstanceMember<P6>)_p6).GetValue(self),
                 ((CInstanceMember<P7>)_p7).GetValue(self),
                 ((CInstanceMember<P8>)_p8).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));
        }
    }

    public class CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;

        public CAgentStaticMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 9);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
        }

        public override Task Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);

            return _fp(((CInstanceMember<P1>)_p1).GetValue(self),
                  ((CInstanceMember<P2>)_p2).GetValue(self),
                  ((CInstanceMember<P3>)_p3).GetValue(self),
                  ((CInstanceMember<P4>)_p4).GetValue(self),
                  ((CInstanceMember<P5>)_p5).GetValue(self),
                  ((CInstanceMember<P6>)_p6).GetValue(self),
                  ((CInstanceMember<P7>)_p7).GetValue(self),
                  ((CInstanceMember<P8>)_p8).GetValue(self),
                  ((CInstanceMember<P9>)_p9).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));
        }
    }

    public class CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> : CAgentMethodVoidBase
    {
        public delegate Task FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;

        public CAgentStaticMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 10);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
        }

        public override Task Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);

            return _fp(((CInstanceMember<P1>)_p1).GetValue(self),
                  ((CInstanceMember<P2>)_p2).GetValue(self),
                  ((CInstanceMember<P3>)_p3).GetValue(self),
                  ((CInstanceMember<P4>)_p4).GetValue(self),
                  ((CInstanceMember<P5>)_p5).GetValue(self),
                  ((CInstanceMember<P6>)_p6).GetValue(self),
                  ((CInstanceMember<P7>)_p7).GetValue(self),
                  ((CInstanceMember<P8>)_p8).GetValue(self),
                  ((CInstanceMember<P9>)_p9).GetValue(self),
                  ((CInstanceMember<P10>)_p10).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));
        }
    }

    public class CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> : CAgentMethodVoidBase
    {
        public delegate Task FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;

        public CAgentStaticMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 11);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
        }

        public override Task Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);

            return _fp(((CInstanceMember<P1>)_p1).GetValue(self),
                  ((CInstanceMember<P2>)_p2).GetValue(self),
                  ((CInstanceMember<P3>)_p3).GetValue(self),
                  ((CInstanceMember<P4>)_p4).GetValue(self),
                  ((CInstanceMember<P5>)_p5).GetValue(self),
                  ((CInstanceMember<P6>)_p6).GetValue(self),
                  ((CInstanceMember<P7>)_p7).GetValue(self),
                  ((CInstanceMember<P8>)_p8).GetValue(self),
                  ((CInstanceMember<P9>)_p9).GetValue(self),
                  ((CInstanceMember<P10>)_p10).GetValue(self),
                  ((CInstanceMember<P11>)_p11).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));
        }
    }

    public class CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> : CAgentMethodVoidBase
    {
        public delegate Task FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11, P12 p12);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;
        IInstanceMember _p12;

        public CAgentStaticMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
            _p12 = rhs._p12;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 12);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
            _p12 = AgentMeta.ParseProperty<P12>(paramStrs[11], Workspace);
        }

        public override Task Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);
            Debugs.Check(_p12 != null);

            return _fp(((CInstanceMember<P1>)_p1).GetValue(self),
                  ((CInstanceMember<P2>)_p2).GetValue(self),
                  ((CInstanceMember<P3>)_p3).GetValue(self),
                  ((CInstanceMember<P4>)_p4).GetValue(self),
                  ((CInstanceMember<P5>)_p5).GetValue(self),
                  ((CInstanceMember<P6>)_p6).GetValue(self),
                  ((CInstanceMember<P7>)_p7).GetValue(self),
                  ((CInstanceMember<P8>)_p8).GetValue(self),
                  ((CInstanceMember<P9>)_p9).GetValue(self),
                  ((CInstanceMember<P10>)_p10).GetValue(self),
                  ((CInstanceMember<P11>)_p11).GetValue(self),
                  ((CInstanceMember<P12>)_p12).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 11);
            treeTask.SetVariable(paramName, ((CInstanceMember<P12>)_p12).GetValue(self));
        }
    }

    public class CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11, P12 p12, P13 p13);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;
        IInstanceMember _p12;
        IInstanceMember _p13;

        public CAgentStaticMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
            _p12 = rhs._p12;
            _p13 = rhs._p13;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 13);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
            _p12 = AgentMeta.ParseProperty<P12>(paramStrs[11], Workspace);
            _p13 = AgentMeta.ParseProperty<P13>(paramStrs[12], Workspace);
        }

        public override Task Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);
            Debugs.Check(_p12 != null);
            Debugs.Check(_p13 != null);

            return _fp(((CInstanceMember<P1>)_p1).GetValue(self),
                 ((CInstanceMember<P2>)_p2).GetValue(self),
                 ((CInstanceMember<P3>)_p3).GetValue(self),
                 ((CInstanceMember<P4>)_p4).GetValue(self),
                 ((CInstanceMember<P5>)_p5).GetValue(self),
                 ((CInstanceMember<P6>)_p6).GetValue(self),
                 ((CInstanceMember<P7>)_p7).GetValue(self),
                 ((CInstanceMember<P8>)_p8).GetValue(self),
                 ((CInstanceMember<P9>)_p9).GetValue(self),
                 ((CInstanceMember<P10>)_p10).GetValue(self),
                 ((CInstanceMember<P11>)_p11).GetValue(self),
                 ((CInstanceMember<P12>)_p12).GetValue(self),
                 ((CInstanceMember<P13>)_p13).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 11);
            treeTask.SetVariable(paramName, ((CInstanceMember<P12>)_p12).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 12);
            treeTask.SetVariable(paramName, ((CInstanceMember<P13>)_p13).GetValue(self));
        }
    }

    public class CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> : CAgentMethodVoidBase
    {
        public delegate void FunctionPointer(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, P6 p6, P7 p7, P8 p8, P9 p9, P10 p10, P11 p11, P12 p12, P13 p13, P14 p14);

        FunctionPointer _fp;
        IInstanceMember _p1;
        IInstanceMember _p2;
        IInstanceMember _p3;
        IInstanceMember _p4;
        IInstanceMember _p5;
        IInstanceMember _p6;
        IInstanceMember _p7;
        IInstanceMember _p8;
        IInstanceMember _p9;
        IInstanceMember _p10;
        IInstanceMember _p11;
        IInstanceMember _p12;
        IInstanceMember _p13;
        IInstanceMember _p14;

        public CAgentStaticMethodVoid(FunctionPointer f, Workspace workspace) : base(workspace)
        {
            _fp = f;
        }

        public CAgentStaticMethodVoid(CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> rhs, Workspace workspace)
        : base(workspace, rhs)
        {
            _fp = rhs._fp;
            _p1 = rhs._p1;
            _p2 = rhs._p2;
            _p3 = rhs._p3;
            _p4 = rhs._p4;
            _p5 = rhs._p5;
            _p6 = rhs._p6;
            _p7 = rhs._p7;
            _p8 = rhs._p8;
            _p9 = rhs._p9;
            _p10 = rhs._p10;
            _p11 = rhs._p11;
            _p12 = rhs._p12;
            _p13 = rhs._p13;
            _p14 = rhs._p14;
        }

        public override IMethod Clone()
        {
            return new CAgentStaticMethodVoid<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this, Workspace);
        }

        public override void Load(string instance, string[] paramStrs)
        {
            Debugs.Check(paramStrs.Length == 14);

            _instance = instance;
            _p1 = AgentMeta.ParseProperty<P1>(paramStrs[0], Workspace);
            _p2 = AgentMeta.ParseProperty<P2>(paramStrs[1], Workspace);
            _p3 = AgentMeta.ParseProperty<P3>(paramStrs[2], Workspace);
            _p4 = AgentMeta.ParseProperty<P4>(paramStrs[3], Workspace);
            _p5 = AgentMeta.ParseProperty<P5>(paramStrs[4], Workspace);
            _p6 = AgentMeta.ParseProperty<P6>(paramStrs[5], Workspace);
            _p7 = AgentMeta.ParseProperty<P7>(paramStrs[6], Workspace);
            _p8 = AgentMeta.ParseProperty<P8>(paramStrs[7], Workspace);
            _p9 = AgentMeta.ParseProperty<P9>(paramStrs[8], Workspace);
            _p10 = AgentMeta.ParseProperty<P10>(paramStrs[9], Workspace);
            _p11 = AgentMeta.ParseProperty<P11>(paramStrs[10], Workspace);
            _p12 = AgentMeta.ParseProperty<P12>(paramStrs[11], Workspace);
            _p13 = AgentMeta.ParseProperty<P13>(paramStrs[12], Workspace);
            _p14 = AgentMeta.ParseProperty<P14>(paramStrs[13], Workspace);
        }

        public override Task Run(Agent self)
        {
            Debugs.Check(_p1 != null);
            Debugs.Check(_p2 != null);
            Debugs.Check(_p3 != null);
            Debugs.Check(_p4 != null);
            Debugs.Check(_p5 != null);
            Debugs.Check(_p6 != null);
            Debugs.Check(_p7 != null);
            Debugs.Check(_p8 != null);
            Debugs.Check(_p9 != null);
            Debugs.Check(_p10 != null);
            Debugs.Check(_p11 != null);
            Debugs.Check(_p12 != null);
            Debugs.Check(_p13 != null);
            Debugs.Check(_p14 != null);

            _fp(((CInstanceMember<P1>)_p1).GetValue(self),
                ((CInstanceMember<P2>)_p2).GetValue(self),
                ((CInstanceMember<P3>)_p3).GetValue(self),
                ((CInstanceMember<P4>)_p4).GetValue(self),
                ((CInstanceMember<P5>)_p5).GetValue(self),
                ((CInstanceMember<P6>)_p6).GetValue(self),
                ((CInstanceMember<P7>)_p7).GetValue(self),
                ((CInstanceMember<P8>)_p8).GetValue(self),
                ((CInstanceMember<P9>)_p9).GetValue(self),
                ((CInstanceMember<P10>)_p10).GetValue(self),
                ((CInstanceMember<P11>)_p11).GetValue(self),
                ((CInstanceMember<P12>)_p12).GetValue(self),
                ((CInstanceMember<P13>)_p13).GetValue(self),
                ((CInstanceMember<P14>)_p14).GetValue(self));
        }

        public override void SetTaskParams(Agent self, BehaviorTreeTask treeTask)
        {
            string paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 0);
            treeTask.SetVariable(paramName, ((CInstanceMember<P1>)_p1).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 1);
            treeTask.SetVariable(paramName, ((CInstanceMember<P2>)_p2).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 2);
            treeTask.SetVariable(paramName, ((CInstanceMember<P3>)_p3).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 3);
            treeTask.SetVariable(paramName, ((CInstanceMember<P4>)_p4).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 4);
            treeTask.SetVariable(paramName, ((CInstanceMember<P5>)_p5).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 5);
            treeTask.SetVariable(paramName, ((CInstanceMember<P6>)_p6).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 6);
            treeTask.SetVariable(paramName, ((CInstanceMember<P7>)_p7).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 7);
            treeTask.SetVariable(paramName, ((CInstanceMember<P8>)_p8).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 8);
            treeTask.SetVariable(paramName, ((CInstanceMember<P9>)_p9).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 9);
            treeTask.SetVariable(paramName, ((CInstanceMember<P10>)_p10).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 10);
            treeTask.SetVariable(paramName, ((CInstanceMember<P11>)_p11).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 11);
            treeTask.SetVariable(paramName, ((CInstanceMember<P12>)_p12).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 12);
            treeTask.SetVariable(paramName, ((CInstanceMember<P13>)_p13).GetValue(self));

            paramName = string.Format("{0}{1}", Tasks.LOCAL_TASK_PARAM_PRE, 13);
            treeTask.SetVariable(paramName, ((CInstanceMember<P14>)_p14).GetValue(self));
        }
    }

}//namespace behaviac
