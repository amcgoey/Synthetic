using System;

using CoreNodeModels;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;

using revitDB = Autodesk.Revit.DB;

using Synthetic.Pulldowns;

namespace Synthetic.Revit.FilterRules
{
    /// <summary>
    /// 
    /// </summary>
    [NodeName("FilterParameter.StringRules")]
    [NodeDescription("Choose how a to evaluate a filter of a string parameter.")]

    //[OutPortNames("FilterStringRule")]
    //[OutPortTypes("synthColl.FilterStringRules")]
    //[OutPortDescriptions("An enum representing the Filter String Rule evaluator.")]

    [IsDesignScriptCompatible]
    public class FilterStringRulesDropDown : GenericEnumerationDropDown
    {
        /// <summary>
        /// 
        /// </summary>
        public FilterStringRulesDropDown() : base("FilterStringRule", typeof(FilterStringRules)) { }
    }
}


