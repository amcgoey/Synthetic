using System;

using CoreNodeModels;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;

using revitDB = Autodesk.Revit.DB;

using Synthetic.Pulldowns;

namespace Synthetic.Revit.Select
{
    /// <summary>
    /// 
    /// </summary>
    [NodeName("Select ElementOnPhaseStatus")]
    [NodeDescription("Selects a ElementOnPhaseStatus")]

    [IsDesignScriptCompatible]
    public class DropDownElementOnPhaseStatus : GenericEnumerationDropDown
    {
        /// <summary>
        /// 
        /// </summary>
        public DropDownElementOnPhaseStatus() : base("Select ElementOnPhaseStatus",
            typeof(revitDB.ElementOnPhaseStatus))
        { }
    }
}


