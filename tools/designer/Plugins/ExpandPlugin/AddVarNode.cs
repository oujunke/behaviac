using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Behaviac.Design;
using Behaviac.Design.Attributes;
using Behaviac.Design.Nodes;

namespace ExpandPlugin.Nodes
{
    // NodeDesc attribute is used for the icon in the designer.
    // If using your icon for the node, you should follow such steps:
    //   1) Add your icon image (e.g. "exampleNodeIcon") into the Resources.resx
    //   2) Add the following codes:
    //      [NodeDesc("Actions", "exampleNodeIcon")]
    //
    [NodeDesc("Actions", NodeIcon.Action)]
    public class AddVarNode : Behaviac.Design.Nodes.Node
    {
        public AddVarNode()
            : base("添加局部变量", "加入局部变量")
        {
            Plugin.AddInstanceName("EN", "AddVarNode", "添加局部变量", "添加局部变量");
        }

        public override string ExportClass
        {
            get { return "AddVarNode"; }
        }
        
        private bool _ignoreTimeScale = false;
        [DesignerBoolean("IgnoreTimeScale", "IgnoreTimeScaleDesc", "CategoryBasic", DesignerProperty.DisplayMode.NoDisplay, 0, DesignerProperty.DesignerFlags.NoFlags)]
        public bool IgnoreTimeScale
        {
            get { return _ignoreTimeScale; }
            set { this._ignoreTimeScale = value; }
        }

        [DesignerTypeEnum("变量类型","选择要添加的变量类型",)]
        public string VarType { set; get; }
        protected VariableDef _time = new VariableDef(1000.0f);
        [DesignerPropertyEnum("Duration", "DurationDesc", "CategoryBasic", DesignerProperty.DisplayMode.Parameter, 1, DesignerProperty.DesignerFlags.NoFlags, DesignerPropertyEnum.AllowStyles.ConstAttributes, "", "", ValueTypes.Int)]
        public VariableDef Time
        {
            get { return _time; }
            set { this._time = value; }
        }

        protected override void CloneProperties(Node newnode)
        {
            base.CloneProperties(newnode);

            AddVarNode dec = (AddVarNode)newnode;
            if (_time != null)
                dec._time = (VariableDef)_time.Clone();
        }

        private readonly static Brush __defaultBackgroundBrush = new SolidBrush(Color.FromArgb(157, 75, 39));
        protected override Brush DefaultBackgroundBrush
        {
            get { return __defaultBackgroundBrush; }
        }

        public override NodeViewData CreateNodeViewData(NodeViewData parent, Behaviac.Design.Nodes.BehaviorNode rootBehavior)
        {
            NodeViewData nvd = base.CreateNodeViewData(parent, rootBehavior);
            nvd.ChangeShape(NodeShape.Rectangle);

            return nvd;
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
                if (!Plugin.IsIntergerType(valueType) && !Plugin.IsFloatType(valueType))
                {
                    result.Add(new Node.ErrorCheck(this, ErrorCheckLevel.Error, "Time should be a float number type!"));
                }
            }

            base.CheckForErrors(rootBehavior, result);
        }
    }
}
