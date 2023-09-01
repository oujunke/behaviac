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
    [NodeDesc("Actions", NodeIcon.WaitFrame)]
    public class IntervalFrames : Behaviac.Design.Nodes.Sequence
    {
        public IntervalFrames() : base(Resources.IntervalFrames, Resources.IntervalFramesDesc)
        {
            _exportName = "IntervalFrames";
        }
        public override string ExportClass
        {
            get
            {
                return "IntervalFrames";
            }
        }

        private RightValueDef _count = new RightValueDef(new VariableDef((int)100));
        [DesignerRightValueEnum("Count", "CountDesc", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, DesignerPropertyEnum.AllowStyles.ConstAttributesMethod, MethodType.Getter, "", "", ValueTypes.Int)]
        public RightValueDef Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
            }
        }
        [DesignerRightValueEnum("Front", "FrontDesc", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 0, DesignerProperty.DesignerFlags.NoFlags, DesignerPropertyEnum.AllowStyles.ConstAttributesMethod, MethodType.Getter, "", "", ValueTypes.Bool)]
        public RightValueDef Front { set; get; }

        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            IntervalFrames dec = (IntervalFrames)newnode;

            if (_count != null)
            {
                dec._count = (RightValueDef)_count.Clone();
            }
        }

        public override void CheckForErrors(BehaviorNode rootBehavior, List<ErrorCheck> result)
        {
            Type valueType = (this._count != null) ? this._count.ValueType : null;

            if (valueType == null)
            {
                result.Add(new Node.ErrorCheck(this, ErrorCheckLevel.Error, "Count is not set!"));
            }
            else
            {
                string typeName = Plugin.GetNativeTypeName(valueType.FullName);

                if (!Plugin.IsIntergerNumberType(typeName))
                {
                    result.Add(new Node.ErrorCheck(this, ErrorCheckLevel.Error, "Count should be an integer number type!"));
                }
            }

            base.CheckForErrors(rootBehavior, result);
        }
    }
}
