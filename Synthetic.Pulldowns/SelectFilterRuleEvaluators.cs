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
    [NodeName("FilterParameter.StringRules")]
    [NodeDescription("Choose how a to evaluate a filter of a string parameter.")]

    [IsDesignScriptCompatible]
    public class FilterStringRulesDropDown : GenericInheritedClassesPulldown
    {
        /// <summary>
        /// 
        /// </summary>
        public FilterStringRulesDropDown() : base("StringRule",
            typeof(revitDB.FilterStringRuleEvaluator),
            Synthetic.Revit.CollectorFilterRules.FilterStringRules) { }
    }

    /// <summary>
    /// 
    /// </summary>
    [NodeName("FilterParameter.NumericRules")]
    [NodeDescription("Choose how a to evaluate a filter of a numeric parameter.")]

    [IsDesignScriptCompatible]
    public class FilterNumericRulesDropDown : GenericInheritedClassesPulldown
    {
        /// <summary>
        /// 
        /// </summary>
        public FilterNumericRulesDropDown() : base("NumericRule",
            typeof(revitDB.FilterNumericRuleEvaluator),
            Synthetic.Revit.CollectorFilterRules.FilterNumericRules) { }
    }
}


