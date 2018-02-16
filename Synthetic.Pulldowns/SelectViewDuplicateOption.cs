using System;

using CoreNodeModels;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;

using revitDB = Autodesk.Revit.DB;

using Synthetic.Pulldowns;

namespace Synthetic.Revit.CollectorFilterRulesSelect
{
    /// <summary>
    /// 
    /// </summary>
    [NodeName("View.ViewDuplicateOption")]
    [NodeDescription("Select the View Duplication type")]

    [IsDesignScriptCompatible]
    public class DropDownViewDuplicateOption : GenericEnumerationDropDown
    {
        /// <summary>
        /// 
        /// </summary>
        public DropDownViewDuplicateOption() : base("ViewDuplciatOption",
            typeof(revitDB.ViewDuplicateOption)) { }
    }
}


