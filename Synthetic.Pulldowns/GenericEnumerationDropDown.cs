using System;
using System.Collections.Generic;
using System.Linq;
using DSRevitNodesUI;

using ProtoCore.AST.AssociativeAST;
using Dynamo.Utilities;

using Synthetic.Core;

namespace Synthetic.Pulldowns
{
    /// <summary>
    /// Generic UI Dropdown node baseclass for Enumerations.
    /// This class populates a dropdown with all enumeration values of the specified type.
    /// Written by Konrad Sobon in post on Dynamo Forum.
    /// https://forum.dynamobim.com/t/c-dynamo-nodes-dropdown-input-node/10225/4
    /// </summary>
    public abstract class GenericEnumerationDropDown : RevitDropDownBase
    {
        /// <summary>
        /// Generic Enumeration Dropdown
        /// </summary>
        /// <param name="name">Node Name</param>
        /// <param name="enumerationType">Type of Enumeration to Display</param>
        public GenericEnumerationDropDown(string name, Type enumerationType) : base(name) { this.EnumerationType = enumerationType; PopulateDropDownItems(); }

        /// <summary>
        /// Type of Enumeration
        /// </summary>
        private Type EnumerationType
        {
            get;
            set;
        }

        protected override CoreNodeModels.DSDropDownBase.SelectionState PopulateItemsCore(string currentSelection)
        {
            PopulateDropDownItems();
            return SelectionState.Done;
        }

        /// <summary>
        /// Populate Items in Dropdown menu
        /// </summary>
        public void PopulateDropDownItems()
        {
            if (this.EnumerationType != null)
            {
                // Clear the dropdown list
                Items.Clear();

                // Get all enumeration names and add them to the dropdown menu
                foreach (string name in Enum.GetNames(EnumerationType))
                {
                    Items.Add(new CoreNodeModels.DynamoDropDownItem(name, Enum.Parse(EnumerationType, name)));
                }

                Items = Items.OrderBy(x => x.Name).ToObservableCollection();
            }
        }

        /// <summary>
        /// Assign the selected Enumeration value to the output
        /// </summary>
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // If the dropdown is still empty try to populate it again          
            if (Items.Count == 0 || Items.Count == -1)
            {
                if (this.EnumerationType != null && Enum.GetNames(this.EnumerationType).Length > 0)
                {
                    PopulateItems();
                }
            }

            var args = new List<AssociativeNode>
             {
                AstFactory.BuildStringNode(this.EnumerationType.ToString()),
                AstFactory.BuildStringNode(((System.Object) Items[SelectedIndex].Item).ToString())
             };

            var func = new Func<string, string, object>(Enumeration.Parse);

            var functionCall = AstFactory.BuildFunctionCall( func, args);
            var assign = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall);

            // return the enumeration value
            return new List<AssociativeNode> { assign };
        }
    }
}
