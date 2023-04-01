using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBehaviorBase.BModels
{
    public class BMethod
    {
        //Name="Enter" DisplayName="Enter" Desc="进入下级变量域" Class="GameAgent" ReturnType="void" ReturnTypeFullName="System.Void" Static="false" Public="true" istask="false" 
        public string Name { set; get; }
        public bool Public { set; get; }
        public bool Static { set; get; }
        public bool IsTask { set; get; }
        public string DisplayName { set; get; }
        public string Desc { set; get; }
        public string Class { set; get; }
        public string ReturnTypeFullName { set; get; }
        public string ReturnType { set; get; }
        public List<BMember> Param { set; get; }
    }
}
