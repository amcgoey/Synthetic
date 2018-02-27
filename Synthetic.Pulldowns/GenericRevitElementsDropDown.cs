using System;
using System.Collections.Generic;
using System.Linq;
using DSRevitNodesUI;

using ProtoCore.AST.AssociativeAST;
using Dynamo.Utilities;

using Synthetic.Core;
using Synthetic.Revit;

using RevitServices.Persistence;
using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;

namespace Synthetic.Pulldowns
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class GenericRevitElementsDropDown : RevitDropDownBase
    {
        /// <summary>
        /// Generic Dropdown that selects a Revit element given a type.
        /// </summary>
        /// <param name="name">Node Name</param>
        /// <param name="objectType">Type of Enumeration to Display</param>
        /// <param name="selectionFunction"></param>
        public GenericRevitElementsDropDown(string name, Type objectType, Func<string, object> selectionFunction) : base(name)
        {
            this.ObjectType = objectType;
            this.SelectionFunction = selectionFunction;
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
        private Func<Type, string> ListFunction
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private Func<string, object> SelectionFunction
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

                revitDoc doc = DocumentManager.Instance.CurrentDBDocument;
                revitDB.ElementFilter typeFilter = new revitDB.ElementClassFilter(ObjectType);

                revitDB.FilteredElementCollector collect = new revitDB.FilteredElementCollector(doc);
                collect.WherePasses(typeFilter);

                // Get all enumeration names and add them to the dropdown menu
                foreach (revitDB.Element elem in collect)
                {
                    Items.Add(new CoreNodeModels.DynamoDropDownItem(elem.Name, elem));
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
                if (this.ObjectType != null)
                {
                    PopulateItems();
                }
            }

            var args = new List<AssociativeNode>
             {
                AstFactory.BuildStringNode(((revitDB.Element) Items[SelectedIndex].Item).Name)
             };

            var func = SelectionFunction;

            var functionCall = AstFactory.BuildFunctionCall(func, args);
            var assign = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall);

            // return the enumeration value
            return new List<AssociativeNode> { assign };
        }
    }
}
