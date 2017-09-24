using System;
using System.Linq;
using System.Collections.Generic;
using CoreNodeModels;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;

using revitDB = Autodesk.Revit.DB;

namespace Synthetic.Revit.Parameters
{
    /// <summary>
    /// 
    /// </summary>
    [NodeName("Parameters.SelectBuiltIn")]
    [NodeDescription("Select Built-in parameters")]

    [OutPortNames("ParameterId")]
    [OutPortTypes("int")]
    [OutPortDescriptions("Element Id of the parameter as an integer")]

    [IsDesignScriptCompatible]
    public class DropDownBuiltInParameters : DSDropDownBase
    {
        /// <summary>
        /// 
        /// </summary>
        public DropDownBuiltInParameters() : base("item")
        {
            RegisterAllPorts();
        }

        /// <summary>
        /// Adds items to the pulldown list.
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

            //gets each item from the ParameterTypes as a string
            foreach (var constant in Enum.GetValues(typeof(revitDB.BuiltInParameter)))
            {
                Items.Add(new DynamoDropDownItem(constant.ToString(), constant));
            }

            //Adds items to the dropdown list
            Items = Items.OrderBy(x => x.Name).ToObservableCollection();


            // Set the selected index to something other
            // than -1, the default, so that your list
            // has a pre-selection.

            //SelectedIndex = 0;

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

            return new List<AssociativeNode> { assign };
        }
    }
}