using System;

using CoreNodeModels;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;

using revitDB = Autodesk.Revit.DB;

using Synthetic.Pulldowns;

namespace Synthetic.Revit.CollectorFilterRules
{
    /// <summary>
    /// 
    /// </summary>
    [NodeName("FilterParameter.StringRules")]
    [NodeDescription("Choose how a to evaluate a filter of a string parameter.")]

    [IsDesignScriptCompatible]
    public class FilterStringRulesDropDown : GenericEnumerationDropDown
    {
        /// <summary>
        /// 
        /// </summary>
        public FilterStringRulesDropDown() : base("StringRule", typeof(CollectorFilterRules.StringRules)) { }
    }

    /// <summary>
    /// 
    /// </summary>
    [NodeName("FilterParameter.NumericRules")]
    [NodeDescription("Choose how a to evaluate a filter of a numeric parameter.")]

    [IsDesignScriptCompatible]
    public class FilterNumericRulesDropDown : GenericEnumerationDropDown
    {
        /// <summary>
        /// 
        /// </summary>
        public FilterNumericRulesDropDown() : base("NumericRule", typeof(CollectorFilterRules.NumericRules)) { }
    }
}


