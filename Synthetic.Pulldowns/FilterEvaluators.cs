using System;
using System.Linq;
using System.Collections.Generic;
using CoreNodeModels;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;

using revitDB = Autodesk.Revit.DB;

namespace Synthetic.Revit.Collector
{
    /// <summary>
    /// 
    /// </summary>
    [NodeName("FilterEvaluators")]
    [NodeDescription("Create a Filter Evaluator")]
    [IsDesignScriptCompatible]
    public class DropDownFilterEvaluators : DSDropDownBase
    {
        /// <summary>
        /// 
        /// </summary>
        public DropDownFilterEvaluators() : base("item")
        {
            InPortData.Add(new PortData("parameterId", "The Element Id of the parameter as an integer"));
            OutPortData[0] = new PortData("FilterRule", "Filter rule");

            RegisterAllPorts();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentSelection"></param>
        /// <returns></returns>
        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            // The Items collection contains the elements
            // that appear in the list. For this example, we
            // clear the list before adding new items, but you
            // can also use the PopulateItems method to add items
            // to the list.

            Items.Clear();

            // Create a number of DynamoDropDownItem objects 
            // to store the items that we want to appear in our list.

            var newItems = new List<DynamoDropDownItem>()
            {
                new DynamoDropDownItem("String Begins With", new revitDB.FilterStringBeginsWith()),
                new DynamoDropDownItem("String Contains", new revitDB.FilterStringContains()),
                new DynamoDropDownItem("String EndsWith", new revitDB.FilterStringEndsWith())
            };

            Items.AddRange(newItems);

            // Set the selected index to something other
            // than -1, the default, so that your list
            // has a pre-selection.

            SelectedIndex = 0;
            return SelectionState.Done;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // Build an AST node for the type of object contained in your Items collection.

            var intNode = AstFactory.BuildIntNode((int)Items[SelectedIndex].Item);
            var assign = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), intNode);
            AstFactory.

            return new List<AssociativeNode> { assign };
        }
    }
}