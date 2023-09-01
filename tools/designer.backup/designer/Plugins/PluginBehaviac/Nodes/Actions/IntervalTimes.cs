using Behaviac.Design;
using Behaviac.Design.Attributes;
using Behaviac.Design.Nodes;
using PluginBehaviac.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace PluginBehaviac.Nodes
{
    [NodeDesc("Actions", NodeIcon.Wait)]
    public class IntervalTimes : Behaviac.Design.Nodes.Sequence
    {
        public IntervalTimes() : base(Resources.IntervalTimes, Resources.IntervalTimesDesc)
        {
            _exportName = "IntervalTimes";
        }
        public override string ExportClass
        {
            get
            {
                return "IntervalTimes";
            }
        }

        private RightValueDef _time = new RightValueDef(new VariableDef((int)100));
        [DesignerRightValueEnum("Wait", "WaitDesc", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, DesignerPropertyEnum.AllowStyles.ConstAttributesMethod, MethodType.Getter, "", "", ValueTypes.Int)]
        public RightValueDef Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
            }
        }
        [DesignerRightValueEnum("Front", "FrontDesc", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, DesignerPropertyEnum.AllowStyles.ConstAttributesMethod, MethodType.Getter, "", "", ValueTypes.Bool)]
        public RightValueDef Front { set; get; }

        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            IntervalTimes dec = (IntervalTimes)newnode;

            if (_time != null)
            {
                dec._time = (RightValueDef)_time.Clone();
            }
        }

        public override void CheckForErrors(BehaviorNode rootBehavior, List<ErrorCheck> result)
        {
            Type valueType = (this._time != null) ? this._time.ValueType : null;

            if (valueType == null)
            {
                result.Add(new Node.ErrorCheck(this, ErrorCheckLevel.Error, "Time is not set!"));
            }
            else
            {
                string typeName = Plugin.GetNativeTypeName(valueType.FullName);

                if (!Plugin.IsIntergerNumberType(typeName))
                {
                    result.Add(new Node.ErrorCheck(this, ErrorCheckLevel.Error, "Time should be an integer number type!"));
                }
            }

            base.CheckForErrors(rootBehavior, result);
        }
    }
}
