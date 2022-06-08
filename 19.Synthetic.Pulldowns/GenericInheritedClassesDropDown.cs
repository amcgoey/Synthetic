using System;
using System.Collections.Generic;
using System.Linq;
using DSRevitNodesUI;

using ProtoCore.AST.AssociativeAST;
using Dynamo.Utilities;

using Synthetic.Core;
using Synthetic.Revit;

namespace Synthetic.Pulldowns
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class GenericInheritedClassesDropDown : RevitDropDownBase
    {
        /// <summary>
        /// Generic Dropdown that creates an instance of a type from a list of classes that inherit from a base class.
        /// </summary>
        /// <param name="name">Node Name</param>
        /// <param name="objectType">Type of Enumeration to Display</param>
        /// <param name="function"></param>
        public GenericInheritedClassesDropDown(string name, Type objectType, Func<string, object> function) : base(name)
        {
            this.ObjectType = objectType;
            this.Function = function;
            PopulateDropDownItems();
        }

        /// <summary>
        /// 
        /// </summary>
        private Type ObjectType
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private Func<string, object> Function
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentSelection"></param>
        /// <returns></returns>
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
            if (this.ObjectType != null)
            {
                // Clear the dropdown list
                Items.Clear();

                // Get all enumeration names and add them to the dropdown menu
                foreach (Type type in Assemblies.GetEnumerableOfType(this.ObjectType))
                {
                    Items.Add(new CoreNodeModels.DynamoDropDownItem(type.ToString(), type));
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
                if (this.ObjectType != null && Assemblies.GetEnumerableOfType(this.ObjectType).Count > 0)
                {
                    PopulateItems();
                }
            }

            var args = new List<AssociativeNode>
             {
                AstFactory.BuildStringNode(((System.Object) Items[SelectedIndex].Item).ToString())
             };

            var func = Function;

            var functionCall = AstFactory.BuildFunctionCall(func, args);
            var assign = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall);

            // return the enumeration value
            return new List<AssociativeNode> { assign };
        }
    }
}
